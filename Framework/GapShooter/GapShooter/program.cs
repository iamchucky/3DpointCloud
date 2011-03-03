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
using Magic.Common.Shapes;
using Magic.Common.Mapack;

namespace Magic.PathPlanning
{
	public class GapShooter
	{
		IPath pathCurrentlyTracked;
		IPathSegment segmentCurrentlyTracked;
		RobotPose currentPoint = new RobotPose();
		public Vector2 goalpoint = new Vector2(); //must be in robot frame
		private double a = 10; //parameter for polar histogram
		private double dmax = 1; //parameter for maximum laser range to consider
		private double b; //parameter for polar histogram (b = a/dmax)
		private double alpha = .5; //resolution of polar histogram (angle of a single sector)
		private double threshold = 20; // polar histogram threshold for finding valleys, less strict when higher
		private bool trapped = false; // represents if histogram was able to find a valley
		private int kthreshold; // threshold for steering angle, adjust to control distance from obstacles
		private int ktarget = 0; // direction of target
		private int numk;
		private double[] hkold;
		private double hysteresismin;
		private double Rbloat;

		public GapShooter()
		{
			pathCurrentlyTracked = null;
			kthreshold = (int)(Math.Round(160 / alpha));
			b = a / Math.Pow(dmax, 2);
			hysteresismin = threshold * .75;
			numk = (int)(Math.Round(360 / alpha));
			hkold = new double[numk];
			for (int i = 0; i < numk; i++)
			{
				hkold[i] = 0;
			}
			Rbloat = .35 + .05;
		}

		public IPath PathCurrentlyTracked
		{
			get { return pathCurrentlyTracked; }
		}

		public void UpdatePose(RobotPose pose)
		{
			currentPoint = new RobotPose(pose);
			currentPoint.z = 0;
		}

		public void UpdatePath(IPath path)
		{
			if (path == null || path.Count == 0) return;

			double dist = 0.45;
			PointOnPath lookAhead = path.AdvancePoint(path.StartPoint, ref dist);

			Vector2 goalpointGlobal = lookAhead.pt;
			double xg = (goalpointGlobal.X - currentPoint.x) * Math.Cos(currentPoint.yaw) + (goalpointGlobal.Y - currentPoint.y) * Math.Sin(currentPoint.yaw);
			double yg = -(goalpointGlobal.X - currentPoint.x) * Math.Sin(currentPoint.yaw) + (goalpointGlobal.Y - currentPoint.y) * Math.Cos(currentPoint.yaw);
			goalpoint = new Vector2(xg, yg);
		}

