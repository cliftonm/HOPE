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
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Text Display"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;
		protected TextBox tb;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "Text", "TextToSpeech" };
		}

		public void ProcessCarrier(ICarrier carrier)
		{
			// Create the textbox if it doesn't exist.
			if (tb == null)
			{
				Form form = new Form();
				form.Text = "Text Output";
				form.Location = new Point(100, 100);
				form.Size = new Size(400, 400);
				form.TopMost = true;
				tb = new TextBox();
				tb.Multiline = true;
				tb.WordWrap = true;
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
				tb.Text = tb.Text + text.StripHtml();
				tb.Text = tb.Text + "\r\n";
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
