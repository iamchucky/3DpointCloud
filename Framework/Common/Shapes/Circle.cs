using System;
using System.Collections.Generic;
using System.Text;
using Magic.Common.Mapack;

namespace Magic.Common.Shapes {
	[Serializable]
	public struct Circle : IEquatable<Circle> {
		public static readonly Circle Infinite = new Circle(double.PositiveInfinity, new Vector2(0, 0));
		private static readonly Vector2[] emptyCoords = new Vector2[0];

		public double r;
		public Vector2 center;

		public Circle(double r, Vector2 center) {
			this.r = r;
			this.center = center;
		}

		public static double GetCurvature(Vector2 p0, Vector2 p1, Vector2 p2) {
			Vector2 d01 = p0 - p1;
			Vector2 d21 = p2 - p1;
			Vector2 d20 = p2 - p0;

			return 2*d01.Cross(d21)/(d01.Length*d21.Length*d20.Length);
		}

		public static Circle FromPoints(Vector2 p0, Vector2 p1, Vector2 p2) {
			Vector2 t1 = p1 - p0;
			Vector2 t2 = p1 - p2;

			Vector2 c1 = (p0 + p1) / 2.0;
			Vector2 c2 = (p1 + p2) / 2.0;

			// check if the points are colinear
			if (Math.Abs(t1.Cross(t2)) < 1e-20) {
				return Infinite;
			}
			else {
				double k = t2.Dot(c2 - c1) / t1.Cross(t2);
				Vector2 center = c1 + t1.Rotate90() * k;
				double radius = p0.DistanceTo(center);
				return new Circle(radius, center);
			}
		}

		public static Circle FromPointSlopeRadius(Vector2 p0, Vector2 tangent, double r) {
			if (r <= 0)
				throw new ArgumentOutOfRangeException("r", "Radius must be positive");

			Vector2 center = p0 + tangent.Rotate90().Normalize(r);
			return new Circle(r, center);
		}

		public static Circle FromPointSlopePoint(Vector2 p0, Vector2 tangent, Vector2 p1) {
			Line l1 = new Line(p0, p0 + tangent.Rotate90());
			Line l2 = new Line((p0+p1)/2, (p0+p1)/2 + (p1 - p0).Rotate90());

			Vector2 pt;
			if (l1.Intersect(l2, out pt)) {
				return new Circle(pt.DistanceTo(p0), pt);
			}
			else {
				return Circle.Infinite;
			}
		}

		public static Circle FromLines(Line oncircle, Line toline, out Vector2 tolinePoint) {
			tolinePoint = default(Vector2);

			Vector2 m = (oncircle.P1 - oncircle.P0).Normalize();
			Vector2 u = (toline.P1 - toline.P0).Normalize();

			if (Math.Abs(m.Cross(u)) < 1e-10) {
				return Circle.Infinite;
			}

			Vector2 c = m.Rotate90();
			Vector2 t = u.Rotate90();

			Vector2 t_c = t - c;

			Matrix2 A = new Matrix2(
				u.X, t_c.X,
				u.Y, t_c.Y);

			Vector2 d = oncircle.P0 - toline.P0;

			if (Math.Abs(A.Determinant()) < 1e-10) {
				return Circle.Infinite;
			}

			Vector2 pvec = A.Inverse() * d;

			Vector2 center = oncircle.P0 + pvec.Y*c;

			tolinePoint = toline.P0 + pvec.X*u;
			return new Circle(Math.Abs(pvec.Y), center);
		}

		public Vector2 GetPoint(double theta) {
			return center + new Vector2(r, 0).Rotate(theta);
		}

		public bool Intersect(LineSegment line, out Vector2[] pts) {
			double[] K;
			return Intersect(line, out pts, out K);
		}

		public bool Intersect(LineSegment line, out Vector2[] pts, out double[] K) {
			Vector2 t = line.P1 - line.P0;
			Vector2 s = line.P0 - center;

			double t_dot_s = t.Dot(s);
			double tnorm2 = t.Dot(t);
			double snorm2 = s.Dot(s);

			// check the discriminant to see if there is an intersection or not
			double disc = t_dot_s*t_dot_s - tnorm2*(snorm2 - r*r);
			if (disc < 0) {
				// there are no intersecting points
				goto NoIntersection;
			}
			else if (Math.Abs(disc) < 1e-10) {
				// there is one intersecting point, trim to line segment
				double u = -t_dot_s/tnorm2;
				if (u >= 0 && u <= 1) {
					pts = new Vector2[] { line.P0 + t * u };
					K = new double[] { u };
					return true;
				}
				else {
					goto NoIntersection;
				}
			}
			else {
				// there are two intersecting points on the line, trim to line segment
				double sqrt_d = Math.Sqrt(disc);
				double u1 = (-t_dot_s - sqrt_d) / tnorm2;
				double u2 = (-t_dot_s + sqrt_d) / tnorm2;

				if (u1 >= 0 && u1 <= 1 && u2 >= 0 && u2 <= 1) {
					pts = new Vector2[] { line.P0 + t * u1, line.P0 + t * u2 };
					K = new double[] { u1, u2 };
					return true;
				}
				else if (u1 >= 0 && u1 <= 1) {
					pts = new Vector2[] { line.P0 + t * u1 };
					K = new double[] { u1 };
					return true;
				}
				else if (u2 >= 0 && u2 <= 1) {
					pts = new Vector2[] { line.P0 + t * u2 };
					K = new double[] { u2 };
					return true;
				}
				else {
					goto NoIntersection;
				}
			}

		NoIntersection:
			pts = default(Vector2[]);
			K = default(double[]);
			return false;
		}

