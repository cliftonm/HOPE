using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace ImageWriterReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Image Writer"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			// Other protocols might be added in the future.
			AddReceiveProtocol("ThumbnailImage", (Action<dynamic>)(signal => SaveImage(signal.Image.Value, signal.SourceImageFilename.Filename)));
		}

		protected void SaveImage(Bitmap image, dynamic filename)
		{
			// TODO: The addition "-thumbnail" extension should be configurable.
			string fn = Path.Combine(filename.Path.Value, filename.Name.Value + "-thumbnail" + filename.FileExtension.Value);

			if (!File.Exists(fn))
			{
				image.Save(fn);
			}
		}
	}
}
