using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tao.Platform.Windows;
using System;
using Magic.Common;
using Magic.Common.Sensors;
using Magic.Common.Messages;
using Tao.OpenGl;

namespace Magic.Rendering
{
	/// <summary>
	/// Provides the top level interface for the rendering components. This manages mouse clicks, rendering lists, and other useful tools.
	/// </summary>
	public class Renderer
	{
		#region Singleton

		public static Renderer Instance
		{
			get
			{
				if (instance == null)
					throw new Exception("Need a default constructor");
				return instance;
			}
		}

		private static Renderer instance;
		#endregion

		public event EventHandler BeforeRendering;
		public event EventHandler AfterRendering;
		SimpleOpenGlControl control;
		object renderLock = new object();
		object robotPoseLock = new object();
		PointF translation = new PointF(0, 0);
		public event EventHandler<ToolChangedArgs> toolChanged;
		Color clearColor = Color.White;

		public PointF Translation
		{
			get { return translation; }
			set { translation = value; }
		}
		public Color ClearColor
		{
			get { return clearColor; }
			set { clearColor = value; }
		}
		PointF curMousePoint = new PointF(0, 0);
		WorldTransform w = new WorldTransform();

		public WorldTransform WorldTransform
		{
			get { return w; }
		}
		GLUtility.GLCameraOrtho camOrtho;

		public GLUtility.GLCameraOrtho CamOrtho
		{
			get { return camOrtho; }
		}
		GLUtility.GLCameraFree camFree;

		public GLUtility.GLCameraFree CamFree
		{
			get { return camFree; }
		}
		GLUtility.GLCameraChase camChase;

		public GLUtility.GLCameraChase CamChase
		{
			get { return camChase; }
		}
		GLUtility.GLCamera camPrev, camCurrent;


		List<IRender> drawables = new List<IRender>();
		List<ISelectable> selectables = new List<ISelectable>();
		List<IRenderTool> tools = new List<IRenderTool>();
		Dictionary<int, RobotPose> robotPoses = new Dictionary<int, RobotPose>();

		double fps = 30;

		//these are used for the SetOffset method
		float xOff; float yOff; float headingOff;

		System.Timers.Timer renderTimer;

		/// <summary>
		/// Gets the current rendering "camera"
		/// </summary>
		public GLUtility.GLCamera CurrentCamera
		{ get { return camCurrent; } }

		/// <summary>
		/// Constructor for the renderer
		/// </summary>
		/// <param name="control">Gives the reference to the SimpleOpenGLControl you've created on a form</param>
		/// <param name="fps">The frames per second (max) to render at. Set this to be about 30 for good performance.</param>
		public Renderer(SimpleOpenGlControl control, double fps)
		{
			this.control = control;
			this.fps = fps;
			#region UI Initialization

			camOrtho = new GLUtility.GLCameraOrtho(w);
			camFree = new GLUtility.GLCameraFree(w);
			camChase = new GLUtility.GLCameraChase(w);
			camPrev = camCurrent = camOrtho;

			#endregion

			//This is baaaaad. Fix it.
			instance = this;
		}


		/// <summary>
		/// Constructor for the renderer with a default select  tool (old behvaior)
		/// </summary>
		/// <param name="control">Gives the reference to the SimpleOpenGLControl you've created on a form</param>
		/// <param name="fps">The frames per second (max) to render at. Set this to be about 30 for good performance.</param>
		public Renderer(SimpleOpenGlControl control, double fps, bool useIntegratedSelectTool)
			: this(control, fps)
		{
			SelectTool s = new SelectTool(new ToolManager());
			AddTool(s, true);
		}

