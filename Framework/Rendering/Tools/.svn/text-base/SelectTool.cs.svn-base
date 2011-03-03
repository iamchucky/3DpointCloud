using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Magic.Common;
using Magic.Rendering.Renderables;
using System.Drawing.Drawing2D;

namespace Magic.Rendering
{
	public class SelectTool : IRenderTool
	{
		bool isActive = false;
		bool tempActive = true;
		private PointF origTrans;
        bool zoomPanOnly = false;

		Rect selectionRectangle;

		private PointF downPoint;
		private PointF currentPoint;

		private Cursor currentCursor = Cursors.Default;
        public event EventHandler<GestureExpHRIEventArgs> GestureExpData;

		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;

        private ToolManager myToolManager;
		public SelectTool(ToolManager tm)
		{
            this.myToolManager = tm;

			//list of possible modes
			modeList = new List<string>();
			modeList.Add("Select/Deselect");

			//tool's default mode
			defaultMode = "Select/Deselect";
		}


		#region IRenderTool Members

		public Cursor Cursor
		{
			get { return currentCursor; }
		}

		
		public void OnMouseUp(Renderer r, MouseEventArgs e)
		{
            if (tempActive == true)
            {
                currentCursor = Cursors.Default;
                Vector2 worldMouseLoc = Vector2.FromPointF(r.ScreenToWorld(
                                        new PointF(e.X, e.Y)));
                if (e.Button == MouseButtons.Right)
                {
                    //if (GestureExpData != null)
                    //    GestureExpData(this, new GestureExpSelectToolEventArgs("right up click, at " + e.X + ", " + e.Y + " (screen), " + worldMouseLoc.X + ", " + worldMouseLoc.Y + " (world)"));
                }
                else if ((e.Button == MouseButtons.Left) && (!zoomPanOnly))
                {
                    if (selectionRectangle.Equals(new Rect()))
                    {
                        // Send mouseUp to every IMouseInteract that intersects the mouse click location   
                        foreach (ISelectable renderable in r.Selectables)
                        {
                            if (renderable == null)
                            {
                                continue;
                            }
                            else if (!myToolManager.PathTool.TempActive)//if (!((ToolManager.PathTool.IsActive) && (ToolManager.PathTool.TempActive)))
                            {
                                if (renderable.GetBoundingPolygon().IsInside(worldMouseLoc))
                                {
                                    if (renderable.IsSelected)
                                    {
                                        renderable.OnDeselect();
                                        if (GestureExpData != null)
                                            GestureExpData(this, new GestureExpHRIEventArgs("Tool:SelectTool|left click up|deselect using click|" + e.X + "|" + e.Y + "|(screen)|" + worldMouseLoc.X + "|" + worldMouseLoc.Y + "|(world)"));
                                    }
                                    else
                                    {
                                        renderable.OnSelect();
                                        if (GestureExpData != null)
                                            GestureExpData(this, new GestureExpHRIEventArgs("Tool:SelectTool|left click up|select using click|" + e.X + "|" + e.Y + "|(screen)|" + worldMouseLoc.X + "|" + worldMouseLoc.Y + "|(world)"));
                                    }
                                    if ((myToolManager.PathTool.IsActive) && (!myToolManager.PathTool.TempActive))
                                    {
                                        myToolManager.PathTool.TempReactivate();
                                        tempActive = false;
                                    }
                                }
                            }
                        }
                        //if (GestureExpData != null)
                        //    GestureExpData(this, new GestureExpHRIEventArgs("Tool:SelectTool left click up   no change   " + e.X + " " + e.Y + " (screen)    " + worldMouseLoc.X + " " + worldMouseLoc.Y + " (world)"));
                    }
                    else
                    {
                        selectionRectangle = new Rect();
                        //if (GestureExpData != null)
                        //    GestureExpData(this, new GestureExpHRIEventArgs("Tool:SelectTool|left click up|draw box|" + e.X + "|" + e.Y + "|(screen)|" + worldMouseLoc.X + "|" + worldMouseLoc.Y + "|(world)"));
                        if ((myToolManager.PathTool.IsActive) && (!myToolManager.PathTool.TempActive))
                        {
                            myToolManager.PathTool.TempReactivate();
                            tempActive = false;
                        }
                    }
                }
            }
		}

		public void OnMouseDown(Renderer r, MouseEventArgs e)
		{
			if (tempActive == true)
			{
				downPoint = new Point(e.X, e.Y);
				origTrans = r.Translation;
				currentCursor = Cursors.NoMove2D;
			}
			Vector2 worldMouseLoc = Vector2.FromPointF(r.ScreenToWorld(new PointF(e.X, e.Y)));
			if (e.Button == MouseButtons.Right)
			{
                //if (GestureExpData != null)
                //    GestureExpData(this, new GestureExpSelectToolEventArgs("right down click, at " + e.X + ", " + e.Y + " (screen), " + worldMouseLoc.X + ", " + worldMouseLoc.Y + " (world)"));
			}
			else if (e.Button == MouseButtons.Left)
			{
                //if (GestureExpData != null)
                //    GestureExpData(this, new GestureExpHRIEventArgs("Tool:SelectTool|left click down|no change|" + e.X + "|" + e.Y + "|(screen)|" + worldMouseLoc.X + "|" + worldMouseLoc.Y + "|(world)"));
			}
		
		}

