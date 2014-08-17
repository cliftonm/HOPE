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
						tokenValue = now.ToString("MMM", CultureInfo.InvariantCulture);
						break;

					case "dateofmonth":
						tokenValue = now.Day.ToString();
						break;

					case "hour12":
						int h = now.Hour % 12;
						tokenValue = h.ToString();
						break;

					case "minute":
						tokenValue = now.Minute.ToString();
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
    }
}
