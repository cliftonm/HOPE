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
	public class ApodScraper : BaseReceptor
	{
		public override string Name { get { return "APOD"; } }
		protected int totalErrors = 0;

		public ApodScraper(IReceptorSystem rsys) : base(rsys)
		{
			// AddEmitProtocol("ImageFilename");
			AddEmitProtocol("WebImage");
			AddEmitProtocol("Url");
			AddEmitProtocol("ExceptionMessage");
			AddReceiveProtocol("Date", (Action<dynamic>)(signal => GetImageForDate(new DateTime(signal.Year, signal.Month, signal.Day))));
			AddReceiveProtocol("WebPageHtml", (Action<dynamic>)(signal => ProcessPage(signal.Url.Value, signal.Html.Value)));

			// (new string[] { "RequireTable", "ScrapeWebpage", "ImageFilename", "DatabaseRecord", "DebugMessage", "APOD", "HaveImageMetadata" }).ForEach(p => AddEmitProtocol(p));
		}

		public override void Initialize()
		{
		}

		public override void EndSystemInit()
		{
			GetImageForDate(DateTime.Now);
		}

		protected void GetImageForDate(DateTime date)
		{
			// Get today's image.
			string url = "http://apod.nasa.gov/apod/ap" + date.ToString("yyMMdd") + ".html";

			// Use this URL as an example.
			// string url = "http://apod.nasa.gov/apod/ap141207.html";

			EmitUrl(url);
		}
/*
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
*/
		protected async void ProcessPage(string url, string html)
		{
			HtmlDocument doc = new HtmlDocument();

			if (html == null)
			{
				EmitException("Page load error: " + url);
			}
			else
			{
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
				}

				try
				{
					imageURL = doc.DocumentNode.SelectNodes("//img")[0].Attributes["src"].Value;
				}
				catch(Exception ex)
				{
					// TODO: Change to log
					EmitException(ex);
				}

				if (String.IsNullOrEmpty(imageURL))
				{
					// There is no image.  Perhaps a video (see http://apod.nasa.gov/apod/ap140526.html as an example.)
					// Anyways, we cannot continue.
					// LogImage(url, null, keywords, null, null, errors);
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
					// TODO: Change to log
					EmitException(ex);
					// errors.Add("Problem with title" + "\r\n" + ex.Message);
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
					// TODO: Change to log
					EmitException(ex);
					// errors.Add("Problem with explanation" + "\r\n" + ex.Message);
				}

				if (!String.IsNullOrEmpty(imageURL))
				{
					try
					{
						Image img = await Task.Run(() =>
							{
								// DO NOT CLOSE THE STREAM UNTIL THE IMAGE HAS BEEN SAVED.
								// http://stackoverflow.com/questions/1053052/a-generic-error-occurred-in-gdi-jpeg-image-to-memorystream
								// http://stackoverflow.com/questions/10077219/download-image-from-url-in-c-sharp
								WebClient webClient = new WebClient();
								byte[] data = webClient.DownloadData("http://apod.nasa.gov/apod/" + imageURL);
								MemoryStream stream = new MemoryStream(data);
								Image imgdata = Image.FromStream(stream);
								// Don't dispose of the stream or the image, otherwise the viewer (or any other process) will not have valid image data.
								/*
								// Save the big image.
								string imgfn = ImagesFolder + "\\" + imageURL.RightOfRightmostOf('/');
								img.Save(imgfn);
								img.Dispose();
								stream.Dispose();
								 */
								webClient.Dispose();

								return imgdata;
							});


						// Put it out there into the wild.
						// EmitImageFile(fn);
						EmitImage(img, title, explanation, url);

						// LogImage(url, fn, keywords, title, explanation, errors);

					}
					catch (Exception ex)
					{
						EmitException(ex);
						// errors.Add("Problem loading image from site." + "\r\n" + ex.Message);
					}
				}
			}
		}

		protected void EmitImage(Image image, string title, string explanation, string url)
		{
			CreateCarrier("WebImage", signal =>
				{
					signal.Image.Value = image as Image;
					signal.Image.Title.Text.Value = title;
					// TODO: The ! is a real kludge to indicate clearing the text display.
					signal.Image.Description.Text.Value = "!" + explanation;
					signal.Url.Value = url;
				});
		}

		protected void EmitUrl(string url)
		{
			CreateCarrier("Url", signal => signal.Value = url);
		}
