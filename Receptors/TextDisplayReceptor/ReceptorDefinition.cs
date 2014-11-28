using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace TextDisplayReceptor
{
	public class ReceptorDefinition : WindowedBaseReceptor
	{
		public override string Name { get { return "Text Display"; } }

		[MycroParserInitialize("tbText")]
		protected TextBox tb;

		public ReceptorDefinition(IReceptorSystem rsys) : base("TextViewer.xml", true, rsys)
		{
			AddReceiveProtocol("Text", (Action<dynamic>)(signal =>
				{
					form.IfNull(() =>
						{
							InitializeUI();
							UpdateFormLocationAndSize();
						});

					string text = signal.Value;

					if (!String.IsNullOrEmpty(text))
					{
						tb.AppendText(text.StripHtml());
						tb.AppendText("\r\n");
					}
				}));
		}
	}
}
