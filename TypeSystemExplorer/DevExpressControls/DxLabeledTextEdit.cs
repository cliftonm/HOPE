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
