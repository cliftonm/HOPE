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

namespace CarrierTabbedListViewerReceptor
{
	public class CarrierTabbedListViewer : BaseReceptor
	{
		public override string Name { get { return "Carrier Tabbed List Viewer"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "CarrierTabbedListViewerConfig.xml"; } }

		protected DataGridView dgvTabs;
		protected DataTable dt;
		protected TabControl tcProtocols;
		protected Form form;
		protected Dictionary<string, TabPage> protocolTabPageMap;
		protected Dictionary<string, DataGridView> protocolGridMap;

		/// <summary>
		/// For serialization only, not displayed on the configuration form.
		/// </summary>
		[UserConfigurableProperty("Internal")]
		public string ProtocolTabs { get; set; }

		[UserConfigurableProperty("WindowName")]
		public string WindowName { get; set; }

		[UserConfigurableProperty("X")]
		public int WindowX { get; set; }

		[UserConfigurableProperty("Y")]
		public int WindowY { get; set; }

		[UserConfigurableProperty("W")]
		public int WindowWidth { get; set; }

		[UserConfigurableProperty("H")]
		public int WindowHeight { get; set; }

		public CarrierTabbedListViewer(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("ExceptionMessage");
			protocolTabPageMap = new Dictionary<string, TabPage>();
			protocolGridMap = new Dictionary<string, DataGridView>();
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
			InitializeConfigTable();
			UpdateReceivedProtocolsAndTabs();
			UpdateCaption();
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			base.ProcessCarrier(carrier);
			ShowSignal(carrier.Protocol.DeclTypeName, carrier.Signal);
		}

		public override void PrepopulateConfig(MycroParser mp)
		{
			base.PrepopulateConfig(mp);
			dgvTabs = (DataGridView)mp.ObjectCollection["dgvTabs"];

			if (dt == null)
			{
				// Create the table if it doesn't exist, otherwise
				// we populate the grid with the existing configuration.
				dt = InitializeDataTable();
			}

			dgvTabs.DataSource = new DataView(dt);

			// Stupid DGV doesn't pay attention to column captions:
			foreach (DataGridViewColumn col in dgvTabs.Columns)
			{
				col.HeaderText = dt.Columns[col.HeaderText].Caption;
			}
		}

		/// <summary>
		/// Update the received protocols given the new configuration.
		/// </summary>
		public override bool UserConfigurationUpdated()
		{
			bool ret = true;
			List<string> badProtocols = new List<string>();

			// TODO: Most of this (verifying a list of protocols) is probaby a rather common thing to do.  Move into STS as "VerifyProtocolsExist".

			(from row in dt.AsEnumerable()
			 where ((row[1] != null) && (!String.IsNullOrEmpty(row[1].ToString())))
			 select row[1].ToString()).ForEach(p =>
				 {
					 bool exists = rsys.SemanticTypeSystem.VerifyProtocolExists(p);

					 if (!exists)
					 {
						 badProtocols.Add(p);
						 ret = false;
					 }
				 });

			if (ret)
			{
				UpdateReceivedProtocolsAndTabs();
				UpdateTabProtocolProperty();
				UpdateCaption();
			}
			else
			{
				ConfigurationError="The semantic type(s):\r\n"+String.Join("\r\n", badProtocols)+"\r\n do not exist.";
			}

			return ret;
		}

		protected void UpdateCaption()
		{
			if (!String.IsNullOrEmpty(WindowName))
			{
				form.Text = WindowName;
			}
		}

		/// <summary>
		/// Instantiate the UI.
		/// </summary>
		protected void InitializeUI()
		{
			// Setup the UI:
			MycroParser mp = new MycroParser();
			form = mp.Load<Form>("CarrierTabbedListViewer.xml", this);
			tcProtocols = (TabControl)mp.ObjectCollection["tcProtocols"];
			form.Show();

			// Wire up the location changed event after the form has initialized,
			// so we don't generate this event during form creation.  That way,
			// the user's config will be preserved and used when the system
			// finishes initialization.
			form.LocationChanged += OnLocationChanged;
			form.SizeChanged += OnSizeChanged;
		}

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

		protected DataTable InitializeDataTable()
		{
			DataTable dt = new DataTable();
			DataColumn dcTabName = new DataColumn("tabname");
			dcTabName.Caption = "Tab Name";
			DataColumn dcProtocol = new DataColumn("protocol");
			dcProtocol.Caption = "Protocol";
			dt.Columns.AddRange(new DataColumn[] {dcTabName, dcProtocol});

			return dt;
		}

