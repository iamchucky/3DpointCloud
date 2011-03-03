using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Magic.Common.Shapes;
using Magic.Common;
using Magic.Common.Mapack;
using System.Windows.Forms;

namespace Magic.Rendering.Renderables
{
	public class FakeObjectRenderer : IRender
	{
		Color color;
		string name;
		PointF location;
		string type;
		List<PointF> polygonPoints;
		double zero = 0;
		double angle = 0;
		double arrowlength = 1;

		public FakeObjectRenderer(string type, PointF location, string name)
		{
			this.color = Color.BlueViolet;
			this.type = type;
			this.location = location;
			this.name = name;
			this.polygonPoints = new List<PointF>();
		}

		public FakeObjectRenderer(string type, PointF location, string name,Color color)
		{
			this.color = color;
			this.type = type;
			this.location = location;
			this.name = name;
			this.polygonPoints = new List<PointF>();
		}

		public FakeObjectRenderer(string type, PointF location, string name, Color color, double angle)
		{
			this.color = color;
			this.type = type;
			this.location = location;
			this.name = name;
			this.polygonPoints = new List<PointF>();
			this.angle = angle;
		}

		public FakeObjectRenderer(string type, PointF location, string name, Color color, double angle, double arrowlength)
		{
			this.color = color;
			this.type = type;
			this.location = location;
			this.name = name;
			this.polygonPoints = new List<PointF>();
			this.angle = angle;
			this.arrowlength = arrowlength;
		}

		public FakeObjectRenderer(string type, PointF location, string name, Color color, List<PointF> polygonPoints)
		{
			this.color = color;
			this.type = type;
			this.location = location;
			this.name = name;
			this.polygonPoints = polygonPoints;
		}

		public PointF Location
		{
			get{return location;}
		}

		public List<PointF> PolygonPoints
		{
			get { return polygonPoints; }
		}

		public string Type
		{
			get { return type; }
		}

		public Color Color
		{
			get { return color; }
		}

		public double Angle
		{
			get { return angle; }
		}

		#region IRender Members

