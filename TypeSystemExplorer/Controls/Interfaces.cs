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
