using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Path;
using Magic.PathPlanning;
using Magic.Common.Shapes;
using Magic.Common.DataTypes;
using Magic.OccupancyGrid;
using Magic.PathSmoother;
using Magic.Common.Mapack;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing;

namespace Magic.PathPlanning
{
	public class DStarPlanner
	{
		int robotID;
		RobotPose pose;
		DStarAlgorithm dstar;
        PathSmoother.PathSmoother smoother;

		List<Waypoint> waypoints;
		List<Polygon> bloatedObstacles;
		object bloatedObstacleLock = new object();
		object ogLock = new object();

		OccupancyGrid2D og;
		int waypointsAchieved;

		double achieveThreshold, vehicleRadius, collideThreshold, collideBackoff, resX, resY, extentX, extentY;
		double collapseThreshold, bezierSegmentation, blurWeight;

		public event EventHandler<WaypointAchievedEventArgs> WaypointAchieved;

        Bitmap lastBmp;

		public DStarPlanner(double vehicleRadius, double achieveThreshold, double collideThreshold, double collideBackoff, double collapseThreshold, double bezierSegmentation, double blurWeight, double resX, double resY, double extentX, double extentY, int robotID)
		{
			this.vehicleRadius = vehicleRadius;
			this.achieveThreshold = achieveThreshold;
			this.collideThreshold = collideThreshold;
			this.collideBackoff = collideBackoff;

			this.collapseThreshold = collapseThreshold;
			this.bezierSegmentation = bezierSegmentation;
            this.blurWeight = blurWeight;

			this.resX = resX;
			this.resY = resY;
			this.extentX = extentX;
			this.extentY = extentY;

			waypointsAchieved = 0;
			this.robotID = robotID;

			dstar = new DStarAlgorithm(blurWeight, resX, resY, extentX, extentY);
            smoother = new PathSmoother.PathSmoother(vehicleRadius, resX, resY, extentX, extentY);
		}

		private double DoPathsCollide(IPath path1, IPath path2)
		{
			int path1Idx = 0, path2Idx = 0;
			double path1Dist = 0, path2Dist = 0, collideDist1, collideDist2;

			// path1Dist and path2Dist are the cumulative distances traveled for each path
			// calculated at the end of the current line segment.
			path1Dist = path1[0].Length;
			path2Dist = path2[0].Length;

			while (path1Idx < path1.Count && path2Idx < path2.Count)
			{
				if (DoSegmentsCollide(path1[path1Idx], path2[path2Idx], out collideDist1, out collideDist2))
				{
					if ((path1Dist - path1[path1Idx].Length + collideDist1) - (path2Dist - path2[path2Idx].Length + collideDist2) < 1.5)
						return (path1Dist - path1[path1Idx].Length + collideDist1);
				}

				if (path1Dist < path2Dist)
				{
					path1Idx++;

					if (path1Idx >= path1.Count)
						break;

					path1Dist += path1[path1Idx].Length;
				}
				else if (path1Dist >= path2Dist)
				{
					path2Idx++;

					if (path2Idx >= path2.Count)
						break;

					path2Dist += path2[path2Idx].Length;
				}
			}

			return -1;
		}

