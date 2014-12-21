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
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Zip Code Service"; } }
		public override bool IsEdgeReceptor { get { return true; } }

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

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("Zip5");
			AddEmitProtocol("USLocation");
			AddEmitProtocol("ExceptionMessage");
		}

		public override async void ProcessCarrier(ICarrier carrier)
		{
			try
			{
				// TODO: Do we expose this configuration capability to the user?
				// TODO: We don't really want this, but we need it to prevent the Weather Service receptor from triggering another "get the US Location for the zipcode"
				// when it emits the WeatherInfo ST.  Fascinating stuff and is why Semtrex will be necessary.
				if (carrier.ProtocolPath == "Zip5")
				{
					string zipcode = carrier.Signal.Value;	// We only use the zip5 portion.

					Tuple<string, string> location = await Task.Run(() =>
					{
						string city = String.Empty;
						string stateAbbr = String.Empty;

						USZip zip = new USZip();
						XmlNode node = zip.GetInfoByZIP(zipcode);
						XDocument zxdoc = XDocument.Parse(node.InnerXml);
						city = zxdoc.Element("Table").Element("CITY").Value;
						stateAbbr = zxdoc.Element("Table").Element("STATE").Value;
						return new Tuple<string, string>(city, stateAbbr);

					});

					Emit(zipcode, location.Item1, location.Item2);
				}
			}
			catch (Exception ex)
			{
				// Catch must happen on the main thread so that when we emit the exception, it is handled on the main thread.
				// TODO: Carriers should be able to be on their own threads and receceptors should indicate whether the 
				// action needs to be marshalled onto the main application thread.
				EmitException(ex);
			}
		}

		protected void Emit(string zipCode, string city, string stateAbbr)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("USLocation");
			dynamic signal = rsys.SemanticTypeSystem.Create("USLocation");
			signal.Zip5.Value = zipCode;
			signal.City.Value = city;
			signal.USState.Value = "";
			int idx = Array.IndexOf(abbreviations, stateAbbr);

			if (idx != -1)
			{
				// The state name is in the adjacent +1 index.
				signal.USState.Value = abbreviations[idx+1];
			}

			signal.USStateAbbreviation.Value = stateAbbr;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}



