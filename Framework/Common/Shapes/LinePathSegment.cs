using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;
using Magic.Common.Mapack;

namespace Magic.Common.Path
{
  [Serializable]
	public class LinePathSegment : ISpeedPathSegment
	{
		protected Vector2 start;
		protected Vector2 end;
		protected double? endSpeed;
		protected bool? stopline;

		public LinePathSegment(Vector2 start, Vector2 end)
		{
			this.start = start;
			this.end = end;
			endSpeed = null;
			stopline = null;
		}

		public LinePathSegment(Vector2 start, Vector2 end, double? endSpeed, bool? stopline) {
			this.start = start;
			this.end = end;
			this.endSpeed = endSpeed;
			this.stopline = stopline;
		}

		#region IPathSegment Members

		public virtual Vector2 Start
		{
			get { return start; }
			set { start = value; }
		}

		public virtual PointOnPath StartPoint
		{
			get { return new PointOnPath(this, 0, start); }
		}

		public virtual Vector2 End
		{
			get { return end; }
			set { end = value; }
		}

		public virtual PointOnPath EndPoint
		{
			get { return new PointOnPath(this, Length, end); }
		}

		public virtual double Length
		{
			get { return start.DistanceTo(end); }
		}

		public virtual double DistanceToGo(PointOnPath pt)
		{
			Debug.Assert(Equals(pt.segment));
			return Length - pt.dist;
		}

		public virtual double DistanceOffPath(Vector2 pt)
		{
			PointOnPath pop = ClosestPoint(pt);
			return pop.pt.DistanceTo(pt);
		}

		public virtual PointOnPath ClosestPoint(Vector2 pt)
		{
			Vector2 tan = end - start;
			double l = tan.VectorLength;
			tan /= l;

			double t = tan * (pt - start);

			if (t < 0)
				return new PointOnPath(this, 0, start);
			else if (t > l)
				return new PointOnPath(this, l, end);
			else
				return new PointOnPath(this, t, t * tan + start);
		}

		public virtual PointOnPath AdvancePoint(PointOnPath pt, ref double dist)
		{
			Debug.Assert(Equals(pt.segment));
			if (dist >= 0)
			{
				double d = Math.Min(DistanceToGo(pt), dist);
				Vector2 tan = end - start;
				tan = tan / tan.VectorLength;
				dist -= d;
				return new PointOnPath(this, pt.dist + d, pt.pt + tan * d);
			}
			else
			{
				double d = Math.Max(-pt.pt.DistanceTo(this.start), dist);
				Vector2 tan = end - start;
				tan = tan / tan.VectorLength;
				dist -= d;
				return new PointOnPath(this, pt.dist + d, pt.pt + tan * d);
			}
		}

		public virtual bool Equals(IPathSegment other)
		{
			LinePathSegment lps = other as LinePathSegment;
			if (lps != null)
			{
				return lps.start == start && lps.end == end;
			}
			else
			{
				return false;
			}
		}

		public Vector2 Tangent(PointOnPath pt) {
			return (end - start).Normalize();
		}

		public double Curvature(PointOnPath pt) {
			return 0;
		}

		public IPathSegment Clone() {
			return new LinePathSegment(start, end, endSpeed, stopline);
		}

		public void Transform(Matrix3 m) {
			Vector3 s = new Vector3(start.X, start.Y, 1);
			Vector3 e = new Vector3(end.X, end.Y, 1);

			s = m * s;
			e = m * e;

			start = new Vector2(s.X, s.Y);
			end = new Vector2(e.X, e.Y);
		}

		#endregion

		#region ISpeedPathSegment Members

		public virtual bool EndSpeedSpecified {
			get { return endSpeed.HasValue; }
		}

		public virtual double EndSpeed {
			get { return endSpeed.GetValueOrDefault(0); }
		}

		public virtual bool StopLine {
			get { return stopline.GetValueOrDefault(false); }
		}

		#endregion
	}
}
