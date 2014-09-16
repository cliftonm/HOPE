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

		[UserConfigurableProperty("X")]
		public int WindowX { get; set; }

		[UserConfigurableProperty("Y")]
		public int WindowY { get; set; }

		[UserConfigurableProperty("W")]
		public int WindowWidth { get; set; }

		[UserConfigurableProperty("H")]
		public int WindowHeight { get; set; }

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("Text", (Action<dynamic>)(signal =>
				{
					form.IfNull(() =>
						{
							InitializeViewer();
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

		public override void EndSystemInit()
		{
			base.EndSystemInit();
		}

		// TODO: This stuff on window location and size changing and setting needs to be moved
		// to a common lib that a receptor instance project can easily just wire in, as this
		// is going to be common behavior for receptors with UI's.  Gawd, sometimes I really 
		// wish C# supported multiple inheritence.
		protected void OnLocationChanged(object sender, EventArgs e)
		{
			WindowX = form.Location.X;
			WindowY = form.Location.Y;
		}

		protected void OnSizeChanged(object sender, EventArgs e)
		{
			WindowWidth = form.Size.Width;
			WindowHeight = form.Size.Height;
		}

		protected void UpdateFormLocationAndSize()
		{
			// Only update if user has changed the size from its declarative value.
			if (WindowX != 0 && WindowY != 0)
			{
				form.Location = new Point(WindowX, WindowY);
			}

			// Only update if user has changed the size from its declarative value.
			if (WindowWidth != 0 && WindowHeight != 0)
			{
				form.Size = new Size(WindowWidth, WindowHeight);
			}
		}

		protected void InitializeViewer()
		{
			Tuple<Form, MycroParser> ret = InitializeViewer("TextViewer.xml");
			form = ret.Item1;
			tb = (TextBox)ret.Item2.ObjectCollection["tbText"];
			form.Show();
			form.FormClosing += WhenFormClosing;
			form.LocationChanged += OnLocationChanged;
			form.SizeChanged += OnSizeChanged;
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
