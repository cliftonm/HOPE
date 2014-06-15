using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;
using Clifton.Threading;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace ThumbnailCreatorReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Thumbnail Converter"; } }
		
		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			this.rsys = rsys;
			AddReceiveProtocol("ImageFilename");
			AddEmitProtocol("DebugMessage");
			AddEmitProtocol("ThumbnailImage");
		}

		protected void ProcessImage(object state)
		{
			string fn = (string)state;

			Bitmap bitmap = new Bitmap(fn);
			// Reduce the size of the image.  If we don't do this, scrolling and rendering of scaled images is horrifically slow.
			Image image = new Bitmap(bitmap, 1024, 1024 * bitmap.Height / bitmap.Width);
			image.Tag = fn;
			bitmap.Dispose();

			((Control)Application.OpenForms[0]).BeginInvoke(() => OutputImage(fn, image));
			Thread.Sleep(100);
		}

		public override async void ProcessCarrier(ICarrier carrier)
		{
			if (carrier.Signal.Filename != null)
			{
				string fn = carrier.Signal.Filename;
				// Only process if the file exists.
				if (File.Exists(fn))
				{
//					ManagedThreadPool.QueueUserWorkItem(new WaitCallback(ProcessImage), fn);

					Image ret = await Task.Run<Image>(() =>
					{
						Bitmap bitmap = new Bitmap(fn);
						// Reduce the size of the image.  If we don't do this, scrolling and rendering of scaled images is horrifically slow.
						Image image = new Bitmap(bitmap, 1024, 1024 * bitmap.Height / bitmap.Width);
						image.Tag = fn;
						bitmap.Dispose();

						return image;
					});

					OutputImage(fn, ret);

					// This is fast enough we don't need to run this as a separate thread unless these files are perhaps coming from a slow network.
					// - no, we can leverage multiple cores when we're processing a whole swarm of images 
					/*
					Bitmap bitmap = new Bitmap(fn);
					// Reduce the size of the image.  If we don't do this, scrolling and rendering of scaled images is horrifically slow.
					Image image = new Bitmap(bitmap, 256, 256 * bitmap.Height / bitmap.Width);
					image.Tag = fn;
					bitmap.Dispose();
					OutputImage(fn, image);
					*/
				}
				else
				{
					FileMissing(fn);
				}
			}
			else
			{
				NoFilenameProvided();
			}
		}

		protected void FileMissing(string fn)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DebugMessage");
			dynamic signal = rsys.SemanticTypeSystem.Create("DebugMessage");
			signal.Message = "Thumbnail Converter: The image file "+fn+" is missing!";
			rsys.CreateCarrier(this, protocol, signal);
		}

		protected void NoFilenameProvided()
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DebugMessage");
			dynamic signal = rsys.SemanticTypeSystem.Create("DebugMessage");
			signal.Message = "Thumbnail Converter: No image filename was provided.";
			rsys.CreateCarrier(this, protocol, signal);
		}

		protected void OutputImage(string filename, Image image)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ThumbnailImage");
			dynamic signal = rsys.SemanticTypeSystem.Create("ThumbnailImage");
			// Insert "-thumbnail" into the filename.
			signal.ImageFilename.Filename = filename.LeftOfRightmostOf('.') + "-thumbnail." + filename.RightOfRightmostOf('.');
			image.Tag = signal.ImageFilename.Filename;
			signal.Image = image;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
