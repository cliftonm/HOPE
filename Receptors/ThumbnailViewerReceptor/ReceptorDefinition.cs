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
			AddReceiveProtocol("ThumbnailImage", (Action<dynamic>)(signal => ShowImage(signal.Image.Value, signal.SourceImageFilename.Filename)));
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
