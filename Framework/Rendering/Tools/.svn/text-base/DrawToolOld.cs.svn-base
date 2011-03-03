using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Magic.Common;
using Magic.Common.Path;
using Magic.Rendering;
using Magic.Rendering.Renderables;
using System.IO;
using System.Threading;


namespace Magic.Rendering
{
    /// <summary>
    /// allows a path to be drawn from several waypoints
    /// </summary>
    public class DrawToolOld : IRenderToolWithResult<DrawToolOldCompletedEventArgs>
    {
        //indicate if this tool is active
        private bool isActive = false;

        /// <summary>
        /// create events when tool completes, a gesture is cleared, and a new gesture is assigned
        /// </summary>
		public event EventHandler<DrawToolOldCompletedEventArgs> ToolCompleted;
		public event EventHandler<CloseGUIEventArgs> CloseGUI;
		public event EventHandler<NewGestureEventArgs> NewGesture;

		// assign string variables that will be used for saving
		string TargetFolder = "//10.0.0.148/public/GestureData";
		string gestureName;// = "CIRCLE";
		string RealUserName;// = "Subject01";
		string UserName = "junk";// = "Subject01";

		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;

		// keep a list of allowable gestures
		List<string> gestureList = new List<string>();

		// variables used for drawing
		int startTime = -1;
		bool isDrawing = false;
        int countStrokes = 0;
        int pixelWidth = 4;
        int pixelHeight = 4;
		float w = new float();
		float h = new float(); 
		Point mousePixel = new Point();
        Point worldPixel = new Point();
        Point lastPixel = new Point();
		Vector2 drawPixel = new Vector2();
		Vector2 drawPixel2 = new Vector2();
		List<Vector4> pixelList = new List<Vector4>();
        List<PointF> drawPixelList = new List<PointF>();
        Vector4 logPixelData = new Vector4();
        RectangleF pixelRect = new RectangleF();
		Cursor currentCursor = Cursors.Default;
		public static PointF ctrPtWorld = new PointF();
		public static PointF pthStartWorld = new PointF();
		public static PointF pthEndWorld = new PointF();
		public static PointF ln1StartWorld = new PointF();
		public static PointF ln1EndWorld = new PointF();
		public static PointF ln2StartWorld = new PointF();
		public static PointF ln2EndWorld = new PointF();
		public static PointF ln3StartWorld = new PointF();
		public static PointF ln3EndWorld = new PointF();

		// the number of gestures we want to collect (each)
		int gestureCount = 1;
		int circleCount = 0;
		int squareCount = 0;
		int sevenCount = 0;
		int eightCount = 0;
		int triangleCount = 0;
		int pathCount = 0;
		int arrowCount = 0;
		int xCount = 0;
		int sentenceCount = 0;
		int loopIdx = 0;

		// points for drawing goals/obstacles
		List<PointF> pointList = new List<PointF>();
		Random random = new Random(); 
		int rn = new int();
		double rd = new double();
		double rn1 = new double();
		double rn2 = new double();

        private ToolManager myToolManager;
		public DrawToolOld(ToolManager tm)
        {
            this.myToolManager = tm;
			//list of possible modes
			modeList = new List<string>();
			modeList.Add("Draw");

			//tool's default mode
			defaultMode = "Draw";
		}


