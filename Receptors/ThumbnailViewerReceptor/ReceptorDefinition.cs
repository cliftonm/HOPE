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
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Thumbnail Viewer"; } }
		public bool IsEdgeReceptor { get { return true; } }
		public bool IsHidden { get { return false; } }

		public IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "ThumbnailImage" };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { "SystemShowImage", "ViewImage", "GetImageMetadata" };
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public void ProcessCarrier(ICarrier carrier)
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
