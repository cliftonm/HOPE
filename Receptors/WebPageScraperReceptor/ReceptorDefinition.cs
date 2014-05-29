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

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public async void ProcessCarrier(ICarrier carrier)
		{
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
							return client.DownloadString(carrier.Signal.URL);
						}
					});

				Emit(html, carrier.Signal.ResponseProtocol);
			}
			catch (Exception ex)
			{
				EmitError(ex.Message, carrier.Signal.ResponseProtocol);
			}
		}

		protected void Emit(string html, string protocolName)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocolName);
			dynamic signal = rsys.SemanticTypeSystem.Create(protocolName);
			signal.HTML = html;
			rsys.CreateCarrier(this, protocol, signal);
		}

		protected void EmitError(string error, string protocolName)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocolName);
			dynamic signal = rsys.SemanticTypeSystem.Create(protocolName);
			signal.Errors = error;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
