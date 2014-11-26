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
