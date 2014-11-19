using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

// TODO: This is hard-coded for the Query protocol at the moment.
// TODO: The UI should really be dynamic based on the native types defined in the protocol.

namespace SignalCreatorReceptor
{
	public class SignalCreator : BaseReceptor, ReceptorUiHelpers.ISupportModelessConfiguration
	{
		public override string Name { get { return "Signal Creator"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "SignalCreatorConfig.xml"; } }

		[UserConfigurableProperty("Protocol Name:")]
		public string ProtocolName { get; set; }

		// Used for persisting protocol data as a serialzed XML string.
		[UserConfigurableProperty("Protocol Data:")]
		public string ProtocolData { get; set; }

		public bool CloseAfterCreate { get; set; }
		public bool Modeless { get { return true; } }

		protected Form form;
		protected ComboBox cbProtocols;
		protected List<Control> protocolControls;			// All controls added at runtime for the specific protocol.
		protected List<Control> inputProtocolControls;		// Just the input controls (not the labels).
		protected XDocument xdoc;

		//[UserConfigurableProperty("Data")]
		//public string Data { get; set; }

		public SignalCreator(IReceptorSystem rsys)
			: base(rsys)
		{
			AddReceiveProtocol("Resend", (Action<dynamic>)(s => Resend()));
			protocolControls = new List<Control>();
			inputProtocolControls = new List<Control>();
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();
			UpdateXDocument();

			if (!String.IsNullOrEmpty(ProtocolName))
			{
				AddEmitProtocol(ProtocolName);

				if (Enabled)
				{
					// TODO: User should define whether the signal is emitted right away on startup.
					CreateCarrier();
				}
			}
		}

		public override void PrepopulateConfig(MycroParser mp)
		{
			base.PrepopulateConfig(mp);
			form = (Form)mp.ObjectCollection["form"];
			cbProtocols = (ComboBox)mp.ObjectCollection["cbProtocols"];
			List<string> semanticTypes = ReceptorUiHelpers.Helper.PopulateProtocolComboBox(cbProtocols, rsys, ProtocolName);
			cbProtocols.SelectedValueChanged += ProtocolSelectedValueChanged;

			if (!String.IsNullOrEmpty(ProtocolName))
			{
				RenderUI();
			}
			else
			{
				ProtocolName = semanticTypes[0];
				RenderUI();
			}
		}

		protected void ProtocolSelectedValueChanged(object sender, EventArgs e)
		{
			RemoveOldControls();
			ProtocolName = cbProtocols.SelectedValue.ToString();
			RenderUI();
		}

		protected void RenderUI()
		{
			List<IFullyQualifiedNativeType> nativeTypes = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypes(ProtocolName).OrderBy(fqn => fqn.Ordinality).ToList();
			int y = 130;

			foreach (IFullyQualifiedNativeType nt in nativeTypes)
			{
				try
				{
					Type implType = nt.NativeType.GetImplementingType(rsys.SemanticTypeSystem);
					AddInputFor(implType, nt, y);
					y += 25;
				}
				catch(Exception ex)
				{
					// If the implementing type is not known by the native type system (for example, List<dynamic> used in the WeatherInfo protocol, we ignore it.
					// TODO: We need a way to support implementing lists and displaying them in the viewer as a sub-collection.
					// WeatherInfo protocol is a good example.
				}
			}

			form.Height = y + 50;
		}

		protected void RemoveOldControls()
		{
			protocolControls.ForEach(ctrl => form.Controls.Remove(ctrl));
			protocolControls.Clear();
			inputProtocolControls.Clear();
		}

		/// <summary>
		/// Adds a label and control for the native type.  All types are implemented as textboxes at the moment.
		/// </summary>
		protected void AddInputFor(Type t, IFullyQualifiedNativeType nt, int y)
		{
			string labelText = (String.IsNullOrEmpty(nt.Alias) ? nt.Name : nt.Alias) + ":";
			Label lbl = new Label();
			lbl.Text = labelText;
			lbl.Location = new Point(20, y + 3);
			lbl.Size = new Size(100, 15);
			form.Controls.Add(lbl);

			TextBox tb = new TextBox();
			tb.Location = new Point(125, y);
			tb.Size = new Size(200, 20);
			tb.Tag = nt;
			PopulateTextboxWithSignal(tb, nt);
			form.Controls.Add(tb);

			protocolControls.Add(lbl);
			protocolControls.Add(tb);
			inputProtocolControls.Add(tb);
		}

		protected void PopulateTextboxWithSignal(TextBox tb, IFullyQualifiedNativeType nt)
		{
			if (!String.IsNullOrEmpty(ProtocolData))
			{
				var item = xdoc.Descendants().Elements().Where(el => el.Attribute("Name").Value == nt.FullyQualifiedNameSansRoot).FirstOrDefault();

				if (item != null)
				{
					tb.Text = item.Attribute("Data").Value;
				}
			}
		}

		public override bool UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			bool ret = rsys.SemanticTypeSystem.VerifyProtocolExists(ProtocolName);
			ConfigurationError = String.Empty;		 // no error.

			if (ret)
			{
				RemoveEmitProtocols(); 
				AddEmitProtocol(ProtocolName);
				SerializeSignalData();
				UpdateXDocument();

				if (Enabled)
				{
					CreateCarrier();
				}
			}
			else
			{
				ConfigurationError = "The semantic type '" + ProtocolName + "' is not defined.";
			}

			// Return true if we close the dialog after creating the carrier.
			// Otherwise, the user would like to keep the dialog open.
			// If ConfigurationError is not null, then the caller should display a popup error dialog.
			// If ConfigurationError is null or an empty string, then the caller should simply keep the dialog open.

			// This is used to keep the dialog open so it's easier for
			// the user to create more than one signal quickly.
			return CloseAfterCreate;
		}

		protected void Resend()
		{
			if (Enabled)
			{
				CreateCarrier();
			}
		}

		protected void UpdateXDocument()
		{
			// Update master XDocument so we parse it only once.
			if (!String.IsNullOrEmpty(ProtocolData))
			{
				xdoc = XDocument.Parse(ProtocolData);
			}
		}

		protected void SerializeSignalData()
		{
			XElement doc = new XElement("Signal");

			foreach (Control ctrl in inputProtocolControls)
			{
				string data = ctrl.Text;
				IFullyQualifiedNativeType nt = (IFullyQualifiedNativeType)ctrl.Tag;
				doc.Add(new XElement("NativeType", new XAttribute("Name", nt.FullyQualifiedNameSansRoot), new XAttribute("Data", data)));				
			}

			ProtocolData = doc.ToString();
		}

		protected void CreateCarrier()
		{
			if (xdoc != null)
			{
				ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);
				dynamic outsignal = rsys.SemanticTypeSystem.Create(ProtocolName);
				PopulateSignalNativeTypeValues(outsignal);
				rsys.CreateCarrier(this, outprotocol, outsignal);
			}
		}

		protected void PopulateSignalNativeTypeValues(object signal)
		{
			foreach(XElement elem in xdoc.Descendants().Elements())
			{
				string fqn = elem.Attribute("Name").Value;
				string data = elem.Attribute("Data").Value;
				rsys.SemanticTypeSystem.SetFullyQualifiedNativeTypeValue(signal, fqn, data);
			}
		}
	}
}