		/// <summary>
		/// This adds a renderable object to the list of items to render by the renderable.
		/// Keep in mind this is an ordered list, so items are drawn in the order they are added
		/// (i.e. items added first are drawn first and items after are drawn on top of those)
		/// Items that are selectable are ALSO added to the list of selectable renderables
		/// </summary>
		/// <param name="renderable"></param>
		public void AddRenderable(IRender renderable)
		{
			lock (renderLock)
			{
				if (renderable is ISelectable) selectables.Add((ISelectable)renderable);
				drawables.Add(renderable);
			}
		}
		public void RemoveRenderable(IRender renderable)
		{
			lock (renderLock)
			{
				if (renderable is ISelectable) selectables.Remove((ISelectable)renderable);
				drawables.Remove(renderable);
			}
		}

        public void ClearRenderables()
        {
            lock (renderLock)
            {
                drawables.Clear();
                selectables.Clear();
                tools.Clear();
            }
        }

		/// <summary>
		/// Returns the list of IRenders
		/// </summary>
		public List<IRender> Drawables
		{
			get { return drawables; }
		}

		public List<IRenderTool> Tools
		{
			get { return tools; }
		}


		public void SetCursor(Cursor c)
		{
			control.Cursor = c;
		}

		/// <summary>
		/// Returns the list of selectables
		/// </summary>
		public List<ISelectable> Selectables
		{
			get { return selectables; }
		}

		/// <summary>
		/// This adds a tool object to the list of items to render by the renderable.       
		/// </summary>
		/// <param name="renderable"></param>
		public void AddTool(IRenderTool tool)
		{
			tools.Add(tool);
		}

		public void RemoveTool(IRenderTool tool)
		{
			tools.Remove(tool);
		}

		/// <summary>
		/// This adds a tool object to the list of items to render by the renderable, and optionally activates it
		/// </summary>
		/// <param name="tool"></param>
		/// <param name="activate"></param>
		public void AddTool(IRenderTool tool, bool activate)
		{
			tools.Add(tool);
			if (activate) this.ActivateTool(tool);
		}

		/// <summary>
		/// Changes toolToActivate to active
		/// </summary>
		/// <param name="toolToActivate"></param>
		public void ActivateTool(IRenderTool toolToActivate)
		{
			foreach (IRenderTool tool in tools)
			{
				if (tool == toolToActivate)
				{
					tool.IsActive = true;
					if (toolChanged != null) toolChanged(this, new ToolChangedArgs(tool, true));
				}
				//control.Cursor = tool.Cursor;
			}
		}

		/// <summary>
		/// Changes toolToDeactivate to deactivated
		/// </summary>
		/// <param name="toolToDeactivate"></param>
		public void DeactivateTool(IRenderTool toolToDeactivate)
		{
			foreach (IRenderTool tool in tools)
			{
				if (tool == toolToDeactivate)
				{
					tool.IsActive = false;
					if (toolChanged != null) toolChanged(this, new ToolChangedArgs(tool, false));
				}
			}
			control.Cursor = Cursors.Default;
		}

		/// <summary>
		/// Activates the tool, and turns off all others
		/// </summary>
		/// <param name="toolToActivate"></param>
		public void DeactivateAllToolsExcept(IRenderTool toolToActivate)
		{
			foreach (IRenderTool tool in tools)
				tool.IsActive = false;
			ActivateTool(toolToActivate);
		}

		/// <summary>
		/// Call this so the renderer "knows" where the robots are. 
		/// This allows Renderables that have the "DrawVehicleRelative" property set
		/// to true to be drawn correctly.
		/// </summary>
		/// <param name="robotID"></param>
		/// <param name="pose"></param>
		public void UpdateRobotPose(int robotID, RobotPose pose)
		{
			if (pose == null) return;
			lock (robotPoseLock)
			{
				if (robotPoses.ContainsKey(robotID))
					robotPoses[robotID] = pose;
				else
				{
					robotPoses.Add(robotID, pose);
				}
			}
		}

		public List<int> RobotsInKey()
		{
			List<int> robotKeys = new List<int>();
			foreach (int key in robotPoses.Keys)
			{
				robotKeys.Add(key);
			}
			return robotKeys;
		}

		public RobotPose GetRobotPose(int robotID)
		{
			if (robotPoses.ContainsKey(robotID))
				return robotPoses[robotID];
			else
				return null;
		}

