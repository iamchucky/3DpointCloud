using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Magic.Common;
using Magic.Common.Shapes;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Magic.Rendering.Renderables
{
	public class HRIContextMenu : ContextMenuStrip
    {
		private List<IRenderTool> enabledTools=new List<IRenderTool>();
		private List<IRenderTool> disabledTools=new List<IRenderTool>();

		public event EventHandler<ToolEnabledArgs> ToolEnabled;
		public event EventHandler<ToolDisabledArgs> ToolDisabled;
		public event EventHandler<EnableModeArgs> EnableMode;

		#region BuildContextMenu
		public void BuildContextMenu()
		{
			this.Items.Clear();

			if (enabledTools.Count > 0)
			{
				//ToolStripItem item = new ToolStripMenuItem(defaultMode);
				//this.Items.Add(item);
				foreach (IRenderTool tool in enabledTools)
				{
					if (tool.DefaultMode == null) continue;
					if (!tool.DefaultMode.Equals("NONE"))
					{
						ToolStripItem item = new ToolStripMenuItem(tool.DefaultMode);
						item.Click += new EventHandler(mode_Engage);
						this.Items.Add(item);
					}
				}
			}
			this.Items.Add(new ToolStripSeparator());

			if (enabledTools.Count > 0)
			{
				foreach (IRenderTool t in enabledTools)
				{
					ToolStripMenuItem item = new ToolStripMenuItem(t.GetName());
					if (t.ModeList == null) continue;
					foreach (string s in t.ModeList)
					{
						item.DropDownItems.Add(s);
					}
					this.Items.Add(item);

					item.DropDownItems.Add("Disable");
					item.DropDownItemClicked += new ToolStripItemClickedEventHandler(mode_DropDown);
				}
			}
			this.Items.Add(new ToolStripSeparator());
			this.Items.Add("Disable All");
			this.ItemClicked +=new ToolStripItemClickedEventHandler(disableAll_ItemClicked);
			

			if (disabledTools.Count > 0)
			{
				foreach (IRenderTool t in disabledTools)
				{
					ToolStripMenuItem item = new ToolStripMenuItem(t.GetName());
					item.DropDownItems.Add("Enable");
					item.DropDownItemClicked += new ToolStripItemClickedEventHandler(enable_DropDown);
					this.Items.Add(item);
				}
			}
		}
		#endregion


		#region Enable, Disable, and Disable All buttons

		void mode_Engage(object sender, EventArgs e)
		{
			bool success = false;
			Console.WriteLine(sender.ToString());

			foreach (IRenderTool tool in enabledTools)
			{
				foreach (string mode in tool.ModeList)
				{
					if (mode.Equals(sender.ToString()) == true)
					{
						EnableMode(this, new EnableModeArgs(tool, mode));
						success = true;
						break;
					}
				}
				if (success == true)
				{
					this.Close();
					break;
				}
			}
		}

		void mode_DropDown(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem.Text.Equals("Disable"))
			{
				foreach (IRenderTool tool in enabledTools)
				{
					if (tool.GetName().Equals(e.ClickedItem.OwnerItem.Text) == true)
					{
						ToolDisabled(this, new ToolDisabledArgs(tool));
						break;
					}
				}
				this.Close();
			}
			else
			{
				foreach (IRenderTool tool in enabledTools)
				{
					if (tool.GetName().Equals(e.ClickedItem.OwnerItem.Text) == true)
					{
						foreach (string mode in tool.ModeList)
						{
							if (mode.Equals(e.ClickedItem.Text) == true)
							{
								EnableMode(this, new EnableModeArgs(tool, mode));
								break;
							}
						}
						this.Close();
						break;
					}
				}
			}
		}

		void enable_DropDown(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem.Text.Equals("Enable"))
			{
				foreach (IRenderTool tool in disabledTools)
				{
					if (tool.GetName().Equals(e.ClickedItem.OwnerItem.Text) == true)
					{
						ToolEnabled(this, new ToolEnabledArgs(tool));
						break;
					}
				}
				this.Close();
			}
		}

		void disableAll_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem.Text.Equals("Disable All"))
			{
				while (enabledTools.Count > 0)
				{
					ToolDisabled(this,new ToolDisabledArgs(enabledTools.First()));
					//DisableTool(enabledTools.First());
				}
				this.Close();
			}
		}


		#endregion


		#region Context Menu Events
		public void Initialize(List<IRenderTool> toolList, IRenderTool activeTool)
		{
			foreach (IRenderTool tool in toolList)
			{
				disabledTools.Add(tool);
			}
            if (activeTool!=null)
			    EnableTool(activeTool);

			BuildContextMenu();
		}

		public List<IRenderTool> GetEnabledTools
		{
			get { return enabledTools; }
		}

		public List<IRenderTool> GetDisabledTools
		{
			get { return disabledTools; }
		}



		public void EnableTool(IRenderTool tool)
		{
			if (!enabledTools.Contains(tool))
			{
				enabledTools.Add(tool);
				disabledTools.Remove(tool);
				BuildContextMenu();
			}
		}

		public void DisableTool(IRenderTool tool)
		{
			if (!disabledTools.Contains(tool))
			{
				enabledTools.Remove(tool);
				disabledTools.Add(tool);
				BuildContextMenu();
			}
		}


		public class EnableModeArgs : EventArgs
		{
			IRenderTool tool;
			string mode;

			public IRenderTool ToolToEnable
			{
				get { return tool; }
			}
			public string ModeToEnable
			{
				get { return mode; }
			}
			public EnableModeArgs(IRenderTool t, string m)
			{
				tool = t;
				mode = m;
			}
		}

		public class ToolEnabledArgs : EventArgs
		{
			IRenderTool toolEnabled;

			public IRenderTool ToolToEnable
			{
				get {return toolEnabled;}
			}

			public ToolEnabledArgs(IRenderTool tool)
			{
				toolEnabled = tool;
			}
		}

		public class ToolDisabledArgs : EventArgs
		{
			IRenderTool toolDisabled;

			public IRenderTool ToolToDisable
			{
				get { return toolDisabled; }
			}

			public ToolDisabledArgs(IRenderTool tool)
			{
				toolDisabled = tool;
			}
		}
		#endregion
	}
}