		private bool DoSegmentsCollide(IPathSegment seg1, IPathSegment seg2, out double collideDist1, out double collideDist2)
		{
			bool doesIntersect = false;
			Vector2 intersectPoint;
			Vector2[] p = new Vector2[4];

			collideDist1 = Double.MaxValue;
			collideDist2 = Double.MaxValue;

			LineSegment ls1 = new LineSegment(seg1.Start, seg1.End);
			LineSegment ls2 = new LineSegment(seg2.Start, seg2.End);

			doesIntersect = ls1.Intersect(ls2, out intersectPoint);

			p[0] = ls1.ClosestPoint(seg2.Start);
			p[1] = ls1.ClosestPoint(seg2.End);
			p[2] = ls2.ClosestPoint(seg1.Start);
			p[3] = ls2.ClosestPoint(seg1.End);

			if (doesIntersect)
			{
				collideDist1 = seg1.Start.DistanceTo(intersectPoint);
				collideDist2 = seg2.Start.DistanceTo(intersectPoint);
			}

			if ((p[0] - seg2.Start).Length < collideThreshold)
			{
				if (collideDist1 > seg1.Start.DistanceTo(p[0]))
				{
					collideDist1 = seg1.Start.DistanceTo(p[0]);
					collideDist2 = 0;
					doesIntersect = true;
				}
			}

			if ((p[1] - seg2.End).Length < collideThreshold)
			{
				if (collideDist1 > seg1.Start.DistanceTo(p[1]))
				{
					collideDist1 = seg1.Start.DistanceTo(p[1]);
					collideDist2 = seg2.Length;
					doesIntersect = true;
				}
			}

			if ((p[2] - seg1.Start).Length < collideThreshold)
			{
				collideDist1 = 0;
				collideDist2 = seg2.Start.DistanceTo(p[2]);
			}

			if ((p[3] - seg1.End).Length < collideThreshold)
			{
				if (collideDist1 > seg1.Length)
				{
					collideDist1 = seg1.Length;
					collideDist2 = seg2.Start.DistanceTo(p[3]);
				}
			}

			return doesIntersect;
		}

		#region IPathPlanner Members

		public void Dispose()
		{
			Console.WriteLine("Hasta la vista, baby. (From Dstar)");
		}

		public bool IsPathClear(IPath path, List<Polygon> obstacles)
		{
			if (path == null || path.Count == 0) return false;
			return smoother.IsEntireLinePathClear(path, obstacles);
		}

		public IPath GetPathToGoal(IPath[] otherPaths, out IPath sparsePath, out bool inObstacle, out bool wpInObstacle)
		{
			inObstacle = false;
			wpInObstacle = false;
			sparsePath = null;

			if (waypoints == null) return null;
			if (waypointsAchieved >= waypoints.Count) return null;
			if (og == null) return null;
			if (bloatedObstacles == null)
			{
				Console.WriteLine("Convolved obstacles fail!");
				return null;
			}

			// Add current pose position to filteredWaypoints. (FilteredWaypoints is what we will be
			// planning over. Waypoints is list of user given waypoints.)
			List<Waypoint> filteredWaypoints = new List<Waypoint>();
			filteredWaypoints.Add(new Waypoint(pose.ToVector2(), true, 0));

			lock (ogLock)
			{
				if (og.GetCell(pose.x, pose.y) == 255)
				{
					inObstacle = true;
					og.SetCell(pose.x, pose.y, 0);
				}
			}

			// Only add user waypoints to PathPoints if they have not been achieved yet.
			// If they are inside an obstacle, return a null path.
			foreach (Waypoint wp in waypoints)
			{
				if (!wp.Achieved)
				{
					lock (bloatedObstacleLock)
					{
						foreach (Polygon p in bloatedObstacles)
						{
                            if (p.IsInside(wp.Coordinate))
                            {
                                wpInObstacle = true;
								return null;
                            }
						}
					}
					filteredWaypoints.Add(wp);
				}
			}

			// Plan each segment of the filteredWaypoints individually and stitch them together.
			List<Waypoint> completeRawPath = new List<Waypoint>();

			for (int i = 0; i < filteredWaypoints.Count - 1; i++)
			{
				bool pathOk;
                List<Waypoint> rawPath;

                lock (ogLock)
                {
                    rawPath = dstar.FindPath(filteredWaypoints[i], filteredWaypoints[i + 1], og, out pathOk);
                }

				if (!pathOk) return null;
				else
				{
					int j;

					// We want to only add the first path waypoint if it is the first
					// segment of the whole path.
					if (i == 0) j = 0;
					else j = 1;

					for (; j < rawPath.Count; j++)
						completeRawPath.Add(rawPath.ElementAt(j));
				}
			}

			if (completeRawPath.Count > 0)
			{
				IPath bezierPath;
				PointPath segmentedBezierPath;

				lock (bloatedObstacleLock)
				{
					bezierPath = smoother.Wp2BezierPath(completeRawPath, bloatedObstacles, collapseThreshold);
				}

				sparsePath = new PointPath(completeRawPath);
				segmentedBezierPath = smoother.ConvertBezierPathToPointPath(bezierPath, bezierSegmentation);

				// We want to check our sparse path against other sparse paths to see
				// if there are any intersections. If there are, we want to know if the
				// intersections will be a problem. If it is a problem, bloat that sparse path,
				// add it to obstacles, and replan.
				for (int i = 0; i < otherPaths.Length; i++)
				{
					if (otherPaths[i] != null && otherPaths[i].Count > 0 && i + 1 != robotID)
                    {
                        double intersectDist = DoPathsCollide(sparsePath, otherPaths[i]);

                        if (intersectDist != -1)
                        {
                            PointOnPath collisionPt;
                            int collisionSegmentIndex;
							
							double tempDist = intersectDist - collideBackoff;
							tempDist = Math.Max(0, tempDist);

                            collisionPt = segmentedBezierPath.AdvancePoint(segmentedBezierPath.StartPoint, ref tempDist);
                            collisionSegmentIndex = segmentedBezierPath.IndexOf(collisionPt.segment);

                            segmentedBezierPath.RemoveRange(collisionSegmentIndex, segmentedBezierPath.Count - collisionSegmentIndex);

							tempDist = intersectDist - collideBackoff;
							tempDist = Math.Max(0, tempDist);

							collisionPt = sparsePath.AdvancePoint(sparsePath.StartPoint, ref tempDist);
							collisionSegmentIndex = sparsePath.IndexOf(collisionPt.segment);

							sparsePath.RemoveRange(collisionSegmentIndex, sparsePath.Count - collisionSegmentIndex - 1);
							sparsePath[collisionSegmentIndex].End = collisionPt.pt;
                        }
                    }
				}

				return segmentedBezierPath;
			}

			return null;
		}

