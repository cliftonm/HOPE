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
	public class ThumbnailCreator : BaseReceptor
	{
		public override string Name { get { return "Thumbnail Creator"; } }
		public override string ConfigurationUI { get { return "ThumbnailConverterConfig.xml"; } }

		[UserConfigurableProperty("Max Size:")]
		public int MaxSize { get; set; }
																								
		public ThumbnailCreator(IReceptorSystem rsys) : base(rsys)
		{
			this.rsys = rsys;
			AddReceiveProtocol("ImageFilename", (Action<dynamic>)(signal => ProcessImage(signal.Filename)));
			AddReceiveProtocol("Image", (Action<dynamic>)(signal => ProcessInMemoryImage(signal.Value)));
			AddEmitProtocol("ThumbnailImage", false);
			MaxSize = 320;
		}

		protected void ProcessImage(dynamic filename)
		{
			string fn = Path.Combine(filename.Path.Value, filename.Name.Value + filename.FileExtension.Value);
			Bitmap bitmap = new Bitmap(fn);
			// Resize image to the specified max width, proportionally scaling the image.
			Bitmap image = new Bitmap(bitmap, MaxSize, MaxSize * bitmap.Height / bitmap.Width);
			image.Tag = fn;
			bitmap.Dispose();
			OutputImage(filename, image);
		}

		protected void ProcessInMemoryImage(Bitmap bitmap)
		{
			Bitmap thumbnailImage = new Bitmap(bitmap, MaxSize, MaxSize * bitmap.Height / bitmap.Width);
			OutputImage(String.Empty, thumbnailImage);
		}
/*
		public override async void ProcessCarrier(ICarrier carrier)
		{
			if (carrier.Signal.Filename != null)
			{
				string fn = carrier.Signal.Filename;
				// Only process if the file exists.
				if (File.Exists(fn))
				{
//					ManagedThreadPool.QueueUserWorkItem(new WaitCallback(ProcessImage), fn);

					try
					{
						Image ret = await Task.Run<Image>(() =>
						{
							Bitmap bitmap = new Bitmap(fn);
							// Reduce the size of the image.  If we don't do this, scrolling and rendering of scaled images is horrifically slow.
							// Image image = new Bitmap(bitmap, 1024, 1024 * bitmap.Height / bitmap.Width);
							Image image = new Bitmap(bitmap, 1024, 1024 * bitmap.Height / bitmap.Width);
							image.Tag = fn;
							bitmap.Dispose();

							return image;
						});

						OutputImage(fn, ret);
					}
					catch
					{
						// TODO: Some exception occurred with the image, so we'll ignore it for now.
					}

					// This is fast enough we don't need to run this as a separate thread unless these files are perhaps coming from a slow network.
					// - no, we can leverage multiple cores when we're processing a whole swarm of images 
					
					//Bitmap bitmap = new Bitmap(fn);
					//// Reduce the size of the image.  If we don't do this, scrolling and rendering of scaled images is horrifically slow.
					//Image image = new Bitmap(bitmap, 256, 256 * bitmap.Height / bitmap.Width);
					//image.Tag = fn;
					//bitmap.Dispose();
					//OutputImage(fn, image);
					
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
*/
		protected void OutputImage(dynamic inputFilename, Bitmap image)
		{
			CreateCarrier("ThumbnailImage", signal =>
				{
					signal.SourceImageFilename.Filename = inputFilename;
					signal.Image.Value = image;
				});
		}
	}
}