        public void BuildConflicts(List<IRenderTool> lt)
		{
			dependencies = new List<IRenderTool>();
			conflicts = new List<IRenderTool>();
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

		/// <summary>
		/// set new target folder for saving data
		///    warning : new target folder (and required sub-folders) must already exist!
		/// </summary>
		/// <param name="NewTargetFolder"></param>
		public void SetTargetFolder(string NewTargetFolder)
		{
			TargetFolder = NewTargetFolder;
		}

		/// <summary>
		/// load gesture names
		///    note : should only be done once at the beginning of the run
		///    note : names correspond to the folders data will be saved to
		/// </summary>
		/// <returns></returns>
		public List<string> loadGestureNames()
		{
			//gestureList.Add("CIRCLE");
			//gestureList.Add("SQUARE");
			//gestureList.Add("SEVEN");
			//gestureList.Add("EIGHT");
			//gestureList.Add("ARROW");
			//gestureList.Add("PATH");
			//gestureList.Add("X");
			//gestureList.Add("TRIANGLE");
			gestureList.Add("SENTENCE");
			UpdatePointList();
			return gestureList;
		}
		public List<string> loadGestureNamesSingle()
		{
			gestureList.Add("SQUARE");
			gestureList.Add("SEVEN");
			gestureList.Add("EIGHT");
			gestureList.Add("ARROW");
			gestureList.Add("TRIANGLE");
			UpdatePointList();
			return gestureList;
		}
		public List<string> loadGestureNamesNo()
		{
			gestureList.Add("CIRCLE");
			gestureList.Add("SQUARE");
			gestureList.Add("ARROW");
			gestureList.Add("PATH");
			gestureList.Add("TRIANGLE");
			UpdatePointList();
			return gestureList;
		}
		public void SetUser(string NewUserName)
		{
			RealUserName = NewUserName;
			gestureCount = 1;
			loopIdx = 0;
		}

		/// <summary>
		/// change to a new gesture
		/// </summary>
		/// <param name="gestureList"></param>
		/// <returns></returns>
		public string ChangeGesture(List<string> gestureList)
		{
			rn = RandomNumberInt(0,gestureList.Count);

			try
			{
				gestureName = gestureList.ElementAt(rn);
				if (gestureName.Equals("CIRCLE"))
				{
					circleCount++;
					if (circleCount + 1 > gestureCount)
						gestureList.Remove("CIRCLE");
				}
				else if (gestureName.Equals("SQUARE"))
				{
					squareCount++;
					if (squareCount + 1 > gestureCount)
						gestureList.Remove("SQUARE");
				}
				else if (gestureName.Equals("ARROW"))
				{
					arrowCount++;
					if (arrowCount + 1 > gestureCount)
						gestureList.Remove("ARROW");
				}
				else if (gestureName.Equals("SEVEN"))
				{
					sevenCount++;
					if (sevenCount + 1 > gestureCount)
						gestureList.Remove("SEVEN");
				}
				else if (gestureName.Equals("EIGHT"))
				{
					eightCount++;
					if (eightCount + 1 > gestureCount)
						gestureList.Remove("EIGHT");
				}
				else if (gestureName.Equals("TRIANGLE"))
				{
					triangleCount++;
					if (triangleCount + 1 > gestureCount)
						gestureList.Remove("TRIANGLE");
				}
				else if (gestureName.Equals("PATH"))
				{
					pathCount++;
					if (pathCount + 1 > gestureCount)
						gestureList.Remove("PATH");
				}
				else if (gestureName.Equals("X"))
				{
					xCount++;
					if (xCount + 1 > gestureCount)
						gestureList.Remove("X");
				}
				else if (gestureName.Equals("SENTENCE"))
				{
					sentenceCount++;
					if (sentenceCount + 1 > gestureCount)
						gestureList.Remove("SENTENCE");
				}
			}
			catch(ArgumentOutOfRangeException)
			{
				gestureName = "Data collection complete!";
				if (gestureList.Count == 0)
				{
					if (loopIdx == 1)
					{
						Console.WriteLine("FINISHED!");
						CloseGUI(this, new CloseGUIEventArgs(UserName));
					}
					else
					{
					    loopIdx = 1;
						UserName = RealUserName;
						gestureCount = 25;

					//    if (RealUserName.EndsWith("_onestroke"))
					//        ChangeGesture(loadGestureNamesSingle());
					//    else if (RealUserName.EndsWith("_X"))
					//        ChangeGesture(loadGestureNamesNo());
					//    else
						ChangeGesture(loadGestureNames());

						UpdatePointList();
					}
					circleCount = 0;
					squareCount = 0;
					sevenCount = 0;
					eightCount = 0;
					triangleCount = 0;
					pathCount = 0;
					arrowCount = 0;
					xCount = 0;
					sentenceCount = 0;

				}
			}
			UpdatePointList();
			NewGesture(this, new NewGestureEventArgs(gestureName));
			return gestureName;
		}

		/// <summary>
		/// update the list of points used for drawing
		///    note : should be done every time we set a new gesture!
		/// </summary>
		public void UpdatePointList()
		{
			pointList.Clear();
			while (pointList.Count < 8)
			{
				rn1 = RandomNumber(50, 340);
				rn2 = RandomNumber(50, 340);
				pointList.Add(new PointF((float)rn1, (float)rn2));
			}
		}

		/// <summary>
		/// manual finish option (for tablet)
		/// </summary>
		public void ManualFinish()
		{
			if (pixelList.Count > 0)
			{
				ToolCompleted(this, new DrawToolOldCompletedEventArgs(pixelList, TargetFolder, UserName, gestureName, pointList, pixelWidth, pixelHeight));			
				countStrokes = 0;
				isDrawing = false;
				pixelList = new List<Vector4>();
				drawPixelList = new List<PointF>();
				gestureName = ChangeGesture(gestureList);
			}
			//NewGesture(this, new NewGestureEventArgs(gestureName));
		}

		/// <summary>
		/// get a new random number (double) between min and max
		///    note : min and max are also doubles
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public double RandomNumber(double min, double max)
		{
			rd = random.NextDouble();
			return rd * (max - min) + min;
		}

		/// <summary>
		/// get a new random number (int) between min and max
		///    note : min and max are also ints
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public int RandomNumberInt(int min, int max)
		{
			return random.Next(min, max);
		}

		/// <summary>
		/// On left-mouse down, log pixel data (x,y,z,t,stroke)
		/// On right-mouse down, complete the gesture
		/// </summary>
		/// <param name="r"></param>
		/// <param name="e"></param>
        public void OnMouseDown(Renderer r, MouseEventArgs e)
        {
			// log pixel data
			if (e.Button == MouseButtons.Left)
			{
				if (countStrokes < 0)
				{
					countStrokes = 0;
				}
				isDrawing = true;
				currentCursor = Cursors.Arrow;
				countStrokes = countStrokes + 1;
				w = (float).13 * pixelWidth / 4;
				h = (float).13 * pixelHeight / 4;
			}

			// fix for when time loops
            if (startTime.Equals(-1))
            {
				startTime = DateTime.Now.DayOfYear *1000*60*24*365 + DateTime.Now.Hour * 60000 * 24 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
			}

			// complete gesture and reset 
			if (e.Button == MouseButtons.Right)
			{
				if (pixelList.Count > 0)
				{
					ToolCompleted(this, new DrawToolOldCompletedEventArgs(pixelList, TargetFolder, UserName, gestureName, pointList, pixelWidth, pixelHeight));
					countStrokes = 0;
					isDrawing = false;
					pixelList = new List<Vector4>();
					drawPixelList = new List<PointF>();
					gestureName = ChangeGesture(gestureList);
					//NewGesture(this,new NewGestureEventArgs(gestureName));
				}
			}
        }

		/// <summary>
		/// OnMouseMove : log & draw pixel data if a gesture is being drawn
		/// </summary>
		/// <param name="r"></param>
		/// <param name="e"></param>
        public void OnMouseMove(Renderer r, MouseEventArgs e)
        {
			if (isDrawing)
			{
				mousePixel.X = e.X / pixelWidth;
				mousePixel.Y = e.Y / pixelHeight;
				if (mousePixel.X <= 100 + pixelWidth / 2 &&
					mousePixel.Y <= 100 + pixelHeight / 2 &&
					mousePixel.X >= 0 &&
					mousePixel.Y >= 0)
				{
					if (!lastPixel.Equals(mousePixel))
					{
						int currentTime = DateTime.Now.DayOfYear * 1000 * 60 * 24 * 365 + DateTime.Now.Hour * 60000 * 24 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
						int deltaTime = (currentTime - startTime);
						lastPixel = mousePixel;
						logPixelData.X = mousePixel.X;
						logPixelData.Y = mousePixel.Y;
						logPixelData.Z = deltaTime;     // time
						logPixelData.W = countStrokes;  // stroke #

						pixelList.Add(logPixelData);
						worldPixel.X = mousePixel.X * pixelWidth + pixelWidth / 2;
						worldPixel.Y = mousePixel.Y * pixelWidth + pixelHeight / 2;
						PointF p = r.ScreenToWorld(worldPixel);
						drawPixelList.Add(p);
					}
				}
			}
        }

		// stop drawing
        public void OnMouseUp(Renderer r, MouseEventArgs e)
        {
			currentCursor = Cursors.Cross;
			if(e.Button == MouseButtons.Left)
			{
				isDrawing = false;
				lastPixel = Point.Empty;
			}
        }

		/// <summary>
		/// clear the current gesture data (but don't go to new gesture)
		/// </summary>
		public void ResetCharacter()
		{
			countStrokes = 0;
			isDrawing = false;
			pixelList = new List<Vector4>();
			drawPixelList = new List<PointF>();
		}

		/// <summary>
		/// clear just the last stroke to be drawn
		/// </summary>
		public void ResetStroke()
		{
			List<Vector4> dummyPixelList = new List<Vector4>();
			List<PointF> dummyDrawPixelList = new List<PointF>();
			int i = 0;
			foreach (Vector4 points in pixelList)
			{
				if(points.W < countStrokes)
				{
					dummyPixelList.Add(new Vector4(points.X, points.Y, points.Z, points.W));
					dummyDrawPixelList.Add(drawPixelList.ElementAt(i));
				}
				i++;
			}
			pixelList = dummyPixelList;
			drawPixelList = dummyDrawPixelList;
			countStrokes = countStrokes - 1;
		}


		/// <summary>
		/// Drawing code
		/// </summary>
		/// <param name="r"></param>
        public void Draw(Renderer r)
        {
			if (pointList.Count == 0)
				UpdatePointList();
			
			if ((gestureName == "CIRCLE") || (gestureName == "SQUARE") || (gestureName == "TRIANGLE"))
			{
				//DrawToolOld.ResetRefPoints();
				ctrPtWorld = r.ScreenToWorld(pointList.ElementAt(0));
				GLUtility.DrawCircle(new GLPen(Color.Blue, 0.2f), ctrPtWorld, 0.3f);
			}

			else if (gestureName == "X")
			{
				//DrawToolOld.ResetRefPoints();
				ctrPtWorld = r.ScreenToWorld(pointList.ElementAt(0));
				GLUtility.DrawCross(new GLPen(Color.Red, 0.2f), new Vector2(ctrPtWorld.X,ctrPtWorld.Y), 0.5f);
			}

			else if ((gestureName == "ARROW") || (gestureName == "PATH") || (gestureName == "SENTENCE"))
			{
				//DrawToolOld.ResetRefPoints();
				pthStartWorld = r.ScreenToWorld(pointList.ElementAt(0));
				pthEndWorld = r.ScreenToWorld(pointList.ElementAt(1));
				GLUtility.DrawCircle(new GLPen(Color.Blue, 0.2f), pthStartWorld, 0.3f);
				GLUtility.DrawCross(new GLPen(Color.Red, 0.2f), new Vector2(pthEndWorld.X, pthEndWorld.Y), 0.5f);
			}
			else
			{
				//DrawToolOld.ResetRefPoints();
			}
			if ((gestureName == "PATH")|| (gestureName == "SENTENCE"))
			{
				//DrawToolOld.ResetRefPoints();
				ln1StartWorld = r.ScreenToWorld(pointList.ElementAt(2));
				ln1EndWorld = r.ScreenToWorld(pointList.ElementAt(3));
				ln2StartWorld = r.ScreenToWorld(pointList.ElementAt(4));
				ln2EndWorld = r.ScreenToWorld(pointList.ElementAt(5));
				ln3StartWorld = r.ScreenToWorld(pointList.ElementAt(6));
				ln3EndWorld = r.ScreenToWorld(pointList.ElementAt(7));
				GLUtility.DrawLine(new GLPen(Color.Black, 1.0f), ln1StartWorld, ln1EndWorld);
				GLUtility.DrawLine(new GLPen(Color.Black, 1.0f), ln2StartWorld, ln2EndWorld);
				GLUtility.DrawLine(new GLPen(Color.Black, 1.0f), ln3StartWorld, ln3EndWorld);
			}
            
			// draw pixels
            if (pixelList.Count>0)
            {
                foreach (PointF v3 in drawPixelList)
                {
					drawPixel.X = ((float)v3.X - (w / 2));
					drawPixel.Y = ((float)v3.Y - (h / 2));

					drawPixel2.X = ((float)v3.X - (w / 4));
					drawPixel2.Y = ((float)v3.Y - (h / 4));

					float scale = r.Scale();

					pixelRect = new RectangleF((float)drawPixel.X, (float)drawPixel.Y, w, h);
					RectangleF pixelRect2 = new RectangleF((float)drawPixel2.X, (float)drawPixel2.Y, w / 2, h / 2);

					GLUtility.DrawRectangle(new GLPen(Color.Black, 0.1f), pixelRect);
					GLUtility.DrawRectangle(new GLPen(Color.Black, 0.1f), pixelRect2);
				}
            }
        }
        #endregion

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }


