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
		public override string Name { get { return "Thumbnail Image Writer"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("ThumbnailImage");
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			string fn = carrier.Signal.ImageFilename.Filename;

			// Only save the file if it doesn't already exists.  
			// In actual usage, with this receptor online, we can write a lot of duplicate thumbnails!
			if (!File.Exists(fn))
			{
				Image img = carrier.Signal.Image;

				// Can't do this because it results in the visualizer throwing an "object is in use elsewhere" exception.
				//Task.Run(() =>
				//	{
				//		lock (img)
				//		{
				//			img.Save(fn);
				//		}
				//	});

				// TODO: Make sure we save the image in the filename specified by the extension.
				img.Save(fn, ImageFormat.Jpeg);
			}
		}
	}
}
