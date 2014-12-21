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
			AddEmitProtocol("ThumbnailImage");
			AddEmitProtocol("ExceptionMessage");
			MaxSize = 320;
		}

		protected void ProcessImage(dynamic filename)
		{
			string fn = Path.Combine(filename.Path.Text.Value, filename.Name.Text.Value + filename.FileExtension.Text.Value);
			Bitmap bitmap = new Bitmap(fn);
			// Resize image to the specified max width, proportionally scaling the image.
			Bitmap image = new Bitmap(bitmap, MaxSize, MaxSize * bitmap.Height / bitmap.Width);
			image.Tag = fn;
			bitmap.Dispose();
			OutputImage(fn, image);
		}

		protected void ProcessInMemoryImage(Image bitmap)
		{
			Bitmap thumbnailImage = new Bitmap(bitmap, MaxSize, MaxSize * bitmap.Height / bitmap.Width);
			// TODO: These are not necessarily PNG's!
			OutputImage(Path.GetRandomFileName().LeftOf('.') + ".jpg", thumbnailImage);
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
		protected void OutputImage(string fn, Bitmap image)
		{
			CreateCarrier("ThumbnailImage", signal =>
				{
					signal.ImageFilename.Filename.Path.Text.Value = Path.GetDirectoryName(fn);
					signal.ImageFilename.Filename.Name.Text.Value = Path.GetFileNameWithoutExtension(fn);
					signal.ImageFilename.Filename.FileExtension.Text.Value = Path.GetExtension(fn);
					signal.Image.Value = image;
				});
		}
	}
}