		public void UpdateObstacles(List<Polygon> obstacles, out List<Polygon> bloated)
		{
			UpdateObstacles(obstacles, vehicleRadius, out bloated);
		}

		public void UpdateObstacles(List<Polygon> obstacles, double r, out List<Polygon> bloated)
		{
			if (pose == null)
			{
				bloated = default(List<Polygon>);
				return;
			}

			lock (bloatedObstacleLock)
			{
				bloatedObstacles = new List<Polygon>();

				foreach (Polygon p in obstacles)
                    bloatedObstacles.Add(Polygon.ConvexMinkowskiConvolution(Polygon.VehiclePolygonWithRadius(r), Polygon.GrahamScan(p.points, 1e-3)));
			}

			bloated = bloatedObstacles;

			// Following is deprecated for new height map planning

			Bitmap bmp = new Bitmap((int)Math.Round(2 * extentX / resX), (int)Math.Round(2 * extentY / resY));
			Graphics canvas = Graphics.FromImage(bmp);
			Pen pen = new Pen(Color.Blue, 1);
			PointF[] points;

			lock (bloatedObstacleLock)
			{
				foreach (Polygon p in bloatedObstacles)
				{
					points = new PointF[p.points.Count];

					for (int i = 0; i < p.points.Count; i++)
					{
						points[i] = new PointF((float)(p.points.ElementAt(i).X / resX + extentX / resX),
							(float)(p.points.ElementAt(i).Y / resY + extentY / resY));
					}

					canvas.DrawPolygon(pen, points);
				}
			}

			bmp = Blur(bmp, pose, (int)(10 / resX) * 2);
			bmp = Blur(bmp, pose, (int)(10 / resX) * 2);
			//bmp = Blur(bmp, pose, (int)(10 / resX) * 2);
			//bmp = Blur(bmp, pose, (int)(10 / resX) * 2);
			//bmp = Blur(bmp);
			//bmp = Blur(bmp);

            lock (ogLock)
            {
                og = new OccupancyGrid2D(resX, resY, extentX, extentY);
                og.SetCellsFast(bmp);//, pose, og, 10);
            }
		}

