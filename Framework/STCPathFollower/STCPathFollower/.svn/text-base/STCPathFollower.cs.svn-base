using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Robots;
using System.Threading;
using Magic.Common.Path;
using Magic.UnsafeMath;


namespace Magic.PathPlanning
{
    public class STCPathFollower
    {
        RobotPose currentPoint = new RobotPose();
        IPath pathCurrentlyTracked;
        IPathSegment segmentCurrentlyTracked;
        Boolean pathsame;
        Boolean endpointsame; // used to keep track of whether the path was changed by DStarplanner or the user, and thus whether we need to unwrap
        double thetarold = 1337; //our version of null
        double delqthetarold = 1337;
        double qold = 1337;
        double[] xr, yr, vr, wr, hr;
		double[] timestamps;
		int counter = 0;
		TextWriter tw2 = new StreamWriter("getCommandresults.txt");

        public STCPathFollower()
        {
            pathCurrentlyTracked = null;
        }

        public IPath PathCurrentlyTracked
        {
            get { return pathCurrentlyTracked; }
        }

        public void UpdatePose(RobotPose pose)
        {
            {
                this.currentPoint.x = pose.x;
                this.currentPoint.y = pose.y;
                this.currentPoint.yaw = pose.yaw;
            }
        }

        public void UpdatePath(IPath path)
        {
            if (path == null || path.Count == 0) return;
            if (PathUtils.CheckPathsEqual(path, pathCurrentlyTracked)) { this.endpointsame = true; this.pathsame = true; return; } //
            if (pathCurrentlyTracked != null && pathCurrentlyTracked.EndPoint.pt.X == path.EndPoint.pt.X && pathCurrentlyTracked.EndPoint.pt.Y == path.EndPoint.pt.Y)
            {
                this.endpointsame = true;
            }
            this.pathsame = false;
            this.endpointsame = false;
            pathCurrentlyTracked = path;
            segmentCurrentlyTracked = path[0];
        }

        private void GenrefVWHuniformdt(IPath path, double deltadist, double dt)
        {
            PointOnPath currentPointOnPath = path[0].ClosestPoint(currentPoint.ToVector2());
            int numpoints = (int)((path.Length) / deltadist);
			int n = 5; //number of additional points
            xr = new double[numpoints+n];
            yr = new double[numpoints+n];
            double starttimestamp = (DateTime.Now.Ticks - 621355968000000000) / 10000000;
            timestamps = new double[numpoints];
            for (int i = 0; i < timestamps.Length; i++)
            {
                timestamps[i] = starttimestamp + i * dt;
            }

            for (int i = 0; i <= numpoints+3; i++)
            {
                double d = deltadist*(i);
                PointOnPath lookaheadpointi = path.AdvancePoint(currentPointOnPath, ref d);
                xr[i] = lookaheadpointi.pt.X;
                yr[i] = lookaheadpointi.pt.Y;
            }
			
            double[] xrdot = new double[xr.Length - 1];
            double[] yrdot = new double[xrdot.Length];
            double[] xr2dot = new double[xrdot.Length - 1];
            double[] yr2dot = new double[xr2dot.Length];
            double[] vr = new double[xr.Length];
            double[] wr = new double[xr.Length];
            double[] hr = new double[xr.Length];
			
            for (int i = 0; i < xr.Length - 1; i++)
            {
                xrdot[i] = (xr[i + 1] - xr[i]) / dt;
                yrdot[i] = (yr[i + 1] - yr[i]) / dt;
            }
            for (int i = 0; i < xrdot.Length - 1; i++)
            {
                xr2dot[i] = (xrdot[i + 1] - xrdot[i]) / dt;
                yr2dot[i] = (yrdot[i + 1] - yrdot[i]) / dt;
            }
            for (int i = 0; i < xrdot.Length; i++)
            {
                vr[i] = Math.Sqrt(Math.Pow(xrdot[i], 2) + Math.Pow(yrdot[i], 2));
                hr[i] = Math.Atan2(yrdot[i], xrdot[i]); // in radians
                // unwrap headings
                if (i > 0)
                {
                    while (hr[i] - hr[i - 1] >= Math.PI)
                    {
                        hr[i] -= 2 * Math.PI; ;
                    }
                    while (hr[i] - hr[i - 1] <= -Math.PI)
                    {
                        hr[i] += 2 * Math.PI;
                    }
                }
                if (i < xr2dot.Length)
                {
                    wr[i] = ((xrdot[i] * yr2dot[i] - yrdot[i] * xr2dot[i]) / (Math.Pow(vr[i], 2))); // in radians/sec
                }
            }
			//pad the reference vectors
			for (int i = 1; i < n; i++)
			{
				wr[numpoints + i] = 0;
				vr[numpoints + i] = 0;
				hr[numpoints + i] = 0;
			}
			
            TextWriter tw = new StreamWriter("parametrization.txt");
            for (int i = 0; i < timestamps.Length; i++)
            {
                tw.WriteLine("index timestamp x y vr wr hr");
                tw.WriteLine("{0}  {1}  {2}  {3}  {4}  {5}  {6}",i,timestamps[i],xr[i],yr[i],vr[i],wr[i],hr[i]);
            }
            tw.Close();
        }

