using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using System.Drawing;
using System.Windows.Forms;
using Magic.Rendering.Renderables;

namespace Magic.Rendering
{
	public class PointInspectTool : IRenderTool
	{

		Vector2 currentPoint;
		bool isActive;
		private bool tempActive = true;
		bool shouldMove = false;
		Cursor currentCursor = Cursors.Default;
        public event EventHandler<GestureExpHRIEventArgs> GestureExpData;
        bool sketchmode = true;
		string POItype = "SQUARE";
		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;

		//bool arguments for mode
		bool modeNewPOI = false;
		bool modeDeletePOI = false;
		bool modeMovePOI = false;

		//store list of POIs
		List<NotepointRenderer> poiList;
		NotepointRenderer pointToMove;

        private ToolManager myToolManager;
		public PointInspectTool(ToolManager tm)
        {
            this.myToolManager = tm;
			poiList = new List<NotepointRenderer>();
			//Notepoint pointToMove = new Notepoint();
		}

		public void ClearAllPoints()
		{
			poiList = new List<NotepointRenderer>();
		}

		public List<NotepointRenderer> GetAllPoints()
		{
			return poiList;
		}


        public void BuildConflicts(List<IRenderTool> parallelTools)
        {
            dependencies = new List<IRenderTool>();
            conflicts = new List<IRenderTool>();

            foreach (IRenderTool tool in parallelTools)
            {
                if (tool.GetName().StartsWith("Path") ||
                    tool.GetName().StartsWith("Sketch") ||
                    tool.GetName().StartsWith("Select"))
                {
                    conflicts.Add(tool);
                }
            }

            //conflicts.Add(ToolManager.SketchTool);
            //conflicts.Add(ToolManager.PathTool);
            //conflicts.Add(ToolManager.SelectTool);

			//list of possible modes
			modeList = new List<string>();
			if (sketchmode)
			{
				modeList.Add("Add new SQUARE");
				modeList.Add("Add new TRIANGLE");
				modeList.Add("Add new SPIRAL");
                modeList.Add("Take a PICTURE");
				//defaultMode = "Add new SQUARE";
			}
			else
			{
				modeList.Add("Add new POI");
				defaultMode = "Add new POI";
			}
			modeList.Add("Move existing POI");
			modeList.Add("Delete existing POI");

			//tool's default mode
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
			modeNewPOI = false;
			modeDeletePOI = false;
			modeMovePOI = false;

			if (modeName.Equals("Add new POI"))
			{
				modeNewPOI = true;
				myToolManager.SelectTool.TempDeactivate();
                myToolManager.PathTool.TempDeactivate();
			}
			if (modeName.Equals("Add new SQUARE"))
			{
				modeNewPOI = true;
				POItype = "SQUARE";
                myToolManager.SelectTool.TempDeactivate();
                myToolManager.PathTool.TempDeactivate();
			}
			if (modeName.Equals("Add new TRIANGLE"))
			{
				modeNewPOI = true;
				POItype = "TRIANGLE";
                myToolManager.SelectTool.TempDeactivate();
                myToolManager.PathTool.TempDeactivate();
			}
			if (modeName.Equals("Add new SPIRAL"))
			{
				modeNewPOI = true;
				POItype = "SPIRAL";
                myToolManager.SelectTool.TempDeactivate();
                myToolManager.PathTool.TempDeactivate();
			}
			else if (modeName.Equals("Move existing POI"))
			{
				modeMovePOI = true;
                myToolManager.SelectTool.TempDeactivate();
                myToolManager.PathTool.TempDeactivate();
			}
			else if (modeName.Equals("Delete existing POI"))
			{
				modeDeletePOI = true;
                myToolManager.SelectTool.TempDeactivate();
                myToolManager.PathTool.TempDeactivate();
			}
            else if (modeName.Equals("Take a PICTURE"))
            {
                modeNewPOI = true;
                POItype = "PICTURE";
                myToolManager.SelectTool.TempDeactivate();
                myToolManager.PathTool.TempDeactivate();
            }
		}

