using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Path;
using Magic.Common.Shapes;
using Magic.Common.Splines;

namespace Magic.PathSmoother
{
	public class PathSmoother
	{
		double vehicleRadius, resX, resY, extentX, extentY;

		public PathSmoother(double vehicleRadius, double resX, double resY, double extentX, double extentY)
		{
			this.vehicleRadius = vehicleRadius;
			this.resX = resX;
			this.resY = resY;
			this.extentX = extentX;
			this.extentY = extentY;
		}

		public IPath Wp2CubicPath(IPath path, List<Polygon> obstacles, double nothing)
		{
			PointPath ret = new PointPath();

			CubicBezier[] bez = SmoothingSpline.BuildCardinalSpline(PathUtils.IPathToPoints(path).ToArray(), null, null, 1.0);
			foreach (CubicBezier b in bez)
			{
				ret.Add(new BezierPathSegment(b, null, false));
			}
			return ret;
		}

		public IPath Wp2BezierPath(List<Waypoint> path, List<Polygon> obstacles, double collapseThreshold)
		{
			List<IPathSegment> segments = new List<IPathSegment>();
			List<Vector2> newPath = new List<Vector2>();
			PointPath bezPath;
			Vector2 P0, P1, P2, P3, dXY, newPoint;
			dXY = new Vector2(0, 0);

			double previousCellValue = path.ElementAt(0).TerrainCost;
			bool inPlateau = false, inLocalMin = false;
			double localMin = Double.MaxValue, localMax = Double.MinValue;

			// Go through the path and mark local minimas and maximas
			for (int i = 1; i < path.Count; i++)
			{
				if (inPlateau)
				{
					if (previousCellValue != path.ElementAt(i).TerrainCost)
					{
						inPlateau = false;
						//path.ElementAt(i - 1).Critical = true;
					}
				}
				else
				{
					if (previousCellValue == path.ElementAt(i).TerrainCost)
					{
						inPlateau = true;
						//path.ElementAt(i - 1).Critical = true;
					}

					if (inLocalMin)
					{
						if (path.ElementAt(i).TerrainCost < localMin)
							localMin = path.ElementAt(i).TerrainCost;
						else
						{
							localMin = Double.MaxValue;
							inLocalMin = false;
							//path.ElementAt(i - 1).Critical = true;
						}
					}
					else
					{
						if (path.ElementAt(i).TerrainCost > localMax)
							localMax = path.ElementAt(i).TerrainCost;
						else
						{
							localMax = Double.MinValue;
							inLocalMin = true;
							path.ElementAt(i - 1).Critical = true;
						}
					}
				}

				previousCellValue = path.ElementAt(i).TerrainCost;


			}


			// Collapse nodes down based on proximity to one another
			for (int i = 0; i < path.Count - 1; i++)
			{
				if (path.ElementAt(i).Coordinate.DistanceTo(path.ElementAt(i + 1).Coordinate) < collapseThreshold)
				{
					if (!path.ElementAt(i).UserWaypoint && !path.ElementAt(i).Critical)
					{
						if (!path.ElementAt(i + 1).UserWaypoint && !path.ElementAt(i + 1).Critical)
						{
							newPoint = (path.ElementAt(i).Coordinate + path.ElementAt(i + 1).Coordinate) / 2;
							if (IsClear(newPoint, path.ElementAt(i - 1).Coordinate, obstacles)
								&& IsClear(newPoint, path.ElementAt(i + 2).Coordinate, obstacles))
							{
								path.RemoveRange(i, 2);
								path.Insert(i, new Waypoint(newPoint, false, 0));
								i--;
							}
						}
						else
						{
							newPoint = path.ElementAt(i + 1).Coordinate;
							if (IsClear(newPoint, path.ElementAt(i - 1).Coordinate, obstacles))
							{
								path.RemoveAt(i);
								i--;
							}
						}
					}
					else
					{
						if (!path.ElementAt(i + 1).UserWaypoint && !path.ElementAt(i + 1).Critical)
						{
							newPoint = path.ElementAt(i).Coordinate;
							if (IsClear(newPoint, path.ElementAt(i + 2).Coordinate, obstacles))
							{
								path.RemoveAt(i + 1);
								i--;
							}
						}
					}
				}
			}

			// Change waypoints to Bezier curves
			for (int i = path.Count - 1; i > 0; i--)
			{
				P3 = path.ElementAt(i).Coordinate;
				P0 = path.ElementAt(i - 1).Coordinate;

				if (i == path.Count - 1)
					P2 = P3 - ((P3 - P0) / 3);
				else
				{
					if (dXY.Length > ((P3 - P0) / 4).Length)
						dXY *= ((P3 - P0) / 4).Length / dXY.Length;
					P2 = P3 - dXY;
				}

				if (i == 1)
					P1 = P0 + ((P3 - P0) / 3);
				else
				{
					dXY = (P3 - path.ElementAt(i - 2).Coordinate) / 6;
					if (dXY.Length > ((P3 - P0) / 4).Length)
						dXY *= ((P3 - P0) / 4).Length / dXY.Length;
					P1 = P0 + dXY;
				}

				segments.Add(new BezierPathSegment(P0, P1, P2, P3, 0.05, false));
			}

			segments.Reverse();
			bezPath = new PointPath(segments);

			return bezPath;
		}

