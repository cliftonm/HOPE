using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;

using Clifton.ExtensionMethods;
using Clifton.Tools.Data;

using TypeSystemExplorer.Controls;

namespace TypeSystemExplorer.DevExpressControls
{
	public delegate void NodeSelectedDlgt(object sender, object tag);
	public delegate void NodeCheckedDlgt(object sender, object tag, bool state);
	public delegate void NodeRightClickDlgt(object sender, object tag, Point mousePosition);

	public static class NodeHelpers
	{
		// Note that there is also a NodeIterator feature of the TreeView, which we are not using here.
		/// <summary>
		/// Returns an enumerable traversing the children, grandchildren, etc., of the current node.
		/// </summary>
		public static IEnumerable<TreeListNode> Hierarchy(this TreeListNode node)
		{
			foreach (TreeListNode childNode in node.Nodes)
			{
				yield return childNode;

				foreach (TreeListNode subchild in childNode.Hierarchy())
				{
					yield return subchild;
				}
			}
		}

		// Note that there is also a NodeIterator feature of the TreeView, which we are not using here.
		/// <summary>
		/// Returns an enumerable traversing the nodes, children, grandchildren, etc., of the tree.
		/// </summary>
		public static IEnumerable<TreeListNode> Hierarchy(this DxTreeList tree)
		{
			foreach (TreeListNode node in tree.Nodes)
			{
				yield return node;

				foreach (TreeListNode childNode in node.Hierarchy())
				{
					yield return childNode;
				}
			}
		}

		public static void ForEach(this TreeListNodes nodes, Action<TreeListNode> action)
		{
			foreach (TreeListNode node in nodes)
			{
				action(node);
			}
		}
	}

	public class DxTreeListColumn
	{
		public string Caption {get;set;}
		[DefaultValue(false)]
		public bool AllowEdit { get; set; }
		[DefaultValue(true)]
		public bool Visible {get;set;} 

		public int VisibleIndex { get; set; }
		public int Width {get;set;}
		public int SortIndex { get; set; }
		public SortOrder SortOrder { get; set; }

		public DxTreeListColumn()
		{
			Caption = "Title";
			AllowEdit = false;
			Visible = false;
			VisibleIndex = 0;
		}
	}

	public class DxTreeList : TreeList, ISupportInitialize
	{
		public event NodeSelectedDlgt NodeSelected;
		public event NodeCheckedDlgt NodeChecked;
		public event NodeRightClickDlgt NodeRightClick;

		public bool ShowCheckBoxes
		{
			get { return OptionsView.ShowCheckBoxes; }
			set { OptionsView.ShowCheckBoxes = value; }
		}

		public object SelectedNodeTag { get { return FocusedNode.Tag; } }

		public TreeListNode LastClickNode { get; protected set; }

		public object SelectedNode
		{
			get { return FocusedNode; }
			set { FocusedNode = (TreeListNode)value; }
		}

		public List<DxTreeListColumn> TreeListColumns { get; protected set; }
		
		public bool ShowColumns
		{
			get { return OptionsView.ShowColumns; }
			set { OptionsView.ShowColumns = value; }
		}

		public bool ShowIndicator
		{
			get { return OptionsView.ShowIndicator; }
			set { OptionsView.ShowIndicator = value; }
		}

		public bool ShowHorizLines
		{
			get { return OptionsView.ShowHorzLines; }
			set { OptionsView.ShowHorzLines = value; }
		}

		public int Index
		{
			get { return FocusedRowIndex; }
			set { FocusedRowIndex = value; }
		}

		public DxTreeList()
		{
			TreeListColumns = new List<DxTreeListColumn>();

			MouseDown += new MouseEventHandler(OnMouseDown);
			AfterCheckNode += new NodeEventHandler(OnAfterCheckNode);

			OptionsSelection.EnableAppearanceFocusedCell = false;	// no cell selection.
			OptionsSelection.EnableAppearanceFocusedRow = true;		// background color is displayed for the full row.
			OptionsSelection.MultiSelect = false;
		}

		public override void EndInit()
		{
			TreeListColumns.ForEach(t => AddColumn(t));
			base.EndInit();
		}

		public void Clear()
		{
			Nodes.Clear();
		}

		public string GetSelectedNodeText(int colNum)
		{
			return FocusedNode.GetValue(colNum).ToString();
		}