		public Vector2 CurrentPoint
		{
			get { return currentPoint; }
		}
		#region IRenderTool Members

		public Cursor Cursor
		{
			get { return currentCursor; }
		}

		public void OnMouseUp(Renderer r, MouseEventArgs e)
		{
			Vector2 worldMouseLoc = Vector2.FromPointF(r.ScreenToWorld(new PointF(e.X, e.Y)));
			if (e.Button == MouseButtons.Right)
			{
                //if (GestureExpData != null)
                //    GestureExpData(this, new GestureExpPointToolEventArgs("Tool:PointTool   right click up  " + e.X + ", " + e.Y + " (screen), " + worldMouseLoc.X + ", " + worldMouseLoc.Y + " (world)"));
			}
			else if(e.Button.Equals(MouseButtons.Left))
			{
				if ((modeMovePOI == true) && (shouldMove == true)) 
				{
					shouldMove = false;
                    myToolManager.SelectTool.TempReactivate();
                    myToolManager.PathTool.TempReactivate();
					modeMovePOI = false;
					if (GestureExpData != null)
                        GestureExpData(this, new GestureExpHRIEventArgs("Tool:PointTool|left click up|drop point|" + e.X + "|" + e.Y + "|(screen)|" + worldMouseLoc.X + "|" + worldMouseLoc.Y + "|(world)"));
				}
				else if (modeNewPOI == true)
				{
					modeNewPOI = false;
                    myToolManager.SelectTool.TempReactivate();
                    myToolManager.PathTool.TempReactivate();
                    //if (GestureExpData != null)
                    //    GestureExpData(this, new GestureExpPointToolEventArgs("Tool:PointTool   left click up   create point at " + e.X + ", " + e.Y + " (screen), " + worldMouseLoc.X + ", " + worldMouseLoc.Y + " (world)"));
				}
			}

		}

