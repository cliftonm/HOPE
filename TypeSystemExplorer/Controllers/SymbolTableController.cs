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
	public class SymbolTableController : ViewController<SymbolTableView>
	{
		public SymbolTableController()
		{
		}

		public override void EndInit()
		{
			ApplicationController.SymbolTableController = this;
			base.EndInit();
		}

		protected void OnFocus(object sender, EventArgs args)
		{
		}
	}
}
