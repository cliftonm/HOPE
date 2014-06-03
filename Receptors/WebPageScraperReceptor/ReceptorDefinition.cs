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

	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Webpage Scraper"; } }
		public bool IsEdgeReceptor { get { return true; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "ScrapeWebpage" };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { "WebpageHtml" };
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public async void ProcessCarrier(ICarrier carrier)
		{
			string url = carrier.Signal.URL.Value;

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
				EmitError(url, ex.Message);
			}
		}

		protected void Emit(string url, string html)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("WebpageHtml");
			dynamic signal = rsys.SemanticTypeSystem.Create("WebpageHtml");
			signal.URL = url;
			signal.HTML = html;
			rsys.CreateCarrier(this, protocol, signal);
		}

		protected void EmitError(string url, string error)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("WebpageHtml");
			dynamic signal = rsys.SemanticTypeSystem.Create("WebpageHtml");
			signal.URL = url;
			signal.Errors = error;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
