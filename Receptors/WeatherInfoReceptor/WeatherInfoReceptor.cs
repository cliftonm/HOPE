/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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

	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Weather Info"; } }
		protected Dictionary<string, FullInfo> zipcodeInfoMap;

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			zipcodeInfoMap = new Dictionary<string, FullInfo>();
			AddReceiveProtocol("WeatherInfo");
			AddReceiveProtocol("USLocation");
			AddEmitProtocol("Announce");
			AddEmitProtocol("Text");
			AddEmitProtocol("ExceptionMessage");
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			try
			{
				FullInfo fullInfo;

				// Duck-typing! -- Both USLocation and WeatherInfo have a Zip5 member.
				if (!zipcodeInfoMap.TryGetValue(carrier.Signal.Zip5.Value, out fullInfo))
				{
					fullInfo = new FullInfo();
					zipcodeInfoMap[carrier.Signal.Zip5.Value] = fullInfo;
				}

				if (carrier.Protocol.DeclTypeName == "WeatherInfo")
				{
					fullInfo.WeatherInfo = carrier.Signal;
				}

				if (carrier.Protocol.DeclTypeName == "USLocation")
				{
					fullInfo.LocationInfo = carrier.Signal;
				}

				if (fullInfo.Completed)
				{
					string conditions = ProcessConditions(fullInfo.WeatherInfo);
					string hazards = ProcessHazards(fullInfo.WeatherInfo);
					Say("Here is the weather for " + fullInfo.LocationInfo.City.Value + " " + fullInfo.LocationInfo.USState.Value + ". ");
					Say("The low is " + fullInfo.WeatherInfo.Low + ".");
					Say("The high is " + fullInfo.WeatherInfo.High + ".");
					Say("The weather is: " + fullInfo.WeatherInfo.Summary + ".");
					Say(conditions);
					Say(hazards);
				}
			}
			catch (Exception ex)
			{
				EmitException(ex);
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
				string phenomena = h.Phenomena;
				string significance = h.Significance;
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
			CreateCarrierIfReceiver("Text", signal => signal.Value = msg);
			CreateCarrierIfReceiver("Announce", signal => signal.Text.Value = msg);
		}
	}
}
