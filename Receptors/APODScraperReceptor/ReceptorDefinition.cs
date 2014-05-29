using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace APODScraperReceptor
{
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "APOD"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;
		protected Dictionary<string, Action<dynamic>> protocolActionMap;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;

			protocolActionMap = new Dictionary<string, Action<dynamic>>();
			protocolActionMap["TimerEvent"] = new Action<dynamic>((s) => TimerEvent(s));
			protocolActionMap["APODWebpage"] = new Action<dynamic>((s) => ProcessPage(s));
		}

		public string[] GetReceiveProtocols()
		{
			return protocolActionMap.Keys.ToArray();
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public void ProcessCarrier(ICarrier carrier)
		{
			protocolActionMap[carrier.Protocol.DeclTypeName](carrier.Signal);
		}

		protected void TimerEvent(dynamic signal)
		{
			if (signal.EventName == "ScrapeAPOD")
			{
				DateTime eventDate = signal.EventDateTime;
				// Create a URL in this format:
				// http://apod.nasa.gov/apod/ap140528.html
				string url = "http://apod.nasa.gov/apod/ap" + eventDate.ToString("yyMMdd") + ".html";
				Emit(url);
			}
		}

		protected async void ProcessPage(dynamic signal)
		{
			HtmlDocument doc = new HtmlDocument();
			string html = signal.HTML;
			doc.LoadHtml(html);

			string keywords = String.Empty;
			string imageURL = String.Empty;
			string title = String.Empty;
			string explanation = String.Empty;

			try
			{
				keywords = doc.DocumentNode.SelectNodes("/html/head/meta[@name='keywords']")[0].Attributes["content"].Value;
			}
			catch
			{
				// Older pages don't have keywords.
			}

			try
			{
				imageURL = doc.DocumentNode.SelectNodes("//img")[0].Attributes["src"].Value;
			}
			catch
			{
				// We must have an image URL.
			}

			if (String.IsNullOrEmpty(imageURL))
			{
				// There is no image.  Perhaps a video (see http://apod.nasa.gov/apod/ap140526.html as an example.)
				// Anyways, we cannot continue.
				return;		
			}

			int idx;

			try
			{
				// The title is immediately following the image, embedded in various tags but before "Explanation" and before "Credit:"
				// Basically, we want to find the first non-markup text.
				idx = html.ToLower().IndexOf("<img");					// must find the img tag first.
				idx += html.Substring(idx).IndexOf("</a>");		// find the end of the image.
				idx += 4;
				bool foundTitle = false;

				while (!foundTitle)
				{
					if (html[idx] == '<')
					{
						while (html[idx] != '>') ++idx;
						++idx;
					}

					while ((html[idx] == ' ') || (html[idx] == '\n'))
					{
						++idx;
					}

					if (html[idx] != '<')
					{
						foundTitle = true;
					}
				}

				int endIdx = html.Substring(idx).IndexOf('<');
				title = html.Substring(idx, endIdx).Trim();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debugger.Break();
				// Error out here if we totally fail.
			}

			try
			{
				// Get the explanation.  This is everything until a <p>
				idx = html.IndexOf("Explanation:");
				idx += html.Substring(idx).IndexOf(">");		// find the closing tag of whatever is decorating "Explanation:"
				int endIdx = html.Substring(idx).IndexOf("<p>");
				explanation = html.Substring(idx, endIdx);
				explanation = explanation.Replace('\n', ' ').Trim();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debugger.Break();
				// Error out here if we totally fail.
			}

			if (!String.IsNullOrEmpty(imageURL))
			{
				try
				{
					string fn = await Task.Run(() =>
						{
							// DO NOT CLOSE THE STREAM UNTIL THE IMAGE HAS BEEN SAVED.
							// http://stackoverflow.com/questions/1053052/a-generic-error-occurred-in-gdi-jpeg-image-to-memorystream
							// http://stackoverflow.com/questions/10077219/download-image-from-url-in-c-sharp
							WebClient webClient = new WebClient();
							byte[] data = webClient.DownloadData("http://apod.nasa.gov/apod/" + imageURL);
							MemoryStream stream = new MemoryStream(data);
							Image img = Image.FromStream(stream);

							// Save the big image.
							string imgfn = "Images\\" + imageURL.RightOfRightmostOf('/');
							img.Save(imgfn);
							img.Dispose();
							stream.Dispose();
							webClient.Dispose();

							return imgfn;
						});


					// Put it out there into the wild.
					ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ImageFilename");
					dynamic fsignal = rsys.SemanticTypeSystem.Create("ImageFilename");
					fsignal.Filename = fn;
					// TODO: The null here is really the "System" receptor.
					rsys.CreateCarrier(this, protocol, fsignal);

				}
				catch(Exception ex)
				{
					System.Diagnostics.Debugger.Break();
					// Error out here if we totally fail.
				}
			}
				


		}

		protected void Emit(string url)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ScrapeWebpage");
			dynamic signal = rsys.SemanticTypeSystem.Create("ScrapeWebpage");
			signal.URL = url;
			signal.ResponseProtocol = "APODWebpage";
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
