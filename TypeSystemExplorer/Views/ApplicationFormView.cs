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
	public delegate void RightClickDlgt(int x, int y);

	public class ApplicationFormView : Form, IMycroParserInstantiatedObject
	{
		/// <summary>
		/// Assigned by the mainform.xml instantiation: DockPanel="{dockPanel}"
		/// </summary>
		public DockPanel DockPanel { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		// Cannot be named ApplicationModel because this otherwise becomes a property of the view 
		// rather than a distinct instantiation of the instance in mainform.xml
		public ApplicationModel Model { get; protected set; }
		public Dictionary<string, object> ObjectCollection { get; set; }
		public StatusBarPanel BrowserStatus { get; protected set; }
		public bool ShowProtocols { get; set; }

		public ApplicationFormView()
		{
			// Icon = Intertexti.Properties.Resources.intertexti;
		}

		public void CloseAll()
		{
			// ToArray, so we get a copy rather than iterating through the original list, which is being modified.
			DockPanel.Contents.ToArray().ForEach(t => t.DockHandler.Close());
		}

		public void CloseDocuments()
		{
			// This works too.
			// DockPanel.DocumentsToArray().ForEach(t => t.DockHandler.Close());
			// ToArray, so we get a copy rather than iterating through the original list, which is being modified.
			// DockPanel.Contents.Where(t => t is GenericDocument).ToArray().ForEach(t => ((IDockContent)t).DockHandler.Close());
		}

		public void SetMenuCheckState(string menuName, bool checkedState)
		{
			((ToolStripMenuItem)ObjectCollection[menuName]).Checked = checkedState;
		}

		public void SetMenuEnabledState(string menuName, bool enabledState)
		{
			((ToolStripMenuItem)ObjectCollection[menuName]).Enabled = enabledState;
		}

		public void SetCaption(string text)
		{
			//if (!String.IsNullOrEmpty(text))
			//{
			//	string filename = Path.GetFileNameWithoutExtension(text);
			//	string path = Path.GetDirectoryName(text);

			//	if (String.IsNullOrEmpty(path))
			//	{
			//		// Use the application path if we don't have a formal path defined.
			//		path = Path.GetDirectoryName(Path.GetFullPath(text));
			//	}

			//	Text = /*"Intertexti - " +*/ filename + " (" + path + ")";
			//}
			//else
			//{
			//	Text = "- new"; // Intertexti - new";
			//}

			//WhenModelIsDirty(Model.IsDirty);

			Text = text;
		}

		protected void WhenModelIsDirty(bool isDirty)
		{
			string title = Text.LeftOfRightmostOf(" *");

			if (isDirty)
			{
				title = title + " *";
			}

			Text = title;
		}
	}
}
