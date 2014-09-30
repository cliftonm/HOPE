using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.MycroParser;

namespace Clifton.Receptor.Interfaces
{
	public abstract class WindowedBaseReceptor : BaseReceptor
	{
		[UserConfigurableProperty("WindowName")]
		public string WindowName { get; set; }

		[UserConfigurableProperty("X")]
		public int WindowX { get; set; }

		[UserConfigurableProperty("Y")]
		public int WindowY { get; set; }

		[UserConfigurableProperty("W")]
		public int WindowWidth { get; set; }

		[UserConfigurableProperty("H")]
		public int WindowHeight { get; set; }

		protected Clifton.MycroParser.MycroParser mycroParser;
		protected Form form;
		protected string displayFormFilename;
		protected bool showOnStartup;

		public WindowedBaseReceptor(string displayFormFilename, bool showOnStartup, IReceptorSystem rsys)
			: base(rsys)
		{
			this.displayFormFilename = displayFormFilename;
			this.showOnStartup = showOnStartup;
		}

		public override void Initialize()
		{
			base.Initialize();

			if (showOnStartup)
			{
				InitializeUI();
			}
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (showOnStartup)
			{
				UpdateFormLocationAndSize();
				UpdateCaption();
			}
		}

		protected virtual void InitializeUI()
		{
			mycroParser = new Clifton.MycroParser.MycroParser();
			form = mycroParser.Load<Form>(displayFormFilename, this);
			form.Show();

			// Wire up the location changed event after the form has initialized,
			// so we don't generate this event during form creation.  That way,
			// the user's config will be preserved and used when the system
			// finishes initialization.
			form.LocationChanged += OnLocationChanged;
			form.SizeChanged += OnSizeChanged;
			form.FormClosing += OnFormClosing;
		}

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

		protected void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			form = null;
		}

		protected virtual void UpdateFormLocationAndSize()
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

		protected virtual void UpdateCaption()
		{
			if (!String.IsNullOrEmpty(WindowName))
			{
				form.Text = WindowName;
			}
		}

		protected virtual void ReinitializeUI()
		{
			InitializeUI();
			UpdateFormLocationAndSize();
			UpdateCaption();
		}
	}
}