		public void UpdateOG(OccupancyGrid2D og)
		{
			lock (ogLock)
			{
				this.og = og;
			}
		}
		private unsafe Bitmap Blur(Bitmap bmp, RobotPose pose, int blurDiameter)
		{
			Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

			BitmapData newData = newBmp.LockBits(new Rectangle(0, 0, newBmp.Width, newBmp.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			byte* newPtr = (byte*)newData.Scan0;
			byte* ptr = (byte*)data.Scan0;

			int poseIdxX = (int)((pose.x / resX) + (extentX / resX));
			int poseIdxY = (int)((pose.y / resY) + (extentY / resY));

			newPtr += ((poseIdxY - blurDiameter/2) * data.Stride) + (poseIdxX - blurDiameter/2) * 3;
			ptr += ((poseIdxY - blurDiameter/2) * data.Stride) + (poseIdxX - blurDiameter/2) * 3;

			for (int i = 0; i < blurDiameter; i++)
			{
				for (int j = 0; j < blurDiameter; j++)
				{
					int sum = 0;

					if (ptr[0] == 255) sum = 255;
					else
					{
						sum += *(ptr);

						if (j > 0)
							sum += *(ptr - 3);

						if (j < data.Width - 1)
							sum += *(ptr + 3);

						if (i > 0)
							sum += *(ptr - data.Stride);

						if (i < data.Height - 1)
							sum += *(ptr + data.Stride);


						sum /= 4;
						if (sum > 255) sum = 255;
					}

					newPtr[0] = (byte)sum;

					ptr += 3;
					newPtr += 3;
				}

				ptr += (data.Width - blurDiameter) * 3;
				newPtr += (data.Width - blurDiameter) * 3;
			}

			newBmp.UnlockBits(newData);
			bmp.UnlockBits(data);

			return newBmp;
		}

		private unsafe Bitmap Blur(Bitmap bmp)
		{
			Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

			BitmapData newData = newBmp.LockBits(new Rectangle(0, 0, newBmp.Width, newBmp.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			byte* newPtr = (byte*)newData.Scan0;
			byte* ptr = (byte*)data.Scan0;

			for (int i = 0; i < data.Height; i++)
			{
				for (int j = 0; j < data.Width; j++)
				{
					int sum = 0;

					if (ptr[0] == 255) sum = 255;
					else
					{
						sum += *(ptr);

						if (j > 0) 
							sum += *(ptr - 3);
						
						if (j < data.Width-1) 
							sum += *(ptr + 3);

						if (i > 0) 
							sum += *(ptr - data.Stride);

						if (i  < data.Height -1) 
							sum += *(ptr + data.Stride);


						sum /= 4;
						if (sum > 255) sum = 255;
					}

					newPtr[0] = (byte)sum;

					ptr += 3;
					newPtr += 3;
				}
			}

			newBmp.UnlockBits(newData);
			bmp.UnlockBits(data);

			return newBmp;
		}

		public IOccupancyGrid2D DEBUGGETOG()
		{
			return this.og;
		}

		public void UpdatePose(RobotPose pose)
		{
			this.pose = pose;

			if (waypoints == null || waypoints.Count == 0) return;
			if (waypointsAchieved >= waypoints.Count) return;

			if (pose.ToVector2().DistanceTo(waypoints.ElementAt(waypointsAchieved).Coordinate) < achieveThreshold)
			{
				waypoints.ElementAt(waypointsAchieved).Achieved = true;
				WaypointAchieved(this, new WaypointAchievedEventArgs(waypoints[waypointsAchieved]));
				waypointsAchieved++;
			}
		}

		public bool UpdateWaypoints(List<Vector2> waypoints)
		{
			this.waypoints = new List<Waypoint>();
			foreach (Vector2 v in waypoints)
				this.waypoints.Add(new Waypoint(v.X, v.Y, true, 0));
			waypointsAchieved = 0;
			return true;
		}

		public List<Polygon> GetObstacles()
		{
			return null;
		}

		#endregion
	}

	public class WaypointAchievedEventArgs : EventArgs
	{
		private Waypoint waypoint;

		public WaypointAchievedEventArgs(Waypoint waypoint)
		{
			this.waypoint = waypoint;
		}

		public Waypoint Waypoint
		{
			get { return waypoint; }
		}
	}
}