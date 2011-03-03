using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common.DataTypes;
using Magic.Common.Shapes;
using Magic.OccupancyGrid;
using Magic.Common;
using System.Diagnostics;

namespace Magic.Poly2OcGrid
{
	public class Poly2OcGrid
	{
		IOccupancyGrid2D og;
		List<Polygon> obstacles;
		public List<Polygon> pixels;

		double resX;
		double resY;
		double extentX;
		double extentY;

		public Poly2OcGrid(double resX, double resY, double extentX, double extentY)
		{
			this.resX = resX;
			this.resY = resY;
			this.extentX = extentX;
			this.extentY = extentY;

			og = new OccupancyGrid2D(resolutionX, resolutionY, extentX, extentY);
			obstacles = new List<Polygon>();
		}

		public void UpdateOcGrid(List<Polygon> obstacles)
		{
			Bitmap bmp = new Bitmap((int) Math.Round(2 * DEFAULT_EXTENT_X / DEFAULT_RESOLUTION_X), (int) Math.Round(2 * DEFAULT_EXTENT_Y / DEFAULT_RESOLUTION_Y));
			Graphics canvas = Graphics.FromImage(bmp);
			Pen pen = new Pen(Color.Blue, 1);
			PointF[] points;

			Stopwatch sw = new Stopwatch();
			sw.Start();
			foreach (Polygon p in obstacles)
			{
				points = new PointF[p.points.Count];

				for (int i = 0; i < p.points.Count; i++)
					points[i] = new PointF((float) (p.points.ElementAt(i).X / DEFAULT_RESOLUTION_X + DEFAULT_EXTENT_X / DEFAULT_RESOLUTION_X),
						(float) (p.points.ElementAt(i).Y / DEFAULT_RESOLUTION_Y + DEFAULT_EXTENT_Y / DEFAULT_RESOLUTION_Y));

				canvas.DrawPolygon(pen, points);
			}
			sw.Stop();
			Console.WriteLine("Poly time: " + sw.ElapsedMilliseconds);

			og.SetCells(bmp);		}

		public void UpdatePixels()
		{
			pixels = new List<Polygon>();

			for (int i = 0; i < og.Width; i++)
				for (int j = 0; j < og.Height; j++)
					if (og.GetCellByIdx(i, j) != 0)
						DrawPixel((i - 200) * resolutionX, (j - 200) * resolutionY);

		}

		private void DrawPixel(double x, double y)
		{
			Polygon p;
			List<Vector2> points = new List<Vector2>();

			points.Add(new Vector2(x, y));
			points.Add(new Vector2(x + resolutionX, y));
			points.Add(new Vector2(x + resolutionX, y + resolutionY));
			points.Add(new Vector2(x, y + resolutionY));

			p = new Polygon(points);
			pixels.Add(p);
		}
	}
}
