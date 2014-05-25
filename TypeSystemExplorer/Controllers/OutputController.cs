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
