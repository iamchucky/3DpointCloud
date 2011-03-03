using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Magic.Common;
using Magic.Common.Shapes;
using Magic.Common.Path;
using Magic.Rendering;
using Magic.Rendering.Renderables;


namespace Magic.Rendering
{
	/// <summary>
	/// allows a path to be drawn from several waypoints
	/// </summary>
    public class PathTool : IRenderToolWithResult<PathToolCompletedEventArgs>
	{
		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// MAKE SURE THIS IS FALSE IF USING OLD HRI!
		private bool newVersion = true;
		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		//indicate if this tool is active
		private bool isActive = false;
		private bool tempActive = true;

		/// <summary>
		/// define EventHandlers and EventArgs
		/// </summary>
		public event EventHandler<PathToolCompletedEventArgs> ToolCompleted;
		public event EventHandler UpdateDefault;
        public event EventHandler<GestureExpHRIEventArgs> GestureExpData;

		//where the mouse button was first pushed
		private Point downPoint = new Point(0, 0);

		//where the mouse point is currently
		private PointF mousePoint = new PointF(0, 0);

		//the icon of the cursor to display
		private Cursor currentCursor = Cursors.Cross;

		WaypointRenderer movePointWP = new WaypointRenderer();
		WaypointRenderer origMovePointWP = new WaypointRenderer();

		//bool arguments for moving points, drawing paths, and clearing paths
		bool shouldMove = false;
		bool shouldDraw = false;
		bool clearIndicator = false;

		//bool arguments for mode
		bool modeNewWaypoint = true;
		bool modeDeleteWaypoint = false;
		bool modeMoveWaypoint = true;

		//count the number of points in the path (used for rendering)
		int count = new int();

		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;

		//path to create
		PathToRobotRenderer newPath = new PathToRobotRenderer();
		PathToRobotRenderer clonePathWP = new PathToRobotRenderer();
		WaypointRenderer wpFirst = new WaypointRenderer();

        private ToolManager myToolManager;
        public PathTool(ToolManager tm)
        {
            this.myToolManager = tm;
			defaultMode = "Send Path";
        }

		public void ClearAllPaths()
		{
			newPath = new PathToRobotRenderer();
			bool modeNewWaypoint = true;
			bool modeDeleteWaypoint = false;
			bool modeMoveWaypoint = true;
		}