/*
		protected void InitializeImagesFolder()
		{
			if (!Directory.Exists(ImagesFolder))
			{
				Directory.CreateDirectory(ImagesFolder);
			}
		}

		protected void EmitUrl(string url)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ScrapeWebpage");
			dynamic signal = rsys.SemanticTypeSystem.Create("ScrapeWebpage");
			signal.URL.Value = url;
			rsys.CreateCarrier(this, protocol, signal);
		}

		protected void EmitImageFile(string fn)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ImageFilename");
			dynamic fsignal = rsys.SemanticTypeSystem.Create("ImageFilename");
			fsignal.Filename = fn;
			rsys.CreateCarrierIfReceiver(this, protocol, fsignal);
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

			// TODO: Fix this, as we're going to handle persistence completely differently than the initial implementation.
			// ICarrier recordCarrier = rsys.CreateInternalCarrier(protocol, record);
			// return recordCarrier;

			return null;
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
			dbsignal.ResponseProtocol = "APOD";			// will respond actually with "APODRecordset"
			// Wildcard prefix to ignore path information.
			// TODO: Use parameters
			dbsignal.Where = "ImageFilename LIKE '%" + imageFile + "'";
			rsys.CreateCarrier(this, dbprotocol, dbsignal);
		}

		protected void ProcessAPODRecordset(dynamic signal)
		{
			// Allows for custom protocols.
			ISemanticTypeStruct respProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("HaveImageMetadata");
			dynamic respSignal = rsys.SemanticTypeSystem.Create("HaveImageMetadata");
			List<dynamic> records = signal.Records;

			// TODO: What if more than one image filename matches?
			if (records.Count > 0)
			{
				dynamic firstMatch = records[0];
				respSignal.ImageFilename.Filename = firstMatch.ImageFilename.Filename;
				ICarrier responseCarrier = CreateAPODRecordCarrier();
				responseCarrier.Signal.URL.Value = firstMatch.URL.Value;
				responseCarrier.Signal.Keywords = firstMatch.Keywords;
				responseCarrier.Signal.Title = firstMatch.Title;
				responseCarrier.Signal.Explanation = firstMatch.Explanation;
				respSignal.Metadata = responseCarrier;

				// Off it goes!
				rsys.CreateCarrier(this, respProtocol, respSignal);
			}
			// else, APOD knows nothing about this image file, so there's no response.
		}

		/// <summary>
		/// Search the APOD database for matches.
		/// </summary>
		protected void SearchFor(dynamic signal)
		{
			string searchFor = signal.SearchString;

			ISemanticTypeStruct dbprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DatabaseRecord");
			dynamic dbsignal = rsys.SemanticTypeSystem.Create("DatabaseRecord");
			dbsignal.TableName = "APOD";
			dbsignal.Action = "select";
			dbsignal.ResponseProtocol = "APODSearchResults";			// will respond actuall with "APODRecordset"
			// TODO: Use parameters
			dbsignal.Where = "Keywords LIKE '%" + searchFor + "%' or Title LIKE '%" + searchFor + "%' or Explanation LIKE '%" + searchFor + "%'";
			rsys.CreateCarrier(this, dbprotocol, dbsignal);
		}

		/// <summary>
		/// Create carriers for the images that meet the returned search criteria.
		/// </summary>
		protected void ProcessSearchResults(dynamic signal)
		{
			List<dynamic> records = signal.Records;

			foreach (dynamic d in records)
			{
				// Issue only if the image filename exists.
				if (d.ImageFilename.Filename != null)
				{
					ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ImageFilename");
					dynamic outsignal = rsys.SemanticTypeSystem.Create("ImageFilename");
					outsignal.Filename = d.ImageFilename.Filename;
					rsys.CreateCarrier(this, outprotocol, outsignal);
				}
			}
		}
 */
	}
}
