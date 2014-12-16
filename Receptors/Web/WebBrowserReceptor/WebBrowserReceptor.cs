using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CefSharp;
using CefSharp.WinForms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace WebBrowserReceptor
{
    public class WebBrowserReceptor : WindowedBaseReceptor
    {
		// protected WebBrowser browser;
		protected IWinFormsWebBrowser browser;

		public override string Name { get { return "Web Page Viewer"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "WebPageViewerConfig.xml"; } }

		/// <summary>
		/// The URL that the user entered in the config screen to navigate to when this receptor loads.
		/// </summary>
		[UserConfigurableProperty("User URL:")]
		public string UserUrl { get; set; }

		public WebBrowserReceptor(IReceptorSystem rsys)
			: base("webPageViewer.xml", false, rsys)
		{
			Cef.Initialize();
			AddReceiveProtocol("Url", (Action<dynamic>)(signal => ShowPage(signal.Value)));
		}

		public override bool UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();

			if (!String.IsNullOrEmpty(UserUrl))
			{
				ShowPage(UserUrl);
			}

			return true;
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (!String.IsNullOrEmpty(UserUrl))
			{
				ShowPage(UserUrl);
			}
		}

		protected void ShowPage(string url)
		{
			form.IfNull(() => ReinitializeUI());

			Subname = WindowName;
			
			//if (!url.BeginsWith("http"))
			//{
			//	url = "http://" + url;
			//}

			// browser.Navigate(new Uri(url));

			browser.Load(url);
		}

		protected override void PostFormCreate()
		{
 			base.PostFormCreate();

			string url = (String.IsNullOrEmpty(UserUrl) ? "www.google.com" : UserUrl);
			browser = new ChromiumWebBrowser(url);
			((Control)browser).Dock = DockStyle.Fill;
			form.Controls.Add((Control)browser);
			// browser = (WebBrowser)mycroParser.ObjectCollection["browser"];
		}
    }
}