        #region IRender Members

        public string GetName()
        {
            return "Draw";
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


    #region Define EventArgs
    /// <summary>
    /// When the gesture is finished, write the path data to file
    /// </summary>
    public class DrawToolOldCompletedEventArgs : EventArgs
    {
		List<Vector4> pixelList;

		public List<Vector4> PixelList
		{
			get { return pixelList; }
		}

		public DrawToolOldCompletedEventArgs(List<Vector4> lp, string TargetFileName, string UserName, string gestureName,List<PointF> pointList,int pixelWidth,int pixelHeight)
        {
			pixelList = lp;
			List<PointF> refPointsList = new List<PointF>(); ;
			PointF pt = new PointF();
			foreach (PointF pt2 in pointList)
			{
				pt.X = pt2.X / pixelWidth;
				pt.Y = pt2.Y / pixelHeight;
				refPointsList.Add(pt);
			}
			WritePathTotextfile(pixelList,TargetFileName, UserName,gestureName);
			WritePathWithRefPointsTotextfile(refPointsList, pixelList, TargetFileName, UserName + "Points", gestureName);
		}
	

		public static void WritePathTotextfile(List<Vector4> pixelList, string TargetFileName, string UserName, string gestureName)
		{
			string file = TargetFileName + "/" + gestureName + "/" + UserName + ".txt";
			
			if(!Directory.Exists(TargetFileName + "/" + gestureName))
			{
				Directory.CreateDirectory(TargetFileName + "/" + gestureName);
			}

			StreamWriter fs = File.AppendText(file);
			fs.WriteLine("start of next character. User = " + UserName);
			foreach (Vector4 point in pixelList)  //GetSegmentEnumerator())
			{
                fs.WriteLine(point.X + "," + point.Y + "," + point.Z + "," + point.W + ";");
			}
			fs.Close();
		}
		public static void WritePathWithRefPointsTotextfile(List<PointF> refPointsList, List<Vector4> pixelList, string TargetFileName, string UserName, string gestureName)
		{
			string file = TargetFileName + "/" + gestureName + "/" + UserName + ".txt";

			if (!Directory.Exists(TargetFileName + "/" + gestureName))
			{
				Directory.CreateDirectory(TargetFileName + "/" + gestureName);
			}

			StreamWriter fs = File.AppendText(file);
			fs.WriteLine("start of next character. User = " + UserName);
			foreach (PointF point in refPointsList)  //GetSegmentEnumerator())
			{
				fs.WriteLine(point.X + "," + point.Y);
			}
			foreach (Vector4 point in pixelList)  //GetSegmentEnumerator())
			{
				fs.WriteLine(point.X + "," + point.Y + "," + point.Z + "," + point.W + ";");
			}
			fs.Close();
		}
    }

	public class CloseGUIEventArgs : EventArgs
	{
		string UserName;

		public string userName
		{
			get { return UserName; }
		}

		public CloseGUIEventArgs(string userName)
		{
			UserName = userName;
		}
	}

	public class ToolCleared : EventArgs
	{
		public Boolean wtf()
		{
			return true;
		}
	}

	/// <summary>
	/// get the name of the new gesture
	/// </summary>
	public class NewGestureEventArgs : EventArgs
	{
		string gestureName;

		//the accessor (property) which wraps up properties
		public string gn
		{
			get {return gestureName;}
		}

		public NewGestureEventArgs(string gn)
		{
			gestureName = gn;
		}
	}
    #endregion
}
        