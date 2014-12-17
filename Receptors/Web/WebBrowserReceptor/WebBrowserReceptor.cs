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

// Only perform layout when control has completly finished resizing
// ResizeBegin += (s, e) => SuspendLayout();
// ResizeEnd += (s, e) => ResumeLayout(true);

namespace WebBrowserReceptor
{
    public class WebBrowserReceptor : WindowedBaseReceptor
    {
		// protected WebBrowser browser;
		protected IWinFormsWebBrowser browser;
		protected string internalUrl = "www.google.com";

		[MycroParserInitialize("tbAddress")]
		protected TextBox tbAddress;

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
			internalUrl = url;
			form.IfNull(() => ReinitializeUI()).Else(() => browser.Load(url));
			Subname = WindowName;
		}

		protected override void InitializeUI()
		{
			base.InitializeUI();
			browser = (ChromiumWebBrowser)mycroParser.ObjectCollection["browser"];
			browser.Load(internalUrl);
		}

		protected void OnAddressKey(object sender, KeyEventArgs key)
		{
			if (key.KeyCode == Keys.Enter)
			{
				key.Handled = key.SuppressKeyPress = true;		// Suppress beep
				internalUrl = tbAddress.Text;
				browser.Load(internalUrl);
			}
		}

		protected void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
		{
			((Control)browser).BeginInvoke(() => Subname = args.Title);
		}
	
		private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
		{
			((Control)browser).BeginInvoke(() => tbAddress.Text = args.Address);
		}
	}
}
