using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace ImageViewerReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Image Viewer"; } }

		protected Form form;
		protected PictureBox pb;

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("ImageFilename", (Action<dynamic>)(signal =>
				{
					form.IfNull(() => InitializeViewer());
					// TODO: As remarked in VisualizerController, we need computed types that can perform this kind of function for us when we use a getter like "FullyQualifiedFilename"
					Image img = Image.FromFile(Path.Combine(signal.Filename.Path.Value, signal.Filename.Name.Value + signal.Filename.FileExtension.Value));
					MakeWindowFitImage(img);
					pb.Image = img;
				}));
		}

		protected void InitializeViewer()
		{
			Tuple<Form, MycroParser> ret = InitializeViewer("ImageViewer.xml");
			form = ret.Item1;
			pb = (PictureBox)ret.Item2.ObjectCollection["pb"];
			form.Show();
			form.FormClosing += WhenFormClosing;
			form.SizeChanged += ProportionalResize;
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

		protected void WhenFormClosing(object sender, FormClosingEventArgs e)
		{
			// Will need to create a new form when new text arrives.
			form = null;
			pb = null;
			e.Cancel = false;
		}

		protected void ProportionalResize(object sender, EventArgs args)
		{
			MakeWindowFitImage(pb.Image);
		}

		protected void MakeWindowFitImage(Image img)
		{
			// Give the form's current width, what does the height need to be to maintain aspect ratio?
			form.ClientSize = new Size(form.ClientSize.Width, (int)(form.ClientSize.Width * (double)img.Height / (double)img.Width));
		}
	}
}
