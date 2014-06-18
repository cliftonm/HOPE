using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
			AddReceiveProtocol("Text");
			AddReceiveProtocol("TextToSpeech");
		}

		public override void Terminate()
		{
			form.Close();
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			// Create the textbox if it doesn't exist.
			if (tb == null)
			{
				form = new Form();
				form.Text = "Text Output";
				form.Location = new Point(100, 100);
				form.Size = new Size(400, 400);
				form.StartPosition = FormStartPosition.Manual;
				form.TopMost = true;
				tb = new TextBox();
				tb.Multiline = true;
				tb.WordWrap = true;
				tb.ReadOnly = true;
				tb.ScrollBars = ScrollBars.Vertical;
				form.Controls.Add(tb);
				tb.Dock = DockStyle.Fill;
				form.Show();
				form.FormClosing += WhenFormClosing;
			}

			string text = String.Empty;

			if (carrier.Protocol.DeclTypeName == "Text")
			{
				text = carrier.Signal.Value;
			}
			else
			{
				text = carrier.Signal.Text;
			}

			if (!String.IsNullOrEmpty(text))
			{
				tb.AppendText(text.StripHtml());
				tb.AppendText("\r\n");
			}
		}

		protected void WhenFormClosing(object sender, FormClosingEventArgs e)
		{
			// Will need to create a new form when new text arrives.
			tb = null;
			e.Cancel = false;
		}
	}
}
