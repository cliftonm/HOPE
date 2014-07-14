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
	public class Exceptions : BaseReceptor
	{
		public override string Name { get { return "Exceptions"; } }
		protected TextBox tb;
		protected Form form;

		public Exceptions(IReceptorSystem rsys)
			: base(rsys)
		{
			AddReceiveProtocol("Exception",
				// cast is required to resolve Func vs. Action in parameter list.
				(Action<dynamic>)(signal => ShowException(signal)));

			InitializeUI();
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

		protected void InitializeUI()
		{
			form = new Form();
			form.Text = "Exceptions";
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

		/// <summary>
		/// Remove ourselves.
		/// </summary>
		protected void WhenFormClosing(object sender, FormClosingEventArgs e)
		{
			tb = null;
			e.Cancel = false;
			rsys.Remove(this);
		}

		protected void ShowException(dynamic signal)
		{
			string rname = signal.ReceptorName;
			string msg = signal.Message;
			tb.AppendText(rname);
			tb.AppendText(": ");
			tb.AppendText(msg);
			tb.AppendText("\r\n");
		}
	}
}
