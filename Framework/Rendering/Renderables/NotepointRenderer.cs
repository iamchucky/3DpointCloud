using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Shapes;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public class NotepointRenderer : INotepoint
	{
		string name = "POI";
		Color color = Color.DarkGray;
		bool selected = false;
		double x; double y; double z;
		string comments = null;
		Polygon bodyPlygn;

		public NotepointRenderer()
		{
			// Create the polygon that represents the body
			//bodyPlygn = new Polygon();
			//bodyPlygn.Add(new Vector2(this.X, this.Y));
		}

		#region INotepoint Members

		public string Comments
		{
			get	{ return comments ; }
			set	{ comments = value; }
		}

		#endregion

		#region IKeypoint Members

		public Color Color
		{
			get { return color; }
			set { color = value; }
		}

		public double X
		{
			get { return x; }
			set { x = value; }
		}

		public double Y
		{
			get { return y; }
			set { y = value; }
		}

		public double Z
		{
			get { return z; }
			set { z = value; }
		}
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		#endregion

		#region ISelectable Members

		public bool IsSelected
		{
			get { return selected; }
		}

		public void OnSelect()
		{
			color = Color.BlueViolet;
			selected = true;
		}

		public void OnDeselect()
		{
			color = Color.DarkGray;
			selected = false;
		}

		#endregion

		#region IHittable Members

		public Polygon GetBoundingPolygon()
		{
			bodyPlygn = new Polygon();
			bodyPlygn.Add(new Vector2(this.X + .25, this.Y + .25));
			bodyPlygn.Add(new Vector2(this.X + .25, this.Y - .25));
			bodyPlygn.Add(new Vector2(this.X - .25, this.Y - .25));
			bodyPlygn.Add(new Vector2(this.X - .25, this.Y + .25));
			return bodyPlygn;
		}

		public bool HitTest()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IRender Members

		public string GetName()
		{
			return name;
		}

		public void Draw(Renderer cam)
		{
			//Console.WriteLine("trying to draw notepoint");
			PointF npCenter = new PointF((float)x,(float)y);
			GLUtility.DrawCircle(new GLPen(color, 0.25f), npCenter, .1f);
			GLUtility.DrawCircle(new GLPen(color, 0.25f), npCenter, .25f);


            GLUtility.DrawString(comments, Color.Black, npCenter);
			if (name != null)
			{
				GLUtility.DrawString(name, Color.Black, npCenter);
			}

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
