/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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
	public class ImageViewer : WindowedBaseReceptor
	{
		public override string Name { get { return "Image Viewer"; } }

		[MycroParserInitialize("pb")]
		protected PictureBox pb;
		protected string url = null;

		public ImageViewer(IReceptorSystem rsys) : base("ImageViewer.xml", true, rsys)
		{
/*
			AddReceiveProtocol("ImageFilename", (Action<dynamic>)(signal =>
				{
					form.IfNull(() => InitializeViewer());
					// TODO: As remarked in VisualizerController, we need computed types that can perform this kind of function for us when we use a getter like "FullyQualifiedFilename"
					Image img = Image.FromFile(Path.Combine(signal.Filename.Path.Value, signal.Filename.Name.Value + signal.Filename.FileExtension.Value));
					MakeWindowFitImage(img);
					pb.Image = img;
				}));
*/
			AddEmitProtocol("Url");

			AddReceiveProtocol("Image", (Action<dynamic>)(signal =>
				{
					form.IfNull(() =>
						{
							InitializeUI();
							form.FormClosing += WhenFormClosing;
							form.SizeChanged += ProportionalResize;
						});
					
					pb.Image = signal.Value;

					string title = signal.Title.Text.Value;

					if (!String.IsNullOrEmpty(title))
					{
						WindowName = title;
						UpdateCaption();
					}
				}));
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			base.ProcessCarrier(carrier);

			//if (carrier.ParentCarrier != null)
			{
				// Additional behavior if this is a web image
				// if (carrier.ParentCarrier.Protocol.DeclTypeName == "WebImage")
				if (carrier.ProtocolPath == "WebImage")
				{
					url = carrier.Signal.Url.Value;

					if (!String.IsNullOrEmpty(url))
					{
						AddEmitProtocol("Url");
					}
					else
					{
						RemoveEmitProtocol("Url");
					}
				}
			}
		}

		// Wire up the double-click handler.
		protected override void InitializeUI()
		{
			base.InitializeUI();
			pb.DoubleClick += OnDoubleClick;
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

		// TODO: Need a flag for making the window fit the image, or the image fit the window.
		protected void ProportionalResize(object sender, EventArgs args)
		{
			MakeWindowFitImage(pb.Image);
		}

		protected void MakeWindowFitImage(Image img)
		{
			// Give the form's current width, what does the height need to be to maintain aspect ratio?
			form.ClientSize = new Size(form.ClientSize.Width, (int)(form.ClientSize.Width * (double)img.Height / (double)img.Width));
		}

		protected void OnDoubleClick(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(url))
			{
				CreateCarrier("Url", signal => signal.Value = url);
			}
		}
	}
}
