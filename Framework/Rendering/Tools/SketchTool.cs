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
    public class SketchTool : IRenderToolWithResult<SketchToolCompletedEventArgs>
	{
		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// MAKE SURE THIS IS FALSE IF USING OLD HRI!
		private bool newVersion = true;
		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //indicate if this tool is active
        private bool isActive = false;
		private bool tempActive = true;
		bool shouldDraw = false;
		Cursor currentCursor = Cursors.Default;
		int startTime = -1;
		int nowTime;
		double strokeCount;
		RectangleF pixelRect;
		float basePixelWidth = 0.16f;
		float pixelWidth = 0.16f;
		GLPen pen = new GLPen(Color.Black,.1f);

		List<Vector4> pixelListWorld = new List<Vector4>();
		List<Vector4> pixelListScreen = new List<Vector4>();

        /// <summary>
        /// define EventHandlers and EventArgs
        /// </summary>
        public event EventHandler<SketchToolCompletedEventArgs> ToolCompleted;
        public event EventHandler<GestureExpHRIEventArgs> GestureExpData;
		public event EventHandler UpdateDefault;


        //list of dependencies (other tools that must be simultaneously active)
        List<IRenderTool> dependencies;

        //list of conflicts (other tools that can not be simultaneously active)
        List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;


        private ToolManager myToolManager;
		public SketchTool(ToolManager tm)
        {
            this.myToolManager = tm;
        }

        /// <summary>
        /// Operations to perform on MouseDown (takes: Renderer r, MouseEventArgs e)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="e"></param>
        #region Operations to perform on MouseDown
        public void OnMouseDown(Renderer r, MouseEventArgs e)
        {
			#region Left Click
			//check that button was a left click
			if (e.Button == MouseButtons.Left)
			{
				PointF p = r.ScreenToWorld(e.Location);

                if (GestureExpData != null)
                    GestureExpData(this, new GestureExpHRIEventArgs("Tool:SketchTool|new stroke"));
										
				if (pixelListWorld.Count == 0)
				{
					shouldDraw = true;
					startTime = DateTime.Now.DayOfYear * 1000 * 60 * 24 * 365 + DateTime.Now.Hour * 60000 * 24 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
					pixelListWorld.Add(new Vector4(p.X, p.Y, 0, 1));
					pixelListScreen.Add(new Vector4(e.X, e.Y, 0, 1));
					strokeCount = 1;
				}
				else
				{
					shouldDraw = true;
					nowTime = DateTime.Now.DayOfYear * 1000 * 60 * 24 * 365 + DateTime.Now.Hour * 60000 * 24 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
					strokeCount++;// strokeCount = pixelListScreen.Last().W + 1;
					pixelListWorld.Add(new Vector4(p.X, p.Y, nowTime - startTime, strokeCount));
					pixelListScreen.Add(new Vector4(e.X, e.Y, nowTime - startTime, strokeCount));
				}
			}
			#endregion
        }
        #endregion



        /// <summary>
        /// Operations to perform on MouseMove:
        /// </summary>
        /// <param name="r"></param>
        /// <param name="e"></param>
        #region Operations to perform on MouseMove
        public void OnMouseMove(Renderer r, MouseEventArgs e)
        {
			if (shouldDraw)
			{
				PointF p = r.ScreenToWorld(e.Location);

				nowTime = DateTime.Now.DayOfYear * 1000 * 60 * 24 * 365 + DateTime.Now.Hour * 60000 * 24 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
				pixelListWorld.Add(new Vector4(p.X, p.Y, nowTime - startTime, strokeCount));
				pixelListScreen.Add(new Vector4(e.X, e.Y, nowTime - startTime, strokeCount));
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
			shouldDraw = false;
        }
        #endregion


        /// <summary>
        /// Draw the sketch pixels
        /// </summary>
        /// <param name="r"></param>
        #region sketch drawing code

        public void Draw(Renderer r)
        {
			if (pixelListWorld.Count > 0)
			{
				foreach (Vector4 v in pixelListWorld)
				{
					PointF pixelPointWorld = new PointF((float)v.X, (float)v.Y);
					pixelRect = new RectangleF((float)pixelPointWorld.X - pixelWidth / 2, (float)pixelPointWorld.Y - pixelWidth / 2, pixelWidth, pixelWidth);
					GLUtility.FillRectangle(pen.color, pixelRect);
				}
			}
        }
        #endregion

		public bool IsDrawing
		{
			get { return shouldDraw; }
		}

		public Color ChangePenColor
		{
			set { pen.color = value; }
		}
		public float BasePixelWidth
		{
			set { basePixelWidth = value; }
			get { return basePixelWidth; }
		}
		public float PixelWidth
		{
			set { pixelWidth = value; }
			get { return pixelWidth; }
		}

		public List<Vector4> WorldPixels
		{
			get { return pixelListWorld;}
			set { pixelListWorld = value; }
		}
		public List<Vector4> ScreenPixels
		{
			get { return pixelListScreen;}
			set { pixelListScreen = value; }
		}

        #region IRender Members : GetName(), ClearBuffer(), VehicleRelative, VehicleRelativeID

        public string GetName()
        {
			return "Sketch Tool";
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

		#region IRenderTool Members : IsActive, Cursor, ModeList, DefaultMode, EnableMode, Dependencies, Conflicts, BuildConflicts()

		public bool IsActive
		{
			get
			{
				return isActive;
			}
			set{isActive = value;}
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
			get{return defaultMode;}
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

            foreach (IRenderTool tool in parallelTools)
            {
                if (tool.GetName().StartsWith("Path") ||
                    tool.GetName().StartsWith("Select") ||
                    tool.GetName().StartsWith("Angle") ||
                    tool.GetName().StartsWith("Point") ||
                    tool.GetName().StartsWith("Ruler"))
                {
                    conflicts.Add(tool);
                }
            }
			//conflicts.Add(ToolManager.PDFTool);
            //conflicts.Add(ToolManager.PathTool);
            //conflicts.Add(ToolManager.SelectTool);
            //conflicts.Add(ToolManager.AngleTool);
            //conflicts.Add(ToolManager.PointInspectTool);
            //conflicts.Add(ToolManager.RulerTool);

			modeList = new List<string>();
			modeList.Add("Send sketch");
			modeList.Add("Delete last stroke");
			modeList.Add("Clear entire sketch");

			defaultMode = "Send sketch";
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

			if (modeName.Equals("Send sketch"))
			{
				SendSketch();
			}
			else if (modeName.Equals("Delete last stroke"))
			{
				DeleteStroke();
			}
			else if (modeName.Equals("Clear entire sketch"))
			{
				ClearSketch();
			}
			UpdateDefault(this, new EventArgs());
		}

		public void SendSketch()
		{
			// SEND SKETCH STUFF HERE!!!
			ToolCompleted(this, new SketchToolCompletedEventArgs(pixelListWorld, pixelListScreen));

			tempActive = true;
			//pixelListScreen.Clear();
			//pixelListWorld.Clear();
			//shouldDraw = false;
			defaultMode = "Send sketch";
		}
		public void DeleteStroke()
		{
			if (pixelListWorld.Count > 0)
			{
				double deleteStroke = pixelListWorld.Last<Vector4>().W;

				List<Vector4> pixelListClone = new List<Vector4>(pixelListWorld);
				foreach (Vector4 v in pixelListClone)
				{
					if (v.W == deleteStroke)
					{
						pixelListWorld.Remove(v);
					}
				}

				pixelListClone = new List<Vector4>(pixelListScreen);
				foreach (Vector4 v in pixelListClone)
				{
					if (v.W == deleteStroke)
					{
						pixelListScreen.Remove(v);
					}
				}
			}
            strokeCount--;
			shouldDraw = false;
			tempActive = true;
			defaultMode = "Send sketch";
		}
		public void ClearSketch()
		{
			pixelListScreen.Clear();
			pixelListWorld.Clear();
			shouldDraw = false;
			tempActive = true;
			defaultMode = "Send sketch";
		}

		#endregion

	}


	#region Define EventArgs

	/// <summary>
    /// When the path is completed to send, send along the path
    /// </summary>
    public class SketchToolCompletedEventArgs : EventArgs
    {
		List<Vector4> worldpixels;
		List<Vector4> screenpixels;

		public List<Vector4> WorldPixels
		{
			get { return worldpixels; }
		}
		public List<Vector4> ScreenPixels
		{
			get { return screenpixels; }
		}

		public SketchToolCompletedEventArgs(List<Vector4> wp,List<Vector4> sp)
        {
			worldpixels = wp;
			screenpixels = sp;
        }
    }
}

	#endregion