		private Boolean IsClear(Vector2 point, List<Polygon> obstacles)
		{
			foreach (Polygon p in obstacles)
				if (p.IsInside(point)) return false;

			return true;
		}

		/// <summary>
		/// walks along the bezier path and creates a straight line segment approximation
		/// </summary>
		/// <param name="path"></param>
		/// <param name="sampleDistance"></param>
		/// <returns></returns>
		public PointPath ConvertBezierPathToPointPath(IPath path, double sampleDistance)
		{
			PointPath pathOut = new PointPath();

			foreach (IPathSegment seg in path)
			{
				if (seg is BezierPathSegment == false) throw new InvalidOperationException();
				BezierPathSegment bseg = seg as BezierPathSegment;

				PointOnPath p = seg.StartPoint;

				double refDist = 0;
				while (refDist == 0)
				{
					PointOnPath p2;
					refDist = sampleDistance;
					p2 = seg.AdvancePoint(p, ref refDist);
					pathOut.Add(new LinePathSegment(p.pt, p2.pt));
					p = p2;
				}
			}
			return pathOut;
		}

		public Boolean IsEntireBezPathClear(IPath p, List<Polygon> obstacles)
		{
			foreach (IPathSegment seg in p)
			{
				if (seg is BezierPathSegment == false) return false;
				BezierPathSegment bseg = seg as BezierPathSegment;
				if (!IsBezSegmentClear(bseg, obstacles, .2)) return false;
			}

			return true;
		}

		public Boolean IsEntireLinePathClear(IPath p, List<Polygon> obstacles)
		{
			foreach (IPathSegment seg in p)
			{
				if (seg is LinePathSegment == false) return false;
				LinePathSegment lseg = seg as LinePathSegment;
				if (IsLineSegmentClear(lseg, obstacles) == false) return false;
			}
			return true;
		}

		private Boolean IsBezSegmentClear(BezierPathSegment seg, List<Polygon> obstacles, double advanceDistance)
		{
			PointOnPath p = seg.StartPoint;

			double refDist = 0;
			while (refDist == 0)
			{
				PointOnPath p2;
				refDist = advanceDistance;
				p2 = seg.AdvancePoint(p, ref refDist);
				foreach (Polygon poly in obstacles)
				{
					if (poly.ConvexDoesIntersect(p.pt, p2.pt)) return false;
				}
				p = p2;
			}
			return true;
		}

		private Boolean IsLineSegmentClear(LinePathSegment seg, List<Polygon> obstacles)
		{
			foreach (Polygon poly in obstacles)
			{
				if (poly.IsInside(seg.Start)) return false;
				if (poly.IsInside(seg.End)) return false;
			}
			return true;
		}

		private Boolean IsClear(BezierPathSegment seg, double minLength, List<Polygon> obstacles)
		{
			foreach (Polygon p in obstacles)
				if (p.Intersect(seg))
				{
					if (seg.Length < minLength)
					{
						return false;
					}

					else
					{
						CubicBezier[] newBeziers = seg.cb.Subdivide(0.5);

						BezierPathSegment newSegA = new BezierPathSegment(newBeziers[0], seg.EndSpeed, seg.StopLine);
						BezierPathSegment newSegB = new BezierPathSegment(newBeziers[1], seg.EndSpeed, seg.StopLine);

						return IsClear(newSegA, minLength, obstacles) && IsClear(newSegB, minLength, obstacles);
					}
				}
			return true;
		}

		private Boolean IsClear(Vector2 start, Vector2 end, List<Polygon> obstacles)
		{
			Vector2[] temp;
			LineSegment line = new LineSegment(start, end);

			foreach (Polygon p in obstacles)
			{
				if (p.Intersect(line, out temp))
					return false;
			}

			return true;
		}
	}
}
