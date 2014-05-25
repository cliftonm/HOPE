using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using TypeSystemExplorer.Controls;
using TypeSystemExplorer.Controllers;
using TypeSystemExplorer.Models;

using Clifton.Tools.Data;

namespace TypeSystemExplorer.Views
{
	public class XmlEditorView : UserControl
	{
		public ApplicationModel Model { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		public XmlEditorController Controller { get; protected set; }

		public XmlTextEditorControl Editor { get; set; }

		public void Clear()
		{
			Editor.Document.TextContent = String.Empty;
		}
	}
}
