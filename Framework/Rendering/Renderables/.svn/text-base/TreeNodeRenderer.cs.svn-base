using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public class TreeNodeRenderer : IRender 
	{
		ITreeNode root;
		ITreeNode goal;
		GLPen edgePen = new GLPen(Color.Green, 1.0f);
		GLPen circPen = new GLPen(Color.Red, 1.0f);
		GLPen goalPen = new GLPen(Color.Blue, 2.0f);
		GLPen optimalPathPen = new GLPen(Color.Goldenrod, 2.0f);
		public void SetRoot(ITreeNode n)
		{ this.root = n; }

		public void SetGoal(ITreeNode goal)
		{
			this.goal = goal;
		}

		#region IRender Members

		public string GetName()
		{
			return "TreeRenderer";
		}

		public void Draw(Renderer cam)
		{
				DrawNodeRecrusive(root);
				if (goal != null)
				{
					GLUtility.DrawCircle(goalPen, goal.Point, .1f);
					DrawBackFromParent(goal);
				}
		}

		public void DrawBackFromParent(ITreeNode n)
		{
			if (n.IsRoot) return;
			DrawSimulationResults(n, optimalPathPen);
			//GLUtility.DrawLine(pathPen, n.Point, n.Parent.Point);
			DrawBackFromParent(n.Parent);
		}

		public void DrawNodeRecrusive(ITreeNode n)
		{
			if (n == null) return;
			//draw a link from this node to each of its children
			DrawSimulationResults(n, edgePen);
			foreach (ITreeNode c in n.Children)
			{
				GLUtility.DrawCircle(circPen, n.Point, .10f); 				
				//GLUtility.DrawLine(edgePen, n.Point, c.Point);				
				DrawNodeRecrusive(c);	
			}

		}

		public void DrawSimulationResults(ITreeNode n, GLPen p)
		{
			for (int i = 0; i < n.SimulationPoints.Count - 1; i++)
			{
				GLUtility.DrawLine(p,n.SimulationPoints[i], n.SimulationPoints[i + 1]);				
			}

		}
		public void ClearBuffer()
		{
			root = null;
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

	
		
	}
}
