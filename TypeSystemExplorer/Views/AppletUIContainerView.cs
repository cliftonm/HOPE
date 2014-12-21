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
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using WeifenLuo.WinFormsUI.Docking;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Tools.Strings;
using Clifton.Tools.Strings.Extensions;

using TypeSystemExplorer.Controllers;
using TypeSystemExplorer.Models;

namespace TypeSystemExplorer.Views
{
	public class AppletUIContainerView : Form
	{
		/// <summary>
		/// Assigned by the mainform.xml instantiation: DockPanel="{dockPanel}"
		/// </summary>
		public DockPanel DockPanel { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }

		public AppletUIContainerView()
		{
		}
	}
}
