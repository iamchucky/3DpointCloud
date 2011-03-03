using System;
using System.Collections.Generic;
using System.Text;
using Magic.Common.Mapack;

namespace Magic.Common.Shapes {
	[Serializable]
	public struct Line : IEquatable<Line> {
		public Vector2 P0, P1;

		public Line(Vector2 p0, Vector2 p1) {
			this.P0 = p0;
			this.P1 = p1;
		}

		public bool Intersect(Line l, out Vector2 pt) {
			Vector2 K;
			return Intersect(l, out pt, out K);
		}

		public bool Intersect(Line l, out Vector2 pt, out Vector2 K) {
			Vector2 P = P1 - P0;
			Vector2 S = l.P1 - l.P0;

			double cross = P.Cross(S);
			if (Math.Abs(cross) < 1e-10) {
				pt = default(Vector2);
				K = default(Vector2);
				return false;
			}

			Matrix2 A = new Matrix2(
				P.X, -S.X,
				P.Y, -S.Y
				);
			K = A.Inverse()*(l.P0 - P0);
			pt = P0 + P*K.X;
			return true;
		}

		public bool Intersect(LineSegment l, out Vector2 pt) {
			return l.Intersect(this, out pt);
		}

		public bool Intersect(LineSegment l, out Vector2 pts, out Vector2 K) {
			return l.Intersect(this, out pts, out K);
		}

		public bool Intersect(Circle c, out Vector2[] pts) {
			return c.Intersect(this, out pts);
		}

		public bool Intersect(Circle c, out Vector2[] pts, out double[] K) {
			return c.Intersect(this, out pts, out K);
		}

		public Line Transform(IVector2Transformer transformer) {
			if (transformer == null)
				throw new ArgumentNullException("transformer");

			return new Line(transformer.TransformPoint(P0), transformer.TransformPoint(P1));
		}

		public Vector2 ClosestPoint(Vector2 pt) {
			Vector2 t = UnitVector;
			return P0 + t*(t.Dot(pt - P0));
		}

		public bool IsToLeft(Vector2 pt) {
			return (pt-P0).Cross(P1-P0) > 0;
		}

		public Line ShiftLateral(double dist) {
			Vector2 norm = (P1 - P0).Normalize().Rotate90();
			return new Line(P0 + norm*dist, P1 + norm*dist);
		}

		public Vector2 UnitVector { get { return (P1-P0).Normalize(); } }

		#region IEquatable<Line> Members

		public bool Equals(Line other) {
			return P0.Equals(other.P0) && P1.Equals(other.P1);
		}

		#endregion

		public override bool Equals(object obj) {
			if (obj is Line)
				return Equals((Line)obj);
			else
				return false;
		}

		public override int GetHashCode() {
			return P0.GetHashCode() ^ P1.GetHashCode();
		}

		public override string ToString() {
			return "(" + P0.ToString() + ")->(" + P1.ToString() + ")";
		}
	}
}
