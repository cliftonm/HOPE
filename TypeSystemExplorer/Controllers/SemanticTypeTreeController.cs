using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using TypeSystemExplorer.Controllers;
using TypeSystemExplorer.Models;
using TypeSystemExplorer.Views;

using Clifton.ApplicationStateManagement;
using Clifton.ExtensionMethods;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

namespace TypeSystemExplorer.Controllers
{
	public class SemanticTypeTreeController: ViewController<SemanticTypeTreeView>
	{
		public SemanticTypeTreeController()
		{
		}

		protected void Opening()
		{
//			Program.AppState.RestoreState("TOC");
		}

		protected void Closing()
		{
//			Program.AppState.SaveState("TOC");
		}

		protected void NodeSelected(object sender, TreeViewEventArgs e)
		{
			ApplicationController.PropertyGridController.IfNotNull(t => t.ShowObject(e.Node.Tag));
		}
	}
}