		public object AddNode(object parent, string text, object tag, bool isChecked)
		{
			TreeListNode tln = AppendNode(new object[] { text }, (TreeListNode)parent, tag);
			tln.Checked = isChecked;

			return tln;
		}

		public object AddNode(object parent, object[] fields, object tag, bool isChecked)
		{
			TreeListNode tln = AppendNode(fields, (TreeListNode)parent, tag);
			tln.Checked = isChecked;

			return tln;
		}

		public object GetNodeAt(int x, int y)
		{
			TreeListHitInfo hitInfo = CalcHitInfo(new Point(x, y));

			return hitInfo.Node;
		}

		public List<DxTreeListColumn> GetTreeColumns()
		{
			List<DxTreeListColumn> cols = new List<DxTreeListColumn>();

			foreach (TreeListColumn tlc in Columns)
			{
				DxTreeListColumn col = new DxTreeListColumn()
				{
					Caption = tlc.Caption,
					Visible = tlc.Visible,
					VisibleIndex = tlc.VisibleIndex,
					Width = tlc.Width,
					SortIndex = tlc.SortIndex,
					SortOrder = tlc.SortOrder,
				};

				cols.Add(col);
			}

			return cols;
		}

		public void SetColumnInfo(int idx, int visibleIndex, int width, bool visible, int sortIndex, SortOrder sortOrder)
		{
			Columns[idx].VisibleIndex = visibleIndex;
			Columns[idx].Width = width;
			Columns[idx].Visible = visible;
			Columns[idx].SortIndex = sortIndex;
			Columns[idx].SortOrder = sortOrder;
		}

		public void RemoveNodesWithTag(object tag, Action<TreeListNode> NodeDeletingCallback=null)
		{
			List<Tuple<TreeListNodes, TreeListNode>> markForDeletion = new List<Tuple<TreeListNodes, TreeListNode>>();

			foreach (TreeListNode node in Nodes)
			{
				if (node.Tag == tag)
				{
					markForDeletion.Add(new Tuple<TreeListNodes, TreeListNode>(Nodes, node));
				}

				foreach (TreeListNode childNode in node.Hierarchy())
				{
					if (node.Tag == tag)
					{
						markForDeletion.Add(new Tuple<TreeListNodes, TreeListNode>(node.ParentNode.Nodes, node));
					}
				}
			}

			markForDeletion.ForEach(n =>
				{
					NodeDeletingCallback.IfNotNull(t => t(n.Item2));
					n.Item1.Remove(n.Item2);
				});
		}

		/// <summary>
		/// Adds a column to the DevExpress tree control.  Various properties of the column are defined in our class DxTreeListColumn
		/// which is used by the MyXaml deserializer for easier use in the XML.
		/// </summary>
		/// <param name="col"></param>
		protected void AddColumn(DxTreeListColumn col)
		{
			TreeListColumn tlc = Columns.Add();
			tlc.Caption = col.Caption;
			tlc.OptionsColumn.AllowEdit = col.AllowEdit;
			tlc.Visible = col.Visible;

			if (col.Visible)
			{
				tlc.VisibleIndex = 0;
			}
		}

		protected void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				TreeListHitInfo hitInfo = CalcHitInfo(e.Location);

				if (hitInfo.Node != null)
				{
					// Ignore clicks on the expand button.  All other types are currently valid.
					if (hitInfo.HitInfoType != HitInfoType.Button)
					{
						if (hitInfo.Node.Tag != null)
						{
							if (NodeSelected != null)
							{
								LastClickNode = hitInfo.Node;
								NodeSelected(this, hitInfo.Node.Tag);
							}
						}
					}
				}
			}
			else if (e.Button == MouseButtons.Right)
			{
				TreeListHitInfo hitInfo = CalcHitInfo(e.Location);

				if (hitInfo.Node != null)
				{
					if (hitInfo.Node.Tag != null)
					{
						if (NodeRightClick != null)
						{
							LastClickNode = hitInfo.Node;
							SelectedNode = LastClickNode;
							NodeRightClick(this, hitInfo.Node.Tag, hitInfo.MousePoint);
						}
					}
				}
			}
		}

		protected void OnAfterCheckNode(object sender, NodeEventArgs e)
		{
			if (e.Node.Tag != null)
			{
				if (NodeChecked != null)
				{
					NodeChecked(sender, e.Node.Tag, e.Node.Checked);
				}
			}
		}
	}
}
