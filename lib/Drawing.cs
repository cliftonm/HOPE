using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Drawing
{
	public static class DrawingExtensionMethods
	{
		/// <summary>
		/// TODO: should actually be called "subtract".
		/// </summary>
		public static Size Delta(this Point p1, Point p2)
		{
			return new Size(p2.X - p1.X, p2.Y - p1.Y);
		}

		/// <summary>
		/// TODO: should actually be called Delta, which returns the absolute diff.
		/// </summary>
		public static Size AbsDelta(this Point p1, Point p2)
		{
			return new Size(Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
		}
	}
}
