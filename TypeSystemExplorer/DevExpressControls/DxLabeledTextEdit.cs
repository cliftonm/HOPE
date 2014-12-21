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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraEditors;

using Clifton.ExtensionMethods;
using Clifton.Tools.Data;

using TypeSystemExplorer;
using TypeSystemExplorer.Controls;

namespace TypeSystemExplorer.DevExpressControls
{
	public partial class DxLabeledTextEdit : UserControl
	{
		public string LabelText
		{
			get { return lblTextBox.Text; }
			set { lblTextBox.Text = value; }
		}

		public string TextBoxText
		{
			get { return tbData.Text; }
			set { tbData.Text = value; }
		}

		public event TextChangedDlgt TextDataChanged;
		public event EventHandler TextBoxGotFocus;

		public DxLabeledTextEdit()
		{
			InitializeComponent();
			tbData.GotFocus += ((object sender, EventArgs args) => TextBoxGotFocus.IfNotNull(e => e(sender, args)));
			tbData.LostFocus += ((object sender, EventArgs args) => TextDataChanged.IfNotNull(e => e(tbData.Text)));
			tbData.KeyPress += ((object sender, KeyPressEventArgs args) =>
			{
				if (args.KeyChar == 13)
				{
					TextDataChanged.IfNotNull(e => e(tbData.Text));
				}
			});
		}
	}
}