		/// <summary>
		/// This function MUST be called when the form with the SimpelOpenGLControl is first shown.
		/// A good place to call is the FormLoadEvent.
		/// </summary>
		public void OnFormShown()
		{
			control.InitializeContexts();
			control.AutoFinish = true;
			control.AutoSwapBuffers = true;
			control.MouseDown += new MouseEventHandler(control_MouseDown);
			control.MouseUp += new MouseEventHandler(control_MouseUp);
			control.MouseMove += new MouseEventHandler(control_MouseMove);
			w.ScreenSize = new SizeF(control.Size);
			w.Scale = 30.0f;
			GLUtility.InitGL(control.Width, control.Height, Color.White, control, false);

			xOff = 0;
			yOff = 0;
			renderTimer = new System.Timers.Timer(1.0 / fps * 1000.0);
			renderTimer.AutoReset = true;
			renderTimer.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
			renderTimer.Enabled = true;
		}

		public void Rescale(float s)
		{
			w.Scale = s;
		}

		public void Rescale(float ww,float hh)
		{
			float w2 = ww / (float)control.Width;
			float h2 = hh / (float)control.Height;
			if ((w2 >= h2) && (.75f / w2 < 50f))
				w.Scale = .75f / w2;
			else if ((w2 < h2) && (.75f / h2 < 50f))
				w.Scale = .75f / h2;
			else
				w.Scale = 50f;
		}

		public void ShowOrtho()
		{
			camPrev = camCurrent;
			camCurrent = camOrtho;
		}

		public void ShowChase()
		{
			camPrev = camCurrent;
			camCurrent = camChase;
		}

		public void ShowFree()
		{
			camPrev = camCurrent;
			camCurrent = camFree;
		}

		/// <summary>
		/// Sets the viewpoint offset
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="headingOff"></param>
		public void UpdateOffset(float x, float y, float headingOff)
		{
			this.xOff = x;
			this.yOff = y;
			this.headingOff = headingOff;
		}
		public void UpdateOffset(float x, float y)
		{
			this.xOff = x;
			this.yOff = y;
		}

		/// <summary>
		/// Handles MouseMove in OpenGL control. Sends OnMouseMove to every active control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void control_MouseMove(object sender, MouseEventArgs e)
		{

			foreach (IRenderTool t in tools)
			{
				if (t.IsActive)
				{
					lock (renderLock)
					{
						t.OnMouseMove(this, e);
					}
				}
			}


		}

		/// <summary>
		/// Handles mouseUp in OpenGl control. Sends OnMouseUp to every active control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void control_MouseUp(object sender, MouseEventArgs e)
		{
			foreach (IRenderTool t in tools)
			{
				if (t.IsActive)
				{
					lock (renderLock)
					{
						t.OnMouseUp(this, e);
					}
				}
			}
		}

		/// <summary>
		/// Handles MouseDown in OpenGL control. Sends OnMouseDown to every active control.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void control_MouseDown(object sender, MouseEventArgs e)
		{
			foreach (IRenderTool t in tools)
			{
				if (t.IsActive)
				{
					lock (renderLock)
					{
						t.OnMouseDown(this, e);
					}
				}
			}

		}

