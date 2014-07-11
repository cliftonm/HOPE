using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace CarrierListViewerReceptor
{
	public class CarrierListViewer : BaseReceptor
    {
		public override string Name { get { return "Carrier List Viewer"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "CarrierListViewerConfig.xml"; } }

		[UserConfigurableProperty("Protocol Name:")]
		public string ProtocolName { get; set; }

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
			CreateViewerTable();
			ListenForProtocol();
		}

		/// <summary>
		/// When the user configuration fields have been updated, reset the protocol we are listening for.
		/// </summary>
		public override void UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			CreateViewerTable();
			ListenForProtocol();
		}

		public override void Terminate()
		{
			base.Terminate();
			form.Close();
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
						dt.Columns.Add(new DataColumn(t.Name));
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
				EmitException("Carrier List Viewer Receptor", ex);
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