        public RobotTwoWheelCommand GetCommand()
        {
            double g = 3;
            double zeta = .7; //parameters for STC
            double dt = .4; // adjust to change speed
            double deltadist = .10; //incremental distance for looking ahead
            // deltadist/dt = nominal speed along path
            
            //get trajectory // this step is performed outside of this function by BehavioralDirector
            //discretize trajectory into [xr, yr] 

            //generate reference velocities at each trajectory point [vr, wr, thetar] if new path
            if (!pathsame)
            {
                this.GenrefVWHuniformdt(pathCurrentlyTracked, deltadist, dt);
            }
            //use current timestamp to find corresponding index in reference vectors
            //double currenttime = TimeSync.CurrentTime;
            double currenttime = (DateTime.Now.Ticks - 621355968000000000) / 10000000.0;
            int indextotrack = (int) (Math.Ceiling((currenttime - timestamps[0]) / dt));
            if (indextotrack >= timestamps.Length)
                indextotrack = timestamps.Length - 1;
            Console.WriteLine("indextotrack = {0}", indextotrack);
            Console.WriteLine("time = {0}", currenttime);

            //unwrap thetar against thetarold, ignore if null (1337)
            if (!endpointsame) { thetarold = 1337; }
            if (thetarold != 1337)
            {
                while (hr[indextotrack] - thetarold >= Math.PI)
                {
                    hr[indextotrack] -= 2 * Math.PI; ;
                }
                while (hr[indextotrack] - thetarold <= -Math.PI)
                {
                    hr[indextotrack] += 2 * Math.PI; 
                }
            }
            //set thetarold to thetar
            thetarold = hr[indextotrack];

            double[] qrefdata = { xr[indextotrack], yr[indextotrack], hr[indextotrack] };
            UMatrix qref = new UMatrix(3, 1, qrefdata);
            //print to qref to screen // use only for debugging
            //string qrefstring = qref.ToString();
            //Console.WriteLine("QREF\n {0}", qrefstring);

            //establish pose vector q = [x, y, theta];
            double x, y, theta;
            x = this.currentPoint.x;
            y = this.currentPoint.y;
            theta = this.currentPoint.yaw; // in radians
            double[] qdata = { x, y, theta };
            UMatrix q = new UMatrix(3, 1, qdata);

            //unwrap pose against qref
            while (q[2, 0] - qref[2, 0] >= Math.PI)
            {
                q[2, 0] -= 2 * Math.PI; ;
            }
            while (q[2, 0] - qref[2, 0] <= -Math.PI)
            {
                q[2, 0] += 2 * Math.PI;
            }

            //establish kinematic state error
            UMatrix delq = new UMatrix(3, 1);
            delq = qref - q;

            //if (!endpointsame) { delqthetarold = 1337; }
            //if (delqthetarold != 1337)
            //{
            //    while (delq[2,0] - delqthetarold >= Math.PI)
            //    {
            //        delq[2,0] -= 2 * Math.PI; ;
            //    }
            //    while (delq[2,0] - delqthetarold <= -Math.PI)
            //    {
            //        delq[2,0] += 2 * Math.PI;
            //    }
            //}
            ////set thetarold to thetar
            //delqthetarold = delq[2,0];


            //print delq to screen // use only for debugging
            string delqstring = delq.ToString();
            //Console.WriteLine("ERROR\n {0}", delqstring);
            //Console.ReadLine();

            //create rotation matrix
            double[] Rotdata = { Math.Cos(theta), Math.Sin(theta), 0, -Math.Sin(theta), Math.Cos(theta), 0, 0, 0, 1 };
            UMatrix Rot = new UMatrix(3, 3, Rotdata);

            //convert to robot frame
            UMatrix e = new UMatrix(3, 1);
            e = Rot * delq; //error in the robot reference frame: first element is along track error, second element is cross track error, last element is angle error
            string estring = e.ToString();
            Console.WriteLine("ERROR\n {0}", estring);

            // calculate Uf = [vf, wf]
            double vf = vr[indextotrack] * Math.Cos(e[2, 0]); // in meters/sec
            double wf = wr[indextotrack] * 180 / Math.PI; // in degrees/sec			
            Console.WriteLine("vf = {0}, wf = {1}",vf,wf);

            //calculate wn = sqrt(wr^2 + g*vr^2)
            double wn = Math.Sqrt(Math.Pow(wr[indextotrack], 2) + g * (Math.Pow(vr[indextotrack], 2)));

            //calculte STC gain matrix
            double k1, k2, k3;
            k1 = 2 * zeta * wn;
            k3 = k1;
            k2 = g * vr[indextotrack];
            double[] Kdata = { k1, 0, 0, 0, k2, k3 };
            UMatrix K = new UMatrix(2, 3, Kdata);

            //find control values Ub = K*e
            UMatrix Ub = new UMatrix(2, 1);
            Ub = K * e;
            Console.WriteLine("vb = {0}, wb = {1}", Ub[0, 0], Ub[1, 0]);

            //find total control u = Uf + Ub
            double vcommand = (Ub[0, 0] + vf) * 3.6509; // multiply by 3.6509 to get "segway units"
            double wcommand = (Ub[1, 0] * 180 / Math.PI + wf) * 3.9; // convert to deg/sec and multiply by 3.9 for "segway units" (counts/deg/sec)

            RobotTwoWheelCommand command = new RobotTwoWheelCommand(vcommand * 2.23693629, wcommand);
            if (Math.Abs(wcommand) >= 600)
                Console.WriteLine("W saturation");

			//create text file of information
			if (indextotrack < timestamps.Length && counter < 15)
			{
				if (indextotrack == timestamps.Length - 1) { counter = counter + 1; }
				tw2.WriteLine("{0}  {1}  {2}  {3}  {4}  {5}  {6}  {7}  {8}  {9}  {10}  {11} ", indextotrack, timestamps[indextotrack], x, y, e[0, 0], e[1, 0], e[2, 0], vf, wf, Ub[0, 0], Ub[1, 0]);
			}
			else { tw2.Close(); }

            return command;
        }

    }
}
