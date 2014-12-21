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

using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;

using TypeSystemExplorer.Controls;
using TypeSystemExplorer.Models;

using Clifton.ExtensionMethods;
using Clifton.Tools.Data;
using Clifton.Windows.Forms;
using Clifton.Windows.Forms.XmlTree;

namespace TypeSystemExplorer.Views
{
	public class SemanticTypeEditorView : PaneView
	{
		public delegate void NotificationDlgt();

		public event NotificationDlgt Opening;
		public event NotificationDlgt Closing;

		public ApplicationModel Model { get; protected set; }
		public XTree TreeView { get; protected set; }
		public override string MenuName { get { return "mnuSemanticTypeEditor"; } }

		public override void EndInit()
		{
			Opening.IfNotNull().Then(() => Opening());
			base.EndInit();
		}

		protected override void WhenHandleDestroyed(object sender, EventArgs e)
		{
			Closing.IfNotNull().Then(() => Closing());
			base.WhenHandleDestroyed(sender, e);
		}

		public void Clear()
		{
			TreeView.Clear();
		}

		public TreeNode AddNode(IXtreeNode inst, TreeNode parent)
		{
			TreeNode node = TreeView.AddNode(inst, parent);

			return node;
		}
	}
}