		/// <summary>
		/// Operations to perform on MouseDown (takes: Renderer r, MouseEventArgs e)
		///     On single left click:
		///         - If not on an existing waypoint, make a new waypoint & execute pathPointAdded event
		///         - If on an existing waypoint, change shouldMove to true and keep track of waypoint to move
		///     On double left click:
		///         - Send path (execute ToolCompleted event) and clear renderer
		///     On single right click:
		///         - If on an existing waypoint, delete the waypoint
		///     On double right click:
		///         - delete and clear the entire path
		/// </summary>
		/// <param name="r"></param>
		/// <param name="e"></param>
		#region Operations to perform on MouseDown
		public void OnMouseDown(Renderer r, MouseEventArgs e)
		{
			#region Left Click
			//check that button was a left click
			PointF p = r.ScreenToWorld(e.Location);
			if (e.Button == MouseButtons.Left)
			{
				//PointF p = r.ScreenToWorld(e.Location);

				if ((newVersion == true) && (tempActive == true)) // (new version)
				{
					if (modeNewWaypoint == true)
					{
						if (newPath.WaypointList.Count > 0)
						{
							if (!defaultMode.Equals("Send path"))
							{
								defaultMode = "Send path";
								if (UpdateDefault != null)
									UpdateDefault(this, new EventArgs()); 
							}
							if (modeMoveWaypoint == true)
							{
								foreach (WaypointRenderer wp in newPath.WaypointList)
								{
									// move an existing waypoint if we click sufficiently close to it
									if ((p.X - wp.X < 0.25) && (p.Y - wp.Y < 0.25) &&
										(p.X - wp.X > -0.25) && (p.Y - wp.Y > -0.25))
									{
										shouldMove = true;
										movePointWP = wp;
										origMovePointWP = wp;
										currentCursor = Cursors.Hand;
                                        myToolManager.SelectTool.TempDeactivate();
										if(GestureExpData != null)
                                            GestureExpData(this, new GestureExpHRIEventArgs("Tool:PathTool|left click down|move waypoint from|" + e.X + "|" + e.Y + "|(screen)|" + p.X + "|" + p.Y + "|(world)"));
										break;
                                        //Tool: SelectTool, mode: Select/Deselect, activated using context menu
									}
								}
							}
						}
						if (((newPath.WaypointList.Count > 0) && (shouldMove == false)) || (newPath.WaypointList.Count == 0))
						{
							WaypointRenderer newPoint = new WaypointRenderer();

							if (newPath.WaypointList.Count > 0)
							{
								if (GestureExpData != null)
                                    GestureExpData(this, new GestureExpHRIEventArgs("Tool:PathTool|left click down|add waypoint|" + e.X + "|" + e.Y + "|(screen)|" + p.X + "|" + p.Y + "|(world)"));
							}
							else
							{
								if (GestureExpData != null)
                                    GestureExpData(this, new GestureExpHRIEventArgs("Tool:PathTool|left click down|new path|" + e.X + "|" + e.Y + "|(screen)|" + p.X + "|" + p.Y + "|(world)"));
							}
                            newPoint.X = p.X;
                            newPoint.Y = p.Y;
                            newPath.AddWaypoint(newPoint);

							modeMoveWaypoint = true;
							if (!defaultMode.Equals("Send path"))
							{
								defaultMode = "Send path";
								if (UpdateDefault != null)
									UpdateDefault(this, new EventArgs());
							}
						}
						shouldDraw = true;
					}
					else if (modeDeleteWaypoint == true)
					{
						if (newPath.WaypointList.Count > 0)
						{
							foreach (WaypointRenderer wp in newPath.WaypointList)
							{
								// if right click is sufficiently close to an existing waypoint, delete it and excecute pathPointRemoved event
								Vector2 v = new Vector2((float)wp.X, (float)wp.Y);
								if (!((wp.X - p.X > -.25) && (wp.Y - p.Y > -.25) &&
									(wp.X - p.X < .25) && (wp.Y - p.Y < .25)))
									clonePathWP.AddWaypoint(wp);
							}
							newPath = clonePathWP;
							if (GestureExpData != null)
                                GestureExpData(this, new GestureExpHRIEventArgs("Tool:PathTool|left click down|delete waypoint|" + e.X + "|" + e.Y + "|(screen)|" + p.X + "|" + p.Y + "|(world)"));
							clonePathWP = new PathToRobotRenderer();
						}
						if (newPath.WaypointList.Count == 0)
						{
							//defaultMode = "Add new waypoint";
							//modeDeleteWaypoint = false;
							//modeNewWaypoint = true;
							if (UpdateDefault != null)
								UpdateDefault(this, new EventArgs());
						}
					}
				}
			}
			#endregion

			if (e.Button == MouseButtons.Right)
			{
                //if (GestureExpData != null)
                //    GestureExpData(this, new GestureExpPathToolEventArgs("Tool:PathTool    right click   ---     " + e.X + " " + e.Y + " (screen)    " + p.X + " " + p.Y + " (world)"));
			}
		}
		#endregion

		public PathToRobotRenderer CurrentPath
		{
			get { return newPath; }
		}

		/// <summary>
		/// Operations to perform on MouseMove:
		///     If we're moving a waypoint, update the waypoint location (for rendering) to the mouse location
		///     Otherwise, don't do anything
		/// </summary>
		/// <param name="r"></param>
		/// <param name="e"></param>
		#region Operations to perform on MouseMove
		public void OnMouseMove(Renderer r, MouseEventArgs e)
		{
			mousePoint = new PointF(e.X, e.Y);
			PointF p = r.ScreenToWorld(e.Location);

			if (shouldMove == true)
			{
				foreach (WaypointRenderer wp in newPath.WaypointList)
				{
					if (movePointWP.Equals(wp))
					{
						WaypointRenderer mouseWP = wp;
						mouseWP.X = p.X;
						mouseWP.Y = p.Y;
						clonePathWP.AddWaypoint(mouseWP);
						movePointWP = mouseWP;
					}
					else
					{
						clonePathWP.AddWaypoint(wp);
					}
				}
				newPath = clonePathWP;
				clonePathWP = new PathToRobotRenderer();
			}
		}
		#endregion


