using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using WeatherService.gov.weather.graphical;

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace WeatherServiceReceptor
{
	public class WeatherInfo
	{
		public string Low { get; set; }
		public string High { get; set; }
		public string Summary { get; set; }
		public string Conditions { get; set; }
		public string Hazards { get; set; }

		public WeatherInfo()
		{
		}
	}

	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Weather Service"; } }

		public override string ConfigurationUI { get { return "ZipcodeConfig.xml"; } }

		[UserConfigurableProperty("Zipcode:")]
		public string Zipcode { get; set; }

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("Zip5");		// We can receive a zipcode from some external source.
			AddEmitProtocol("Zip5");		// We also emit a zipcode because we may be configured to process a zipcode on startup, in which case we need to initiate getting the city, state.
			AddEmitProtocol("WeatherInfo");
			AddEmitProtocol("ExceptionMessage");
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (!String.IsNullOrEmpty(Zipcode))
			{
				// Send to zipcode service
				CreateCarrier("Zip5", signal => signal.Value = Zipcode);
				// We need to process the zipcode as well since we don't receive carriers that we also emit.
				ProcessZipcode(Zipcode);
			}
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			string zipcode = carrier.Signal.Value;
			ProcessZipcode(zipcode);
		}

		public async void ProcessZipcode(string zipcode)
		{
			XDocument xdoc;

			try
			{
				xdoc = await Task.Run(() =>
				{
					ndfdXML weather = new ndfdXML();
					string latLonXml = weather.LatLonListZipCode(zipcode);
					XDocument xdoc2 = XDocument.Parse(latLonXml);
					string[] latLon = xdoc2.Element("dwml").Element("latLonList").Value.Split(',');

					if (String.IsNullOrEmpty(latLon[0]))
					{
						return null;
					}

					decimal lat = Convert.ToDecimal(latLon[0]);
					decimal lon = Convert.ToDecimal(latLon[1]);
					string weatherXml = weather.NDFDgenByDay(lat, lon, DateTime.Now, "1", "e", "24 hourly");
					xdoc2 = XDocument.Parse(weatherXml);

					return xdoc2;
				});
			}
			catch (Exception ex)
			{
				EmitException(ex);
				
				return;
			}

			if (xdoc == null)
			{
				EmitException(new Exception("Bad zipcode: "+zipcode));
			}

			ISemanticTypeStruct outProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("WeatherInfo");
			dynamic outSignal = rsys.SemanticTypeSystem.Create("WeatherInfo");

			try
			{
				outSignal.Zip5.Value = zipcode;
				outSignal.Low = xdoc.Element("dwml").Element("data").Element("parameters").Elements("temperature").Where(el => el.Attribute("type").Value == "minimum").Single().Element("value").Value.Trim();
				outSignal.High = xdoc.Element("dwml").Element("data").Element("parameters").Elements("temperature").Where(el => el.Attribute("type").Value == "maximum").Single().Element("value").Value.Trim();
				outSignal.Summary = xdoc.Element("dwml").Element("data").Element("parameters").Element("weather").Element("weather-conditions").Attribute("weather-summary").Value;
				outSignal.Conditions = new List<dynamic>();
				outSignal.Hazards = new List<dynamic>();

				// Process Conditions:
				var weatherElements = xdoc.Element("dwml").Element("data").Element("parameters").Element("weather").Element("weather-conditions").Elements("value");

				weatherElements.ForEach(v =>
					{
						dynamic condition = rsys.SemanticTypeSystem.Create("WeatherCondition");

						if (v.Attribute("additive") != null)
						{
							condition.Additive = v.Attribute("additive").Value;
						}

						condition.Coverage = v.Attribute("coverage").Value;
						condition.Intensity = v.Attribute("intensity").Value;
						condition.WeatherType = v.Attribute("weather-type").Value;
						condition.Qualifier = v.Attribute("qualifier").Value;

						outSignal.Conditions.Add(condition);
					});

				// Process hazards
				var hazardElements = xdoc.Element("dwml").Element("data").Element("parameters").Element("hazards").Element("hazard-conditions").Elements("hazard");

				hazardElements.ForEach(h =>
					{
						dynamic hazard = rsys.SemanticTypeSystem.Create("WeatherHazard");
						hazard.Phenomena = h.Attribute("phenomena").Value;
						hazard.Significance = h.Attribute("significance").Value;

						outSignal.Hazards.Add(hazard);
					});
			}
			catch (Exception ex)
			{
				EmitException(ex);

				return;
			}

			rsys.CreateCarrier(this, outProtocol, outSignal);
		}
	}
}
