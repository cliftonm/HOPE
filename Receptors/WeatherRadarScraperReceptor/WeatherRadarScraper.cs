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

namespace WeatherRadarScraperReceptor
{
    public class WeatherRadarScraper : BaseReceptor
	{
		public override string Name { get { return "Radar"; } }
		protected int totalErrors = 0;

		public WeatherRadarScraper(IReceptorSystem rsys) : base(rsys)
		{
			// AddEmitProtocol("ImageFilename");
			AddEmitProtocol("Image");
			AddEmitProtocol("Url");
			AddEmitProtocol("ExceptionMessage");
			AddReceiveProtocol("WebPageHtml", (Action<dynamic>)(signal => ProcessPage(signal.Url.Value, signal.Html.Value)));
			AddReceiveProtocol("Resend", (Action<dynamic>)(signal => GetRadarImage()));
		}

		public override void Initialize()
		{
		}


		public override void EndSystemInit()
		{
			GetRadarImage();
		}

		protected void GetRadarImage()
		{
			string url = "http://radar.weather.gov/radar.php?rid=enx&product=N0R";
			EmitUrl(url);
		}

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
				int n = 0;
				Image finalImage = null;
				Graphics gr = null;
				WebClient webClient = new WebClient();

				while (true)
				{
					var nodes = doc.DocumentNode.SelectNodes(String.Format("//div[@id='image{0}']/img", n++));

					if ( (nodes != null) && (nodes.Count == 1) )
					{
						string imageUrl = nodes.Select(node => node.Attributes["src"].Value).Single();

						// This is a computed image.
						if (imageUrl == "#")
						{
							string name = nodes.Select(node => node.Attributes["name"].Value).Single();
							string rid = url.Between("rid=", "&").ToUpper();
							string product = url.Between("product=", "&").ToUpper();

							switch (name)
							{
								case "conditionalimage":
									// http://radar.weather.gov/RadarImg/N0R/ENX_N0R_0.gif
									imageUrl = String.Format("RadarImg/{0}/{1}_{0}_0.gif", product, rid);
									break;

								case "conditionallegend":
									// http://radar.weather.gov/Legend/N0R/ENX_N0R_Legend_0.gif
									imageUrl = String.Format("Legend/{0}/{1}_{0}_Legend_0.gif", product, rid);
									break;
							}
						}

						Image img = await Task.Run(() =>
						{
							byte[] data = webClient.DownloadData("http://radar.weather.gov/" + imageUrl);
							MemoryStream stream = new MemoryStream(data);
							Image imgdata = Image.FromStream(stream);

							return imgdata;
						});

						if (finalImage == null)
						{
							finalImage = img;
							gr = Graphics.FromImage(finalImage);
						}
						else
						{
							gr.DrawImage(img, new Point(0, 0));
							img.Dispose();
						}

					}
					else
					{
						break;
					}
				}

				webClient.Dispose();
				EmitImage(finalImage);
			}
		}

		protected void EmitImage(Image image)
		{
			CreateCarrier("Image", signal => signal.Value = image);
		}

		protected void EmitUrl(string url)
		{
			CreateCarrier("Url", signal => signal.Value = url);
		}
	}
}
