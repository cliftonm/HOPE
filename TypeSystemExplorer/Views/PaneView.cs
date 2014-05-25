using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Clifton.Tools.Data;

using TypeSystemExplorer.Controllers;

namespace TypeSystemExplorer.Views
{
	public abstract class PaneView : UserControl, ISupportInitialize, IPaneView
	{
		public DockContent DockContent { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		public abstract string MenuName { get; }

		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
			ApplicationController.SetMenuCheckedState(MenuName, true);
			HandleDestroyed += WhenHandleDestroyed;
		}

		protected virtual void WhenHandleDestroyed(object sender, EventArgs e)
		{
			ApplicationController.SetMenuCheckedState(MenuName, false);
			ApplicationController.PaneClosed(this);
			HandleDestroyed -= WhenHandleDestroyed;
		}
	}
}
