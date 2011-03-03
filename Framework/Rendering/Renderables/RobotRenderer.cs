using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Magic.Common.Shapes;
using Magic.Common;
using Magic.Common.Mapack;
using System.Windows.Forms;
using System.Linq;
using Magic.Rendering;
using Magic.Common.DataTypes;
using Tao.OpenGl;

namespace Magic.Rendering.Renderables
{
	public class RobotRenderer : IHittable, ISelectable, IProvideContextMenu
	{

		string name;
		string MythName;
		string modeString = "";
		string modeQualifier = "";
		Color color;
		Color colorBloated;
		bool selected = false;
		float x; float y; float heading;
		Polygon bodyPlygn;
		Polygon origPlygn;
		ContextMenu robotContextMenu;
		bool drawCameraView = true;
		float cameraFOVangle = 150f;
		int vehicleID = 0;

		public RobotRenderer(string name, Color color)
		{
			this.name = name;
			this.color = color;

			// Create the polygon that represents the body
			origPlygn = new Polygon();
			origPlygn.Add(new Vector2(.25, .36));
			origPlygn.Add(new Vector2(.25, -.36));
			origPlygn.Add(new Vector2(-.25, -.36));
			origPlygn.Add(new Vector2(-.25, .36));

			bodyPlygn = new Polygon();
			bodyPlygn.Add(new Vector2(.25, .36));
			bodyPlygn.Add(new Vector2(.25, -.36));
			bodyPlygn.Add(new Vector2(-.25, -.36));
			bodyPlygn.Add(new Vector2(-.25, .36));

			robotContextMenu = new ContextMenu();

			this.MythName = establishMythName(name);
		}

		public RobotRenderer(string name, Color color, Color colorBloated)
		{
			this.name = name;
			this.color = color;
			this.colorBloated = colorBloated;

			// Create the polygon that represents the body
			origPlygn = new Polygon();
			origPlygn.Add(new Vector2(.25, .36));
			origPlygn.Add(new Vector2(.25, -.36));
			origPlygn.Add(new Vector2(-.25, -.36));
			origPlygn.Add(new Vector2(-.25, .36));

			bodyPlygn = new Polygon();
			bodyPlygn.Add(new Vector2(.25, .36));
			bodyPlygn.Add(new Vector2(.25, -.36));
			bodyPlygn.Add(new Vector2(-.25, -.36));
			bodyPlygn.Add(new Vector2(-.25, .36));

			robotContextMenu = new ContextMenu();

			this.MythName = establishMythName(name);
		}

		public void SetRobotMode(string str)
		{
			this.modeString = str;
		}
		public void SetRobotModeQualifier(string str)
		{
			this.modeQualifier = str;
		}

		public string establishMythName(string robotIDstring)
		{
			string mythName;
			if(robotIDstring.Equals("Robot 1"))
			{
				mythName = "Michelangelo";
			}
			else if (robotIDstring.Equals("Robot 2"))
			{
				mythName = "Leonardo";
			}
			else if (robotIDstring.Equals("Robot 3"))
			{
				mythName = "Donatello";
			}
			else if (robotIDstring.Equals("Robot 4"))
			{
				mythName = "Raphael";
			}
			else if (robotIDstring.Equals("Robot 5"))
			{
				mythName = "Master Splinter";
			}
			else if (robotIDstring.Equals("Robot 6"))
			{
				mythName = "Casey Jones";
			}
			else if (robotIDstring.Equals("Robot 7"))
			{
				mythName = "April O'Neil";
			}
			else
			{
				mythName = "Shredder";
			}
			return mythName;
		}
		public RobotRenderer(string name, Color color, Polygon bodyPlygn)
		{
			this.name = name;
			this.color = color;
			this.bodyPlygn = new Polygon(bodyPlygn);
			this.origPlygn = new Polygon(bodyPlygn);
			this.MythName = establishMythName(name);
		}
		public RobotRenderer(string name, Color color, Polygon bodyPlygn, bool drawCameraView)
		{
			this.name = name;
			this.color = color;
			this.bodyPlygn = new Polygon(bodyPlygn);
			this.origPlygn = new Polygon(bodyPlygn);
			this.drawCameraView = drawCameraView;
			this.MythName = establishMythName(name);
		}

