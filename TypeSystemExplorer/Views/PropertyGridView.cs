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
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.SemanticTypeSystem;

using TypeSystemExplorer.Controls;
using TypeSystemExplorer.Models;

using Clifton.ExtensionMethods;
using Clifton.Tools.Data;

namespace TypeSystemExplorer.Views
{
	public class PropertyGridView : PaneView
	{
		public delegate void NotificationDlgt();

		public event NotificationDlgt Opening;
		public event NotificationDlgt Closing;

		public ApplicationModel Model { get; protected set; }
		public PropertyGrid PropertyGrid { get; protected set; }
		public override string MenuName { get { return "mnuPropertyGrid"; } }

		public override void EndInit()
		{
			Opening.IfNotNull().Then(() => Opening());
			base.EndInit();
		}

		public void ShowObject(object obj)
		{
			PropertyGrid.SelectedObject = obj;
			// PropertyGrid.Refresh();
		}

		public void Clear()
		{
			PropertyGrid.SelectedObject = null;
		}

		protected override void WhenHandleDestroyed(object sender, EventArgs e)
		{
			Closing.IfNotNull().Then(() => Closing());
			base.WhenHandleDestroyed(sender, e);
		}
	}
}
