using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Magic.Common.DataTypes;
using Magic.Common;
using Tao.OpenGl;

namespace Magic.Rendering.Renderables
{
	public class OccupancyGridRenderer : IRender
	{
		#region IRender Members

		IOccupancyGrid2D grid;
		float transparency = 1.0f;
		string name;
		float highHeight;
		float lowHeight;
		bool logOdds = false;
		bool newRenderingMethod = false;

		public void Rescale(float minHeight, float maxHeight)
		{
			lowHeight = minHeight;
			highHeight = maxHeight;
		}

		public OccupancyGridRenderer(string name, IOccupancyGrid2D grid, float lowHeight, float highHeight)
			: this(name, grid, lowHeight, highHeight, false, false,2.0f)
		{ }

		public OccupancyGridRenderer(string name, IOccupancyGrid2D grid, float lowHeight, float highHeight, bool logOdds)
			: this(name, grid, lowHeight, highHeight, logOdds, false,2.0f)
		{ }
		float sample = 2;
		public OccupancyGridRenderer(string name, IOccupancyGrid2D grid, float lowHeight, float highHeight, bool logOdds, bool newRenderingMethod, float sample)
		{
			this.newRenderingMethod = newRenderingMethod;
			this.logOdds = logOdds;
			this.name = name;
			this.lowHeight = lowHeight;
			this.highHeight = highHeight;
			this.grid = grid;
			this.sample = sample;
		}

		public string GetName()
		{
			return name;
		}

		public void UpdateOG(IOccupancyGrid2D grid)
		{
			this.grid = grid;
		}
		public void Draw(Renderer renderer)
		{
			if (grid == null) return;

			else if (newRenderingMethod)
			{
				Gl.glBegin(Gl.GL_QUADS);
				float stepX = (float)grid.ResolutionX * sample;
				float stepY = (float)grid.ResolutionY * sample;
				float zOffset = -0f;
				if (grid != null)
				{
					for (double x = -grid.ExtentX; x < grid.ExtentX; x += stepX)
					{
					
						for (double y = -grid.ExtentY; y < grid.ExtentY; y += stepY)
						{
							// Get The (X, Y, Z) Value For The Bottom Left Vertex
							float xVert = (float)x;
							float yVert = (float)y;
							float zVert = (float)grid.GetCell(x, y);
							GLUtility.SetGLColor(GLUtility.FalseColor((float)zVert, lowHeight, highHeight), transparency);
							Gl.glVertex3f(xVert, yVert, zVert + zOffset);

							// Get The (X, Y, Z) Value For The Top Left Vertex
							xVert = (float)x;
							yVert = (float)(y + stepY);
							zVert = (float)grid.GetCell(xVert, yVert);
							GLUtility.SetGLColor(GLUtility.FalseColor((float)zVert, lowHeight, highHeight), transparency);
							Gl.glVertex3f(xVert, yVert, zVert + zOffset);

							// Get The (X, Y, Z) Value For The Top Right Vertex
							xVert = (float)(x + stepX);
							yVert = (float)(y + stepY);
							zVert = (float)grid.GetCell(xVert, yVert);
							GLUtility.SetGLColor(GLUtility.FalseColor((float)zVert, lowHeight, highHeight), transparency);
							Gl.glVertex3f(xVert, yVert, zVert + zOffset);

							// Get The (X, Y, Z) Value For The Bottom Right Vertex
							xVert = (float)(x + stepX);
							yVert = (float)y;
							zVert = (float)grid.GetCell(xVert, yVert);
							GLUtility.SetGLColor(GLUtility.FalseColor((float)zVert, lowHeight, highHeight), transparency);
							Gl.glVertex3f(xVert, yVert, zVert + zOffset);

						}
					}
				}
				Gl.glEnd();
			}
			else
			{
				GLUtility.DisableNiceLines();
				for (double x = -grid.ExtentX; x < grid.ExtentX; x += grid.ResolutionX)
				{
					for (double y = -grid.ExtentY; y < grid.ExtentY; y += grid.ResolutionY)
					{
						RectangleF r = new RectangleF((float)(x), (float)(y + grid.ResolutionY), (float)grid.ResolutionX, -(float)grid.ResolutionY);

						Color color;
						if (logOdds) color = GLUtility.FalseColor((float)grid.GetCellReal(x, y), lowHeight, highHeight);

						else
						{
							if (Double.IsNaN(grid.GetCell(x, y))) color = Color.Black;
							else
								color = GLUtility.FalseColor((float)grid.GetCell(x, y), lowHeight, highHeight);
						}
						//GLUtility.FillRectangle(color, r);
						GLUtility.FillRectangle(color, r, transparency);
					}
				}
				GLUtility.EnableNiceLines();
			}
		}

		public void ClearBuffer()
		{

		}

		public void SetTransparency(float transparency)
		{
			this.transparency = transparency;
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
