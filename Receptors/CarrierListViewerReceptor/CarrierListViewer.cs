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

namespace CarrierListViewerReceptor
{
	public class CarrierListViewer : BaseReceptor
    {
		public override string Name { get { return "Carrier List Viewer"; } }
		public override string Subname { get { return ProtocolName; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "CarrierListViewerConfig.xml"; } }

		[UserConfigurableProperty("Protocol Name:")]
		public string ProtocolName { get; set; }

		[UserConfigurableProperty("WindowName")]
		public string WindowName { get; set; }

		[UserConfigurableProperty("X")]
		public int WindowX {get;set;}

		[UserConfigurableProperty("Y")]
		public int WindowY {get;set;}

		[UserConfigurableProperty("W")]
		public int WindowWidth { get; set; }

		[UserConfigurableProperty("H")]
		public int WindowHeight { get; set; }

		protected string oldProtocol;
		protected DataView dvSignals;
		protected DataGridView dgvSignals;
		protected Form form;

		public CarrierListViewer(IReceptorSystem rsys)
		  : base(rsys)
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			InitializeUI();
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();
			UpdateFormLocationAndSize();
			CreateViewerTable();
			ListenForProtocol();
			UpdateCaption();
		}

		/// <summary>
		/// When the user configuration fields have been updated, reset the protocol we are listening for.
		/// </summary>
		public override void UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			CreateViewerTable();
			ListenForProtocol();
			UpdateCaption();
		}

		public override void Terminate()
		{
			base.Terminate();
			form.Close();
		}

		protected void UpdateCaption()
		{
			if (!String.IsNullOrEmpty(WindowName))
			{
				form.Text = WindowName;
			}
			else
			{
				if (!String.IsNullOrEmpty(ProtocolName))
				{
					string updatedText = form.Text.LeftOf('-');
					updatedText = updatedText + " - " + ProtocolName;
					form.Text = updatedText;
				}
			}
		}

		/// <summary>
		/// Instantiate the UI.
		/// </summary>
		protected void InitializeUI()
		{
			// Setup the UI:
			MycroParser mp = new MycroParser();
			form = mp.Load<Form>("CarrierListViewer.xml", this);
			dgvSignals = (DataGridView)mp.ObjectCollection["dgvRecords"];
			form.Show();

			// Wire up the location changed event after the form has initialized,
			// so we don't generate this event during form creation.  That way,
			// the user's config will be preserved and used when the system
			// finishes initialization.
			form.LocationChanged += OnLocationChanged;
			form.SizeChanged += OnSizeChanged;
		}

		// TODO: This stuff on window location and size changing and setting needs to be moved
		// to a common lib that a receptor instance project can easily just wire in, as this
		// is going to be common behavior for receptors with UI's.  Gawd, sometimes I really 
		// wish C# supported multiple inheritence.
		protected void OnLocationChanged(object sender, EventArgs e)
		{
			WindowX = form.Location.X;
			WindowY = form.Location.Y;
		}

		protected void OnSizeChanged(object sender, EventArgs e)
		{
			WindowWidth = form.Size.Width;
			WindowHeight = form.Size.Height;
		}

		protected void UpdateFormLocationAndSize()
		{
			// Only update if user has changed the size from its declarative value.
			if (WindowX != 0 && WindowY != 0)
			{
				form.Location = new Point(WindowX, WindowY);
			}

			// Only update if user has changed the size from its declarative value.
			if (WindowWidth != 0 && WindowHeight != 0)
			{
				form.Size = new Size(WindowWidth, WindowHeight);
			}
		}

		/// <summary>
		/// Create the table and column definitions for the protocol.
		/// </summary>
		protected void CreateViewerTable()
		{
			if (!String.IsNullOrEmpty(ProtocolName))
			{
				DataTable dt = new DataTable();
				ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);
				st.AllTypes.ForEach(t =>
					{
						DataColumn dc = new DataColumn(t.Name, t.GetImplementingType(rsys.SemanticTypeSystem));
						dt.Columns.Add(dc);
					});

				dvSignals = new DataView(dt);
				dgvSignals.DataSource = dvSignals;
			}
		}

		/// <summary>
		/// Remove the old protocol (if it exists) and start listening to the new.
		/// </summary>
		protected void ListenForProtocol()
		{
			if (!String.IsNullOrEmpty(oldProtocol))
			{
				RemoveReceiveProtocol(oldProtocol);
			}

			oldProtocol = ProtocolName;
			AddReceiveProtocol(ProtocolName, (Action<dynamic>)((signal) => ShowSignal(signal)));

			// Add other semantic type emitters:
			RemoveEmitProtocols();
			ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);
			st.SemanticElements.ForEach(se => AddEmitProtocol(se.Name));
		}

		/// <summary>
		/// Add a record to the existing view showing the signal's content.
		/// </summary>
		/// <param name="signal"></param>
		protected void ShowSignal(dynamic signal)
		{
			try
			{
				DataTable dt = dvSignals.Table;
				DataRow row = dt.NewRow();
				ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);

				st.AllTypes.ForEach(t =>
					{
						object val = t.GetValue(rsys.SemanticTypeSystem, signal);
						row[t.Name] = val;
					});

				dt.Rows.Add(row);
			}
			catch (Exception ex)
			{
				EmitException(ex);
			}
		}

		/// <summary>
		/// Emit a semantic protocol with the value in the selected row and the column determined by the semantic element name.
		/// </summary>
		protected void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);

			st.SemanticElements.ForEach(se =>
				{
					CreateCarrier(se.Name, signal => se.SetValue(rsys.SemanticTypeSystem, signal, dvSignals[e.RowIndex][se.Name].ToString()));
				});
		}
	}
}
