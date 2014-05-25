using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraTreeList.Nodes;

using TypeSystemExplorer.DevExpressControls;

namespace TypeSystemExplorer.Controls
{
	public interface ITreeList
	{
		object SelectedNodeTag { get; }
		object SelectedNode { get; set; }

		void Clear();
		object AddNode(object parent, string name, object tag, bool isChecked);
		object AddNode(object parent, object[] fields, object tag, bool isChecked);
		object GetNodeAt(int x, int y);

		List<DxTreeListColumn> GetTreeColumns();
		void SetColumnInfo(int colIdx, int visibleIndex, int width, bool visible, int sortIndex, SortOrder sortOrder);
		TreeListNode LastClickNode { get; }
		void RemoveNodesWithTag(object tag, Action<TreeListNode> NodeDeletingCallback = null);
	}
}
