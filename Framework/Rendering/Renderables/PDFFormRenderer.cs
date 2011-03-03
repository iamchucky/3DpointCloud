using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Shapes;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public class PDFFormRenderer : ISelectable
	{
		bool selected;
        float lineWidth;
        float selectedLineWidthBoldAmount = 0.5f;
		int id;
		Polygon bodyPlygn;
		Color color = Color.DarkOliveGreen;

        List<PDFReferencePointRenderer> PDFREferencePoints;

        public PDFFormRenderer()
        {
			PDFREferencePoints = new List<PDFReferencePointRenderer>();
			bodyPlygn = new Polygon();
			
        }

		public PDFFormRenderer(List<PDFReferencePointRenderer> pdfREferencePoints)
			: this()
        {

			PDFReferencePointRenderer previous = null;
			foreach (PDFReferencePointRenderer p in pdfREferencePoints)
			{
				p.PDFID = this.ID;
				if (previous == null) previous = p;
				else
				{
					AddReferencePoint(p);
					previous = p;
				}
			}
			/*
            WaypointRenderer previous = null;
			foreach (WaypointRenderer p in pdfREferencePoints)
            {
				p.PathID = this.ID;
                if (previous == null) previous = p;
                else
                {
                    AddSegment(previous, p);
                    previous = p;
                }
            }//*/
        }
		/*
        private void AddSegment(WaypointRenderer start, WaypointRenderer end)
        {
            LineSegment ls;
            ls.start = start;
            ls.end = end;
            segments.Add(ls);
        }//*/

		public void AddReferencePoint(PDFReferencePointRenderer p)
        {
			PDFREferencePoints.Add(p);
            //AddSegment(waypoints.Last(), p);
        }

		public int ID
		{
			get { return id; }
			set { id = value; }
		}

		public List<PDFReferencePointRenderer> PDFReferencePointList
		{
			get { return PDFREferencePoints; }
		}
		/*
		public List<LineSegment> LineSegmentList
		{
			get { return segments; }
		}//*/

		public Color Color
		{
			get { return color; }
			set { color = value; }
		}

        #region ISelectable Members

        public bool IsSelected
        {
            get { return selected; }
        }

        public void OnSelect()
        {
            selected = true;
            lineWidth = lineWidth + selectedLineWidthBoldAmount;
        }

        public void OnDeselect()
        {
            selected = true;
            lineWidth = lineWidth - selectedLineWidthBoldAmount;
        }

        #endregion

        #region IHittable Members

        public Magic.Common.Shapes.Polygon GetBoundingPolygon()
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
            return "Path";
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