		private Vector2 TopLeftPoint(Vector2 pt1, Vector2 pt2)
		{
			if (pt1.X > pt2.X)
			{
				if (pt1.Y > pt2.Y) return pt2;
				else return new Vector2(pt2.X, pt1.Y);
			}
			else
			{
				if (pt1.Y < pt2.Y) return pt1;
				else return new Vector2(pt1.X, pt2.Y);
			}
		}

		public void OnMouseMove(Renderer r, MouseEventArgs e)
		{
            if ((tempActive == true) && (!((myToolManager.PathTool.IsActive) && (myToolManager.PathTool.TempActive))))
			{
				Vector2 worldMouseLoc = Vector2.FromPointF(
						r.ScreenToWorld(e.Location));

				if (r.CurrentCamera == r.CamOrtho)
				{
					if ((e.Button == MouseButtons.Left)&&(!zoomPanOnly))
					{
						Vector2 topLeftPoint;
						Vector2 worldDownPoint = Vector2.FromPointF(
								r.ScreenToWorld(downPoint));
						double width = Math.Abs(
								worldDownPoint.X - worldMouseLoc.X);
						double height = Math.Abs(
								worldDownPoint.Y - worldMouseLoc.Y);
						topLeftPoint = TopLeftPoint(worldDownPoint, worldMouseLoc);
						selectionRectangle = new Rect(topLeftPoint.X,
								topLeftPoint.Y, width, height);

						foreach (ISelectable renderable in r.Selectables)
						{
							if (renderable == null) continue;
                            if ((selectionRectangle.Overlaps(renderable.GetBoundingPolygon().CalculateBoundingRectangle()))&&
                                (!renderable.IsSelected))
                            {
                                renderable.OnSelect();
                                GestureExpData(this, new GestureExpHRIEventArgs("Tool:SelectTool|box drag|select using box|" + renderable.GetName() + "|" + renderable.GetBoundingPolygon().Center.X.ToString() + "|" + renderable.GetBoundingPolygon().Center.Y.ToString() + "|(world)"));
                            }
                            else if ((!selectionRectangle.Overlaps(renderable.GetBoundingPolygon().CalculateBoundingRectangle())) &&
                                (renderable.IsSelected))
                            {
                                renderable.OnDeselect();
                                GestureExpData(this, new GestureExpHRIEventArgs("Tool:SelectTool|box drag|deselect using box|" + renderable.GetName() + "|" + renderable.GetBoundingPolygon().Center.X.ToString() + "|" + renderable.GetBoundingPolygon().Center.Y.ToString() + "|(world)"));
                            }
						}
					}
					else if (e.Button == MouseButtons.Right)
					{
						int dx = (int)(e.X - downPoint.X);
						int dy = (int)(-e.Y + downPoint.Y);
						PointF newTranslation = new PointF(
								origTrans.X - dx / r.WorldTransform.Scale,
								origTrans.Y - dy / r.WorldTransform.Scale);
						r.Translation = newTranslation;
					}
				}
				else if (r.CurrentCamera == r.CamFree && e.Button == MouseButtons.Left)
				{
					r.CamFree.Yaw((e.X - currentPoint.X) / 3);
					r.CamFree.Pitch((e.Y - currentPoint.Y) / 2);
				}
				else if (r.CurrentCamera == r.CamChase)
				{
					r.CamChase.Pitch((e.Y - currentPoint.Y) / 2);
				}
				currentPoint = new PointF(e.X, e.Y);
			}
		}


        public void BuildConflicts(List<IRenderTool> parallelTools)
        {
            dependencies = new List<IRenderTool>();
            conflicts = new List<IRenderTool>();

            foreach (IRenderTool tool in parallelTools)
            {
                if (tool.GetName().StartsWith("Point") ||
                    tool.GetName().StartsWith("Sketch") ||
                    tool.GetName().StartsWith("Path"))
                {
                    conflicts.Add(tool);
                }
            }
			//conflicts.Add(ToolManager.PDFTool);
            //conflicts.Add(ToolManager.SketchTool);
            //conflicts.Add(ToolManager.PointInspectTool);
            //conflicts.Add(ToolManager.PathTool);
		}

        public void ZoomPanOnly()
        {
            zoomPanOnly = true;
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
			if(modeName.Equals(this.modeList.First()))
			{
                myToolManager.PathTool.TempDeactivate();
				this.tempActive = true;
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
			return "SelectTool";
		}

		public void Draw(Renderer r)
		{
			if (isActive)
			{
				GLUtility.DrawRectangle(new GLPen(Color.Red, 1.0f),
						new RectangleF((float)selectionRectangle.x,
								(float)selectionRectangle.y,
								(float)selectionRectangle.width,
								(float)selectionRectangle.height));
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
	}
}