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
using System.Windows.Forms;

using Clifton.Windows.Forms;

using TypeSystemExplorer.Controllers;

namespace TypeSystemExplorer.Actions
{
	public class InitializeMruMenu : DeclarativeAction, IMruMenu
	{
		public ToolStripMenuItem MenuFile { get; protected set; }
		public ToolStripMenuItem MenuRecentFiles { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		
		protected MruStripMenuInline mruMenu;

		public override void EndInit()
		{
			string mruRegKey = "SOFTWARE\\TypeSystemExplorer\\Config";
			mruMenu = new MruStripMenuInline(MenuFile, MenuRecentFiles, new MruStripMenu.ClickedHandler(OnMruFile), mruRegKey + "\\MRU", true, 9);
		}

		public void AddFile(string filename)
		{
			mruMenu.AddFile(filename);
			mruMenu.SaveToRegistry();
		}

		protected void OnMruFile(int idx, string filename)
		{
			if (ApplicationController.CheckDirtyModel())
			{
				ApplicationController.LoadApplet(filename);
			}
		}
	}
}
