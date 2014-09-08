using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
			InitializeViewer();
			AddReceiveProtocol("Text", (Action<dynamic>)(signal =>
				{
					string text = signal.Value;

					if (form == null)
					{
						// again!
						InitializeViewer();
					}

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
				form.Close();
			}
			catch
			{
			}
		}

		protected void InitializeViewer()
		{
			MycroParser mp = new MycroParser();
			XmlDocument doc = new XmlDocument();
			doc.Load("TextViewer.xml");
			mp.Load(doc, "Form", null);
			form = (Form)mp.Process();
			tb = (TextBox)mp.ObjectCollection["tbText"];
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
