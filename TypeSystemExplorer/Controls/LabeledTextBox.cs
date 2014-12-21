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

namespace TypeSystemExplorer.Controls
{
	public delegate void TextChangedDlgt(string newText);

	public partial class LabeledTextBox : UserControl
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

		public LabeledTextBox()
		{
			InitializeComponent();
			tbData.LostFocus += ((object sender, EventArgs args) => OnTextChanged());
			tbData.KeyPress += ((object sender, KeyPressEventArgs args) =>
				{
					if (args.KeyChar == 13)
					{
						OnTextChanged();
					}
				});
		}

		protected void OnTextChanged()
		{
			if (TextDataChanged != null)
			{
				TextDataChanged(tbData.Text);
			}
		}
	}
}
