using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Magic.Rendering;
using Magic.Rendering.Renderables;

namespace Magic.Rendering
{
    /// <summary>
    /// This allows for the selection of tools in the renderer....
    /// 
    /// </summary>
    public interface IRenderTool: IRender 
    {
        void OnMouseUp(Renderer r , MouseEventArgs e);
        void OnMouseDown(Renderer r, MouseEventArgs e);
        void OnMouseMove(Renderer r, MouseEventArgs e);        
        bool IsActive { get; set; }
		Cursor Cursor { get; }
		List<string> ModeList { get; }
		string DefaultMode { get; set; }
		List<IRenderTool> Conflicts { get; }
		void BuildConflicts(List<IRenderTool> allTools);
		void EnableMode(string modeName);
		void TempDeactivate();
		void TempReactivate();
    }

    /// <summary>
    /// Allows for a tool that also has a result when the tool is completed.   
    /// </summary>
    /// <typeparam name="T">the results must be of type EventArgs or any class 
    /// that inherits from EventArgs (i.e. : EventArgs)</typeparam>
    public interface IRenderToolWithResult<T> : IRenderTool where T : EventArgs
    {
        event EventHandler<T> ToolCompleted;

    }

    public class GestureExpHRIEventArgs : EventArgs
    {
        string content;

        //the accessor (property) which wraps up properties
        public string Content
        {
            get { return content; }
        }

        public GestureExpHRIEventArgs(string c)
        {
            content = c;
        }
    }
}
