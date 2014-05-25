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
