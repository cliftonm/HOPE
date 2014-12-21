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

using TypeSystemExplorer.Actions;
using TypeSystemExplorer.Models;
using TypeSystemExplorer.Views;

using Clifton.ApplicationStateManagement;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

namespace TypeSystemExplorer.Controllers
{
	public class OutputController : ViewController<OutputView>
	{
		public OutputController()
		{
		}

		public override void EndInit()
		{
			ApplicationController.OutputController = this;
			base.EndInit();
		}

		protected void SaveOutput(object sender, EventArgs args)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			DialogResult ret = sfd.ShowDialog();

			if (ret == DialogResult.OK)
			{
				View.Editor.SaveFile(sfd.FileName);
			}
		}

		protected void LoadOutput(object sender, EventArgs args)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			DialogResult ret = ofd.ShowDialog();

			if (ret == DialogResult.OK)
			{
				View.Editor.LoadFile(ofd.FileName);
			}
		}
	}
}
