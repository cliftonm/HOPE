using System;
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

		[UserConfigurableProperty("Greeter Text:")]
		public string GreeterText { get; set; }

		public DayGreeter(IReceptorSystem rsys)						   
			: base(rsys)
		{
			AddEmitProtocol("Text");
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (!String.IsNullOrEmpty(GreeterText))
			{
				string msg = Parse(GreeterText);
				CreateCarrier("TextToSpeech", signal => signal.Text = msg);
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
		protected string Parse(string text)
		{
			StringBuilder sb = new StringBuilder();

			while (text.Contains("["))
			{
				string left = text.LeftOf('[');
				string token = text.Between('[', ']');
				string tokenValue = "";
				DateTime now = DateTime.Now;

				switch (token.ToLower())
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
						int h = now.Hour % 12;
						tokenValue = NumWordsWrapper(h);
						break;

					case "minute":
						tokenValue = NumWordsWrapper(now.Minute);
						break;

					case "ampm":
						tokenValue = ((now.Hour >=12) ? "PM" : "AM");
						break;
				}

				sb.Append(left);
				sb.Append(tokenValue);
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
				return "zero";
			try
			{
				string[] splitter = n.ToString().Split('.');
				intPart = double.Parse(splitter[0]);
				decPart = double.Parse(splitter[1]);
			}
			catch
			{
				intPart = n;
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
				if ((int)n / 10 > 1)
				{
					if (words != "")
						words += " ";
					words += tensArr[(int)n / 10 - 2];
					tens = true;
					n %= 10;
				}

				if (n < 20)
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
