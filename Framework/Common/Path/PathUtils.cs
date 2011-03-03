using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;

namespace Magic.Common.Path
{
	public static class PathUtils
	{
		public static bool CheckPathsEqual (IPath p1, IPath p2)
		{
			if (p1 == null) return false;
			if (p2 == null) return false;
			if (p1.Count != p2.Count) return false;
			for (int i = 0; i < p1.Count; i++)
			{
				IPathSegment segment1 = p1[i];
				IPathSegment segment2 = p2[i];
				if (segment1.Start != segment2.Start || segment1.End != segment2.End)
					return false;				
			}
			return true;
		}

		public static bool CheckPathsEqual(List<Vector2> v1, List<Vector2> v2)
		{
			if (v1 == null || v2 == null) return false;
			if (v1.Count != v2.Count) return false;
			for (int i = 0; i < v1.Count; i++)
			{
				if (v1[i] != v2[i]) return false;
			}
			return true;
		}

        public static bool CheckPathsEqual(List<Vector2> v1, IPath p2)
        {
            if (p2 == null) return false;

            List<Vector2> v2 = new List<Vector2>();
            for (int i = 0; i < p2.Count; i++)
                v2.Add(p2[i].Start);
            v2.Add(p2.EndPoint.pt);

            return CheckPathsEqual(v1, v2);
        }

		public static List<Vector2> IPathToPoints(IPath p)
		{
			List<Vector2> pts = new List<Vector2>();
			foreach (IPathSegment lp in p)
				pts.Add(lp.Start);
			pts.Add(p.EndPoint.pt);
			return pts;
		}

		

		//public static PointPath SamplePath(IPath path, double res)
		//{
		//  PointPath ret= new PointPath ();
		//  PointOnPath p = path[0].StartPoint;

		//  double amount = res;
		//  while (true)
		//  {			
		//    path.AdvancePoint(p, ref amount);
		//    ret.Add (new LinePathSegment (
		//    if (amount != 0) break;
		//  }

		//  return ret;
		//}
	}
}