		public void Draw(Renderer r)
		{
			if (type.Equals("circle string"))
			{
				GLUtility.DrawString("CIRCLE", color, location);
			}
			else if (type.Equals("path start string"))
			{
				GLUtility.DrawString("PATH START", color, location);
			}
			else if (type.Equals("path end string"))
			{
				GLUtility.DrawString("PATH END", color, location);
			}
			else if (type.Equals("box string"))
			{
				GLUtility.DrawString("SQUARE", color, location);
			}
			else if (type.Equals("x string"))
			{
				GLUtility.DrawString("X", color, location);
			}
			else if (type.Equals("arrow string"))
			{
				PointF arrowHead = new PointF((float)(location.X + arrowlength * Math.Cos(angle)), (float)(location.Y + arrowlength * Math.Sin(angle)));

				GLUtility.DrawString("ARROW TAIL", color, location);
				GLUtility.DrawString("ARROW HEAD", color, arrowHead);
			}
			else if (type.Equals("important string"))
			{
				GLUtility.DrawString("EXCLAMATION MARK", color, location);
			}
			else if (type.Equals("spiral string"))
			{
				GLUtility.DrawString("SPIRAL", color, location);
			}
			else if (type.Equals("polygon string"))
			{
				GLUtility.DrawString("SHADED AREA", color, location);
			}
			else if (type.Equals("triangle string"))
			{
				GLUtility.DrawString("TRIANGLE", color, location);
			}
			else if (type.Equals("circle"))
			{
				PointF upperLeft = new PointF((float)(location.X - 0.25), (float)(location.Y - .25));
				RectangleF rect = new RectangleF(upperLeft, new SizeF(.5f, .5f));
				GLUtility.FillEllipse(color, rect);
			}
			else if (type.Equals("box"))
			{
				PointF upperLeft = new PointF((float)(location.X - 0.25), (float)(location.Y - .25));
				RectangleF rect = new RectangleF(upperLeft, new SizeF(.5f, .5f));
				GLUtility.FillRectangle(color, rect);
			}
			else if (type.Equals("triangle"))
			{
			    PointF p1 = new PointF((float)(location.X - 0.25), (float)(location.Y - 0.25));
			    PointF p2 = new PointF((float)(location.X + 0.25), (float)(location.Y - 0.25));
			    PointF p3 = new PointF((float)(location.X), (float)(location.Y + 0.25));
			    GLUtility.FillTriangle(color, p1, p2, p3);
			}
			else if (type.Equals("spiral"))
			{
				double size = 0.8;
				GLPen spiralPen = new GLPen(color, 3f);
				GLUtility.DrawCircle(spiralPen, location, (float)(size / 2));
				GLUtility.DrawCircle(spiralPen, location, (float)(size * 3 / 8));
				GLUtility.DrawCircle(spiralPen, location, (float)(size / 4));
				GLUtility.DrawCircle(spiralPen, location, (float)(size * 1 / 8));
			}
			else if (type.Equals("important"))
			{
				GLUtility.DrawLine(new GLPen(color, 3f), new Vector2(location.X, location.Y + 0.15), new Vector2(location.X, location.Y - .05));
				//GLUtility.DrawCross(new GLPen(color, 3f), new Vector2(location.X, location.Y - .12), .1f);
				GLUtility.DrawCircle(new GLPen(color, 2f), new Vector2(location.X, location.Y - .12), .02f);
                GLUtility.DrawDiamond(new GLPen(color, 3f), new Vector2((double)location.X, (double)location.Y), .6f);
                GLUtility.DrawDiamond(new GLPen(Color.Red, 3f), new Vector2((double)location.X, (double)location.Y), .8f);
			}
			else if (type.Equals("x"))
			{
				GLUtility.DrawLine(new GLPen(color, 3f), new Vector2((double)location.X - .2, (double)location.Y + .2), new Vector2((double)location.X + .2, (double)location.Y - .2));
				GLUtility.DrawLine(new GLPen(color, 3f), new Vector2((double)location.X + .2, (double)location.Y + .2), new Vector2((double)location.X - .2, (double)location.Y - .2));
			}
			else if (type.Equals("empty circle"))
			{
				PointF upperLeft = new PointF((float)(location.X - 0.25), (float)(location.Y - .25));
				RectangleF rect = new RectangleF(upperLeft, new SizeF(.5f, .5f));
				GLUtility.DrawEllipse(new GLPen(color, 3f), rect);
			}
			else if (type.Equals("arrow"))
			{
				Vector2 arrowHead = new Vector2(location.X + 0.7 * Math.Cos(angle), location.Y + 0.7 * Math.Sin(angle));

				PointF upperLeft = new PointF((float)(location.X - 0.1), (float)(location.Y - .1));
				RectangleF rect = new RectangleF(upperLeft, new SizeF(.2f, .2f));
				GLUtility.FillEllipse(color, rect);

				GLUtility.DrawLine(new GLPen(color, 3f), new Vector2(location.X, location.Y), arrowHead);
			}
			//else if(type.Equals("triangle"))//man
			//{
			//    //head
			//    PointF upperLeftHead = new PointF((float)(location.X - 0.25), (float)(location.Y + .25));
			//    RectangleF rect = new RectangleF(upperLeftHead, new SizeF(.5f, .5f));
			//    GLUtility.DrawEllipse(new GLPen(color, 3f), rect);

			//    //body
			//    GLUtility.DrawLine(new GLPen(color, 3f), new Vector2(location.X, location.Y-.25), new Vector2(location.X,location.Y+.25));
			//    //arms
			//    GLUtility.DrawLine(new GLPen(color, 3f), new Vector2(location.X, location.Y), new Vector2(location.X+.3,location.Y));
			//    GLUtility.DrawLine(new GLPen(color, 3f), new Vector2(location.X, location.Y), new Vector2(location.X-.3,location.Y));
			//    //legs
			//    GLUtility.DrawLine(new GLPen(color, 3f), new Vector2(location.X, location.Y-.25), new Vector2(location.X+.25,location.Y-.5));
			//    GLUtility.DrawLine(new GLPen(color, 3f), new Vector2(location.X, location.Y-.25), new Vector2(location.X-.25,location.Y-.5));

			//}
			else if (type.Equals("polygon"))
			{
				double maxx = -1.0 / zero;
				double maxy = -1.0 / zero;
				double minx = 1.0 / zero;
				double miny = 1.0 / zero;

				if (polygonPoints.Count == 0)
				{
					polygonPoints.Add(new PointF((float)(location.X - 1), (float)(location.Y + 1)));
					polygonPoints.Add(new PointF((float)(location.X - 1), (float)(location.Y - 1)));
					polygonPoints.Add(new PointF((float)(location.X + 2), (float)(location.Y - 2)));
					polygonPoints.Add(new PointF((float)(location.X + 3), (float)(location.Y)));
					polygonPoints.Add(new PointF((float)(location.X + 1), (float)(location.Y + 2)));
					polygonPoints.Add(new PointF((float)(location.X - 1), (float)(location.Y + 1)));

				}


				for (int i = 0; i < polygonPoints.Count - 1; i++)
				{
					if (polygonPoints[i].X > maxx)
						maxx = polygonPoints[i].X;
					if (polygonPoints[i].Y > maxy)
						maxy = polygonPoints[i].Y;
					if (polygonPoints[i].X < minx)
						minx = polygonPoints[i].X;
					if (polygonPoints[i].Y < miny)
						miny = polygonPoints[i].Y;

					this.location = new PointF((float)(0.5 * (maxx + minx)), (float)(0.5 * (maxy + miny)));
					GLUtility.DrawLine(new GLPen(color, 3), polygonPoints[i], polygonPoints[i + 1]);
				}
			}
			else if (type.Equals("fill polygon"))
			{
				double maxx = -1.0 / zero;
				double maxy = -1.0 / zero;
				double minx = 1.0 / zero;
				double miny = 1.0 / zero;

				if (polygonPoints.Count == 0)
				{
					polygonPoints.Add(new PointF((float)(location.X - 1), (float)(location.Y + 1)));
					polygonPoints.Add(new PointF((float)(location.X - 1), (float)(location.Y - 1)));
					polygonPoints.Add(new PointF((float)(location.X + 2), (float)(location.Y - 2)));
					polygonPoints.Add(new PointF((float)(location.X + 3), (float)(location.Y)));
					polygonPoints.Add(new PointF((float)(location.X + 1), (float)(location.Y + 2)));
					polygonPoints.Add(new PointF((float)(location.X - 1), (float)(location.Y + 1)));

				}


				for (int i = 0; i < polygonPoints.Count - 1; i++)
				{
					if (polygonPoints[i].X > maxx)
						maxx = polygonPoints[i].X;
					if (polygonPoints[i].Y > maxy)
						maxy = polygonPoints[i].Y;
					if (polygonPoints[i].X < minx)
						minx = polygonPoints[i].X;
					if (polygonPoints[i].Y < miny)
						miny = polygonPoints[i].Y;

					this.location = new PointF((float)(0.5 * (maxx + minx)), (float)(0.5 * (maxy + miny)));
					GLUtility.DrawLine(new GLPen(color, 3), polygonPoints[i], polygonPoints[i + 1]);
				}

				for (int i = 0; i < polygonPoints.Count - 1; i++)
				{
					GLUtility.FillTriangle(color, 0.3f, polygonPoints[i], polygonPoints[i + 1], new PointF((float)(0.5 * (maxx + minx)), (float)(0.5 * (maxy + miny))));
				}
			}
			else
			{
			}

			//GLUtility.DrawString(name,Color.Black, location);
		}
		
		public string GetName()
		{
			return this.name;
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
