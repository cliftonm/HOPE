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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace CarrierTreeViewerReceptor
{
	public class NodeTag
	{
		public object Signal { get; set; }
		public string ProtocolName { get; set; }
		public int Level { get; set; }
	}

    public class CarrierTreeViewer : WindowedBaseReceptor
    {
		public override string Name { get { return "Carrier Tree Viewer"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "CarrierTreeViewerConfig.xml"; } }

		[MycroParserInitialize("tv")]
		protected TreeView treeView;

		protected DataGridView dgvTypes;
		protected DataTable dt;

		// protected ComboBox cbProtocols;

		[UserConfigurableProperty("Internal")]
		public string ProtocolHierarchy { get; set; }

		// TODO: We really need an internal protocol representation.

		public CarrierTreeViewer(IReceptorSystem rsys)
		  : base("CarrierTreeViewer.xml", true, rsys)
		{
			AddEmitProtocol("ExceptionMessage");
		}

		public override void Initialize()
		{
			base.Initialize();
			InitializeConfigTable();

			// Initialization happens before EndSystemInit so that the receptor is configured to receive carriers.
			ListenForProtocols();
		}

		public override void PrepopulateConfig(MycroParser mp)
		{
			base.PrepopulateConfig(mp);
			dgvTypes = (DataGridView)mp.ObjectCollection["dgvTypes"];

			if (dt == null)
			{
				// Create the table if it doesn't exist, otherwise
				// we populate the grid with the existing configuration.
				dt = InitializeDataTable();
			}

			dgvTypes.DataSource = new DataView(dt);

			// Stupid DGV doesn't pay attention to column captions:
			foreach (DataGridViewColumn col in dgvTypes.Columns)
			{
				col.HeaderText = dt.Columns[col.HeaderText].Caption;
			}

			// This combobox is in the config UI.
			//cbProtocols = (ComboBox)mp.ObjectCollection["cbProtocols"];
			//ReceptorUiHelpers.Helper.PopulateProtocolComboBox(cbProtocols, rsys, ProtocolName);
		}

		/// <summary>
		/// Remove the old protocol (if it exists) and start listening to the new.
		/// </summary>
		protected virtual void ListenForProtocols()
		{
			RemoveReceiveProtocols();
			RemoveEmitProtocols();

			if (!String.IsNullOrEmpty(ProtocolHierarchy))
			{
				ProtocolHierarchy.Split(';').ForEach(pq =>
					{
						string p = pq.LeftOf(',');
						string q = pq.RightOf(',');
						AddReceiveProtocol(p);

						if (!String.IsNullOrEmpty(q))
						{
							AddEmitProtocol(q);
						}
					});
			}

			AddEmitProtocol("ExceptionMessage");
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
				string protocol = row[0].ToString();

				if (!String.IsNullOrEmpty(protocol))
				{
					protocol = protocol.Replace(",", "").Replace(";", "");

					// the format is [protocolname];[protocolname];...etc...
					sb.Append(and);
					sb.Append(protocol);
					and = ";";
				}
			}

			ProtocolHierarchy = sb.ToString();
		}

		protected override void InitializeUI()
		{
			base.InitializeUI();
			treeView.NodeMouseDoubleClick += OnNodeMouseDoubleClick;
		}

		protected void OnNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			NodeTag tag = e.Node.Tag as NodeTag;
			string queryProtocol = ProtocolHierarchy.Split(';')[tag.Level].RightOf(',');

			if (!String.IsNullOrEmpty(queryProtocol))
			{
				CreateCarrier(queryProtocol, signal =>
					{
						// The member is the signal of the current node.
						// TODO: Put this into the STS as a common function.
						PropertyInfo pi = ((object)signal).GetType().GetProperty(tag.ProtocolName);
						pi.SetValue(signal, tag.Signal);
					});
			}
		}

		protected override void ReinitializeUI()
		{
			base.ReinitializeUI();

			ListenForProtocols();
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			base.ProcessCarrier(carrier);

			int idx = 0;
			
			foreach(string pq in ProtocolHierarchy.Split(';'))
			{
				string p = pq.LeftOf(',');
				if (carrier.ProtocolPath == p)			// only top-level protocols are processed, otherwise we end up adding signals at all parent levels of the tree!
				{
					ShowSignal(carrier.Signal, p, idx);
					break;
				}

				++idx;
			}
		}

		/// <summary>
		/// When the user configuration fields have been updated, reset the protocol we are listening for.
		/// Return false if configuration is invalid.
		/// </summary>
		public override bool UserConfigurationUpdated()																			    
		{
			bool ret = true;
			List<string> badProtocols = new List<string>();

			// TODO: Most of this (verifying a list of protocols) is probaby a rather common thing to do.  Move into STS as "VerifyProtocolsExist".
			StringBuilder sbPending = new StringBuilder();
			string and = String.Empty;

			foreach(DataRow row in dt.Rows)
			{
				// protocol and query
				string p = String.Empty;
				string q = string.Empty;

				if ( (row[0] != null) && (!String.IsNullOrEmpty(row[0].ToString())) )
				{
					p=row[0].ToString();
				}

				if ( (row[1] != null) && (!String.IsNullOrEmpty(row[1].ToString())) )
				{
					q=row[1].ToString();
				}

				bool exists = rsys.SemanticTypeSystem.VerifyProtocolExists(p);

				if (!exists)
				{
					badProtocols.Add(p);
					ret = false;
				}
				else
				{
					sbPending.Append(and);
					sbPending.Append(p + "," + q);
					and = ";";
				}
			}

			if (ret)
			{
				ProtocolHierarchy = sbPending.ToString();
				ListenForProtocols();
			}
			else
			{
				ConfigurationError = "The semantic type(s):\r\n" + String.Join("\r\n", badProtocols) + "\r\n do not exist.";
			}

			return ret;
		}

		protected void ConfigureBasedOnSelectedProtocol()
		{
			//if (cbProtocols != null)
			//{
			//	// Update the protocol name if the combobox exists, either in the main UI or the configuration UI.
			//	ProtocolName = cbProtocols.SelectedValue.ToString();
			//}

			ListenForProtocols();
			// UpdateCaption();
		}

		/// <summary>
		/// Add a record to the existing view showing the signal's content.
		/// </summary>
		protected void ShowSignal(dynamic signal, string protocolName, int level)
		{
			// form.IfNull(() => ReinitializeUI());

			if ((form == null) && (doc == null))
			{
				ReinitializeUI();
			}

			// Root level.
			if (level == 0)
			{
				List<IFullyQualifiedNativeType> colValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues((object)signal, protocolName).OrderBy(fqn => fqn.Ordinality).ToList();
				TreeNode tn = new TreeNode(colValues[0].Value.ToString());
				tn.Tag = new NodeTag() { Signal = signal, ProtocolName = protocolName, Level = 0 };
				treeView.Nodes.Add(tn);
			}
			else
			{
				// Get the id's of the parent, grand-parent, etc., up the tree.
				// This is a bit annoying, as it's heavily reliant on the ST implementing "Id"
				Dictionary<int, string> idHierarchy = new Dictionary<int, string>();
				
				for (int i = level - 1; i >= 0; i--)
				{
					string parentProtocol = ProtocolHierarchy.Split(';')[i].LeftOf(',');
					// Get the ID
					PropertyInfo pi = ((object)signal).GetType().GetProperty(parentProtocol);
					dynamic parentInstance = pi.GetValue(signal);
					string id = parentInstance.Id.ToString();
					idHierarchy[i] = id;
				}

				TreeNodeCollection tnc = treeView.Nodes;
				TreeNode tnTarget = null;

				// Now drill down into the tree.
				for (int i = 0; i < level; i++)
				{
					bool found = false;

					foreach (TreeNode tn in tnc)
					{
						if (((dynamic)((NodeTag)tn.Tag).Signal).Id == idHierarchy[i])
						{
							tnTarget = tn;
							tnc = tn.Nodes;
							found = true;
							break;
						}
					}

					if (!found)
					{
						EmitException("Node not found");
						return;
					}
				}

				if (tnTarget == null)
				{
					EmitException("Node not found");
					return;
				}

				{
					// We're left with tnTarget being the parent node to which we add this child.
					List<IFullyQualifiedNativeType> colValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues((object)signal, protocolName).OrderBy(fqn => fqn.Ordinality).ToList();
					TreeNode tn = new TreeNode(colValues[0].Value.ToString());
					tn.Tag = new NodeTag() { Signal = signal, ProtocolName = protocolName, Level = level };
					tnTarget.Nodes.Add(tn);
				}
			}
		}

		protected DataTable InitializeDataTable()
		{
			DataTable dt = new DataTable();
			DataColumn dcProtocol = new DataColumn("protocol");
			dcProtocol.Caption = "Semantic Type";
			DataColumn dcQueryProtocol = new DataColumn("qprotocol");
			dcQueryProtocol.Caption = "Query Protocol";
			dt.Columns.AddRange(new DataColumn[] { dcProtocol, dcQueryProtocol });

			return dt;
		}

		/// <summary>
		/// Initializes the table used for configuration with the serialized tab-protocol list.
		/// </summary>
		protected void InitializeConfigTable()
		{
			dt = InitializeDataTable();

			if (!String.IsNullOrEmpty(ProtocolHierarchy))
			{
				string[] tpArray = ProtocolHierarchy.Split(';');

				foreach (string tp in tpArray)
				{
					DataRow row = dt.NewRow();
					row[0] = tp.LeftOf(',');
					row[1] = tp.RightOf(',');
					dt.Rows.Add(row);
				}
			}
		}
	}
}