		public void UpdatePath(IPath path, LineSegment gap)
		{
			if (path == null || path.Count == 0) return;

			/*double dist = 0.45;
			PointOnPath lookAhead = path.AdvancePoint(path.StartPoint, ref dist);

			Vector2 goalpointGlobal = lookAhead.pt;
			double xg = (goalpointGlobal.X - currentPoint.x) * Math.Cos(currentPoint.yaw) + (goalpointGlobal.Y - currentPoint.y) * Math.Sin(currentPoint.yaw);
			double yg = -(goalpointGlobal.X - currentPoint.x) * Math.Sin(currentPoint.yaw) + (goalpointGlobal.Y - currentPoint.y) * Math.Cos(currentPoint.yaw);
			goalpoint = new Vector2(xg, yg);*/

			double dist = 1.5;
			PointOnPath lookAhead = path.AdvancePoint(path.StartPoint, ref dist);

			Vector2 mean = new Vector2((gap.P0.X + gap.P1.X)/2, (gap.P0.Y + gap.P1.Y)/2);
			double theta = Math.Atan2(gap.P0.Y - gap.P1.Y, gap.P0.X - gap.P1.X);

			Vector2 goal1 = mean + 0.25 * (new Vector2(Math.Cos(theta + Math.PI / 2), Math.Sin(theta + Math.PI / 2)));
			Vector2 goal2 = mean - 0.25 * (new Vector2(Math.Cos(theta + Math.PI / 2), Math.Sin(theta + Math.PI / 2)));

			Vector2 goalpointGlobal = (goal1.DistanceTo(lookAhead.pt) < goal2.DistanceTo(lookAhead.pt)) ? goal1 : goal2;

			double xg = (goalpointGlobal.X - currentPoint.x) * Math.Cos(currentPoint.yaw) + (goalpointGlobal.Y - currentPoint.y) * Math.Sin(currentPoint.yaw);
			double yg = -(goalpointGlobal.X - currentPoint.x) * Math.Sin(currentPoint.yaw) + (goalpointGlobal.Y - currentPoint.y) * Math.Cos(currentPoint.yaw);
			goalpoint = new Vector2(xg, yg);

			/*double dist = 1.5;
			PointOnPath lookAhead = path.AdvancePoint(path.StartPoint, ref dist);

			double goal1Y = (gap.P0.Y + gap.P1.Y) / 2 + (gap.P0.X - gap.P1.X) / (gap.Length) * 0.25;
			double goal1X = (gap.P0.X + gap.P1.X) / 2 + (gap.P0.Y - gap.P1.Y) / (gap.Length) * 0.25;
			Vector2 goal1 = new Vector2(goal1X, goal1Y);

			double goal2Y = (gap.P0.Y + gap.P1.Y) / 2 + (gap.P0.X - gap.P1.X) / (gap.Length) * -0.25;
			double goal2X = (gap.P0.X + gap.P1.X) / 2 + (gap.P0.Y - gap.P1.Y) / (gap.Length) * -0.25;
			Vector2 goal2 = new Vector2(goal2X, goal2Y);

			goalpoint = (goal1.DistanceTo(lookAhead.pt) < goal2.DistanceTo(lookAhead.pt)) ? goal1 : goal2;*/
		}

