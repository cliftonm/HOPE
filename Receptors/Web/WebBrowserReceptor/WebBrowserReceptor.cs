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
			AddReceiveProtocol("Text", (Action<dynamic>)(signal => EmbedSearchString(signal.Value)));
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

		/// <summary>
		/// Embed the search string into the user URL to implement a paramaterized search capability
		/// </summary>
		protected void EmbedSearchString(string search)
		{
			string navTo = UserUrl.Replace("[search]", search);
			ShowPage(navTo);
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
