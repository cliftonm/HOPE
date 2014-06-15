using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace ImageViewerReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Image Viewer"; } }

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("ViewImage");
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			string fn = carrier.Signal.ImageFilename.Filename;
			Form form = new Form();
			form.Text = fn;
			form.Location = new Point(100, 100);
			form.Size = new Size(400, 400);
			form.TopMost = true;
			PictureBox pb = new PictureBox();
			form.Controls.Add(pb);
			pb.Dock = DockStyle.Fill;
			Image img = Image.FromFile(fn);
			pb.Image = img;
			pb.SizeMode = PictureBoxSizeMode.Zoom;
			form.Show();
		}
	}
}
