using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TypeSystemExplorer.Controllers;

namespace TypeSystemExplorer.Actions
{
	public class SetDocumentText : DeclarativeAction
	{
		public ITextController Controller { get; set; }
		public string Text { get; set; }

		public override void EndInit()
		{
			Controller.SetDocumentText(Text);
		}
	}
}
