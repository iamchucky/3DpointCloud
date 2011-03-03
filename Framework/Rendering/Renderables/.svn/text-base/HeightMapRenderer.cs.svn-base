using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Rendering;
using Magic.Common;
using System.Drawing;
using Magic.Common.DataTypes;
using Magic.OccupancyGrid;
using Tao.OpenGl;

namespace Magic.Rendering.Renderables
{
	public class HeightMapRenderer : IRender
	{
		private IOccupancyGrid2D grid;

		public HeightMapRenderer() { }

		public HeightMapRenderer(IOccupancyGrid2D ocg)
		{
			grid = ocg;
		}

		public void UpdateOccupancyGrid(IOccupancyGrid2D ocgrid)
		{
			grid = ocgrid;
		}

		private Color ColorFromHeight(float h)
		{
			if (h >= 1.0f) return Color.FromArgb(255, 255, 0, 0);
			if (h <= 0.0f) return Color.FromArgb(255, 0, 0, 255);

			if (h <= .5f)
			{
				float b = (1.0f - h * 2.0f) * 255.0f;
				float g = 255.0f - b;
				float norm = (b + g) / (255.0f * 2.0f);
				b = Math.Min(255.0f, b / norm);
				g = Math.Min(255.0f, g / norm);
				return Color.FromArgb(255, 0, (int)g, (int)b);
			}
			else
			{
				float r = (2.0f * (h - .5f)) * 255.0f;
				float g = 255.0f - r;
				float norm = (r + g) / (255.0f * 2.0f);
				r = Math.Min(255.0f, r / norm);
				g = Math.Min(255.0f, g / norm);
				return Color.FromArgb(255, (int)r, (int)g, 0);
			}
		}

		private void DrawLine(int i, int j)
		{
			double x, y;
			GLPen pen;
			float heightVal = (float)(grid.GetCellByIdx(i, j));
			if (heightVal >= .01)
			{
				grid.GetReals(i, j, out x, out y);
				pen = new GLPen(ColorFromHeight(heightVal), 20f);

				pen.GLApplyPen();
				Gl.glVertex3f((float)x, (float)y, heightVal);
				Gl.glVertex3f((float)x, (float)y, 0);
			}
		}

		private void IterDonutRegion(Point outerBL, Point innerBL, Point innerTR, Point outerTR, int incrAmt)
		{
			int i, j;

			//Left side
			for (i = outerBL.X; i < innerBL.X; i += incrAmt)
				for (j = outerBL.Y; j < outerTR.Y; j += incrAmt)
					DrawLine(i, j);

			//Bottom-middle
			for (i = innerBL.X; i < innerTR.X; i += incrAmt)
				for (j = outerBL.Y; j < innerBL.Y; j += incrAmt)
					DrawLine(i, j);

			//Top-middle
			for (i = innerBL.X; i < innerTR.X; i += incrAmt)
				for (j = innerTR.Y; j < outerTR.Y; j += incrAmt)
					DrawLine(i, j);

			//Right side
			for (i = innerTR.X; i < outerTR.X; i += incrAmt)
				for (j = outerBL.Y; j < outerTR.Y; j += incrAmt)
					DrawLine(i, j);
		}
		private void IterDonutRegion2(Point outerBL, Point innerBL, Point innerTR, Point outerTR, int incrAmt)
		{
			int i, j;
			//Left side
			for (i = outerBL.X; i < innerBL.X; i += incrAmt)
				for (j = outerBL.Y; j < outerTR.Y; j += incrAmt)
					drawRect(i, j);
			//DrawLine(i, j);

			//Bottom-middle
			for (i = innerBL.X; i < innerTR.X; i += incrAmt)
				for (j = outerBL.Y; j < innerBL.Y; j += incrAmt)
					drawRect(i, j);
			//DrawLine(i, j);

			//Top-middle
			for (i = innerBL.X; i < innerTR.X; i += incrAmt)
				for (j = innerTR.Y; j < outerTR.Y; j += incrAmt)
					drawRect(i, j);
			//DrawLine(i, j);

			//Right side
			for (i = innerTR.X; i < outerTR.X; i += incrAmt)
				for (j = outerBL.Y; j < outerTR.Y; j += incrAmt)
					drawRect(i, j);
			//DrawLine(i, j);
		}

		private void drawRect(int i, int j)
		{
			double x1, y1, x2, y2, x3, y3, x4, y4;

			float heightVal = (float)(grid.GetCellByIdx(i, j));
			if (heightVal >= .1)
			{
				grid.GetReals(i, j, out x1, out y1);
				grid.GetReals(i + 1, j, out x2, out y2);
				grid.GetReals(i, j+1, out x3, out y3);
				grid.GetReals(i + 1, j+1, out x4, out y4);
				//pen = new GLPen(ColorFromHeight(heightVal), 1f);

				//pen.GLApplyPen();
				//Gl.glVertex3f((float)x, (float)y, heightVal);
				//Gl.glVertex3f((float)x, (float)y, 0);

				GLUtility.Draw3DRectangle(new GLPen(ColorFromHeight(heightVal), 1f),
					new Vector3(x1, y1, heightVal),
					new Vector3(x2, y2, grid.GetCellByIdx(i + 1, j)),
					new Vector3(x3, y3, grid.GetCellByIdx(i, j + 1)),
					new Vector3(x4, y4, grid.GetCellByIdx(i + 1, j + 1)));
			}
		}

		#region IRender Members

		public string GetName()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Some of these parameters are off
		/// TODO: adjust them for MAGIC
		/// </summary>
		/// <param name="cam"></param>
		public void Draw(Renderer cam)
		{
			if (grid == null) return;

			Vector3 camLoc = cam.CamFree.Location;
			//Gl.glLineWidth(10f);
			//Gl.glBegin(Gl.GL_LINES);

			if (grid.NumCellX > 300)
			{
				int xstart, ystart, xend, yend, numrings = 4;

				grid.GetIndicies(camLoc.X, camLoc.Y, out xstart, out ystart);

				Point lastBL, lastTR,
					currBL = new Point(xstart, ystart),
					currTR = currBL;

				int incrAmt, range;
				for (int i = 1; i <= numrings; i++)
				{
					switch (i)
					{
						case 1:
							incrAmt = 1;
							range = 50;
							break;
						case 2:
							incrAmt = 4;
							range = 120;
							break;
						case 3:
							incrAmt = 7;
							range = 290;
							break;
						case 4:
						default:
							incrAmt = 14;
							range = 800;
							break;
					}

					if (range > grid.NumCellX) break;

					lastBL = currBL;
					lastTR = currTR;

					grid.GetIndicies(camLoc.X - range, camLoc.Y - range, out xstart, out ystart);
					grid.GetIndicies(camLoc.X + range, camLoc.Y + range, out xend, out yend);

					currBL = new Point(xstart, ystart);
					currTR = new Point(xend, yend);

					IterDonutRegion(currBL, lastBL, lastTR, currTR, incrAmt);
					
				}
			}
			else
				IterDonutRegion2(new Point(), new Point(grid.NumCellX / 2, grid.NumCellY / 2), new Point(grid.NumCellX / 2, grid.NumCellY / 2), new Point(grid.NumCellX, grid.NumCellY), 1);
			//Gl.glEnd();

		}

		
		public void ClearBuffer()
		{
			throw new NotImplementedException();
		}

		public bool VehicleRelative
		{
			get { return false; }
		}

		public int? VehicleRelativeID
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
