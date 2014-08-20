using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Clifton.ExtensionMethods;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace DayGreeterReceptor
{
	public class DayGreeter : BaseReceptor
    {
		public override string Name { get { return "Day Greeter"; } }
		public override string ConfigurationUI { get { return "DayGreeterConfig.xml"; } }

		protected string greeterText;
		// Handles responses that should be grouped together and not emitted until all responses have been received.
		protected Dictionary<string, StringBuilder> groups;

		[UserConfigurableProperty("Greeter Text:")]
		public string GreeterText 
		{
			get { return greeterText; }
			set
			{
				greeterText = value;

				if (systemInitialized)
				{
					ProcessGreeterText();
				}
			}
		}

		public DayGreeter(IReceptorSystem rsys)						   
			: base(rsys)
		{
			AddEmitProtocol("Text");
			AddEmitProtocol("RSSFeedUrl");
			AddReceiveProtocol("RSSFeedItem", (Action<dynamic>)(s => ProcessFeedItem(s)));

			groups = new Dictionary<string, StringBuilder>();
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();
			ProcessGreeterText();
		}

		protected void ProcessGreeterText()
		{
			if (!String.IsNullOrEmpty(GreeterText))
			{
				string msg = Parse(GreeterText);
				CreateCarrier("TextToSpeech", signal => signal.Text = msg);
			}
		}

		protected void ProcessFeedItem(dynamic signal)
		{
			string title = signal.Title;
			string tag = signal.Tag;

			if (groups.ContainsKey(tag))
			{
				groups[tag].Append(title);
				groups[tag].Append("\r\n");
				int m = signal.MofN.M;
				int n = signal.MofN.N;

				// Have all items been received for this feed?
				if (m == n)
				{
					CreateCarrier("TextToSpeech", outsig => outsig.Text = groups[tag].ToString());
					groups.Remove(tag);
				}
			}
			else
			{
				CreateCarrier("TextToSpeech", outsig => outsig.Text = title);
			}
		}

		// Tokens we know about so far:
		// [dayperiod] Morning, Afternoon, Evening
		// [dayofweek] Monday, Tuesday, etc...
		// [month] January, February, etc...
		// [dateofmonth] 1, 2, 3, etc...
		// [hour12] 12 hour format hour of now.
		// [minute] the minute of now.
		// [ampm] "AM" or "PM" text.
		// [feed:url,5] to parse a feed with optional items.
		protected string Parse(string text)
		{
			StringBuilder sb = new StringBuilder();
			StringBuilder sbGroup = null;
			bool inGroup = false;
			Guid groupGuid = Guid.Empty;

			while (text.Contains("["))
			{
				string left = text.LeftOf('[');
				string token = text.Between('[', ']');
				string tokenValue = "";
				DateTime now = DateTime.Now;

				// token is stripped of optional parameters that follow after the ':'
				switch (token.ToLower().LeftOf(':'))
				{
					case "dayperiod":
						if (now.Hour < 12)
						{
							tokenValue = "Morning";
						}
						else if (now.Hour < 18)
						{
							tokenValue = "Afternoon";
						}
						else
						{
							tokenValue = "Evening";
						}
						break;

					case "dayofweek":
						tokenValue = now.DayOfWeek.ToString();
						break;

					case "month":
						tokenValue = now.ToString("MMMM", CultureInfo.InvariantCulture);
						break;

					case "dateofmonth":
						tokenValue = now.Day.ToString();
						break;

					case "hour12":
						int h = now.Hour;

						if (h == 0)
						{
							// midnight to 0:59 is, for example "oh thirty" for 0:30
							tokenValue = "oh";
						}
						else if (h == 12)
						{
							// specific case for noon to 12:59.
							tokenValue = "twelve";
						}
						else
						{
							// Otherwise, back to 1-11.
							tokenValue = NumWordsWrapper(h % 12);
						}
						break;

					case "minute":
						if (now.Minute > 0)
						{
							tokenValue = NumWordsWrapper(now.Minute);

							// No minutes reported at all if exactly 0, example: nine AM for 9:00
							if (now.Minute < 10)
							{
								// oh-five, so we output, for example: "nine oh five" for 9:05.
								tokenValue = "oh " + tokenValue;
							}
						}
						break;

					case "ampm":
						tokenValue = ((now.Hour >=12) ? "PM" : "AM");
						break;

					case "feed":
						{
							// left of ','
							string url = token.RightOf(':').LeftOf(',');
							int numItems = Int32.MaxValue;
							
							if (token.Contains(","))
							{
								numItems = Convert.ToInt32(token.RightOf(','));
								CreateCarrier("RSSFeedUrl", signal =>
									{
										signal.FeedUrl.Value = url;
										signal.MaxItems = numItems;
										signal.Tag = groupGuid.ToString();
									});
							}

							break;
						}
					
					case "group":
						// Everything in this section will be rendered once the request (which we don't know what it is, but there needs to be one)
						// is completed.
						inGroup = true;
						groupGuid = Guid.NewGuid();
						sbGroup = new StringBuilder();
						break;

					case "/group":
						left = left.Trim();			// Remove any CRLF's.  TODO: Kludgy!  
						inGroup = false;
						groups[groupGuid.ToString()] = sbGroup;
						break;
				}

				if (inGroup)
				{
					sbGroup.Append(left);
					sbGroup.Append(tokenValue);
				}
				else
				{
					sb.Append(left);
					sb.Append(tokenValue);
				}

				text = text.RightOf("]");
			}

			sb.Append(text);

			return sb.ToString();
		}

		// http://www.codeproject.com/Questions/455350/how-to-convert-numbers-into-texual-format-using-cs
		protected String NumWordsWrapper(double n)
		{
			string words = "";
			double intPart;
			double decPart = 0;
			
			if (n == 0)
			{
				return "zero";
			}

			string[] splitter = n.ToString().Split('.');
			intPart = double.Parse(splitter[0]);

			if (splitter.Length == 2)
			{
				// We have a fractional component.
				decPart = double.Parse(splitter[1]);
			}

			words = NumWords(intPart);

			if (decPart > 0)
			{
				if (words != "")
					words += " and ";
				int counter = decPart.ToString().Length;
				switch (counter)
				{
					case 1: words += NumWords(decPart) + " tenths"; break;
					case 2: words += NumWords(decPart) + " hundredths"; break;
					case 3: words += NumWords(decPart) + " thousandths"; break;
					case 4: words += NumWords(decPart) + " ten-thousandths"; break;
					case 5: words += NumWords(decPart) + " hundred-thousandths"; break;
					case 6: words += NumWords(decPart) + " millionths"; break;
					case 7: words += NumWords(decPart) + " ten-millionths"; break;
				}
			}
			return words;
		}

		protected String NumWords(double n) //converts double to words
		{
			string[] numbersArr = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
			string[] tensArr = new string[] { "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
			string[] suffixesArr = new string[] { "thousand", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion", "duodecillion", "tredecillion", "Quattuordecillion", "Quindecillion", "Sexdecillion", "Septdecillion", "Octodecillion", "Novemdecillion", "Vigintillion" };
			string words = "";

			bool tens = false;

			if (n < 0)
			{
				words += "negative ";
				n *= -1;
			}

			int power = (suffixesArr.Length + 1) * 3;

			// TODO: Clean this up - it should be resolvable without resorting to a while loop!
			while (power > 3)
			{
				double pow = Math.Pow(10, power);
				if (n > pow)
				{
					if (n % Math.Pow(10, power) > 0)
					{
						words += NumWords(Math.Floor(n / pow)) + " " + suffixesArr[(power / 3) - 1] + ", ";
					}
					else if (n % pow > 0)
					{
						words += NumWords(Math.Floor(n / pow)) + " " + suffixesArr[(power / 3) - 1];
					}
					n %= pow;
				}
				power -= 3;
			}

			if (n >= 1000)
			{
				// TODO: Gross.  this should be cleaner.
				if (n % 1000 > 0) words += NumWords(Math.Floor(n / 1000)) + " thousand, ";
				else words += NumWords(Math.Floor(n / 1000)) + " thousand";
				n %= 1000;
			}
			if (0 <= n && n <= 999)
			{
				if ((int)n / 100 > 0)
				{
					words += NumWords(Math.Floor(n / 100)) + " hundred";
					n %= 100;
				}

				// TODO: Why no "and" here if we say "five hundred [and] ..."

				if ((int)n / 10 > 1)
				{
					if (words != "")
						words += " ";
					words += tensArr[(int)n / 10 - 2];
					tens = true;
					n %= 10;
				}

				if ( (n < 20) && (n > 0) )		// 20, 30, 40, 50, etc... do not have further components.
				{
					if (words != "" && tens == false)
						words += " ";
					words += (tens ? "-" + numbersArr[(int)n - 1] : numbersArr[(int)n - 1]);
					n -= Math.Floor(n);
				}
			}

			return words;
		}

    }
}
