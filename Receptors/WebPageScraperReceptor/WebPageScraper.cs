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
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Clifton.Tools.Strings.Extensions;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace WebPageScraperReceptor
{
	public class FullInfo
	{
		public dynamic WeatherInfo { get; set; }
		public dynamic LocationInfo { get; set; }

		public bool Completed { get { return WeatherInfo != null && LocationInfo != null; } }
	}

	public class WebPageScraper : BaseReceptor
	{
		public override string Name { get { return "Webpage Scraper"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		
		public WebPageScraper(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("Url");
			AddEmitProtocol("WebPageHtml");
		}

		public override async void ProcessCarrier(ICarrier carrier)
		{
			string url = carrier.Signal.Value;

			try
			{
				string html = await Task.Run(() =>
					{
						// http://stackoverflow.com/questions/599275/how-can-i-download-html-source-in-c-sharp
						using (WebClient client = new WebClient())
						{
							// For future reference, if there are parameters, like:
							//   www.somesite.it/?p=1500
							// use:
							//   client.QueryString.Add("p", "1500"); //add parameters
							return client.DownloadString(url);
						}
					});

				Emit(url, html);
			}
			catch (Exception ex)
			{
				EmitException(ex);
			}
		}

		protected void Emit(string url, string html)
		{
			CreateCarrier("WebPageHtml", signal =>
				{
					signal.Url.Value = url;
					signal.Html.Value = html;
				});
		}
	}
}
