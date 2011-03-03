using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Rendering;
using System.Windows.Forms;
using System.Drawing;
using Magic.Common;

namespace Magic.Rendering
{
	class PolygonTool : IRenderToolWithResult<PolygonToolCompletedEventArgs>
	{
		#region Members
		private bool isActive;

		/// <summary>
		/// The points already created
		/// </summary>
		private List<Vector2> currentPoints;

		/// <summary>
		/// where the mouse point is currently
		/// </summary>
		private PointF mousePoint;

		/// <summary>
		/// the icon of the cursor to display
		/// </summary>
		private Cursor currentCursor = Cursors.Cross;
		#endregion

        private ToolManager myToolManager;
		public PolygonTool(ToolManager tm)
        {
            this.myToolManager = tm;
			isActive = false;
			currentPoints = new List<Vector2>();
			mousePoint = new PointF();
		}

		#region IRenderToolWithResult<PolygonToolCompletedEventArgs> Members

		public event EventHandler<PolygonToolCompletedEventArgs> ToolCompleted;

		#endregion

		#region IRenderTool Members

		public void OnMouseUp(Renderer r, System.Windows.Forms.MouseEventArgs e)
		{
			//throw new NotImplementedException();
		}

		public void OnMouseDown(Renderer r, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				currentPoints.Add(Vector2.FromPointF(r.ScreenToWorld(e.Location)));
			else if (e.Button == MouseButtons.Right)
			{
				ToolCompleted(this, new PolygonToolCompletedEventArgs(currentPoints));
				currentPoints.Clear();
			}
		}

		public void OnMouseMove(Renderer r, System.Windows.Forms.MouseEventArgs e)
		{
			mousePoint = new PointF(e.X, e.Y);
		}

		public bool IsActive
		{
			get
			{ return isActive; }
			set
			{ isActive = value; }
		}

		public Cursor Cursor
		{
			get { return currentCursor; }
		}

		public List<string> ModeList
		{
			get { throw new NotImplementedException(); }
		}

		public string DefaultMode
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

		public List<IRenderTool> Conflicts
		{
			get { throw new NotImplementedException(); }
		}

        public void BuildConflicts(List<IRenderTool> lt)
		{
			throw new NotImplementedException();
		}

		public void EnableMode(string modeName)
		{
			throw new NotImplementedException();
		}

		public void TempDeactivate()
		{
			throw new NotImplementedException();
		}

		public void TempReactivate()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IRender Members

		public string GetName()
		{
			return "Polygon Tool";
		}

		public void Draw(Renderer cam)
		{
			Vector2 lastPoint = currentPoints.Count > 0 ? currentPoints.ElementAt<Vector2>(0) : new Vector2();
			foreach (Vector2 p in currentPoints)
			{
				GLUtility.DrawCircle(new GLPen(Color.Chartreuse, 1.0f), new PointF((float)p.X, (float)p.Y), 0.25f);
				if (!p.Equals(currentPoints.ElementAt<Vector2>(0)))
				{
					GLUtility.DrawLine(new GLPen(Color.Chartreuse, 1.0f), lastPoint, p);
					lastPoint = p;
				}

			}
		}

		public void ClearBuffer()
		{
			throw new NotImplementedException();
		}

		public bool VehicleRelative
		{
			get { throw new NotImplementedException(); }
		}

		public int? VehicleRelativeID
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}

	public class PolygonToolCompletedEventArgs : EventArgs
	{
		private List<Vector2> points;
		public List<Vector2> Points { get { return points; } }
		public PolygonToolCompletedEventArgs(List<Vector2> ps)
		{
			points = ps;
		}
	}
}
