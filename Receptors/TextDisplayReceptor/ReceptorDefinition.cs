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
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Text Display"; } }
		protected TextBox tb;
		protected Form form;

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("Text", (Action<dynamic>)(signal =>
				{
					form.IfNull(()=>InitializeViewer());
					string text = signal.Value;

					if (!String.IsNullOrEmpty(text))
					{
						tb.AppendText(text.StripHtml());
						tb.AppendText("\r\n");
					}
				}));
		}

		public override void Terminate()
		{
			try
			{
				form.IfNotNull(f => f.Close());
			}
			catch
			{
			}
		}

		protected void InitializeViewer()
		{
			Tuple<Form, MycroParser> ret = InitializeViewer("TextViewer.xml");
			form = ret.Item1;
			tb = (TextBox)ret.Item2.ObjectCollection["tbText"];
			form.Show();
			form.FormClosing += WhenFormClosing;
		}

		protected void WhenFormClosing(object sender, FormClosingEventArgs e)
		{
			// Will need to create a new form when new text arrives.
			form = null;
			tb = null;
			e.Cancel = false;
		}
	}
}