		public RobotTwoWheelCommand GetCommand(ILidarScan<ILidar2DPoint> LidarScan, int angleTolerance, double threshold, double vmax, double wmax, out List<Polygon> polys)
		{
			// convert laser data into polar direction and magnitude
			//ScanToTextFile(LidarScan); //export scan data to analyze in matlab
			hysteresismin = threshold * .75;
			double[] magnitudes, betas, gammas;
			int n = LidarScan.Points.Count;
			magnitudes = new double[n];
			betas = new double[n];
			gammas = new double[n];
			for (int ii = 0; ii < n; ii++)
			{

				if (LidarScan.Points[ii].RThetaPoint.R < dmax)
				{
					magnitudes[ii] = (a - Math.Pow(LidarScan.Points[ii].RThetaPoint.R, 2) * b);
					if (LidarScan.Points[ii].RThetaPoint.R < Rbloat)
						gammas[ii] = Math.PI / 2;
					else
						gammas[ii] = Math.Asin(Rbloat / LidarScan.Points[ii].RThetaPoint.R);
				}
				else
				{
					magnitudes[ii] = 0;
					gammas[ii] = Math.Asin(Rbloat / dmax);
				}
				double betaii = LidarScan.Points[ii].RThetaPoint.thetaDeg;
				while (betaii < 0) { betaii = betaii + 360; }
				while (betaii > 360) { betaii = betaii - 360; }
				betas[ii] = betaii;
			}

			// create polar histogram
			double[] hk = new double[numk];
			double[] angles = new double[numk]; //corresponding angle
			for (int z = 0; z < numk; z++)
			{
				hk[z] = 0;
				angles[z] = z * alpha;
			}
			for (int j = 0; j < magnitudes.Count(); j++)
			{
				//int kmin = (int)(((betas[j] - gammas[j] + 360) % 360) / alpha);
				//int kmax = (int)(((betas[j] + gammas[j] + 360) % 360) / alpha);
				int k = (int)(betas[j] / alpha);
				//for (int k = kmin; k <= kmax; k++)
					hk[k] = hk[k] + magnitudes[j];
			}
			for (int k = 0; k < numk; k++)
			{
				if (hkold[k] >= threshold && (hk[k] > hysteresismin && hk[k] < threshold))
					hk[k] = hkold[k];
			}
			hkold = hk;

			// smooth the histogram
			double[] hkprime = Smoother(hk, 5);

			/*for (int i = 0; i < numk; i++)
				Console.WriteLine(angles[i] + "," + hk[i] + "," + hkprime[i]);

			Console.WriteLine("END +++++++++++++++++++++++");*/

			// Temporary building of polygons
			//List<Vector2> points = new List<Vector2>();
			polys = new List<Polygon>();
			/*for (int i = 0; i < numk; i++)
			{
				points.Add(new Vector2(hkprime[i] * Math.Cos(angles[i] * Math.PI / 180), hkprime[i] * Math.Sin(angles[i] * Math.PI / 180)));
			}
			polys.Add(new Polygon(points));*/

			// find candidate valleys
			double goalangle = 180 * (goalpoint.ArcTan) / Math.PI;
			ktarget = (int)(Math.Round(goalangle / alpha));
			List<double[]> valleys = FindValleys(hkprime);
			if (!trapped)
			{
				// pick out best candidate valley
				double[] valley = PickBestValley(valleys);
				// calculate steering angle
				double steerangle = FindSteerAngle(valley); //degrees
				// calculate speed commands
				double vmin = 0; // (m/s)
				//double vmax = .2; // (m/s)
				//double wmax = 25; // (deg/s)
				double w = 0;
				double v = 0;
				double steerwindow = angleTolerance;
				if (Math.Abs(steerangle) > steerwindow)
				{
					w = wmax * Math.Sign(steerangle);
				}
				else
				{
					w = wmax * (steerangle / steerwindow);
				}

				/*points = new List<Vector2>();
				points.Add(new Vector2(0, 0));
				points.Add(new Vector2(threshold * 1.5 * Math.Cos(steerangle * Math.PI / 180), threshold * 1.5 * Math.Sin(steerangle * Math.PI / 180)));
				polys.Add(new Polygon(points));

				points = new List<Vector2>();
				points.Add(new Vector2(0, 0));
				points.Add(new Vector2(threshold * 1 * Math.Cos(goalangle * Math.PI / 180), threshold * 1 * Math.Sin(goalangle * Math.PI / 180)));
				polys.Add(new Polygon(points));

				points = new List<Vector2>();
				for (int i = 0; i < 180; i++)
				{
					points.Add(new Vector2(threshold * Math.Cos(i * 2 * Math.PI / 180), threshold * Math.Sin(i * 2 * Math.PI / 180)));
				}
				polys.Add(new Polygon(points));*/

				double hc = Math.Min(hk[0], threshold);
				double vp = vmax * (1 - hc / threshold);
				v = vp * (1 - Math.Abs(w) / wmax) + vmin;
				double vcommand = v * 3.6509; // multiply by 3.6509 to get "segway units"
				double wcommand = w * 3.9; // convert to deg/sec and multiply by 3.9 for "segway units" (counts/deg/sec)	
				//RobotTwoWheelCommand command = new RobotTwoWheelCommand(0, 0);	
				RobotTwoWheelCommand command = new RobotTwoWheelCommand(vcommand * 2.23693629, wcommand);
				return command;
			}
			else // trapped (no valleys were found), so stop, or lift threshold
			{
				RobotTwoWheelCommand command = new RobotTwoWheelCommand(0, 0);
				return command;
			}
		}

        private double[] CombinedHistogram(List<double[]> H)
        {
            double[] combinedH = new double[numk];

            for (int k = 0; k < numk; k++)
            {
                combinedH[k] = 0;
                for (int i = 0; i < H.Count; i++)
                {
                    double value = H[i][k];
                    if (combinedH[k] < value)
                        combinedH[k] = value;
                }
            }
            return combinedH;
        }


