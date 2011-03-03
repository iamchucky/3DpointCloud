using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Path;
using System.Drawing;
using Magic.Common;
using Magic.Common.Splines;

namespace Magic.Rendering.Renderables
{
	public class BezierPathRenderer : IRender
	{
		IPath currentPath;

		public BezierPathRenderer()
		{

		}

		public void SetPath(IPath p)
		{
			currentPath = p;
		}

		#region IRender Members

		public string GetName()
		{
			throw new NotImplementedException();
		}

		public void Draw(Renderer r)
		{
			if (currentPath == null) return;

			CubicBezier bezier;

			foreach (IPathSegment seg in currentPath)
			{
				if (!(seg is BezierPathSegment)) return;

				bezier = ((BezierPathSegment) seg).Bezier;

				GLUtility.DrawCircle(new GLPen(Color.Green, 1.0f), seg.Start.ToPointF(), 0.25f);
				GLUtility.DrawCircle(new GLPen(Color.Green, 1.0f), seg.End.ToPointF(), 0.25f);
				GLUtility.DrawBezier(new GLPen(Color.Green, 1.0f), bezier.P0, bezier.P1, bezier.P2, bezier.P3);
			}
		}

		public void ClearBuffer()
		{

		}

		public bool VehicleRelative
		{
			get { return false; }
		}

		public int? VehicleRelativeID
		{
			get { return null; }
		}

		#endregion
	}
}
