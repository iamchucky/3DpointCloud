using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Magic.Common;

namespace Magic.Rendering
{

    /// <summary>
    /// Allows an angle to be drawn and measured on canvas.
    /// Note that the generic class taken in this interface has to be of a type EventArgs
    /// </summary>
    public class AngleTool : IRenderToolWithResult<AngleToolCompletedEventArgs>
    {
        //indicate if this tool is active (by ActivateTool and DeactivateTool in Renderer)
        private bool isActive = false;

        /// <summary>
        /// figured whenever the tool completes
        /// </summary>
        public event EventHandler<AngleToolCompletedEventArgs> ToolCompleted;
        
        //where the mouse button was first pushed
        private Point clickPoint = new Point(0, 0);

        //where the mouse point is currently
        private Point movePoint = new Point(0, 0);

        //where the mouse button was pushed next (for angles)
        private Point clickPoint2 = new Point(0, 0);

        private int angleState = 0;
        double dist12;
        double dist23;
        double dist13;
        double cosangle;
        double angleDEG;

        //the icon of the cursor to display
        Cursor currentCursor = Cursors.Cross;

		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;

        private ToolManager myToolManager;
		public AngleTool(ToolManager tm)
		{
            this.myToolManager = tm;
			//list of possible modes
			modeList = new List<string>();
			//modeList.Add("Find Angle");

			//tool's default mode
			defaultMode = "NONE";
			//defaultMode = "Find Angle";
		}

        public void BuildConflicts(List<IRenderTool> parallelTools)
		{
			dependencies = new List<IRenderTool>();
			conflicts = new List<IRenderTool>();

            foreach (IRenderTool tool in parallelTools)
            {
                if (tool.GetName().StartsWith("Ruler") ||
                    tool.GetName().StartsWith("Sketch"))
                {
                    conflicts.Add(tool);
                }
            }

			//conflicts.Add(ToolManager.PDFTool);
            //conflicts.Add(ToolManager.RulerTool);
            //conflicts.Add(ToolManager.SketchTool);
		}

		/// <summary>
		/// TempDeactivate and TempReactivate : temporarily deactivate/reactivate a tool without disabling it (use carefully)
		/// </summary>
		public void TempDeactivate()
		{
		}
		public void TempReactivate()
		{
		}

		public void EnableMode(string modeName)
		{
		}

        #region IRenderTool Members

				public Cursor Cursor
				{
					get { return currentCursor; }
				}

        public void OnMouseDown(Renderer r, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {                
                if (angleState == 0)
                { clickPoint = new Point(e.X, e.Y); angleState++; }
                else if (angleState == 1)
                { clickPoint2 = new Point(e.X, e.Y); angleState++; }
                else if (angleState == 2)
                { angleState++; }
                currentCursor = Cursors.SizeAll;
            }
        }

        public void OnMouseMove(Renderer r, MouseEventArgs e)
        {
            movePoint = new Point(e.X, e.Y);
        }      
  
        public void OnMouseUp(Renderer r, MouseEventArgs e)
        {
            if (angleState == 3)
            {
                if (ToolCompleted != null)
                {
                    ToolCompleted(this, new AngleToolCompletedEventArgs(dist12, dist23, dist13, cosangle, angleDEG));
                }
                angleState = 0;
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
            }
        }

        #endregion

        #region IRender Members

        public string GetName()
        {
            return "Angle";
        }

