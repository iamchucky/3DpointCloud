using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public class SelectablePolygonRenderer : ISelectablePolygon
	{
		private Polygon polygon;
		private Color color;
		public bool selected;

		public SelectablePolygonRenderer(Polygon p)
		{
			polygon = p;
			color = Color.Blue;
			selected = false;
		}

		#region ISelectablePolygon Members

		public Polygon Polygon
		{
			get
			{
				return polygon;
			}
			set
			{
				polygon = value;
			}
		}

		public Color Color { get { return color; } }

		#endregion

		#region ISelectable Members

		public bool IsSelected
		{
			get { return selected; }
		}

		public void OnSelect()
		{
			selected = true;
			color = Color.DarkBlue;
		}

		public void OnDeselect()
		{
			selected = false;
			color = Color.Blue;
		}

		#endregion

		#region IHittable Members

		public Magic.Common.Shapes.Polygon GetBoundingPolygon()
		{
			return Polygon;
		}

		public bool HitTest()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IRender Members

		public string GetName()
		{
			return "Polygon";
		}

		public void Draw(Renderer cam)
		{
			GLUtility.DrawLineLoop(new GLPen(Color, selected ? 2.0f : 1.0f), Polygon.ToArray());
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
	}
}
