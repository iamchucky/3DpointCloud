using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Path;
using Magic.Common.Robots;
using Magic.Common.Sensors;
using Magic.Common.Mapack;
using Magic.Common.Shapes;

namespace GapShooter
{
	public class VFHFollower
	{
		RobotPose pose;

		bool useHokuyo;

		SensorPose sickPose;
		Matrix4 sickDCM;
		SensorPose hokuyoPose;
		Matrix4 hokuyoDCM;

		List<Vector2[]> sickHistory;
		List<Vector2[]> hokuyoHistory;
		Double[] rHistogram;

		double angularResolution;
		int numBuckets;
		Vector2 goalPoint;
		bool trapped;
		int kThreshold;

		public VFHFollower(SensorPose sickPose, SensorPose hokuyoPose, double angularResolution, bool useHokuyo)
		{
			pose = null;

			this.useHokuyo = useHokuyo;

			this.sickPose = sickPose;
			sickDCM = Matrix4.FromPose(this.sickPose);

			if (this.useHokuyo)
			{
				this.hokuyoPose = hokuyoPose;
				hokuyoDCM = Matrix4.FromPose(this.hokuyoPose);
			}

			sickHistory = new List<Vector2[]>();
			hokuyoHistory = new List<Vector2[]>();

			this.angularResolution = angularResolution;
			numBuckets = (int) Math.Round(360 / this.angularResolution);
			trapped = false;
			kThreshold = (int) Math.Round(160 / this.angularResolution);

			rHistogram = new Double[numBuckets];
		}

		public void UpdatePath(IPath path)
		{
			if (path == null || path.Count == 0 || pose == null)
				return;

			double dist = 0.45;
			PointOnPath lookAhead = path.AdvancePoint(path.StartPoint, ref dist);

			Vector2 goalPointGlobal = lookAhead.pt;
			double xg = (goalPointGlobal.X - pose.x) * Math.Cos(pose.yaw) + (goalPointGlobal.Y - pose.y) * Math.Sin(pose.yaw);
			double yg = -(goalPointGlobal.X - pose.x) * Math.Sin(pose.yaw) + (goalPointGlobal.Y - pose.y) * Math.Cos(pose.yaw);
			goalPoint = new Vector2(xg, yg);
		}

		public void UpdatePath(IPath path, LineSegment gap)
		{
			if (path == null || path.Count == 0 || pose == null)
				return;

			double dist = 1.5;
			PointOnPath lookAhead = path.AdvancePoint(path.StartPoint, ref dist);

			Vector2 mean = new Vector2((gap.P0.X + gap.P1.X) / 2, (gap.P0.Y + gap.P1.Y) / 2);
			double theta = Math.Atan2(gap.P0.Y - gap.P1.Y, gap.P0.X - gap.P1.X) + Math.PI / 2;

			Vector2 goal1 = mean + 0.25 * (new Vector2(Math.Cos(theta), Math.Sin(theta)));
			Vector2 goal2 = mean - 0.25 * (new Vector2(Math.Cos(theta), Math.Sin(theta)));

			Vector2 goalPointGlobal = ((goal1.X - lookAhead.pt.X) * (goal1.X - lookAhead.pt.X) + (goal1.Y - lookAhead.pt.Y) * (goal1.Y - lookAhead.pt.Y)
				< (goal2.X - lookAhead.pt.X) * (goal2.X - lookAhead.pt.X) + (goal2.Y - lookAhead.pt.Y) * (goal2.Y - lookAhead.pt.Y)) ? goal1 : goal2;

			double xg = (goalPointGlobal.X - pose.x) * Math.Cos(pose.yaw) + (goalPointGlobal.Y - pose.y) * Math.Sin(pose.yaw);
			double yg = -(goalPointGlobal.X - pose.x) * Math.Sin(pose.yaw) + (goalPointGlobal.Y - pose.y) * Math.Cos(pose.yaw);
			goalPoint = new Vector2(xg, yg);
		}

		public void UpdatePose(RobotPose pose)
		{
			this.pose = pose;
		}

