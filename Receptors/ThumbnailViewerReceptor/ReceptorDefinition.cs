using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace ThumbnailViewerReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Thumbnail Viewer"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		
		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("ThumbnailImage", (Action<dynamic>)(signal => ShowImage(signal.Image.Value, signal.ImageFilename.Filename)));
			AddEmitProtocol("SystemShowImage");
			AddEmitProtocol("ImageFilename", false);			// Currently, this is emitted by the visualizer.  See TODO below.
			// AddEmitProtocol("GetImageMetadata", false);
		}

		// TODO: We should be able to implement this in the receptor itself given the proper interface and callback method, in which the visualizer
		// has instruct the receptor to implement its specific rendering.  We would also need to pass along mouse/keyboard events as well.
		/// <summary>
		/// This is really nothing more than a pass-through to the visualizer to show images associated with this receptor in 
		/// whatever way the visualizer wants to.
		/// </summary>
		protected void ShowImage(Bitmap image, dynamic filename)
		{
			CreateCarrier("SystemShowImage", signal =>
				{
					signal.From = this;
					signal.Image.Value = image;
					signal.Filename = filename;
				});
		}
	}
}
