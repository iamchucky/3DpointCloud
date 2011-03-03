using System;
using System.Drawing;
using System.Collections.Generic;

namespace Magic.Common
{

	/// <summary>
	/// 2-dimensional absolute position.
	/// </summary>
	[Serializable]
	public struct Vector2 : IComparable<Vector2>, IEquatable<Vector2>, ILoggable
	{
		public static Vector2 Zero { get { return new Vector2(0, 0); } }
		public static Vector2 NaN { get { return new Vector2(double.NaN, double.NaN); } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		public Vector2(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		public Vector2 Clone()
		{
			return new Vector2(this.X, this.Y);
		}

		/// <summary>
		/// Constructs a unit vector rotated by theta radians
		/// </summary>
		/// <param name="theta">Angle to rotate in radians</param>
		/// <returns></returns>
		public static Vector2 FromAngle(double theta)
		{
			return new Vector2(Math.Cos(theta), Math.Sin(theta));
		}

		public static Vector2 FromPointF(PointF p)
		{
			return new Vector2(p.X, p.Y);
		}
		public static PointF ToPointF(Vector2 p)
		{
			return new PointF((float)p.X, (float)p.Y);
		}

		/// <summary>
		/// Vector addition.
		/// </summary>
		/// <param name="left">Left vector.</param>
		/// <param name="right">Right vector.</param>
		/// <returns>Vector sum.</returns>
		public static Vector2 operator +(Vector2 left, Vector2 right)
		{
			return new Vector2(left.X + right.X, left.Y + right.Y);
		}

		/// <summary>
		/// Vector substraction.
		/// </summary>
		/// <param name="left">Left vector.</param>
		/// <param name="right">Right vector.</param>
		/// <returns>Vector difference.</returns>
		public static Vector2 operator -(Vector2 left, Vector2 right)
		{
			return new Vector2(left.X - right.X, left.Y - right.Y);
		}

		/// <summary>
		/// Multiplication of vector with a scalar.
		/// </summary>
		/// <param name="c">The vector.</param>
		/// <param name="d">The scalar.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 operator *(Vector2 c, double d)
		{
			return new Vector2(c.X * d, c.Y * d);
		}

		/// <summary>
		/// Multiplication of vector with a scalar.
		/// </summary>
		/// <param name="c">The vector.</param>
		/// <param name="d">The scalar.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 operator *(double d, Vector2 c)
		{
			return c * d;
		}

		/// <summary>
		/// Division of vector with a scalar.
		/// </summary>
		/// <param name="c">The vector.</param>
		/// <param name="d">The scalar.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 operator /(Vector2 c, double d)
		{
			return new Vector2(c.X / d, c.Y / d);
		}

		/// <summary>
		/// Vector multiplication.
		/// </summary>
		/// <param name="left">Left vector.</param>
		/// <param name="right">Right vector.</param>
		/// <returns>Scalar product.</returns>
		public static double operator *(Vector2 left, Vector2 right)
		{
			return (left.X * right.X + left.Y * right.Y);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left vector.</param>
		/// <param name="right">Right vector.</param>
		/// <returns>True if both X and Y coordinates are equal.</returns>
		public static bool operator ==(Vector2 left, Vector2 right)
		{
			return (left.X == right.X) && (left.Y == right.Y);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left vector.</param>
		/// <param name="right">Right vector.</param>
		/// <returns>True if left and right are not equal.</returns>
		public static bool operator !=(Vector2 left, Vector2 right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Negates the coordinates, returning a new instance (same as rotating by 180)
		/// </summary>
		public static Vector2 operator -(Vector2 left)
		{
			return new Vector2(-left.X, -left.Y);
		}

		/// <summary>
		/// Euclidean distance between coordinates.
		/// </summary>
		/// <param name="other">Other Coordinates.</param>
		/// <returns>Euclidean distance.</returns>
		public double DistanceTo(Vector2 other)
		{
			return (other - this).VectorLength;
		}

		/// <summary>Modulus of vector in polar coordinates.</summary>
		public double VectorLength
		{
			get
			{
				return Math.Sqrt(this.X * this.X + this.Y * this.Y);
			}
		}

		/// <summary>
		/// Squared length of vector
		/// </summary>
		public double VectorLength2
		{
			get
			{
				return this.X * this.X + this.Y * this.Y;
			}
		}

		/// <summary>
		/// Return the closest point in a list from this point
		/// </summary>
		/// <param name="list">list of points</param>
		/// <param name="distance">distance to the closest point</param>
		/// <returns>Closest point of the list</returns>
		public Vector2 ClosestInList(List<Vector2> list, ref double distance)
		{
			double minDist = Double.MaxValue;
			Vector2 toReturn = new Vector2();
			foreach (Vector2 v in list)
			{
				if (v.DistanceTo(this) < minDist)
				{
					minDist = v.DistanceTo(this);
					toReturn = v;
				}
			}
			distance = minDist;
			return toReturn;
		}

		public Vector2 ClosestInList(List<Vector2> list)
		{
			double dist = 0;
			return ClosestInList(list, ref dist);
		}


		/// <summary>The arcus tangens of the vector.</summary>
		public double ArcTan
		{
			get { return Math.Atan2(this.Y, this.X); }
		}

		// ToDgrees -> angle above x axis in terms of degrees: 0 - 360
		public double ToDegrees()
		{
			double arctan = (Math.Atan2(this.Y, this.X)) * 180 / Math.PI;

			if (arctan >= 0)
				return arctan;
			else
				return (360.0 + arctan);
		}

		public Vector2 Scale(double scale)
		{
			return new Vector2(X * scale, Y * scale);
		}

		public double ToRadians()
		{
			return ToDegrees() * Math.PI / 180.0;
		}

		public PointF ToPointF()
		{
			return new PointF((float)this.X, (float)this.Y);
		}
		/// <summary>
		/// Computes dot product of the two coordinates as vectors
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public double Dot(Vector2 other)
		{
			return ((this.X * other.X) + (this.Y * other.Y));
		}

		/// <summary>
		/// Computes the perpendicular product (2-D version of cross product) of this x other
		/// </summary>
		public double Cross(Vector2 other)
		{
			return X * other.Y - Y * other.X;
		}

		/// <summary>
		/// Length of coordinate as vector
		/// </summary>
		public double Length
		{
			get { return Math.Sqrt(Dot(this)); }
		}

		/// <summary>
		/// Scales the vector to be unit length
		/// </summary>
		/// <returns>Unit length vector</returns>
		public Vector2 Normalize()
		{
			return this / VectorLength;
		}

		/// <summary>
		/// Scales the vector to be length len
		/// </summary>
		/// <param name="len">Target length of vector</param>
		/// <returns>Normalized vector</returns>
		public Vector2 Normalize(double len)
		{
			return this * (len / VectorLength);
		}

		public double Perp(Vector2 v)
		{
			return (X * v.Y - Y * v.X);
		}
		/// <summary>
		/// Rotates the coordinate
		/// </summary>
		/// <param name="radians">THETA IN RADIANS</param>
		/// <returns></returns>
		public Vector2 Rotate(double radians)
		{
			double ct = Math.Cos(radians), st = Math.Sin(radians);
			return new Vector2(ct * X - st * Y, ct * Y + st * X);
		}

		/// <summary>
		/// Returns a new Coordinates rotated by 90 degrees
		/// </summary>
		public Vector2 Rotate90()
		{
			return new Vector2(-Y, X);
		}

		/// <summary>
		/// Returns a new Coordinates rotated by -90 degrees
		/// </summary>
		public Vector2 RotateM90()
		{
			return new Vector2(Y, -X);
		}

		/// <summary>
		/// Returns a new Coordinates rotated by 180 degrees
		/// </summary>
		public Vector2 Rotate180()
		{
			return new Vector2(-X, -Y);
		}

		/// <summary>
		/// Comparison between Coordinates. Gives partial ordering.
		/// </summary>
		/// <param name="other">The other Coordinates.</param>
		/// <returns>
		/// True if X is lower, or X is equal other.X and Y is lower.
		/// </returns>
		public int CompareTo(Vector2 other)
		{
			double x = this.X - other.X;
			if (x == 0)
			{
				double y = this.Y - other.Y;
				if (y == 0)
					return 0;
				else if (y < 0)
					return -1;
				else
					return 1;
			}
			else if (x < 0)
				return -1;
			else
				return 1;
		}

		#region IEquatable<Coordinates> Members

		/// <summary>
		/// Analyzes if two coords are equal
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Vector2 other)
		{
			return this.X.Equals(other.X) && this.Y.Equals(other.Y);
		}

		#endregion

		/// <summary>
		/// Comparison between Coordinates.
		/// </summary>
		/// <param name="obj">The other Coordinates.</param>
		/// <returns>True if X and Y are equal.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Vector2)
			{
				Vector2 other = (Vector2)obj;
				return Equals(other);
			}
			else
				return false;
		}

		/// <summary>Hash function.</summary>
		/// <returns>A hash of X and Y.</returns>
		public override int GetHashCode()
		{
			return (this.X.GetHashCode() << 16) ^ (this.Y.GetHashCode());
		}

		/// <summary>
		/// string representation of coordinate
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return X.ToString() + "," + Y.ToString();
		}

		/// <summary>The transposed vector.</summary>
		public Vector2 Transposed
		{
			get { return new Vector2(this.Y, this.X); }
		}

		/// <summary>
		/// Check for approximate equality within tol in both dimensions
		/// </summary>
		/// <param name="other">Coordinates to compare to</param>
		/// <param name="tol">Tolerance for equality</param>
		/// <returns>True if both the X and Y deviation from other by less than tol</returns>
		public bool ApproxEquals(Vector2 other, double tol)
		{
			return Math.Abs(X - other.X) < tol && Math.Abs(Y - other.Y) < tol;
		}

		public bool IsNaN
		{
			get { return double.IsNaN(X) || double.IsNaN(Y); }
		}

		/// <summary>
		/// The X coordinate.
		/// </summary>
		public double X;

		/// <summary>
		/// The Y coordinate.
		/// </summary>
		public double Y;

		public RobotPose ToRobotPose()
		{
			return new RobotPose(X, Y, 0, 0, 0, 0, 0);
		}

		#region ILoggable Members

		public string ToLog()
		{
			return this.X + "\t" + this.Y;
		}

		#endregion
	}

}
