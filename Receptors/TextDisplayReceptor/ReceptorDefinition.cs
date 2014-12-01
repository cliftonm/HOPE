using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace TextDisplayReceptor
{
	public class ReceptorDefinition : WindowedBaseReceptor
	{
		public override string Name { get { return "Text Display"; } }
		public override string ConfigurationUI { get { return "TextDisplayFilterConfig.xml"; } }

		[MycroParserInitialize("tbText")]
		protected TextBox tb;

		[UserConfigurableProperty("Protocol Filter:")]
		public string ProtocolFilter { get; set; }

		[UserConfigurableProperty("Show All:")]
		public bool ShowAll { get; set; }

		protected ComboBox cbProtocols;

		public ReceptorDefinition(IReceptorSystem rsys) : base("TextViewer.xml", true, rsys)
		{
			ShowAll = true;

			AddReceiveProtocol("Text", (Action<dynamic>)(signal =>
				{
					form.IfNull(() =>
						{
							InitializeUI();
							UpdateFormLocationAndSize();
						});

					string text = signal.Value;

					if (!String.IsNullOrEmpty(text))
					{
						tb.AppendText(text.StripHtml());
						tb.AppendText("\r\n");
					}
				}));
		}

		public override void PrepopulateConfig(MycroParser mp)
		{
			base.PrepopulateConfig(mp);
			// This combobox is in the config UI.
			cbProtocols = (ComboBox)mp.ObjectCollection["cbProtocols"];
			ReceptorUiHelpers.Helper.PopulateProtocolComboBox(cbProtocols, rsys, ProtocolFilter);
		}

		public override bool UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			ConfigureBasedOnSelectedProtocol();

			return true;
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			// Filter what protocol we display in the textbox.
			if ( (!String.IsNullOrEmpty(ProtocolFilter)) && (!ShowAll) )
			{
				if (carrier.ProtocolPath.Contains(ProtocolFilter))
				{
					base.ProcessCarrier(carrier);
				}
			}
			else
			{
				base.ProcessCarrier(carrier);
			}
		}

		protected void ConfigureBasedOnSelectedProtocol()
		{
			if (cbProtocols != null)
			{
				// Update the protocol name if the combobox exists, either in the main UI or the configuration UI.
				ProtocolFilter = cbProtocols.SelectedValue.ToString();
			}
		}

		protected void ClearTextBox(object sender, EventArgs args)
		{
			tb.Clear();
		}
	}
}
