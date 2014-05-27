using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using ZipCodeService.net.webservicex.www;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace ZipCodeReceptor
{
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Zip Code Service"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;

		public string[] abbreviations = new string[] {
			"AL","ALABAMA",
			"AK","ALASKA",
			"AS","AMERICAN SAMOA",
			"AZ","ARIZONA",
			"AR","ARKANSAS",
			"CA","CALIFORNIA",
			"CO","COLORADO",
			"CT","CONNECTICUT",
			"DE","DELAWARE",
			"DC","DISTRICT OF COLUMBIA",
			"FM","FEDERATED STATES OF MICRONESIA",
			"FL","FLORIDA",
			"GA","GEORGIA",
			"GU","GUAM GU",
			"HI","HAWAII",
			"ID","IDAHO",
			"IL","ILLINOIS",
			"IN","INDIANA",
			"IA","IOWA",
			"KS","KANSAS",
			"KY","KENTUCKY",
			"LA","LOUISIANA",
			"ME","MAINE",
			"MH","MARSHALL ISLANDS",
			"MD","MARYLAND",
			"MA","MASSACHUSETTS",
			"MI","MICHIGAN",
			"MN","MINNESOTA",
			"MS","MISSISSIPPI",
			"MO","MISSOURI",
			"MT","MONTANA",
			"NE","NEBRASKA",
			"NV","NEVADA",
			"NH","NEW HAMPSHIRE",
			"NJ","NEW JERSEY",
			"NM","NEW MEXICO",
			"NY","NEW YORK",
			"NC","NORTH CAROLINA",
			"ND","NORTH DAKOTA",
			"MP","NORTHERN MARIANA ISLANDS",
			"OH","OHIO",
			"OK","OKLAHOMA",
			"OR","OREGON",
			"PW","PALAU",
			"PA","PENNSYLVANIA",
			"PR","PUERTO RICO",
			"RI","RHODE ISLAND",
			"SC","SOUTH CAROLINA",
			"SD","SOUTH DAKOTA",
			"TN","TENNESSEE",
			"TX","TEXAS",
			"UT","UTAH",
			"VT","VERMONT",
			"VI","VIRGIN ISLANDS",
			"VA","VIRGINIA",
			"WA","WASHINGTON",
			"WV","WEST VIRGINIA",
			"WI","WISCONSIN",
			"WY","WYOMING",
		};

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "Zipcode" };
		}

		public void Terminate()
		{
		}

		public async void ProcessCarrier(ICarrier carrier)
		{
			Tuple<string, string> location = await Task.Run(() =>
				{
					string city = String.Empty;
					string stateAbbr = String.Empty;

					try
					{
						string zipcode = carrier.Signal.Value;
						USZip zip = new USZip();
						XmlNode node = zip.GetInfoByZIP(zipcode);
						XDocument zxdoc = XDocument.Parse(node.InnerXml);
						city = zxdoc.Element("Table").Element("CITY").Value;
						stateAbbr = zxdoc.Element("Table").Element("STATE").Value;
					}
					catch (Exception ex)
					{
						// TODO: Log exception.
						// Occasionally this web service will crash.
					}

					return new Tuple<string, string>(city, stateAbbr);
				});

			Emit(carrier.Signal.Value, location.Item1, location.Item2);
		}

		protected void Emit(string zipCode, string city, string stateAbbr)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("Location");
			dynamic signal = rsys.SemanticTypeSystem.Create("Location");
			signal.Zipcode = zipCode;
			signal.City = city;
			signal.State = "";
			int idx = Array.IndexOf(abbreviations, stateAbbr);

			if (idx != -1)
			{
				signal.State = abbreviations[idx + 1];
			}

			signal.StateAbbr = stateAbbr;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}



