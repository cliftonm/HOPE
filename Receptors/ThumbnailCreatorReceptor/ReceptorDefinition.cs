using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Tools.Strings.Extensions;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace ThumbnailCreatorReceptor
{
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Thumbnail Converter"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "ImageFilename" };
		}

		public void Terminate()
		{
		}

		public async void ProcessCarrier(ISemanticTypeStruct protocol, dynamic signal)
		{
			string fn = signal.Filename;
			Image ret = await Task.Run<Image>(() =>
				{
					Bitmap bitmap = new Bitmap(fn);
					// Reduce the size of the image.  If we don't do this, scrolling and rendering of scaled images is horrifically slow.
					Image image = new Bitmap(bitmap, 256, 256 * bitmap.Height / bitmap.Width);
					bitmap.Dispose();

					return image;
				});

			OutputImage(fn, ret);
		}

		protected void OutputImage(string filename, Image image)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("ThumbnailImage");
			dynamic signal = rsys.SemanticTypeSystem.Create("ThumbnailImage");
			// Insert "-thumbnail" into the filename.
			signal.Filename = filename.LeftOfRightmostOf('.') + "-thumbnail." + filename.RightOfRightmostOf('.');
			signal.Image = image;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