		bool isAlreadyRendering = false;
		void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (control.IsHandleCreated == false || control.IsDisposed)
			{
				renderTimer.Enabled = false;
				return;
			}
			if (isAlreadyRendering) return;
			isAlreadyRendering = true;
			control.BeginInvoke(new MethodInvoker(delegate
			{
				if (BeforeRendering != null) BeforeRendering(this, null);
				w.ScreenSize = control.Size;
				w.CenterPoint = new PointF(translation.X + xOff, translation.Y + yOff);

				lock (renderLock)
				{
					Gl.glLoadIdentity();       
					GLUtility.SetClearColor(clearColor);
					camCurrent.UpdateWithWorldTransform(w);
					camPrev.UpdateWithWorldTransform(w);
					double camT = 1.0;
					GLUtility.InitScene(camCurrent, camPrev, camT, control.Size);
					GLUtility.DrawGrid3D(1, w, -0.1f, Color.LightGray, Color.Black, Color.Red);

					//make sure everything in the rendering chain gets rendered....
					Dictionary<IRender, bool> didRender = new Dictionary<IRender, bool>(drawables.Count);
					foreach (IRender r in drawables)
						didRender.Add(r, false);
					foreach (IRender r in drawables)
					{
						if (r != null && !r.VehicleRelative)
						{
							r.Draw(this);
							if (didRender.ContainsKey(r)) didRender[r] = true;
						}
					}

					lock (robotPoseLock)
					{
						foreach (KeyValuePair<int, RobotPose> kvp in robotPoses)
						{
							GLUtility.GoToVehicleCoordinates((float)kvp.Value.yaw, new PointF((float)kvp.Value.x, (float)kvp.Value.y), false);
							foreach (IRender r in drawables)
							{
								if (r != null && r.VehicleRelative && r.VehicleRelativeID == kvp.Key)
								{ r.Draw(this); if (didRender.ContainsKey(r)) didRender[r] = true; }
							}
							GLUtility.ComeBackFromVehicleCoordinates();
						}
					}
					//check we rendered everytihng!
					foreach (KeyValuePair<IRender, bool> kvp in didRender)
					{
						if (kvp.Value == false)
						{
							//Console.WriteLine("Renderer warning: did not render " + kvp.Key.GetName() + " correctly becuase robot " + kvp.Key.VehicleRelativeID + " has no pose in the renderer. Rendering at Pose (0,0,0)");
							if (kvp.Key != null)
							{ kvp.Key.Draw(this); }
						}
					}

					//draw the tools....
					foreach (IRenderTool tool in tools)
					{
						if (tool.IsActive)
							tool.Draw(this);
					}


					control.SwapBuffers();
					if (AfterRendering != null) AfterRendering(this, null);
					isAlreadyRendering = false;
				}
			}));

		}


		public void ZoomIn()
		{
			w.Scale += .25f * w.Scale;
		}

		public float Scale()
		{
			return w.Scale;
		}

		public void ZoomOut()
		{
			w.Scale -= .25f * w.Scale;
			if (w.Scale < 1) w.Scale = 1f;
		}

		public void PanNorth(float d)
		{
			yOff = yOff + (d * (30/w.Scale));
		}
		public void PanEast(float d)
		{
			xOff = xOff + (d * (30 / w.Scale));
		}

		/// <summary>
		/// Takes a point in screen coordinates and gives back a point in world coordinates.
		/// NOTE: this only works while the renderer is in orthogonal camera view mode. 
		/// </summary>
		/// <param name="screenCoord"></param>
		/// <returns></returns>
		public PointF ScreenToWorld(PointF screenCoord)
		{
			return new PointF((((screenCoord.X - (w.ScreenSize.Width / 2)) / w.Scale) + translation.X + xOff),
								((-1 * ((screenCoord.Y - (w.ScreenSize.Height / 2)) / w.Scale)) + translation.Y + yOff));
		}

		/// <summary>
		/// Resets the view to the origin.
		/// </summary>
		public void Recenter()
		{
			translation.X = 0;
			translation.Y = 0;
		}

		/// <summary>
		/// Tells all renderable to clear their internal buffers
		/// </summary>
		public void ClearBuffers()
		{
			foreach (IRender render in drawables)
			{
				render.ClearBuffer();
			}
		}
	}

	public class ToolChangedArgs : EventArgs
	{
		private IRenderTool changedTool;
		private bool active;

		public IRenderTool ChangedTool()
		{
			return changedTool;
		}

		public bool Active()
		{
			return active;
		}

		public ToolChangedArgs(IRenderTool tool, bool active)
		{
			this.changedTool = tool;
			this.active = active;
		}
	}
}
