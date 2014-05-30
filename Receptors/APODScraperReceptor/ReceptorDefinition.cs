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
		protected int totalErrors = 0;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;

			protocolActionMap = new Dictionary<string, Action<dynamic>>();
			protocolActionMap["TimerEvent"] = new Action<dynamic>((s) => TimerEvent(s));
			protocolActionMap["APODWebpage"] = new Action<dynamic>((s) => ProcessPage(s));
			protocolActionMap["GetImageMetadata"] = new Action<dynamic>((s) => GetImageMetadata(s));
			protocolActionMap["APODRecordset"] = new Action<dynamic>((s) => ProcessAPODRecordset(s));
		}

		public string[] GetReceiveProtocols()
		{
			return protocolActionMap.Keys.ToArray();
		}

		public void Initialize()
		{
			RequireAPODTable();
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
				EmitUrl(url);
			}
		}

		protected async void ProcessPage(dynamic signal)
		{
			List<string> errors = new List<string>();
			HtmlDocument doc = new HtmlDocument();
			string url = signal.URL;
			string html = signal.HTML;

			if (html == null)
			{
				errors.Add("Page load error: " + signal.Errors);
				LogImage(url, null, null, null, null, errors);
				return;
			}

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
				// Not a critical error.
				// errors.Add("No keywords");
			}

			try
			{
				imageURL = doc.DocumentNode.SelectNodes("//img")[0].Attributes["src"].Value;
			}
			catch
			{
				errors.Add("No image");
			}

			if (String.IsNullOrEmpty(imageURL))
			{
				// There is no image.  Perhaps a video (see http://apod.nasa.gov/apod/ap140526.html as an example.)
				// Anyways, we cannot continue.
				LogImage(url, null, keywords, null, null, errors);
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
				errors.Add("Problem with title");
			}

			try
			{
				// Get the explanation.  This is everything until a <p>
				idx = html.IndexOf("Explanation:");
				idx += html.Substring(idx).IndexOf(">");		// find the closing tag of whatever is decorating "Explanation:"
				++idx;
				int endIdx = html.Substring(idx).IndexOf("<p>");
				explanation = html.Substring(idx, endIdx);
				explanation = explanation.Replace('\n', ' ').Trim();
			}
			catch (Exception ex)
			{
				errors.Add("Problem with explanation");
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
					EmitImageFile(fn);
					LogImage(url, fn, keywords, title, explanation, errors);

				}
				catch(Exception ex)
				{
					errors.Add("Problem loading image from site.");
				}
			}
		}

		protected void RequireAPODTable()
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("RequireTable");
			dynamic signal = rsys.SemanticTypeSystem.Create("RequireTable");
			signal.TableName = "APOD";
			signal.Schema = "APOD";
			rsys.CreateCarrier(this, protocol, signal);
		}

		protected void EmitUrl(string url)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ScrapeWebpage");
			dynamic signal = rsys.SemanticTypeSystem.Create("ScrapeWebpage");
			signal.URL = url;
			signal.ResponseProtocol = "APODWebpage";
			rsys.CreateCarrier(this, protocol, signal);
		}

		protected void EmitImageFile(string fn)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ImageFilename");
			dynamic fsignal = rsys.SemanticTypeSystem.Create("ImageFilename");
			fsignal.Filename = fn;
			rsys.CreateCarrier(this, protocol, fsignal);
		}

		protected void LogImage(string url, string fn, string keywords, string title, string explanation, List<string> errors)
		{
			ICarrier recordCarrier = CreateAPODRecordCarrier();
			recordCarrier.Signal.URL.Value = url;
			recordCarrier.Signal.ImageFilename.Filename = fn;
			recordCarrier.Signal.Keywords.Text.Value = keywords;
			recordCarrier.Signal.Explanation.Text.Value = explanation;
			recordCarrier.Signal.Title.Text.Value = title;
			recordCarrier.Signal.Errors = String.Join(", ", errors.ToArray());

			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DatabaseRecord");
			dynamic signal = rsys.SemanticTypeSystem.Create("DatabaseRecord");
			signal.TableName = "APOD";
			signal.Action = "insert";
			signal.Row = recordCarrier;
			rsys.CreateCarrier(this, protocol, signal);

			// Use the debug message receptor to display error counts.
			if (errors.Count > 0)
			{
				++totalErrors;
				ISemanticTypeStruct dbgMsgProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DebugMessage");
				dynamic dbgMsgSignal = rsys.SemanticTypeSystem.Create("DebugMessage");
				dbgMsgSignal.Message = totalErrors.ToString() + ": " + recordCarrier.Signal.Errors;
				rsys.CreateCarrier(this, dbgMsgProtocol, dbgMsgSignal);
			}
		}

		protected ICarrier CreateAPODRecordCarrier()
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("APOD");
			dynamic record = rsys.SemanticTypeSystem.Create("APOD");
			ICarrier recordCarrier = rsys.CreateInternalCarrier(protocol, record);

			return recordCarrier;
		}

		protected void GetImageMetadata(dynamic signal)
		{
			string imageFile = Path.GetFileName(signal.ImageFilename.Filename);

			// Sort of kludgy, we're stripping off the "-thumbnail" portion of the filename if the user
			// happens to have dropped a thumbnail file.  Rather dependent upon the fact that the thumbnail
			// writer writes image files with string added to the filename!
			imageFile = imageFile.Surrounding("-thumbnail");

			ISemanticTypeStruct dbprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DatabaseRecord");
			dynamic dbsignal = rsys.SemanticTypeSystem.Create("DatabaseRecord");
			dbsignal.TableName = "APOD";
			dbsignal.Action = "select";
			dbsignal.ResponseProtocol = "APOD";
			// Wildcard prefix to ignore path information.
			dbsignal.Where = "ImageFilename LIKE '%" + imageFile + "'";
			rsys.CreateCarrier(this, dbprotocol, dbsignal);
		}

		protected void ProcessAPODRecordset(dynamic signal)
		{
			// Allows for custom protocols.
			ISemanticTypeStruct respProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("HaveImageMetadata");
			dynamic respSignal = rsys.SemanticTypeSystem.Create("HaveImageMetadata");
			List<dynamic> records = signal.Recordset;

			// TODO: What if more than one image filename matches?
			if (records.Count > 0)
			{
				dynamic firstMatch = records[0];
				respSignal.ImageFilename.Filename = firstMatch.ImageFilename.Filename;
				ICarrier responseCarrier = CreateAPODRecordCarrier();
				responseCarrier.Signal.URL = firstMatch.URL;
				responseCarrier.Signal.Keywords = firstMatch.Keywords;
				responseCarrier.Signal.Title = firstMatch.Title;
				responseCarrier.Signal.Explanation = firstMatch.Explanation;
				respSignal.Metadata = responseCarrier;

				// Off it goes!
				rsys.CreateCarrier(this, respProtocol, respSignal);
			}
			// else, APOD knows nothing about this image file, so there's no response.
		}
	}
}
