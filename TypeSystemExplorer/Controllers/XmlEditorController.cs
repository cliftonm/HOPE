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
	public class XmlEditorController : ViewController<XmlEditorView>
	{
		public XmlEditorController()
		{
		}

		public override void EndInit()
		{
			ApplicationController.XmlEditorController = this;
			base.EndInit();
		}

		protected void OnFocus(object sender, EventArgs args)
		{
		}

		protected void SaveXml(object sender, EventArgs args)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			DialogResult ret = sfd.ShowDialog();

			if (ret == DialogResult.OK)
			{
				View.Editor.SaveFile(sfd.FileName);
			}
		}

		protected void LoadXml(object sender, EventArgs args)
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
