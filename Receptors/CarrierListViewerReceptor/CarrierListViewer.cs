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
	public class CarrierListViewer : WindowedBaseReceptor
    {
		public override string Name { get { return "Carrier List Viewer"; } }
		public override string Subname { get { return ProtocolName; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "CarrierListViewerConfig.xml"; } }

		[UserConfigurableProperty("Protocol Name:")]
		public string ProtocolName { get; set; }

		/// <summary>
		/// If true, the protocol select is shown in the UI with the grid using a different XML file.
		/// </summary>
		[UserConfigurableProperty("ShowProtocolPicker")]
		public bool ShowProtocolPicker { get; set; }

		protected string oldProtocol;
		protected bool oldShowProtocolPicker;
		protected DataView dvSignals;
		protected DataGridView dgvSignals;
		protected ComboBox cbProtocols;

		protected List<IFullyQualifiedNativeType> uniqueKey;

		public CarrierListViewer(IReceptorSystem rsys)
		  : base("CarrierListViewer.xml", true, rsys)
		{
			AddEmitProtocol("ExceptionMessage");
			uniqueKey = new List<IFullyQualifiedNativeType>();
		}

		public CarrierListViewer(IReceptorSystem rsys, string xmlFormName)
			: base(xmlFormName, true, rsys)
		{
			AddEmitProtocol("ExceptionMessage");
			uniqueKey = new List<IFullyQualifiedNativeType>();
		}

		public override void Initialize()
		{
			base.Initialize();

			// Initialization happens before EndSystemInit so that the receptor is configured to receive carriers.
			CreateViewerTable();
			ListenForProtocol();
		}

		/// <summary>
		/// When the user configuration fields have been updated, reset the protocol we are listening for.
		/// Return false if configuration is invalid.
		/// </summary>
		public override bool UserConfigurationUpdated()
		{
			bool ret = true;
			base.UserConfigurationUpdated();

			if (oldShowProtocolPicker != ShowProtocolPicker)
			{
				form.IfNotNull(f => f.Close());
				displayFormFilename = GetDisplayFormName();
				ReinitializeUI();
			}

			oldShowProtocolPicker = ShowProtocolPicker;

			if (!ShowProtocolPicker)
			{

				ret = rsys.SemanticTypeSystem.VerifyProtocolExists(ProtocolName);

				if (ret)
				{
					CreateViewerTable();
					ListenForProtocol();
					UpdateCaption();
				}
				else
				{
					ConfigurationError = "The semantic type '" + ProtocolName + "' is not defined.";
				}
			}

			return ret;
		}

		/// <summary>
		/// Return the XML file for displaying the carrier list depending on the ShowProtocolPicker state.
		/// </summary>
		protected virtual string GetDisplayFormName()
		{
			return (ShowProtocolPicker ? "CarrierListViewerWithProtocolPicker.xml" : "CarrierListViewer.xml");
		}

		protected override void UpdateCaption()
		{
			if (form != null)
			{
				string caption = String.Empty;

				if (!String.IsNullOrEmpty(WindowName))
				{
					caption = WindowName;
				}
				if (!String.IsNullOrEmpty(ProtocolName))
				{
					form.Text = caption + " - " + ProtocolName;
				}
			}
		}

		/// <summary>
		/// Instantiate the UI.
		/// </summary>
		protected override void InitializeUI()
		{
			displayFormFilename = GetDisplayFormName();
			base.InitializeUI();

			oldShowProtocolPicker = ShowProtocolPicker;
			dgvSignals = (DataGridView)mycroParser.ObjectCollection["dgvRecords"];
			dgvSignals.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(0xF0, 0xFF, 0xF0);

			if (ShowProtocolPicker)
			{
				cbProtocols = (ComboBox)mycroParser.ObjectCollection["cbProtocols"];
				List<string> types = rsys.SemanticTypeSystem.SemanticTypes.Keys.ToList();
				types.Sort();
				cbProtocols.DataSource = types;
				cbProtocols.SelectedValueChanged += cbProtocols_SelectedValueChanged;

				if (!String.IsNullOrEmpty(ProtocolName))
				{
					cbProtocols.SelectedItem = ProtocolName;
				}
			}
		}

		/// <summary>
		/// This event will not fire unless an item from the list is selected.
		/// </summary>
		protected void cbProtocols_SelectedValueChanged(object sender, EventArgs e)
		{
			ProtocolName = cbProtocols.SelectedValue.ToString();
			CreateViewerTable();
			ListenForProtocol();
			UpdateCaption();
		}

		/// <summary>
		/// Create the table and column definitions for the protocol.
		/// </summary>
		protected virtual void CreateViewerTable()
		{
			if (!String.IsNullOrEmpty(ProtocolName))
			{
				DataTable dt = new DataTable();
				List<IFullyQualifiedNativeType> columns = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypes(ProtocolName).OrderBy(fqn=>fqn.Ordinality).ToList();
				uniqueKey.Clear();

				columns.ForEach(col =>
					{
						try
						{
							DataColumn dc = new DataColumn(col.FullyQualifiedName, col.NativeType.GetImplementingType(rsys.SemanticTypeSystem));

							// If no alias, then use the FQN, skipping the root protocol name.
							String.IsNullOrEmpty(col.Alias).Then(() => dc.Caption = col.FullyQualifiedName.RightOf('.')).Else(() => dc.Caption = col.Alias);
							dt.Columns.Add(dc);
							col.UniqueField.Then(() => uniqueKey.Add(col));
						}
						catch
						{
							// If the implementing type is not known by the native type system (for example, List<dynamic> used in the WeatherInfo protocol, we ignore it.
							// TODO: We need a way to support implementing lists and displaying them in the viewer as a sub-collection.
							// WeatherInfo protocol is a good example.

						}
					});

				dvSignals = new DataView(dt);
				dgvSignals.DataSource = dvSignals;

				foreach(DataColumn dc in dt.Columns)
				{
					dgvSignals.Columns[dc.ColumnName].HeaderText = dc.Caption;
				}
			}
		}

		/// <summary>
		/// Remove the old protocol (if it exists) and start listening to the new.
		/// </summary>
		protected virtual void ListenForProtocol()
		{
			if (!String.IsNullOrEmpty(oldProtocol))
			{
				RemoveReceiveProtocol(oldProtocol);
			}

			if (!String.IsNullOrEmpty(ProtocolName))
			{
				AddReceiveProtocol(ProtocolName); // , (Action<dynamic>)((signal) => ShowSignal(signal)));

				// Add other semantic type emitters:
				RemoveEmitProtocols();
				AddEmitProtocol("ExceptionMessage");
				ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);
				st.SemanticElements.ForEach(se => AddEmitProtocol(se.Name));
			}

			oldProtocol = ProtocolName;
		}

		protected override void ReinitializeUI()
		{
			base.ReinitializeUI();

			CreateViewerTable();
			ListenForProtocol();
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			base.ProcessCarrier(carrier);

			if (carrier.Protocol.DeclTypeName == ProtocolName)
			{
				ShowSignal(carrier.Signal);
			}
		}

		/// <summary>
		/// Add a record to the existing view showing the signal's content.
		/// </summary>
		/// <param name="signal"></param>
		protected void ShowSignal(dynamic signal)
		{
			form.IfNull(() => ReinitializeUI());
			List<IFullyQualifiedNativeType> colValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, ProtocolName);

			if (!RowExists(colValues))
			{
				try
				{
					DataTable dt = dvSignals.Table;
					DataRow row = dt.NewRow();
					colValues.ForEach(cv =>
						{
							try
							{
								row[cv.FullyQualifiedName] = cv.Value;
							}
							catch
							{
								// Ignore columns we can't handle.
								// TODO: Fix this at some point.  WeatherInfo protocol is a good example.
							}
						});
					dt.Rows.Add(row);
				}
				catch (Exception ex)
				{
					EmitException(ex);
				}
			}
		}

		/// <summary>
		/// Returns true if the row, based on the unique key field list, already exists.
		/// </summary>
		protected bool RowExists(List<IFullyQualifiedNativeType> colValues)
		{
			bool ret = false;

			// Do we have a way of determining uniqueness?  If not, then allow all signals.
			if (uniqueKey.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				string and = String.Empty;

				foreach (IFullyQualifiedNativeType fnt in uniqueKey)
				{
					try
					{
						string strval = colValues.Single(cv=>cv.FullyQualifiedName == fnt.FullyQualifiedName).Value.ToString();
						// If the above succeeded:

						sb.Append(and);
						sb.Append(fnt.FullyQualifiedName + " = '" + strval.Replace("'", "''") + "'");
						and = " and ";
					}
					catch
					{
						// We ignore types we can't convert to string, such as collections. 
						// TODO: Collections should never be in the unque key list anyways!
					}
				}

				DataView dvFilter = new DataView(dvSignals.Table);
				dvFilter.RowFilter = sb.ToString();
				ret = (dvFilter.Count > 0);
			}

			return ret;
		}

		/// <summary>
		/// Emit a semantic protocol with the value in the selected row and the column determined by the semantic element name.
		/// </summary>
		protected virtual void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);
			List<IFullyQualifiedNativeType> ntList = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypes(ProtocolName);
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);
			dynamic outsignal = rsys.SemanticTypeSystem.Create(ProtocolName);
			
			ntList.ForEach(nt =>
				{
					// Store the value into the signal using the FQN.
					string colName = nt.FullyQualifiedName;

					// Columns that can't be mapped to native types directly (like lists) are not part of the data table.
					if (dgvSignals.Columns.Contains(colName))
					{
						rsys.SemanticTypeSystem.SetFullyQualifiedNativeTypeValue(outsignal, nt.FullyQualifiedNameSansRoot, dvSignals[e.RowIndex][colName]);
					}
				});

			// Send the record on its way.
			rsys.CreateCarrier(this, st, outsignal);
		}
	}
}