		public bool Intersect(Line line, out Vector2[] pts) {
			double[] K;
			return Intersect(line, out pts, out K);
		}

		public bool Intersect(Line line, out Vector2[] pts, out double[] K) {
			Vector2 t = line.P1 - line.P0;
			Vector2 s = line.P0 - center;

			double t_dot_s = t.Dot(s);
			double tnorm2 = t.Dot(t);
			double snorm2 = s.Dot(s);

			// check the discriminant to see if there is an intersection or not
			double disc = t_dot_s * t_dot_s - tnorm2 * (snorm2 - r * r);
			if (disc < 0) {
				// there are no intersecting points
				goto NoIntersection;
			}
			else if (Math.Abs(disc) < 1e-10) {
				// there is one intersecting point
				double u = -t_dot_s / tnorm2;
				
				pts = new Vector2[] { line.P0 + t * u };
				K = new double[] { u };
				return true;
			}
			else {
				// there are two intersecting points
				double sqrt_d = Math.Sqrt(disc);
				double u1 = (-t_dot_s - sqrt_d) / tnorm2;
				double u2 = (-t_dot_s + sqrt_d) / tnorm2;

				pts = new Vector2[] { line.P0 + t * u1, line.P0 + t * u2 };
				K = new double[] { u1, u2 };
				return true;
			}

		NoIntersection:
			pts = default(Vector2[]);
			K = default(double[]);
			return false;
		}

		public Vector2[] Intersect(Circle circle) {
			double d = center.DistanceTo(circle.center);
			// check if there is any possibility of intersection
			if (d > r + circle.r) {
				return emptyCoords;
			}
			// check if they are the same centers
			else if (center.Equals(circle.center)) {
				return emptyCoords;
			}
			// check if one is completely interned by the other
			else if (d + r < circle.r || d + circle.r < r) {
				return emptyCoords;
			}
			else {
				// there is at least one intersection
				// assume that the circle are both lying on the x-axis to simplify analysis
				double R2 = circle.r * circle.r;
				double r2 = r * r;
				double d2 = d * d;

				double x = (d2 - r2 + R2) / (2 * d);
				double y = Math.Sqrt(R2 - x * x);

				// check if y is close to 0, indicating that this is only one hit
				if (y < 1e-10) {
					return new Vector2[] { circle.center + r * (center - circle.center).Normalize() };
				}
				else {
					Vector2 vec = (center - circle.center).Normalize();
					Vector2 nom_pt = circle.center + x * vec;

					Vector2 perp_vec = vec.Rotate90();
					return new Vector2[] { nom_pt + y * perp_vec, nom_pt - y * perp_vec };
				}
			}
		}

		public Vector2 ClosestPoint(Vector2 pt) {
			Vector2 vec = (pt - center).Normalize();
			return vec*r;
		}

		public bool IsInside(Vector2 pt) {
			return pt.DistanceTo(center) <= r;
		}

		public Vector2[] GetTangentPoints(Vector2 ptFrom) {
			if (IsInside(ptFrom))
				return default(Vector2[]);

			double h = ptFrom.DistanceTo(center);
			double o = r;

			double theta = Math.PI/2.0 - Math.Asin(o/h);

			Vector2 closestPt = ClosestPoint(ptFrom);
			Vector2[] pts = new Vector2[2];
			pts[0] = closestPt.Rotate(theta) + center;
			pts[1] = closestPt.Rotate(-theta) + center;

			return pts;
		}

		public Rect GetBoundingRectangle() {
			return new Rect(center.X-r, center.Y-r, 2*r, 2*r);
		}

		public Circle Transform(IVector2Transformer transform) {
			return new Circle(r, transform.TransformPoint(center));
		}

		public Polygon ToPolygon(int numPoints) {
			double angleSpacing = 2*Math.PI/numPoints;

			Polygon p = new Polygon(numPoints);

			for (int i = 0; i < numPoints; i++) {
				p.Add(center + Vector2.FromAngle(angleSpacing*i)*r);
			}

			return p;
		}

		#region IEquatable<Circle> Members

		public bool Equals(Circle c) {
			// check if both have the same radius and the centers match, or both radii are infinite
			return (c.r == r && c.center.Equals(center)) || (double.IsInfinity(c.r) && double.IsInfinity(r));
		}

		#endregion

		public override bool Equals(object obj) {
			if (obj is Circle) {
				return Equals((Circle)obj);
			}
			else {
				return false;
			}
		}

		public override int GetHashCode() {
			return r.GetHashCode() ^ center.GetHashCode();
		}

		public override string ToString() {
			return "(" + center.ToString() + "),r:" + r.ToString();
		}
	}
}
