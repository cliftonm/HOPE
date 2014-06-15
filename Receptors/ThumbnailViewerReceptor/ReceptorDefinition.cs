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
			AddReceiveProtocol("ThumbnailImage");
			AddEmitProtocol("SystemShowImage");
			AddEmitProtocol("ViewImage");
			AddEmitProtocol("GetImageMetadata");
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			Image image = carrier.Signal.Image;
			ShowImage(image);
		}

		protected void ShowImage(Image image)
		{
			// All we do here is send a message to the system so that it displays our thumbnails as a carousel around or receptor.
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("SystemShowImage");
			dynamic signal = rsys.SemanticTypeSystem.Create("SystemShowImage");
			signal.From = this;
			signal.Image = image;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