        public void Draw(Renderer r)
        {
            #region angle drawing code
            if (IsActive)
            {
                if (angleState == 1)
                {
                    PointF p1 = r.ScreenToWorld(clickPoint);
                    PointF p2 = r.ScreenToWorld(movePoint);
                    GLPen pen = new GLPen(Color.Purple, 1.0f);
                    GLUtility.DrawLine(pen,Vector2.FromPointF(p1), Vector2.FromPointF(p2));
                    GLUtility.DrawEllipse(pen, new RectangleF(p1.X - .1f, p1.Y - .1f, .2f, .2f));
                    GLUtility.DrawEllipse(pen, new RectangleF(p2.X - .1f, p2.Y - .1f, .2f, .2f));
                    dist12 = Math.Sqrt(((p1.X - p2.X) * (p1.X - p2.X)) + ((p1.Y - p2.Y) * (p1.Y - p2.Y)));
                    PointF m12 = new PointF((p1.X + p2.X) / 2.0f, (p1.Y + p2.Y) / 2.0f);
                    GLUtility.DrawString(dist12.ToString("F2") + "m", Color.Black, m12);
                }
                if (angleState == 2)
                {
                    PointF p1 = r.ScreenToWorld(clickPoint);
                    PointF p2 = r.ScreenToWorld(clickPoint2);
                    PointF p3 = r.ScreenToWorld(movePoint);
                    GLPen pen = new GLPen(Color.Purple, 1.0f);
                    GLUtility.DrawLine(pen, p1, p2);
                    GLUtility.DrawLine(pen, p2, p3);
                    GLUtility.DrawEllipse(pen, new RectangleF(p1.X - .1f, p1.Y - .1f, .2f, .2f));
                    GLUtility.DrawEllipse(pen, new RectangleF(p2.X - .1f, p2.Y - .1f, .2f, .2f));
                    GLUtility.DrawEllipse(pen, new RectangleF(p3.X - .1f, p3.Y - .1f, .2f, .2f));
                    dist12 = Math.Sqrt(((p1.X - p2.X) * (p1.X - p2.X)) + ((p1.Y - p2.Y) * (p1.Y - p2.Y)));
                    PointF m12 = new PointF((p1.X + p2.X) / 2.0f, (p1.Y + p2.Y) / 2.0f);
                    GLUtility.DrawString(dist12.ToString("F2") + "m", Color.Black, m12);
                    dist23 = Math.Sqrt(((p2.X - p3.X) * (p2.X - p3.X)) + ((p2.Y - p3.Y) * (p2.Y - p3.Y)));
                    PointF m23 = new PointF((p2.X + p3.X) / 2.0f, (p2.Y + p3.Y) / 2.0f);
                    GLUtility.DrawString(dist23.ToString("F2") + "m", Color.Black, m23);
                    dist13 = Math.Sqrt(((p1.X - p3.X) * (p1.X - p3.X)) + ((p1.Y - p3.Y) * (p1.Y - p3.Y)));

                    cosangle = (dist12 * dist12 + dist23 * dist23 - dist13 * dist13) / (2 * dist12 * dist23);
                    angleDEG = Math.Acos(cosangle) * (180.0 / Math.PI);
                    p2.X += .5f; p2.Y += .5f;
                    GLUtility.DrawString(angleDEG.ToString("F2") + " deg", Color.Black, p2);
                }
            }
            #endregion
        }

        public void ClearBuffer()
        {
			angleState = 0;
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


		#region IRenderTool Members


		public List<string> ModeList
		{
			get { return modeList; }
		}

		public string DefaultMode
		{
			get
			{
				return defaultMode;
			}
			set
			{
				defaultMode = value;
			}
		}

		#endregion

		/// <summary>
		/// Get tool dependencies & conflicts
		/// </summary>
		#region Dependencies & Conflicts
		public List<IRenderTool> Dependencies
		{
			get { return dependencies; }
		}

		public List<IRenderTool> Conflicts
		{
			get { return conflicts; }
		}

		public void AddConflict(IRenderTool tool)
		{
			conflicts.Add(tool);
		}

		#endregion
	}

    /// <summary>
    /// This is returned when the angle tool is completed.  Note that it inherits from EventArgs which is required for a tool to return something.
    /// </summary>
    public class AngleToolCompletedEventArgs : EventArgs
    {
        //the angle
        double dist12;
        double dist23;
        double dist13;
        double cosangle;
        double angleDEG;

        //the accessor (property) which wraps up properties
        public double CosAngle
        {
            get { return cosangle; }
        }
        public double Dist12
        {
            get { return dist12; }
        }
        public double Dist13
        {
            get { return dist13; }
        }
        public double Dist23
        {
            get { return dist23; }
        }
        public double AngleDEG
        {
            get { return angleDEG; }
        }

        /// <summary>
        /// Just a constructor to make an angle
        /// </summary>
        /// <param name="a"></param>
        public AngleToolCompletedEventArgs(double d12,double d23,double d13,double ca,double cd)
        {
            cosangle = ca;
            angleDEG = cd;
            dist13 = d13;
            dist12 = d12;
            dist23 = d23;
        }
    }

}
