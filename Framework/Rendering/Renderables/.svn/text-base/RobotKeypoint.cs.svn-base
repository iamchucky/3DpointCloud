using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common;
using Magic.Common.Shapes;
using Magic.Common.Mapack;

namespace Magic.Rendering.Renderables
{
    class RobotKeypoint : IKeypoint
    {
        int id;
        bool isSelected;
        string name;
        RobotPose pose;
        PathToRobotRenderer runningPath; // the path that the robot is currently running
        PathToRobotRenderer commandedPath; // the path that is currently being sent to the robot by the HRI
        Color color;
        Polygon bodyPlygn;
        Polygon origPlygn;

        float drawLineWidth = 1.0f;
        static float selectedLineWidthBoldAmount = 0.5f;

        public RobotKeypoint(int id)
        {
            color = Color.Black;
            this.id = id;
            name = "Robot " + id;
            runningPath = new PathToRobotRenderer();
            commandedPath = new PathToRobotRenderer();
            pose = new RobotPose();

            origPlygn = new Polygon();
            origPlygn.Add(new Vector2(.25, .36));
            origPlygn.Add(new Vector2(.25, -.36));
            origPlygn.Add(new Vector2(-.25, -.36));
            origPlygn.Add(new Vector2(-.25, .36));

            bodyPlygn = new Polygon(origPlygn.points);
        }

        public RobotKeypoint(int id, Color color)
            : this(id)
        {
            this.color = color;
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public PathToRobotRenderer CommandedPath
        {
            get { return commandedPath; }
            set { commandedPath = value; }
        }

        public void UpdatePose(RobotPose pose)
        {
            try
            {
                UpdatePose(pose.x, pose.y, pose.yaw);
            }
            catch (NullReferenceException e) { }
        }

        public void UpdatePose(double x, double y, double theta)
        {
            pose.x = (float)x;
            pose.y = (float)y;
            pose.yaw = (float)theta;

            UpdatePolygon(this.pose);
        }

        private void UpdatePolygon(RobotPose p)
        {
            Matrix3 transMat = Matrix3.Translation(p.x,p.y) * Matrix3.Rotation(p.yaw - Math.PI / 2.0);
            bodyPlygn.Clear();
            bodyPlygn.AddRange(transMat.TransformPoints(origPlygn));
        }

        // TODO add path getters and setters

        #region IKeypoint Members


        public double X
        {
            get
            {
                return pose.x;
            }
            set
            {
                pose.x = value;
            }
        }

        public double Y
        {
            get
            {
                return pose.y;
            }
            set
            {
                pose.y = value;
            }
        }

        public double Z
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ISelectable Members

        public bool IsSelected
        {
            get { return isSelected; }
        }

        public void OnSelect()
        {
            isSelected = true;
            drawLineWidth = drawLineWidth + selectedLineWidthBoldAmount;
        }

        public void OnDeselect()
        {
            isSelected = false;
            drawLineWidth = drawLineWidth - selectedLineWidthBoldAmount;
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

        #region IRender Members

        public string GetName()
        {
            return name;
        }

        public void Draw(Renderer cam)
        {
            Vector2 bodyPnt = new Vector2(pose.x, pose.y);
            Vector2 bodyHeading = new Vector2(bodyPnt.X + Math.Cos(pose.yaw), bodyPnt.Y + Math.Sin(pose.yaw));
            GLUtility.DrawLineLoop(new GLPen(color, drawLineWidth), bodyPlygn.ToArray());

            // Draw heading
            GLUtility.DrawLine(new GLPen(Color.Red, drawLineWidth), bodyPnt, bodyHeading);
            // Draw the name
            GLUtility.DrawString(name, Color.Black, new PointF((float)bodyPnt.X, (float)bodyPnt.Y));
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
