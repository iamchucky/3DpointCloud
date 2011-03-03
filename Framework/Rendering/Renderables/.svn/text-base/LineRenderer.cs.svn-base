using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common;

namespace Magic.Rendering.Renderables
{
	public class LineRenderer : IRender
	{
		string name;
		Color color;
		float width;
		List<Vector2> pt1List = new List<Vector2>(), pt2List = new List<Vector2>();

		public LineRenderer(string name, Color color, float width)
		{
			this.name = name;
			this.color = color;
			this.width = width;
		}

		public void UpdateLine(Vector2 pt1, Vector2 pt2)
		{
			ClearBuffer();
			pt1List.Add(pt1);
			pt2List.Add(pt2);
		}

		public void UpdateLine(List<Vector2> pt1, List<Vector2> pt2)
		{
			this.pt1List = pt1; this.pt2List = pt2;
		}

		#region IRender Members

		public string GetName()
		{
			return name;
		}

		public void Draw(Renderer cam)
		{
			for (int i = 0; i < pt1List.Count; i++)
			{
				GLUtility.DrawLine(new GLPen(this.color, width), pt1List[i], pt2List[i]);
			}
		}

		public void ClearBuffer()
		{
			pt1List.Clear();
			pt2List.Clear();
		}

		public bool VehicleRelative
		{
			get { return true; }
		}

		public int? VehicleRelativeID
		{
			get { return null; }
		}

		#endregion
	}
}