		private double FindSteerAngle(double[] valley)
		{
			double knear = 0;
			double kfar = 0;
			if (Math.Abs(valley[0] - valley[1]) < kthreshold) // narrow valley
			{
				knear = valley[0];
				kfar = valley[1];
			}
			else /* wide valley */
			{
				if (ktarget > valley[0] + kthreshold / 2 && ktarget < valley[1] - kthreshold / 2) // very close to middle of valley
				{
					knear = kfar = ktarget;
				}
				else // not within valley or too close to edge of valley
				{
					if (Math.Abs(valley[1] - ktarget) > Math.Abs(ktarget - valley[0])) // pick nearest k -> valley0 is closest
					{
						knear = valley[0];
						kfar = valley[0] + kthreshold;
					}
					else // valley1 is closest
					{
						knear = valley[1];
						kfar = valley[1] - kthreshold;
					}
				}
			}

			double ksteer = (knear + kfar) / 2;
			double steerangle = ksteer * alpha;
			if (steerangle > 180) { steerangle = steerangle - 360; }
			return steerangle;
		}

		private double[] PickBestValley(List<double[]> valleys)
		{
			int bestIdx = -1, bestEdge = int.MaxValue;

			for (int i = 0; i < valleys.Count; i++)
			{
				if (Math.Abs(valleys[i][0] - ktarget) < bestEdge)
				{
					bestEdge = (int)Math.Abs(valleys[i][0] - ktarget);
					bestIdx = i;
				}

				if (Math.Abs(valleys[i][1] - ktarget) < bestEdge)
				{
					bestEdge = (int)Math.Abs(valleys[i][1] - ktarget);
					bestIdx = i;
				}
			}

			return valleys[bestIdx];
		}


		/*double[] kmiddle = new double[valleys.Count];
		double kdiff = System.Double.MaxValue;
		int vclosest = 0;
		for (int v = 0; v < valleys.Count; v++)
		{
			if (ktarget > valleys[v][0] && ktarget < valleys[v][1])
				return valleys[v];
			kmiddle[v] = (valleys[v][0] + valleys[v][1]) / 2;
			if (Math.Abs(kmiddle[v] - ktarget) <  kdiff)
			{
				kdiff = Math.Abs(kmiddle[v] - ktarget);
				vclosest = v;
			}
		}
		return valleys[vclosest];
	}*/

		private List<double[]> FindValleys(double[] h)
		{
			List<double[]> valleys = new List<double[]>();
			int kstart = (int)(180 / alpha);
			while (h[kstart] > threshold && kstart < numk + 1) { kstart = kstart + 1; }
			if (kstart == numk) { trapped = true; return null; }
			int k1 = kstart;
			int k2 = kstart + 1;
			while (k2 % numk != kstart)
			{
				if (h[k2 % numk] < threshold) { k2 = k2 + 1; }
				else
				{
					if (h[k1 % numk] < threshold)
					{
						valleys.Add(new double[2] { k1 - numk, k2 - 1 - numk });
						k1 = k2;
					}
					else { k2 = k2 + 1; k1 = k2; }
				}
			}
			if (h[k2 % numk] < threshold && h[k1 % numk] < threshold)
			{
				valleys.Add(new double[2] { k1 - numk, k2 - 1 - numk });
			}
			return valleys;
		}

		private double[] Smoother(double[] h, int L)
		{
			int n = h.Length;
			double[] hprime = new double[n];

			for (int i = 0; i < n; i++)
			{
				i += n;

				double sum = 0;

				for (int j = -L; j <= L; j++)
				{
					sum += (L - Math.Abs(j) + 1) * h[(i + j) % n];
				}

				i -= n;
				hprime[i] = sum / (L * L + 2 * L + 1);
			}

			return hprime;
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

	}
}
