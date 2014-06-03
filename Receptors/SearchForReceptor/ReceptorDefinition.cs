using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace SearchForReceptor
{
	/// <summary>
	/// Displays an input form for the user to enter a search string.
	/// </summary>
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Search For"; } }
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
			CreateForm();
		}

		public void Terminate()
		{
		}

		public string[] GetReceiveProtocols()
		{
			// Doesn't listen to anything.
			return new string[] { };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { "SearchFor" };
		}

		public void ProcessCarrier(ICarrier carrier)
		{
		}

		protected void CreateForm()
		{
			Form form = new Form();
			form.Text = "Search For:";
			form.Location = new Point(100, 100);
			form.Size = new Size(500, 60);
			form.TopMost = true;
			tb = new TextBox();
			tb.KeyPress += OnKeyPress;
			form.Controls.Add(tb);
			tb.Dock = DockStyle.Fill;
			form.Show();
			form.FormClosed += WhenFormClosed;
		}

		protected void OnKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("SearchFor");
				dynamic signal = rsys.SemanticTypeSystem.Create("SearchFor");
				signal.SearchString = tb.Text;
				rsys.CreateCarrier(this, protocol, signal);
			}
		}

		protected void WhenFormClosed(object sender, FormClosedEventArgs e)
		{
			// Remove ourselves when the form is closed.
			rsys.Remove(this);
		}
	}
}