		/// <summary>
		/// Remove protocols no longer of interest and add new protocols.
		/// </summary>
		protected void UpdateReceivedProtocolsAndTabs()
		{
			// TODO: There's something fishy here, because in the CarrierListViewer, we are also clearing all the emitted protocols when we change protocol names.  
			// Why aren't we doing that here?  If we do this, don't forget to add back in the ExceptionMessage protocol.

			// Assume we will remove all protocols.
			List<string> protocolsToBeRemoved = new List<string>(receiveProtocols.Select(p => p.Protocol).ToList());

			foreach (DataRow row in dt.Rows)
			{
				string protocol = row[1].ToString();

				if (!String.IsNullOrEmpty(protocol))
				{
					if (!protocolsToBeRemoved.Contains(protocol))			// does it currently exist or not?
					{
						AddReceiveProtocol(protocol);
						// Add emitters for semantic elements in the receive protocol that we can emit when the user double-clicks.
						// TODO: Do we add just the protocol as being emitted, or recursive sub-SE protocols as well?
						// Note this is the same issue on the CarrierListViewer as well.
						// ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
						// st.SemanticElements.ForEach(se => AddEmitProtocol(se.Name));

						// Create the tab page.
						TabPage tabPage = CreateTabPage(row[0].ToString(), protocol);
						CreateViewerTable(tabPage, protocol);
					}

					protocolsToBeRemoved.Remove(protocol);		 // nope, we don't remove this one.
				}
			}

			protocolsToBeRemoved.ForEach(p =>
				{
					RemoveReceiveProtocol(p);
					tcProtocols.TabPages.Remove(protocolTabPageMap[p]);
					protocolTabPageMap.Remove(p);
					protocolGridMap.Remove(p);
				});
		}

		protected TabPage CreateTabPage(string name, string protocol)
		{
			TabPage tp = new TabPage(name);
			tcProtocols.TabPages.Add(tp);
			protocolTabPageMap[protocol] = tp;

			return tp;
		}

		/// <summary>
		/// Updates the serializable UI property.
		/// </summary>
		protected void UpdateTabProtocolProperty()
		{
			StringBuilder sb = new StringBuilder();
			string and = String.Empty;
			
			foreach (DataRow row in dt.Rows)
			{
				string tabName = row[0].ToString();
				string protocol = row[1].ToString();

				if (!String.IsNullOrEmpty(tabName) && !String.IsNullOrEmpty(protocol))
				{
					// We use ',' and ';' internally, so strip those out.  Sorry user!
					tabName = tabName.Replace(",", "").Replace(";", "");
					protocol = protocol.Replace(",", "").Replace(";", "");

					// the format is [tabname],[protocolname];[tabname],[protocolname];...etc...
					sb.Append(and);
					sb.Append(tabName + "," + protocol);
					and = ";";
				}
			}

			ProtocolTabs = sb.ToString();
		}

		/// <summary>
		/// Initializes the table used for configuration with the serialized tab-protocol list.
		/// </summary>
		protected void InitializeConfigTable()
		{
			dt = InitializeDataTable();

			if (!String.IsNullOrEmpty(ProtocolTabs))
			{
				string[] tpArray = ProtocolTabs.Split(';');

				foreach (string tp in tpArray)
				{
					string tabName = tp.LeftOf(',');
					string protocolName = tp.RightOf(',');
					DataRow row = dt.NewRow();
					row[0] = tabName;
					row[1] = protocolName;
					dt.Rows.Add(row);
				}
			}
		}

		/// <summary>
		/// Create the table and column definitions for the protocol.
		/// </summary>
		protected void CreateViewerTable(TabPage tabPage, string protocolName)
		{
			DataTable dt = new DataTable();
			List<IFullyQualifiedNativeType> columns = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypes(protocolName);

			columns.ForEach(col =>
			{
				try
				{
					DataColumn dc = new DataColumn(col.FullyQualifiedName, col.NativeType.GetImplementingType(rsys.SemanticTypeSystem));
					
					// If no alias, then use the FQN, skipping the root protocol name.
					String.IsNullOrEmpty(col.Alias).Then(() => dc.Caption = col.FullyQualifiedName.RightOf('.')).Else(() => dc.Caption = col.Alias);
					dt.Columns.Add(dc);
				}
				catch
				{
					// If the implementing type is not known by the native type system (for example, List<dynamic> used in the WeatherInfo protocol, we ignore it.
					// TODO: We need a way to support implementing lists and displaying them in the viewer as a sub-collection.
				}

			});

			DataView dv = new DataView(dt);
			DataGridView dgv = new DataGridView();
			dgv.Dock = DockStyle.Fill;
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgv.DataSource = dv;
			dgv.AllowUserToAddRows = false;
			dgv.AllowUserToDeleteRows = false;
			dgv.ReadOnly = true;
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.RowHeadersVisible = false;
			dgv.Tag = protocolName;
			dgv.CellContentDoubleClick += OnCellContentDoubleClick;

			foreach (DataColumn dc in dt.Columns)
			{
				dgv.Columns[dc.ColumnName].HeaderText = dc.Caption;
			}

			tabPage.Controls.Add(dgv);
			protocolGridMap[protocolName] = dgv;
		}

		/// <summary>
		/// Adds a row to the table that matches the protocol associated with the signal.
		/// </summary>
		protected void ShowSignal(string protocol, dynamic signal)
		{
			try
			{
				DataTable dt = ((DataView)protocolGridMap[protocol].DataSource).Table;
				DataRow row = dt.NewRow();
				List<IFullyQualifiedNativeType> colValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, protocol);
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


		/// <summary>
		/// Emit a semantic protocol with the value in the selected row and the column determined by the semantic element name.
		/// </summary>
		protected void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			string protocol = dgv.Tag.ToString();
			DataView dv = (DataView)dgv.DataSource;
			ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);

			st.SemanticElements.ForEach(se =>
			{
				CreateCarrier(se.Name, signal => se.SetValue(rsys.SemanticTypeSystem, signal, dv[e.RowIndex][se.Name].ToString()));
			});
		}
	}
}
