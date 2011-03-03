using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;
using Magic.Common;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public class PolygonRenderer : IRender
	{
		object polygonListLock = new object();
		Color c;
		public PolygonRenderer()
		{
			Random r = new Random();
			c = Color.FromArgb((int)(r.NextDouble() * 255.0), (int)(r.NextDouble() * 255.0), (int)(r.NextDouble() * 255.0));
		}

		public PolygonRenderer(Color c)
		{
			this.c = c;
		}

		#region IRender Members
		List<Polygon> polygonsToDraw = new List<Polygon>();
		public string GetName()
		{
			return "Polygon";
		}


		public void UpdatePolygons(List<Polygon> newPolygons)
		{
			lock (polygonListLock)
			{
				polygonsToDraw = new List<Polygon>(newPolygons.Count);
				foreach (Polygon p in newPolygons)
				{
					//we can shallow copy here im pretty sure...
					polygonsToDraw.Add(p);
				}
			}

		}
		public void Draw(Renderer cam)
		{
			lock (polygonListLock)
			{
				foreach (Polygon eachPolygon in polygonsToDraw)
				{
					//eachPolygon.Add(eachPolygon[0]);
					GLUtility.DrawLineLoop(new GLPen(c, 1.0f), eachPolygon.ToArray());
					//GLUtility.DrawLines(new GLPen(c[i++ & 100], 1.0f), eachPolygon.ToArray());
					//int j = 0;
					//foreach (Vector2 v in eachPolygon.ToArray())
					//{
					//	GLUtility.DrawString(i + ":" + j, Color.Black, v.ToPointF());
					//	j++;
					//}
					//i++;
					//foreach (Vector2 eachVector in eachPolygon)
					//  GLUtility.DrawCross(new GLPen(Color.Blue, 1.0f), eachVector, 1.0f);
				}
			}
		}

		public void ClearPolygons()
		{
			lock (polygonListLock)
				polygonsToDraw.Clear();
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
