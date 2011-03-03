using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Path;
using System.Drawing;
using Magic.Common;
using Magic.Common.Splines;
using System.IO;

namespace Magic.Rendering.Renderables
{
	[Obsolete]
    public class PathFromRobotRenderer : IRender
    {
        IPath currentPath;
        RobotRenderer renderer;
        Color color;
        float nodeWidth = .1f;
		bool draw = true;

        public PathFromRobotRenderer(RobotRenderer rr, Color c, float nodeWidth)
            : this(rr, c)
        {
            this.nodeWidth = nodeWidth;
        }

        public PathFromRobotRenderer(RobotRenderer rr, Color c)
        {
            renderer = rr;
            color = c;
        }

        public PathFromRobotRenderer(RobotRenderer rr)
        {
            renderer = rr;
            color = Color.Salmon;
        }

        public PathFromRobotRenderer()
        {
            color = Color.Salmon;
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
            if (renderer != null)
            {
                //if (renderer.GetBoundingPolygon().Count > 0)
                // Why do we need this?
                //GLUtility.DrawLine(new GLPen(color, 1.0f), renderer.GetBoundingPolygon().Center.ToPointF(), currentPath[0].Start.ToPointF());
            }

			if (draw == true)
			{
				foreach (IPathSegment seg in currentPath)
				{
					if (seg is BezierPathSegment)
					{
						CubicBezier bezier = ((BezierPathSegment)seg).Bezier;

						GLUtility.DrawCircle(new GLPen(color, 1.0f), seg.Start.ToPointF(), 0.25f);
						GLUtility.DrawCircle(new GLPen(color, 1.0f), seg.End.ToPointF(), 0.25f);
						GLUtility.DrawBezier(new GLPen(color, 1.0f), bezier.P0, bezier.P1, bezier.P2, bezier.P3);
						GLUtility.DrawCircle(new GLPen(Color.Pink, 1.0f), bezier.P0.ToPointF(), nodeWidth);
						GLUtility.DrawCircle(new GLPen(Color.Pink, 1.0f), bezier.P1.ToPointF(), nodeWidth);
						GLUtility.DrawCircle(new GLPen(Color.Pink, 1.0f), bezier.P2.ToPointF(), nodeWidth);
						GLUtility.DrawCircle(new GLPen(Color.Pink, 1.0f), bezier.P3.ToPointF(), nodeWidth);
					}
					else
					{
						GLUtility.DrawCircle(new GLPen(color, 1.0f), seg.Start.ToPointF(), nodeWidth);
						GLUtility.DrawCircle(new GLPen(color, 1.0f), seg.End.ToPointF(), nodeWidth);
						GLUtility.DrawLine(new GLPen(color, 1.0f), (float)seg.Start.X, (float)seg.Start.Y, (float)seg.End.X, (float)seg.End.Y);
					}
				}
			}

            if (currentPath.Count != 0 && renderer != null)
            {
                IPathSegment ips = currentPath.ElementAt<IPathSegment>(currentPath.Count / 2);
                /*GLUtility.DrawString(renderer.GetName(), Color.Black, new PointF((ips.End.ToPointF().X + ips.Start.ToPointF().X) / 2,
                        (ips.End.ToPointF().Y + ips.Start.ToPointF().Y) / 2));//*/
            }

        }

        public void ClearBuffer()
        {

        }

        public bool VehicleRelative
        {
            get { return false; }
        }

		public bool DrawBool
		{
			get { return draw; }
			set { draw = value; }
		}

        public int? VehicleRelativeID
        {
            get { return null; }
        }

        #endregion
    }
}
