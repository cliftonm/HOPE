using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Tools.Strings.Extensions;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace WeatherInfoReceptor
{
	public class FullInfo
	{
		public dynamic WeatherInfo { get; set; }
		public dynamic LocationInfo { get; set; }

		public bool Completed { get { return WeatherInfo != null && LocationInfo != null; } }
	}

	public class ReceptorDefinition : IReceptorInstance
	{
#pragma warning disable 67
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;
#pragma warning restore 67

		public string Name { get { return "Weather Info"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		public IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;
		protected Dictionary<string, FullInfo> zipcodeInfoMap;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			zipcodeInfoMap = new Dictionary<string, FullInfo>();
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "WeatherInfo", "Location" };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { "TextToSpeech" };
		}

		public Func<dynamic, dynamic, bool> Qualifier = new Func<dynamic, dynamic, bool>((w, l) =>
			{
				return w.Zipcode == l.Zipcode;
			});

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public void ProcessCarrier(ICarrier carrier)
		{
			FullInfo fullInfo;

			// Duck-typing!
			if (!zipcodeInfoMap.TryGetValue(carrier.Signal.Zipcode, out fullInfo))
			{
				fullInfo = new FullInfo();
				zipcodeInfoMap[carrier.Signal.Zipcode] = fullInfo;
			}

			if (carrier.Protocol.DeclTypeName == "WeatherInfo")
			{
				fullInfo.WeatherInfo = carrier.Signal;				
			}

			if (carrier.Protocol.DeclTypeName == "Location")
			{
				fullInfo.LocationInfo = carrier.Signal;
			}

			if (fullInfo.Completed)
			{
				string conditions = ProcessConditions(fullInfo.WeatherInfo);
				string hazards = ProcessHazards(fullInfo.WeatherInfo);
				Say("Here is the weather for " + fullInfo.LocationInfo.City + " " + fullInfo.LocationInfo.State + ". ");
				Say("The low is " + fullInfo.WeatherInfo.Low + ".");
				Say("The high is " + fullInfo.WeatherInfo.High + ".");
				Say("The weather is: " + fullInfo.WeatherInfo.Summary + ".");
				Say(conditions);
				Say(hazards);
			}
		}

		protected string ProcessConditions(dynamic signal)
		{
			List<string> condition = new List<string>();

			foreach(dynamic c in signal.Conditions)
			{
				string additive = null;

				if (c.Additive != null)
				{
					additive = c.Additive;
				}

				string coverage = c.Coverage;
				string intensity = c.Intensity;
				string type = c.WeatherType;
				string qualifier = c.Qualifier;

				if (additive != null)
				{
					condition.Add(additive);
				}

				condition.Add(coverage);
				// condition.Add("of");

				if (intensity != "none")
				{
					condition.Add(intensity);
				}

				condition.Add(type);

				if (qualifier != "none")
				{
					// condition.Add("with");
					condition.Add(qualifier);
				}
			}

			string conditions = String.Empty;

			if (condition.Count > 0)
			{
				conditions = "Today there will be " + String.Join(" ", condition.ToArray()) + ".";
				conditions = conditions.Replace(",", ", ");
			}
			else
			{
				conditions = "There are no weather conditions.";
			}

			return conditions;
		}

		protected string ProcessHazards(dynamic signal)
		{
			List<string> hazard = new List<string>();
			string hazards;

			foreach (dynamic h in signal.Hazards)
			{
				string phenomena = h.Attribute("phenomena").Value;
				string significance = h.Attribute("significance").Value;
				hazard.Add("A");
				hazard.Add(phenomena);
				hazard.Add(significance);
				hazard.Add("in effect.");
			}

			if (hazard.Count > 0)
			{
				hazards = String.Join(" ", hazard.ToArray());
			}
			else
			{
				hazards = "There are no hazards in effect.";
			}

			return hazards;
		}

		// TODO: Duplicate code.
		protected void Say(string msg)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("TextToSpeech");
			dynamic signal = rsys.SemanticTypeSystem.Create("TextToSpeech");
			signal.Text = msg;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
