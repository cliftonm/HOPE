using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace WebBrowserReceptor
{
    public class WebBrowserReceptor : WindowedBaseReceptor
    {
		protected WebBrowser browser;

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
			
			if (!url.BeginsWith("http"))
			{
				url = "http://" + url;
			}

			browser.Navigate(new Uri(url));
		}

		protected override void InitializeUI()
		{
			base.InitializeUI();
			browser = (WebBrowser)mycroParser.ObjectCollection["browser"];
		}
    }
}
