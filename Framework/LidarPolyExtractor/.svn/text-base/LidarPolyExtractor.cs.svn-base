using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;
using Magic.Common.Sensors;
using Magic.Common;
using Magic.Common.Mapack;

namespace Magic.Sensors
{
    public class LidarPolyExtractor
    {
        RobotPose curRobotPose;
        SensorPose curLidarToBody;
        double lidarMaxPracticalRange = 40;
        double threshold = 0.20;
        double vehicleRadius = 1;
        public LidarPolyExtractor(double threshold, RobotPose curRobotPose, SensorPose curLidarToBody)
        {
            this.curLidarToBody = curLidarToBody;
            this.curRobotPose = curRobotPose;
            this.threshold = threshold;
        }

        public void UpdateRobotPose(RobotPose p)
        {
            curRobotPose = new RobotPose (p);
        }

        public void UpdateSensorPose(SensorPose p)
        {
            curLidarToBody = new SensorPose(p);
        }

        public List<Polygon> SegmentScan(ILidarScan<ILidar2DPoint> scan)
        {
            //this is super stupid, but may be functional for now....
            
            //it may be necessary to sort these
            List<Polygon> clusters = new List<Polygon> ();
            List<Vector2> pts = new List<Vector2>(scan.Points.Count);

            //Get the laser to body coordinate system (this is I for now)
            Matrix4 Tlaser2body = Matrix4.FromPose(curLidarToBody);

            //Get the body to global transformation
            Matrix4 Tbody2global = Matrix4.FromPose(curRobotPose);

            //Get a vector from the current lidar pose
            Vector3 lidarInBody = new Vector3((double)curLidarToBody.x, (double)curLidarToBody.y, (double)curLidarToBody.z);

            //Transform the sensor position in body coordinates to the sensor position in global coordinates
            Vector3 lidarInGlobal = Tbody2global.TransformPoint(lidarInBody);

            foreach (ILidar2DPoint pt in scan.Points)
            {
                if (pt.RThetaPoint.R < lidarMaxPracticalRange)
                {
                    //Extract the lidar point in XYZ (laser coordinates)
                    Vector3 v3 = pt.RThetaPoint.ToVector3();

                    //Convert laser to body coordinate system
                    Vector3 vBody = Tlaser2body.TransformPoint(v3);

                    //Convert body to global cooridnate system
                    Vector3 vGlobal = Tbody2global.TransformPoint(vBody);
                    pts.Add(new Vector2 (vGlobal.X, vGlobal.Y));
                }                                           
            }

            //int slidingWindow = 5;
            Polygon p = new Polygon ();
            p.Add (pts[0]);
            for (int i = 1; i < pts.Count; i++)
            {
                if (Math.Abs(pts[i].DistanceTo(pts[i - 1])) < threshold)
                    p.Add(pts[i]);
                else
                {
                    //create a new poly
                    clusters.Add(p);
                    p = new Polygon();                    
                    p.Add(pts[i]);
                }
            }
            clusters.Add(p); //add the last polygon
            Console.WriteLine("Got " + clusters.Count + " polys");
            
            Polygon vehiclePolygon = VehiclePolygonWithRadius(vehicleRadius); // vehicle model of polygon. Note that radius is parameter

            // Inside foreach, I cannot reassign eachpolygon. So, make a List<Polygon>, assign into this, and return this
            List<Polygon> ret = new List<Polygon>(clusters.Count);
            foreach (Polygon eachPolygon in clusters)
            {
                // Do the MinKowskiConvolution and GrahamScan to remove interior points
                //Polygon convolutionPolygon = Polygon.ConvexMinkowskiConvolution(eachPolygon, vehiclePolygon);
                Polygon convexHull;
                if (eachPolygon.Count > 1)
                {
                    convexHull = Polygon.GrahamScan(eachPolygon, .001);
                }
                else
                {
                    convexHull = eachPolygon;
                }
                Polygon bloated = Polygon.ConvexMinkowskiConvolution(convexHull, vehiclePolygon);
                //convexHull.Add(convexHull[0]);
                ret.Add(bloated);
                //convolutionPolygonList.Add(eachPolygon);

            }
//            convolutionPolygonList = DecomposePolygonList(convolutionPolygonList);
            
            return ret;

        }
        /// <summary>
        /// Create and return octagon model of vehicle from given radius
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Polygon VehiclePolygonWithRadius(double r)
        {
            Polygon poly = new Polygon();
            poly.Add(new Vector2(r, r));
            poly.Add(new Vector2(-r, r));
            poly.Add(new Vector2(-r, -r));
            poly.Add(new Vector2(r, -r));
            /*
            for (int i = -5; i < 7; i += 2)
            {
                poly.Add(new Vector2(r * Math.Cos(Math.PI * i / 8), r * Math.Sin(Math.PI * i / 8)));
            }
             * */
            return poly;
        }
    }
}