		/// <summary>
		/// Operations to perform on MouseUp:
		///     If we just finished moving a waypoint, execute pathPointMoved event
		/// </summary>
		/// <param name="r"></param>
		/// <param name="e"></param>
		#region Operations to perform on MouseUp
		public void OnMouseUp(Renderer r, MouseEventArgs e)
		{
			//throw new NotImplementedException();
			if (shouldMove)
			{
				PointF p = r.ScreenToWorld(e.Location);
				foreach (WaypointRenderer wp in newPath.WaypointList)
				{
					if (wp.Equals(movePointWP))
					{
                        myToolManager.SelectTool.TempReactivate();
						break;
					}
				}
				if (e.Button == MouseButtons.Right)
				{
                    //if (GestureExpData != null)
                    //    GestureExpData(this, new GestureExpPathToolEventArgs("Tool:PathTool right click up      " + e.X + ", " + e.Y + " (screen), " + p.X + ", " + p.Y + " (world)"));
				}
				else if (e.Button == MouseButtons.Left)
				{
					if (GestureExpData != null)
                        GestureExpData(this, new GestureExpHRIEventArgs("Tool:PathTool|left click up|move waypoint to|" + e.X + "|" + e.Y + "|(screen)|" + p.X + "|" + p.Y + "|(world)"));
				}
			}
			shouldMove = false;
		}
		#endregion


		/// <summary>
		/// Draw the waypoints & path (circles & lines)
		/// </summary>
		/// <param name="r"></param>
		#region path drawing code

		public void Draw(Renderer r)
		{
			//if only one waypoint, draw the waypoint
			if (newPath.WaypointList.Count == 1 && shouldDraw)
			{
				WaypointRenderer wpFirst = newPath.WaypointList[0];
				PointF drawPoint = new PointF((float)wpFirst.X, (float)wpFirst.Y);
				GLUtility.DrawCircle(new GLPen(wpFirst.Color, 1.0f), drawPoint, 0.2f);
				if (!shouldMove && tempActive)
				{
					GLUtility.DrawLine(new GLPen(Color.LawnGreen, 1.0f), drawPoint, r.ScreenToWorld(mousePoint));
				}
			}

						// if more than one waypoint, draw waypoints and connect subsequent waypoints with a line
			else if (newPath.WaypointList.Count > 1 && shouldDraw)
			{
				WaypointRenderer wpLast = newPath.WaypointList.First();

				foreach (WaypointRenderer wp in newPath.WaypointList)
				{
					PointF pathPoint = new PointF((float)wp.X, (float)wp.Y);

					if (shouldMove && wp.Equals(movePointWP))
					{
						GLUtility.DrawCircle(new GLPen(Color.Red, 1.0f), pathPoint, 0.2f);
					}
					else
					{
						GLUtility.DrawCircle(new GLPen(wp.Color, 1.0f), pathPoint, 0.2f);
					}
					if (!wp.Equals(newPath.WaypointList.First()))
					{
						PointF dummyLast = new PointF((float)wpLast.X, (float)wpLast.Y);
						GLUtility.DrawLine(new GLPen(newPath.Color, 1.0f), dummyLast, pathPoint);
					}
					wpLast = wp;
				}
				// if we are not moving a waypoint, draw a light green line between last waypoint and current mouse location
				if (!shouldMove && tempActive)
				{
					PointF dummyLast = new PointF((float)wpLast.X, (float)wpLast.Y);
					GLUtility.DrawLine(new GLPen(Color.LawnGreen, 1.0f), dummyLast, r.ScreenToWorld(mousePoint));
				}
			}

			if (newPath.WaypointList.Count > 0)
			{

				foreach (ISelectable selectable in r.Selectables)
				{
					PointF dummyFirst = new PointF();
					RobotRenderer robot = selectable as RobotRenderer;
					if (robot == null) continue;
					else if (robot.IsSelected)
					{
						Polygon p = robot.GetBoundingPolygon();
						if (p.points.Count > 0)
						{
							dummyFirst = new PointF((float)newPath.WaypointList[0].X, (float)newPath.WaypointList[0].Y);
							GLUtility.DrawLine(new GLPen(Color.Green, 1.0f), p.Center.ToPointF(), dummyFirst);
						}
					}
				}
			}
		}
		#endregion


		#region IRender Members : GetName(), ClearBuffer(), VehicleRelative, VehicleRelativeID


		public string GetName()
		{
			return "Path Tool";
		}