		public void OnMouseDown(Renderer r, MouseEventArgs e)
		{
			PointF worldPoint = r.ScreenToWorld(e.Location);
			if (e.Button == MouseButtons.Right)
			{
                //if (GestureExpData != null)
                //    GestureExpData(this, new GestureExpPointToolEventArgs("right down click, at " + e.X + ", " + e.Y + " (screen), " + worldPoint.X + ", " + worldPoint.Y + " (world)"));
			}
			else if(e.Button.Equals(MouseButtons.Left))
			{
				//PointF worldPoint = r.ScreenToWorld(e.Location);
				if (modeNewPOI == true)
				{
                    if (POItype == "PICTURE")
                    {
                        if (r.Selectables.Count() > 0)
                        {
                            foreach (ISelectable sel in r.Selectables)
                            {
                                RobotRenderer rr = sel as RobotRenderer;
                                if (rr != null && rr.IsSelected)
                                {
                                    r.AddRenderable(new FakeObjectRenderer("important", rr.GetBoundingPolygon().Center.ToPointF(), "picture"));
                                    if (GestureExpData != null)
                                    {
                                        GestureExpData(this, new GestureExpHRIEventArgs("Tool:PointTool|left click down|take picture|Robot " + rr.GetName().Substring(rr.GetName().Length-1) + "|" + e.X + "|" + e.Y + "|(screen)|" + worldPoint.X + "|" + worldPoint.Y + "|(world)"));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        NotepointRenderer newPoint = new NotepointRenderer();
                        newPoint.Name = POItype;
                        if (POItype == "SQUARE")
                        {
                            newPoint.Color = Color.DarkMagenta;
                        }
                        else if (POItype == "TRIANGLE")
                        {
                            newPoint.Color = Color.DarkGreen;
                        }
                        else if (POItype == "SPIRAL")
                        {
                            newPoint.Color = Color.DarkRed;
                        }
                        newPoint.X = worldPoint.X;
                        newPoint.Y = worldPoint.Y;
                        newPoint.Z = 0;
                        poiList.Add(newPoint);
                        r.AddRenderable(newPoint);
                        if (GestureExpData != null)
                        {
                            GestureExpData(this, new GestureExpHRIEventArgs("Tool:PointTool|left click down|create|" + newPoint.Name + "|" + e.X + "|" + e.Y + "|(screen)|" + worldPoint.X + "|" + worldPoint.Y + "|(world)"));
                        }
                    }
				}
				else if (modeDeletePOI == true)
				{
					if (poiList.Count > 0)
					{
						List<NotepointRenderer> pointsToRemove= new List<NotepointRenderer>();
						foreach (NotepointRenderer np in poiList)
						{
							if ((worldPoint.X - np.X > -.25) && (worldPoint.Y - np.Y > -.25) &&
								(worldPoint.X - np.X < .25) && (worldPoint.Y - np.Y < .25))
							{
								pointsToRemove.Add(np);
								r.RemoveRenderable(np);
								if (GestureExpData != null)
                                    GestureExpData(this, new GestureExpHRIEventArgs("Tool:PointTool|left click down|remove|" + np.Name + "|" + e.X + "|" + e.Y + "|(screen)|" + worldPoint.X + "|" + worldPoint.Y + "|(world)"));
							}
						}
						if (pointsToRemove.Count>0)
						{
							foreach (NotepointRenderer n in pointsToRemove)
							{
								poiList.Remove(n);
							}
                            myToolManager.SelectTool.TempReactivate();
                            myToolManager.PathTool.TempReactivate();
							modeDeletePOI = false;
						}
					}
				}
				else if (modeMovePOI == true)
				{
					if (poiList.Count > 0)
					{
						foreach (NotepointRenderer np in poiList)
						{
							if ((worldPoint.X - np.X > -.25) && (worldPoint.Y - np.Y > -.25) &&
								(worldPoint.X - np.X < .25) && (worldPoint.Y - np.Y < .25))
							{
								shouldMove = true;
								pointToMove = np;
								if (GestureExpData != null)
                                    GestureExpData(this, new GestureExpHRIEventArgs("Tool:PointTool|left click down|move|" + np.Name + "|" + e.X + "|" + e.Y + "|(screen)|" + worldPoint.X + "|" + worldPoint.Y + "|(world)"));
								break;
							}
						}
					}
				}
			}
		}

		public void OnMouseMove(Renderer r, MouseEventArgs e)
		{
			currentPoint = Vector2.FromPointF(r.ScreenToWorld(e.Location));

			if ((modeMovePOI == true) && (shouldMove == true))
			{
				PointF p = r.ScreenToWorld(new PointF(e.X, e.Y));
				foreach (NotepointRenderer np in poiList)
				{
					if (np.Equals(pointToMove))
					{
						NotepointRenderer mousePoint = np;
						mousePoint.X = p.X;
						mousePoint.Y = p.Y;
						poiList.Remove(np);
						poiList.Add(mousePoint);
						pointToMove = mousePoint;
						break;
					}
				}
			}
		}

		public bool IsActive
		{
			get	{return isActive;}
			set	{isActive = value;}
		}

		#endregion

		#region IRender Members

		public string GetName()
		{
			return "PointInspector";
		}

		public void Draw(Renderer cam)
		{

            GLUtility.DrawEllipse(new GLPen(Color.Red, 1.0f), new RectangleF((float)(currentPoint.X - .1), (float)(currentPoint.Y - .1), .2f, .2f));
            GLUtility.DrawString(currentPoint.X.ToString("F2") + "," + currentPoint.Y.ToString("F2"), Color.Black, currentPoint.ToPointF());

			//if (poiList.Count > 0)
			//{
			//    foreach (NotepointRenderer np in poiList)
			//    {
			//        PointF newPoint = new PointF((float)np.X,(float)np.Y);
			//        if ((shouldMove == true) && np.Equals(pointToMove))
			//        {
			//            GLUtility.DrawCircle(new GLPen(Color.Red, 0.25f), newPoint, 0.25f);
			//        }
			//        else
			//        {
			//            GLUtility.DrawCircle(new GLPen(np.Color, 0.25f), newPoint, 0.25f);
			//        }
			//    }
			//}
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
		
		public bool Sketchmode
		{
			get { return sketchmode; }
			set { sketchmode = value; }
		}
	}

}
