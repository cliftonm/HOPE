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
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Image Viewer"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;

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
			return new string[] { "ViewImage" };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { };
		}

		public void ProcessCarrier(ICarrier carrier)
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