		public void ClearBuffer()
		{
			newPath.WaypointList.Clear();
			newPath.LineSegmentList.Clear();
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

		#region IRenderTool Members : IsActive, Cursor, ModeList, DefaultMode, EnableMode, Dependencies, Conflicts, BuildConflicts()

		public bool IsActive
		{

			get
			{
				return isActive;
			}
			set { isActive = value; }
		}

		public bool TempActive
		{
			get { return tempActive; }
		}

		public Cursor Cursor
		{
			get { return currentCursor; }
		}

		public List<string> ModeList
		{
			get { return modeList; }
		}

		public string DefaultMode
		{
			get { return defaultMode; }
			set { defaultMode = value; }
		}

		public List<IRenderTool> Dependencies
		{
			get { return dependencies; }
		}

		public List<IRenderTool> Conflicts
		{
			get { return conflicts; }
		}

		public void BuildConflicts(List<IRenderTool> parallelTools)
		{
			dependencies = new List<IRenderTool>();
			conflicts = new List<IRenderTool>();
			//conflicts.Add(ToolManager.PDFTool);

            foreach (IRenderTool tool in parallelTools)
            {
                if (tool.GetName().StartsWith("Sketch") ||
                    tool.GetName().StartsWith("Select") ||
                    tool.GetName().StartsWith("Point"))
                {
                    conflicts.Add(tool);
                }
            }

            //conflicts.Add(ToolManager.SketchTool);
            //conflicts.Add(ToolManager.SelectTool);
            //conflicts.Add(ToolManager.PointInspectTool);

			modeList = new List<string>();
			modeList.Add("Add new waypoint");
			modeList.Add("Delete waypoints");
			modeList.Add("Clear path");
			modeList.Add("Send path");

			defaultMode = "Add new waypoint";
		}

		public void AddConflict(IRenderTool tool)
		{
			conflicts.Add(tool);
		}


		/// <summary>
		/// TempDeactivate and TempReactivate : temporarily deactivate/reactivate a tool without disabling it (use carefully)
		/// </summary>
		public void TempDeactivate()
		{
			tempActive = false;
		}
		public void TempReactivate()
		{
			tempActive = true;
		}


		public void EnableMode(string modeName)
		{
			modeNewWaypoint = false;
			modeDeleteWaypoint = false;
			modeMoveWaypoint = false;

			if (modeName.Equals("Add new waypoint"))
			{
				tempActive = true;
				modeNewWaypoint = true;
				currentCursor = Cursors.Cross;
				if (newPath.WaypointList.Count > 0)
				{
					defaultMode = "Send path";
				}
			}
			else if (modeName.Equals("Delete waypoints"))
			{
				tempActive = true;
				currentCursor = Cursors.NoMove2D;
				modeDeleteWaypoint = true;
				defaultMode = "Add new waypoint";
			}
			else if (modeName.Equals("Move waypoint"))
			{
				tempActive = true;
				modeNewWaypoint = true;
				modeMoveWaypoint = true;
				currentCursor = Cursors.Cross;
			}
			else if (modeName.Equals("Clear path"))
			{
				modeNewWaypoint = true;
				newPath.WaypointList.Clear();
				newPath.LineSegmentList.Clear();
				defaultMode = "Add new waypoint";
				if (UpdateDefault != null)
					UpdateDefault(this, new EventArgs());
				clearIndicator = true;
			}
			else if (modeName.Equals("Send path"))
			{
				modeNewWaypoint = true;
				ToolCompleted(this, new PathToolCompletedEventArgs(newPath));
				newPath.WaypointList.Clear();
				newPath.LineSegmentList.Clear();
				defaultMode = "Add new waypoint";
				if (UpdateDefault != null)
					UpdateDefault(this, new EventArgs());
				shouldDraw = false;
			}
			if(UpdateDefault != null)
				UpdateDefault(this, new EventArgs());
		}

		#endregion

	}

	#region Define EventArgs

	/// <summary>
	/// When the path is completed to send, send along the path
	/// </summary>
	public class PathToolCompletedEventArgs : EventArgs
	{
		//the linepath
		PathToRobotRenderer wpPath;

		//the accessor (property) which wraps up properties
		public PathToRobotRenderer Path
		{
			get { return wpPath; }
		}

		public PathToolCompletedEventArgs(PathToRobotRenderer p)
		{
			wpPath = p;
		}
	}
    

}

	#endregion
