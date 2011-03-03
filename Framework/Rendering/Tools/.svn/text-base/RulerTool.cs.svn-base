using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Magic.Rendering
{

	/// <summary>
	/// Allows a ruler to be drawn on the Canavas.
	/// Note that the generic class that taken in this interface has to be of a type EventArgs (see the eventArgs later in this cs file)
	/// </summary>
	public class RulerTool : IRenderToolWithResult<RulerToolCompletedEventArgs>
	{
		//indicate if this tool is active (by ActivateTool and DeactivateTool in Renderer)
		bool isActive = false;
		/// <summary>
		/// Fired whenever the tool completes
		/// </summary>
		public event EventHandler<RulerToolCompletedEventArgs> ToolCompleted;

		//where the mouse button was first pushed
		private Point downPoint = new Point(0, 0);

		//where the mouse point is currently
		private Point movePoint = new Point(0, 0);

		//the icon of the cursor to display
		private Cursor currentCursor = Cursors.Cross;

		//indicates if we should draw (i.e. the mouse has been pushed)       
		bool shouldDraw = false;

		//the last distance measured on a draw event
		double distance;

		//integer to determine whether this is first or second click
		int click = 0;

		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;

        private ToolManager myToolManager;
		public RulerTool(ToolManager tm)
        {
            this.myToolManager = tm;
			//list of possible modes
			modeList = new List<string>();
			//modeList.Add("Measure distance");

			//tool's default mode
			//defaultMode = "Measure distance";
			defaultMode = "NONE";
		}


		public void BuildConflicts(List<IRenderTool> parallelTools)
		{
			dependencies = new List<IRenderTool>();
			conflicts = new List<IRenderTool>();
			//conflicts.Add(ToolManager.PDFTool);

            foreach (IRenderTool tool in parallelTools)
            {
                if (tool.GetName().StartsWith("Angle") ||
                    tool.GetName().StartsWith("Sketch"))
                {
                    conflicts.Add(tool);
                }
            }
            //conflicts.Add(ToolManager.AngleTool);
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
				if (click == 0)
				{
					//remember where we first pushed the mouse down button
					downPoint = new Point(e.X, e.Y);
					//change the cursor icon
					currentCursor = Cursors.Cross;
					//tell the draw method that we should draw
					shouldDraw = true;
					click++;
				}
				else
				{
					shouldDraw = false;
					click = 0;
					if (ToolCompleted != null)
						ToolCompleted(this, new RulerToolCompletedEventArgs(distance));
				}
			}
		}

		public void OnMouseMove(Renderer r, MouseEventArgs e)
		{
			//update the second point of the ruler as the user moves their mouse around
			movePoint = new Point(e.X, e.Y);
		}

		public void OnMouseUp(Renderer r, MouseEventArgs e)
		{
			/*
			//we're done, so you should stop drawing the ruler
			shouldDraw = false;
			//fire the event that the tool is completed! 
			//the check for null makes sure that "someone" is listening, otherwise an exception will be thrown
			if (ToolCompleted != null)
				ToolCompleted(this, new RulerToolCompletedEventArgs(distance));
			 */
		}



		/// <summary>
		/// this is part of the interface that  indicates if the tool is currently in use. In general
		/// most tools will have identical code for this property.
		/// </summary>
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
		/// <summary>
		/// returns the name of this tool. I don't think this is actually used anywhere yet, but its good to have
		/// </summary>
		/// <returns></returns>
		public string GetName()
		{
			return "Ruler";
		}

		/// <summary>
		/// This is called whenever the renderer is going to draw stuff. Essentially this is where you put your 
		/// tool drawing code.
		/// </summary>
		/// <param name="r"></param>
		public void Draw(Renderer r)
		{
			#region ruler drawing code
			if (shouldDraw && isActive)
			{
				PointF p1 = r.ScreenToWorld(downPoint);
				PointF p2 = r.ScreenToWorld(movePoint);
				GLPen pen = new GLPen(Color.Purple, 1.0f);
				GLUtility.DrawLine(pen, p1, p2);
				GLUtility.DrawEllipse(pen, new RectangleF(p1.X - .1f, p1.Y - .1f, .2f, .2f));
				GLUtility.DrawEllipse(pen, new RectangleF(p2.X - .1f, p2.Y - .1f, .2f, .2f));
				distance = Math.Sqrt(((p1.X - p2.X) * (p1.X - p2.X)) + ((p1.Y - p2.Y) * (p1.Y - p2.Y)));
				PointF m = new PointF((p1.X + p2.X) / 2.0f + .3f, (p1.Y + p2.Y) / 2.0f + .3f);
				GLUtility.DrawString(distance.ToString("F2") + "m", Color.Black, m);
			}
			#endregion
		}

		/// <summary>
		/// Called whenever the renderer needs to reset whats going on in here
		/// </summary>
		public void ClearBuffer()
		{
			shouldDraw = false;
			click = 0;
		}

		/// <summary>
		/// Indicates if the tool is drawn relativee to the vehicle. In general, tools always return false.
		/// </summary>
		public bool VehicleRelative
		{
			get { return false; }
		}

		/// <summary>
		/// Indicates which vehicle id we draw relative to. if we're not vehicle relative, return null.
		/// </summary>
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
		#endregion
	}

	/// <summary>
	/// This is reutnred when the ruler tool is compelted. Note that it inherists from EventArgs which is required for a tool to return something
	/// </summary>
	public class RulerToolCompletedEventArgs : EventArgs
	{
		//the distance
		double distance;

		//the accessor (property) which wraps up distance
		public double Distance
		{
			get { return distance; }
		}

		/// <summary>
		/// Just a constructor to make a distance
		/// </summary>
		/// <param name="d"></param>
		public RulerToolCompletedEventArgs(double d)
		{
			distance = d;
		}

	}
}
