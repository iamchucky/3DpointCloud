using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public class PointsRenderer : IRender
	{
		List<Vector2> points;
		private object drawLock = new object();
		Color color;
		float size;

		public PointsRenderer(Color color, float size)
		{
			this.color = color;
			this.size = size;
		}
		

		public void UpdatePoints(List<Vector2> points)
		{
			lock (this.drawLock)
			{
				this.points = points;
			}
		}

		public void UpdatePoints(Vector2 point)
		{
			lock (this.drawLock)
			{
				List<Vector2> list = new List<Vector2>(); list.Add(point);
				this.points = list;
			}
		}

		#region IRender Members

		public string GetName()
		{
			return "I want this/these point(s)!";
		}

		public void Draw(Renderer cam)
		{
			if (points == null) return;
			List<Vector2> copy = new List<Vector2>(points);
			lock (this.drawLock)
			{
				foreach (Vector2 v in copy)
				{
					GLUtility.DrawCross(new GLPen(color, 1.0f), v, size);
				}
			}
		}

		public void ClearBuffer()
		{
			this.points.Clear();
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