		public bool SetCameraOnOff//(bool drawCameraView)
		{
			set { drawCameraView = value; }
			//this.drawCameraView = drawCameraView;
		}

        public Color GetColor()
        {
            return color;
        }


		public void SetName(string name)
		{
			this.name = name;
		}

		public void UpdatePose(RobotPose robotPose)
		{
			try
			{
				UpdatePose(robotPose.x, robotPose.y, robotPose.yaw);
			}
			catch (NullReferenceException e) { }

		}

		#region IRender Members
		public string GetName()
		{
			return name;
		}

		public void UpdatePose(double x, double y, double theta)
		{
			this.x = (float)x;
			this.y = (float)y;
			this.heading = (float)theta;

			// Update the polygon that represents the body
			Matrix3 transMat = Matrix3.Translation(x, y) * Matrix3.Rotation(heading - Math.PI / 2.0);
			//transMat.TransformPointsInPlace(origPlygn);
			bodyPlygn.Clear();
			bodyPlygn.AddRange(transMat.TransformPoints(origPlygn));
		}

		public void Draw(Renderer r)
		{
			float lineWidth = selected ? 2.0f : 1.0f;

			PointF bodyPnt = new PointF(x, y);
			PointF bodyHeading = new PointF(bodyPnt.X + (float)Math.Cos(heading), bodyPnt.Y + (float)Math.Sin(heading));
			GLUtility.DrawLineLoop(new GLPen(color, lineWidth), bodyPlygn.ToArray());

			GLUtility.FillTriangle(color, 0.6f, bodyPlygn[0].ToPointF(), bodyPlygn[1].ToPointF(), bodyPlygn[2].ToPointF());
			GLUtility.FillTriangle(color, 0.6f, bodyPlygn[2].ToPointF(), bodyPlygn[3].ToPointF(), bodyPlygn[0].ToPointF());

			// Draw heading
			//GLUtility.DrawLine(new GLPen(Color.Red, lineWidth), Vector2.FromPointF(bodyPnt), Vector2.FromPointF(bodyHeading));
			GLUtility.DrawLine(new GLPen(Color.Red, lineWidth), bodyPnt, bodyHeading);

			//// Draw the name
			//if (modeString.Equals("")) 
			//    GLUtility.DrawString(MythName + ": " + name, Color.Black, bodyPnt);
			//else if (modeQualifier.Equals(""))
			//    GLUtility.DrawString(name + ": " + modeString, Color.Black, bodyPnt);
			//else// if (defaultRenderer != null)
			//{
			//    try
			//    {
			//        GLUtility.DrawStringMultiLine(name + ": " + modeString + '\n' + "NOTE: " + modeQualifier, Color.Black, bodyPnt, r.CurrentCamera);
			//    }
			//    catch { GLUtility.DrawString(name + ": " + modeString, Color.Black, bodyPnt); }
			//}//else
			////    GLUtility.DrawString(name + ": " + modeString +  "(NOTE: " + modeQualifier + ")", Color.Black, bodyPnt);

			GLUtility.DrawString(name, Color.Black, bodyPnt);

			DrawFlagLine(x, y, color);
			if (drawCameraView && IsSelected)
			{
				PointF p1 = bodyPnt;
				PointF p2 = new PointF(bodyPnt.X + 5 * (float)Math.Cos(heading - 0.5 * (cameraFOVangle * Math.PI / 180)), bodyPnt.Y + 5 * (float)Math.Sin(heading - 0.5 * (cameraFOVangle * Math.PI / 180)));
				PointF p3 = new PointF(bodyPnt.X + 5 * (float)Math.Cos(heading - 0.167 * (cameraFOVangle * Math.PI / 180)), bodyPnt.Y + 5 * (float)Math.Sin(heading - 0.167 * (cameraFOVangle * Math.PI / 180)));
				PointF p4 = new PointF(bodyPnt.X + 5 * (float)Math.Cos(heading + 0.167 * (cameraFOVangle * Math.PI / 180)), bodyPnt.Y + 5 * (float)Math.Sin(heading + 0.167 * (cameraFOVangle * Math.PI / 180)));
				PointF p5 = new PointF(bodyPnt.X + 5 * (float)Math.Cos(heading + 0.5 * (cameraFOVangle * Math.PI / 180)), bodyPnt.Y + 5 * (float)Math.Sin(heading + 0.5 * (cameraFOVangle * Math.PI / 180)));

                GLUtility.FillTriangle(color, 0.1f, p1, p2, p3);
                GLUtility.FillTriangle(color, 0.1f, p1, p3, p4);
                GLUtility.FillTriangle(color, 0.1f, p1, p4, p5);
                //GLUtility.FillTriangle(Color.BlueViolet, 0.3f, p1, p2, p3);
                //GLUtility.FillTriangle(Color.Blue, 0.3f, p1, p3, p4);
                //GLUtility.FillTriangle(Color.Turquoise, 0.3f, p1, p4, p5);

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
			get { return vehicleID; }
			set { vehicleID = (int)value; }
		}

		private void DrawFlagLine(float X, float Y, Color color)
		{
			//double x, y;
			GLPen pen;
			
			pen = new GLPen(color, 50f);
			//pen.GLApplyPen();
			//Gl.glVertex3f(x, y, 20);
			//Gl.glVertex3f(x, y, 1);

			pen.GLApplyPen();
			Gl.glBegin(Gl.GL_LINES);
			Gl.glVertex3f(X, Y, 0);
			Gl.glVertex3f(X, Y, 5);
			Gl.glEnd();
		}

		private Color ColorFromHeight(float h)
		{
			if (h >= 1.0f) return Color.FromArgb(255, 0, 0);
			if (h <= 0.0f) return Color.FromArgb(0, 0, 255);

			if (h <= .5f)
			{
				float b = (1.0f - h * 2.0f) * 255.0f;
				float g = 255.0f - b;
				float norm = (b + g) / (255.0f * 2.0f);
				b = Math.Min(255.0f, b / norm);
				g = Math.Min(255.0f, g / norm);
				return Color.FromArgb(0, (int)g, (int)b);
			}
			else
			{
				float r = (2.0f * (h - .5f)) * 255.0f;
				float g = 255.0f - r;
				float norm = (r + g) / (255.0f * 2.0f);
				r = Math.Min(255.0f, r / norm);
				g = Math.Min(255.0f, g / norm);
				return Color.FromArgb((int)r, (int)g, 0);
			}
		}


		#endregion

		#region IHittable Members

		public Polygon GetBoundingPolygon()
		{
			return bodyPlygn;
		}

		public bool HitTest()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ISelectable Members

		public bool IsSelected
		{
			get
			{
				return selected;
			}

		}

		public void OnSelect()
		{
			selected = true;
		}

		public void OnDeselect()
		{
			selected = false;
		}

		#endregion

		#region IProvideContextMenu Members

		public ICollection<MenuItem> GetMenuItems()
		{
			List<MenuItem> items = new List<MenuItem>();
			items.Add(new MenuItem("Set Path", new EventHandler(
				delegate(object o, EventArgs ea)
				{
					//Renderer.Instance.DeactivateTool(ToolManager.SelectTool);
					//Renderer.Instance.DeactivateTool(ToolManager.ContextMenuTool);
					OnSelect();
					//Renderer.Instance.ActivateTool(ToolManager.PathTool);
				})));
			return items;
		}

		public void OnMenuOpening()
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
