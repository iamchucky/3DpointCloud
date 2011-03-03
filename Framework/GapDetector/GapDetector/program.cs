using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Robots;
using Magic.Common.Sensors;
using System.Threading;
using Magic.Common.Path;
using Magic.UnsafeMath;
using Magic.Common.Shapes;

namespace Magic.PathPlanning
{
	/// <summary>
	/// Finds and represents the abstract notion of a "gap" in the environment in a concrete, usable way for path/motion planning.
	/// Gaps are represented as LineSegment objects.
	/// </summary>
	public class GapDetector
	{
		public double ignoreabovethis = 1.4; // (meters) gaps larger than this value will be ignored.
        public double avoidbelowthis = .75; // (meters) gaps smaller than this value will be avoided.
        public double rangetoconsider = 3; //range of lidar data for which to consider obstacles. Throw out data further than this.
        public double minseparation = .4; // points further apart than this value will be assumed to be on different polygons.
        public bool isSickUpsideDown = true; //assume that sick is upside down (lidar scan sweeps from left to right)

		public List<Polygon> polygons = new List<Polygon>(); // list of convex polygons in environment.
		public List<LineSegment> GapsToAvoid = new List<LineSegment>(); // list of gaps too small to traverse which must be avoided: smaller than avoidbelowthis.
		public List<LineSegment> ThreateningGaps = new List<LineSegment>(); // list of gaps which must be navigated carefully: larger than avoidbelow this and smaller than ignoreabovethis.
		public List<LineSegment> AllGaps = new List<LineSegment>(); // list of all gaps found in environment (= GapsToAvoid + ThreateningGaps);

		/// <summary>
		/// Default constructor.
		/// Primary method for determining gap representations.
		/// </summary>
		public GapDetector(bool IsSickUpsideDown)
		{
			this.isSickUpsideDown = IsSickUpsideDown;
		}

		/// <summary>
		/// Constructor which allows setting of threshold for sensitivity in detecting gaps and how much laser data to use.
		/// </summary>
		/// <param name="lowerbound">(in meters) gaps smaller than this value will be avoided.</param>
		/// <param name="upperbound">(in meters) gaps larger than this value will be ignored.</param>
        /// <param name="range">(in meters) lidar data further than this will be ignored.</param>
        /// <param name="minpolyseparation">(in meters) threshold distance between lidar points that indicates start of a new polygon.</param>
        /// <param name="sickUpsideDown">indicates if sick is upside down, and thus the direction of lidar scan sweep</param>
        public GapDetector(double avoidbelowthis, double ignoreabovethis, double rangetoconsider, double minseparation, bool IsSickUpsideDown)
		{
            this.avoidbelowthis = avoidbelowthis;
            this.ignoreabovethis = ignoreabovethis;
            this.rangetoconsider = rangetoconsider;
            this.minseparation = minseparation;
            this.isSickUpsideDown = IsSickUpsideDown;
		}

		/// <summary>
		/// Method to fill object variables
		/// </summary>
		/// <param name="lidarscandata">lidar scan data in the form of a List of Vector2s</param>
		public void RunDetector(ILidarScan<ILidar2DPoint> LidarScan, RobotPose pose)
		{
            //ScanToTextFile(LidarScan); //export scan data to analyze in matlab
			List<Vector2> lidarscandata = new List<Vector2>(LidarScan.Points.Count);

            for (int ii = 0; ii < LidarScan.Points.Count; ii++)
            {
                if (LidarScan.Points[ii].RThetaPoint.R < rangetoconsider)
                    lidarscandata.Add(LidarScan.Points[ii].RThetaPoint.ToVector2(isSickUpsideDown));
            }

			if (isSickUpsideDown)
				lidarscandata.Reverse();

			polygons = ClusterScanData(lidarscandata);
            //PolygonsToTextFile(); //export polygon data to analyze in matlab
			AllGaps = FindAllGaps(polygons, pose);
			CheckGaps(AllGaps);
		}

		public bool GapInPath(IPath path, out Vector2 ipt, out LineSegment gap)
		{
			foreach (IPathSegment ps in path)
			{
				foreach (LineSegment ls in ThreateningGaps)
				{
					LineSegment s = new LineSegment(ps.Start, ps.End);

					if (ls.Intersect(s, out ipt))
					{
						gap = ls;
						return true;
					}
				}
			}

			ipt = default(Vector2);
			gap = default(LineSegment);
			return false;
		}

		/// <summary>
		/// Takes a list of vector2 points and extracts polygons using ConvexHullDecompose method.
		/// Polygons are separated based on some tunable threshold on distance between consecutive points.
		/// </summary>
		/// <param name="Scan">List of vector 2 points, presumably from lidar scan</param>
		/// <returns>List of convex polygons</returns>
		private List<Polygon> ClusterScanData(List<Vector2> Scan)
		{
			List<Polygon> SeparatedPolys = new List<Polygon>(); // intialize list of extracted polygons that will be returned by function
			List<Vector2> currentPolypoints = new List<Vector2>(); // set of points defining current polygon being extracted

			if (Scan.Count <= 0)
				return SeparatedPolys;

			currentPolypoints.Add(Scan[0]); // initialized with first scan data point
			for (int i = 1; i < Scan.Count; i++)
			{
				double dist = currentPolypoints[currentPolypoints.Count-1].DistanceTo(Scan[i]); // find distance from last point to current point in scan
				if (dist > minseparation || currentPolypoints.Count > 50) // no longer the same polygon -> separate and find hull
				{
					SeparatedPolys.Add(ConvexHullDecompose(currentPolypoints));
					currentPolypoints.Clear();
				}
				currentPolypoints.Add(Scan[i]);
			}
			SeparatedPolys.Add(ConvexHullDecompose(currentPolypoints));
			return SeparatedPolys;
		}

