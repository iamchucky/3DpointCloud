using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Rendering
{
	public class ToolManager
	{
        private RulerTool rulerTool;
        private AngleTool angleTool;
        private SelectTool selectTool;
        private PathTool pathTool;
        //private PDFTool pdfTool;
        private SketchTool sketchTool;
        private ContextMenuTool contextMenuTool;
        private PointInspectTool pointInspectTool;

        public ToolManager()
        {
            rulerTool = new RulerTool(this);
            angleTool = new AngleTool(this);
            selectTool = new SelectTool(this);
            pathTool = new PathTool(this);
            sketchTool = new SketchTool(this);
            contextMenuTool = new ContextMenuTool(this);
            pointInspectTool = new PointInspectTool(this);
        }

		public void AddTools(Renderer renderer)
		{
			renderer.AddTool(rulerTool);
			renderer.AddTool(angleTool);
			renderer.AddTool(selectTool);
			renderer.AddTool(pathTool);
			//renderer.AddTool(pdfTool);
			renderer.AddTool(sketchTool);
			renderer.AddTool(contextMenuTool);
			renderer.AddTool(pointInspectTool);
		}

        public void AddTools(Renderer renderer,IRenderTool tool)
        {
            renderer.AddTool(tool);
        }


		public void BuildAllConflicts(Renderer renderer)
		{
			foreach (IRenderTool tool in renderer.Tools)
			{
                tool.BuildConflicts(renderer.Tools);
			}
		}

		public void RemoveTools(Renderer renderer, IRenderTool tool)
		{
			renderer.RemoveTool(tool);
		}

		public RulerTool RulerTool
		{
			get { return rulerTool; }
		}

		public AngleTool AngleTool
		{
			get { return angleTool; }
		}

		public SelectTool SelectTool
		{
			get { return selectTool; }
		}

		public PathTool PathTool
		{
			get { return pathTool; }
		}

		//public PDFTool PDFTool
		//{
		//    get { return pdfTool; }
		//}

		public SketchTool SketchTool
		{
			get { return sketchTool; }
		}
		public ContextMenuTool ContextMenuTool
		{
			get { return contextMenuTool; }
		}

		public PointInspectTool PointInspectTool
		{
			get { return pointInspectTool; }
		}
	}
}