		public RobotTwoWheelCommand GetCommand(ILidarScan<ILidar2DPoint> sickScan, ILidarScan<ILidar2DPoint> hokuyoScan, int angleTolerance, double threshold, double dMax, double vMax, double wMax, out List<Polygon> polys)
		{
			double hysteresis = threshold * .9;
			List<Double[]> histograms = new List<double[]>();

			// Transform scan to global coordinate and add to queue
			Vector2[] globalSickScan = TransformToGlobal(sickScan, sickDCM);
			sickHistory.Add(globalSickScan);
			histograms.Add(BuildHistogram(globalSickScan, dMax));
			histograms.Add(BuildHistogram(sickHistory[0], dMax));
			if (sickHistory.Count >= 30)
				sickHistory.RemoveAt(0);

			if (useHokuyo)
			{
				Vector2[] globalHokuyoScan = TransformToGlobal(hokuyoScan, hokuyoDCM);
				hokuyoHistory.Add(globalHokuyoScan);
				histograms.Add(BuildHistogram(globalHokuyoScan, dMax));
				histograms.Add(BuildHistogram(hokuyoHistory[0], dMax));

				if (hokuyoHistory.Count >= 30)
					hokuyoHistory.RemoveAt(0);
			}

			Double[] histogram = CombinedHistogram(histograms);

			for (int i = 0; i < numBuckets; i++)
			{
				if (rHistogram[i] >= threshold && histogram[i] > hysteresis && histogram[i] < threshold)
					histogram[i] = rHistogram[i];
			}

			rHistogram = histogram;

			histogram = Smooth(histogram, 10);

			double goalAngle = 180 * (goalPoint.ArcTan) / Math.PI;

			int targetBucket = (int)(Math.Round(goalAngle / angularResolution));

			List<double[]> valleys = FindValleys(histogram, threshold);

			polys = new List<Polygon>();

			if (!trapped)
			{
				// pick out best candidate valley
				double[] valley = PickBestValley(valleys, targetBucket);

				// calculate steering angle
				double steerAngle = FindSteerAngle(valley, targetBucket); //degrees

				// calculate speed commands
				double vmin = 0; // (m/s)

				//double vmax = .2; // (m/s)
				//double wmax = 25; // (deg/s)

				double w = 0;
				double v = 0;

				if (Math.Abs(steerAngle) > angleTolerance)
				{
					w = wMax * Math.Sign(steerAngle);
				}
				else
				{
					w = wMax * (steerAngle / angleTolerance);
				}

				List<Vector2> points = new List<Vector2>();

				for (int i = 0; i < numBuckets; i++)
				{
					points.Add(new Vector2(histogram[i] * Math.Cos(i * angularResolution * Math.PI / 180), histogram[i] * Math.Sin(i * angularResolution * Math.PI / 180)));
				}
				polys.Add(new Polygon(points));

				points = new List<Vector2>();
				points.Add(new Vector2(0, 0));
				points.Add(new Vector2(threshold * 1.5 * Math.Cos(steerAngle * Math.PI / 180), threshold * 1.5 * Math.Sin(steerAngle * Math.PI / 180)));
				polys.Add(new Polygon(points));

				points = new List<Vector2>();
				points.Add(new Vector2(0, 0));
				points.Add(new Vector2(threshold * 1 * Math.Cos(goalAngle * Math.PI / 180), threshold * 1 * Math.Sin(goalAngle * Math.PI / 180)));
				polys.Add(new Polygon(points));

				points = new List<Vector2>();
				for (int i = 0; i < 180; i++)
				{
					points.Add(new Vector2(threshold * Math.Cos(i * 2 * Math.PI / 180), threshold * Math.Sin(i * 2 * Math.PI / 180)));
				}
				polys.Add(new Polygon(points));

				double hc = Math.Min(histogram[0], threshold);
				double vp = vMax * (1 - hc / threshold);
				v = vp * (1 - Math.Abs(w) / wMax) + vmin;
				double vcommand = v * 3.6509; // multiply by 3.6509 to get "segway units"
				double wcommand = w * 3.9; // convert to deg/sec and multiply by 3.9 for "segway units" (counts/deg/sec)	

				return new RobotTwoWheelCommand(vcommand * 2.23693629, wcommand);
			}
			else // trapped (no valleys were found), so stop, or lift threshold
				return new RobotTwoWheelCommand(0, 0);
		}

		private double[] Smooth(double[] histo, int smoothRadius)
		{
			int n = histo.Length;
			double[] newHisto = new double[n];

			for (int i = 0; i < n; i++)
			{
				i += n;

				double sum = 0;

				for (int j = -smoothRadius; j <= smoothRadius; j++)
				{
					if (j >= 0)
						sum += (smoothRadius - j + 1) * histo[(i + j) % n];
					else
						sum += (smoothRadius + j + 1) * histo[(i + j) % n];
				}

				i -= n;
				newHisto[i] = sum / (smoothRadius * smoothRadius + 2 * smoothRadius + 1);
			}

			return newHisto;
		}

		private List<double[]> FindValleys(double[] h, double threshold)
		{
			List<double[]> valleys = new List<double[]>();
			int kstart = (int)(180 / angularResolution);
			while (h[kstart] > threshold && kstart < numBuckets + 1) { kstart = kstart + 1; }
			if (kstart == numBuckets) { trapped = true; return null; }
			int k1 = kstart;
			int k2 = kstart + 1;
			while (k2 % numBuckets != kstart)
			{
				if (h[k2 % numBuckets] < threshold) { k2 = k2 + 1; }
				else
				{
					if (h[k1 % numBuckets] < threshold)
					{
						valleys.Add(new double[2] { k1 - numBuckets, k2 - 1 - numBuckets });
						k1 = k2;
					}
					else { k2 = k2 + 1; k1 = k2; }
				}
			}
			if (h[k2 % numBuckets] < threshold && h[k1 % numBuckets] < threshold)
			{
				valleys.Add(new double[2] { k1 - numBuckets, k2 - 1 - numBuckets });
			}
			return valleys;
		}

