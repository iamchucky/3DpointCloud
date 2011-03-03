using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Shapes;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public class PDFReferencePointRenderer : IPDFReferencePoint
	{
		int pdfID;
		int sequenceID;
		Color color = Color.DarkOliveGreen;
		double x; double y; double z;
		bool selected = false;
		string name;
		Polygon bodyPlygn;

		public PDFReferencePointRenderer()
		{
			// Create the polygon that represents the body
			bodyPlygn = new Polygon();
			bodyPlygn.Add(new Vector2(this.X + .25, this.Y + .25));
			bodyPlygn.Add(new Vector2(this.X + .25, this.Y - .25));
			bodyPlygn.Add(new Vector2(this.X - .25, this.Y + .25));
			bodyPlygn.Add(new Vector2(this.X - .25, this.Y - .25));
		}

		#region IPDFReferencePoint Members

		public int PDFID
		{
			get { return pdfID; }
			set { pdfID = value; }
		}

		public int SequenceID
		{
			get { return sequenceID; }
			set { sequenceID = value; }
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

		#endregion

		#region ISelectable Members

		public bool IsSelected
		{
			get { return selected; }
		}

		public void OnSelect()
		{
			selected = true;
			color = Color.LightGreen;
		}

		public void OnDeselect()
		{
			selected = false;
			color = Color.Green;
		}

		#endregion

		#region IHittable Members

		public Polygon GetBoundingPolygon()
		{
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
