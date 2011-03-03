using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
//using UrbanChallenge.Common;

namespace Magic.Rendering
{
	static class Utility {
		public static RectangleF CalcBoundingBox(params PointF[] pts) {
			float minx = float.MaxValue, miny = float.MaxValue;
			float maxx = float.MinValue, maxy = float.MinValue;

			for (int i = 0; i < pts.Length; i++) {
				if (pts[i].X < minx)
					minx = pts[i].X;

				if (pts[i].X > maxx)
					maxx = pts[i].X;

				if (pts[i].Y < miny)
					miny = pts[i].Y;

				if (pts[i].Y > maxy)
					maxy = pts[i].Y;
			}

			return new RectangleF(minx, miny, maxx - minx, maxy - miny);
		}

	}
}