		private double[] PickBestValley(List<double[]> valleys, int targetBucket)
		{
			int bestIdx = -1, bestEdge = int.MaxValue;

			for (int i = 0; i < valleys.Count; i++)
			{
				if (Math.Abs(valleys[i][0] - targetBucket) < bestEdge)
				{
					bestEdge = (int)Math.Abs(valleys[i][0] - targetBucket);
					bestIdx = i;
				}

				if (Math.Abs(valleys[i][1] - targetBucket) < bestEdge)
				{
					bestEdge = (int)Math.Abs(valleys[i][1] - targetBucket);
					bestIdx = i;
				}
			}

			return valleys[bestIdx];
		}

		private double FindSteerAngle(double[] valley, int targetBucket)
		{
			double knear = 0;
			double kfar = 0;
			if (Math.Abs(valley[0] - valley[1]) < kThreshold) // narrow valley
			{
				knear = valley[0];
				kfar = valley[1];
			}
			else /* wide valley */
			{
				if (targetBucket > valley[0] + kThreshold / 2 && targetBucket < valley[1] - kThreshold / 2) // very close to middle of valley
				{
					knear = kfar = targetBucket;
				}
				else // not within valley or too close to edge of valley
				{
					if (Math.Abs(valley[1] - targetBucket) > Math.Abs(targetBucket - valley[0])) // pick nearest k -> valley0 is closest
					{
						knear = valley[0];
						kfar = valley[0] + kThreshold;
					}
					else // valley1 is closest
					{
						knear = valley[1];
						kfar = valley[1] - kThreshold;
					}
				}
			}
			double ksteer = (knear + kfar) / 2;
			double steerAngle = ksteer * angularResolution;
			while (steerAngle > 180)
				steerAngle -= 360;
			while (steerAngle < -180)
				steerAngle += 360;
			return steerAngle;
		}

		public Vector2[] TransformToGlobal(ILidarScan<ILidar2DPoint> scan, Matrix4 sensorDcm)
		{
			Vector2[] globalScan = new Vector2[scan.Points.Count];
			Vector4 point;

			Matrix4 transformDcm = Matrix4.FromPose(pose) * sensorDcm;

			int n = scan.Points.Count;

			for (int i = 0; i < n; i++)
			{
				point = transformDcm * scan.Points[i].RThetaPoint.ToVector4();
				if (point.Z >= 0.05)
					globalScan[i] = new Vector2(point.X, point.Y);
			}

			return globalScan;
		}

		public double[] CombinedHistogram(List<double[]> H)
		{
			double[] combinedH = new double[numBuckets];
			double n = H.Count;

			for (int i = 0; i < numBuckets; i++)
			{
				combinedH[i] = 0;

				for (int j = 0; j < n; j++)
				{
					if (combinedH[i] < H[j][i])
						combinedH[i] = H[j][i];
				}
			}

			return combinedH;
		}

		public Double[] BuildHistogram(Vector2[] globalScan, double dMax)
		{
			Double[] histo = new Double[numBuckets];

			double a = 100;
			double b = a / (dMax * dMax);

			Double[] magnitudes = BuildMagnitudes(globalScan, a, b, dMax);
			Double[] betas = BuildBetas(globalScan);

			int bucket;

			int n = magnitudes.Length;

			for (int i = 0; i < n; i++)
			{
				bucket = (int)(betas[i] / angularResolution);
				histo[bucket] += magnitudes[i];
			}

			return histo;
		}
		
		public Double[] BuildMagnitudes(Vector2[] globalScan, double a, double b, double dMax)
		{
			double distSquared;

			Double[] magnitudes = new Double[globalScan.Length];
			Double[] ranges = new Double[globalScan.Length];

			int n = globalScan.Length;

			for (int i = 0; i < n; i++)
			{
				if (globalScan[i] != null)
				{
					distSquared = (globalScan[i].X - pose.x) * (globalScan[i].X - pose.x) + (globalScan[i].Y - pose.y) * (globalScan[i].Y - pose.y);
					if (distSquared < dMax * dMax)
						magnitudes[i] = (a - distSquared * b);
					else
						magnitudes[i] = 0.0;
				}
			}
			return magnitudes;
		}

		public Double[] BuildBetas(Vector2[] globalScan)
		{
			Double[] betas = new Double[globalScan.Length];
            Double[] angles = new Double[globalScan.Length];
			double angle;

			int n = globalScan.Length;

			for (int i = 0; i < n; i++)
			{
				angle = (Math.Atan2(globalScan[i].Y - pose.y, globalScan[i].X - pose.x) - pose.yaw) * 180/Math.PI;
                angles[i] = angle;

                while (angle < 0)
                    angle += 360;
                while (angle >= 360)
                    angle -= 360;				

				betas[i] = angle;
			}

			return betas;
		}
	}
}
