using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Magic.Common;
using System.Drawing;
using Magic.Rendering;

namespace Magic.Rendering
{
	public class ContextMenuTool : IRenderTool
	{

		bool isActive = false;
		private Cursor currentCursor = Cursors.Default;
		private string name = "ContextMenuTool";
		public event EventHandler<ContextMenuArgs> showContextMenu;
		private List<MenuItem> defaultMenu;

		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//default mode for tool
		string defaultMode;

        private ToolManager myToolManager;
		public ContextMenuTool(ToolManager tm)
		{
            this.myToolManager = tm;
			defaultMenu = new List<MenuItem>();
			defaultMenu.Add(new MenuItem("hello"));
			defaultMenu.Add(new MenuItem("how are you?"));

			//list of possible modes
			modeList = new List<string>();
			modeList.Add("Show Context Menu");

			//tool's default mode
			defaultMode = "Show Context Menu";
		}

        public void BuildConflicts(List<IRenderTool> parallelTools)
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

		#region IRenderTool Members

        public void OnMouseUp(Renderer r, System.Windows.Forms.MouseEventArgs e)
        {
			if (this.isActive)
			{
				Vector2 worldMouseLoc = Vector2.FromPointF(r.ScreenToWorld(
							new PointF(e.X, e.Y)));
				if (e.Button == MouseButtons.Right)
				{
					bool context = false;
					foreach (IRender renderable in r.Drawables)
					{
						IProvideContextMenu crend = renderable as IProvideContextMenu;
						if (crend == null) continue;
						if (crend.GetBoundingPolygon().IsInside(worldMouseLoc))
						{
							context = true;
							if (showContextMenu != null) showContextMenu(this, new ContextMenuArgs(crend.GetMenuItems(), e.Location));
						}
					}
					//if (!context) showContextMenu(this, new ContextMenuArgs(defaultMenu, e.Location));
				}
			}
        }


		public void OnMouseDown(Renderer r, System.Windows.Forms.MouseEventArgs e)
		{

		}

		public void OnMouseMove(Renderer r, System.Windows.Forms.MouseEventArgs e)
		{

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

		public System.Windows.Forms.Cursor Cursor
		{
			get { return currentCursor; }
		}

		#endregion

		#region IRender Members

		public string GetName()
		{
			return name;
		}

		public void Draw(Renderer cam)
		{

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

		public List<IRenderTool> Conflicts
		{
			get { return conflicts; }
		}

		public void EnableMode(string modeName)
		{
		}

		#endregion
	}

	public class ContextMenuArgs : EventArgs
	{
		ICollection<MenuItem> menuItems;
		Point loc;

		public ICollection<MenuItem> MenuItems
		{
			get { return menuItems; }
		}

		public Point Loc
		{
			get { return loc; }
		}

		public ContextMenuArgs(ICollection<MenuItem> menuItems, Point loc)
		{
			this.menuItems = menuItems;
			this.loc = loc;
		}
	}
}