		/// <summary>
		/// method for decomposing a set of points into a convex polygon hull
		/// </summary>
		/// <param name="points">set of points to decompose into convex hull</param>
		/// <returns>Convex polygon hull</returns>
		private Polygon ConvexHullDecompose(List<Vector2> points)
		{
            if (points.Count < 3)
            {
                return new Polygon(points);
            }
            
            List<Vector2> convexpoints = new List<Vector2>(); //set of points that represent previous guess at convex decomposition of points
			List<Vector2> newconvexpoints = new List<Vector2>(points.Count); //set of points that represent current guess at convex decomposition of points

            foreach (Vector2 pt in points)
                newconvexpoints.Add(new Vector2(pt.X, pt.Y));

			while (convexpoints.Count != newconvexpoints.Count) //keep decomposing until result is the same
			{
                convexpoints.Clear();
                foreach (Vector2 pt in newconvexpoints)
                    convexpoints.Add(new Vector2(pt.X, pt.Y));

				newconvexpoints.Clear();
				newconvexpoints.Add(convexpoints[0]); //update current guess to first value
				double prevangle = (convexpoints[1] - convexpoints[0]).ArcTan; //previous vector angle
				for (int i = 2; i < convexpoints.Count; i++) 
				{
					double currangle = (convexpoints[i] - convexpoints[i-1]).ArcTan; //current vector angle
                    
                    while (currangle - prevangle > Math.PI) //unwrap relative to previous angle
                        currangle = currangle - 2 * Math.PI;
                    
                    while (currangle - prevangle < Math.PI)
                        currangle = currangle + 2 * Math.PI;
                    
                    if (currangle <= prevangle) //check convexity, if convex add point to list of current guess
                        newconvexpoints.Add(convexpoints[i-1]);
                    
                    prevangle = currangle; //update previous vector angle
                }
                newconvexpoints.Add(convexpoints[convexpoints.Count-1]); //keep last value				
            }
            return new Polygon(convexpoints);
		}


		/// <summary>
		/// Finds gaps between polygon objects indiscriminately.
		/// </summary>
		/// <param name="polys">List of polygons to find gaps between</param>
		/// <returns>List of LineSegment objects representing gaps found</returns>
		private List<LineSegment> FindAllGaps(List<Polygon> polys, RobotPose pose)
		{
			List<LineSegment> gaps = new List<LineSegment>();
			for (int i = 0; i < polys.Count; i++)
			{
				Polygon poly1 = polys[i];
				for (int j = i + 1; j < polys.Count; j++)
				{
					Polygon poly2 = polys[j];
					LineSegment gap = poly1.ShortestLineToOther(poly2);
					if (gap.Length <= ignoreabovethis)
					{
						Vector2 startGlobal = new Vector2(Math.Cos(pose.yaw) * gap.P0.X - Math.Sin(pose.yaw) * gap.P0.Y + pose.x,
							Math.Sin(pose.yaw) * gap.P0.X + Math.Cos(pose.yaw) * gap.P0.Y + pose.y);

						Vector2 endGlobal = new Vector2(Math.Cos(pose.yaw) * gap.P1.X - Math.Sin(pose.yaw) * gap.P1.Y + pose.x,
							Math.Sin(pose.yaw) * gap.P1.X + Math.Cos(pose.yaw) * gap.P1.Y + pose.y);

						LineSegment gapGlobal = new LineSegment(startGlobal, endGlobal);

						gaps.Add(gapGlobal);
					}
				}
			}
			return gaps;
		}

		/// <summary>
		/// Checks gap values against thresholds to determine whether the should be ignored, avoided, or cautiously traversed.
		/// Does not return anything, but updates class variables GapsToAvoid and ThreateningGaps.
		/// </summary>
		/// <param name="all">List of LineSegment objects representing all gaps found earlier</param>
		private void CheckGaps(List<LineSegment> all)
		{
            ThreateningGaps.Clear();
            GapsToAvoid.Clear();

			for (int i = 0; i < all.Count; i++)
			{
				if (all[i].Length < avoidbelowthis) { GapsToAvoid.Add(all[i]); }
				else { ThreateningGaps.Add(all[i]); }
			}
		}

        private void ScanToTextFile(ILidarScan<ILidar2DPoint> LidarScan)
        {
            TextWriter Raw = new StreamWriter("C:\\Users\\labuser\\Desktop\\RawData.txt");
            TextWriter Pts = new StreamWriter("C:\\Users\\labuser\\Desktop\\PointData.txt");
            Raw.WriteLine();
            Pts.WriteLine();            
            for (int i = 0; i < LidarScan.Points.Count; i++)
            {
                Raw.Write(LidarScan.Points[i].RThetaPoint.R);
                Raw.Write(" ");
                Raw.WriteLine(LidarScan.Points[i].RThetaPoint.theta);

                Pts.WriteLine(LidarScan.Points[i].RThetaPoint.ToVector2().ToString());
            }
            Raw.Close();
            Pts.Close();
        }

        private void PolygonsToTextFile()
        {
            TextWriter tw = new StreamWriter("C:\\Users\\labuser\\Desktop\\PolygonData.txt");
            tw.WriteLine();
            for (int i = 0; i < polygons.Count; i++)
            {
                tw.WriteLine(polygons[i].ToString());
            }
            tw.Close();
        }

    }
}
