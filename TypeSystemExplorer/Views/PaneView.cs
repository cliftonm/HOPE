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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Clifton.Tools.Data;

using TypeSystemExplorer.Controllers;

namespace TypeSystemExplorer.Views
{
	public abstract class PaneView : UserControl, ISupportInitialize, IPaneView
	{
		public DockContent DockContent { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		public abstract string MenuName { get; }

		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
			ApplicationController.SetMenuCheckedState(MenuName, true);
			HandleDestroyed += WhenHandleDestroyed;
		}

		protected virtual void WhenHandleDestroyed(object sender, EventArgs e)
		{
			ApplicationController.SetMenuCheckedState(MenuName, false);
			ApplicationController.PaneClosed(this);
			HandleDestroyed -= WhenHandleDestroyed;
		}
	}
}
