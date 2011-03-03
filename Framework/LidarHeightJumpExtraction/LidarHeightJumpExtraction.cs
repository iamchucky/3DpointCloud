using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Magic.Common;
using Magic.Common.Sensors;
using Magic.Common.Mapack;
using Magic.OccupancyGrid;

namespace Magic.LidarHeightJumpExtraction
{
	public class LidarHeightJumpExtraction
	{
		public static List<Vector2> ExtractGlobalFeature(ILidarScan<ILidar2DPoint> scan, SensorPose sensorPose, RobotPose currentPose, double wheelRadius)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			List<Vector2> features = new List<Vector2>();
			List<ILidar2DPoint> filteredScan = new List<ILidar2DPoint>();
			//foreach (ILidar2DPoint pt in scan.Points)
			for (int i = 0; i < scan.Points.Count; i++)
			{
				if (scan.Points[i].RThetaPoint.R > 0.20)
					filteredScan.Add(scan.Points[i]);
			}

			Matrix4 laser2robotDCM = new Matrix4(1, 0, 0, 0,
												 0, Math.Cos(sensorPose.pitch), Math.Sin(sensorPose.pitch), 0,
												 0, -Math.Sin(sensorPose.pitch), Math.Cos(sensorPose.pitch), sensorPose.z,
												 0, 0, 0, 1);

			int neighbor = 3;
			for (int i = neighbor; i < filteredScan.Count - neighbor; i++)
			{
				// AGAIN, these codes are written respect to ENU coordinate frame
				double currentX = -filteredScan[i].RThetaPoint.R * Math.Sin(filteredScan[i].RThetaPoint.theta) - sensorPose.y;
				double currentY = filteredScan[i].RThetaPoint.R * Math.Cos(filteredScan[i].RThetaPoint.theta) + sensorPose.x;
				double lastX = -filteredScan[i - neighbor].RThetaPoint.R * Math.Sin(filteredScan[i - neighbor].RThetaPoint.theta) - sensorPose.y;
				double lastY = filteredScan[i - neighbor].RThetaPoint.R * Math.Cos(filteredScan[i - neighbor].RThetaPoint.theta) + sensorPose.x;
				double nextX = -filteredScan[i + neighbor].RThetaPoint.R * Math.Sin(filteredScan[i + neighbor].RThetaPoint.theta) - sensorPose.y;
				double nextY = filteredScan[i + neighbor].RThetaPoint.R * Math.Cos(filteredScan[i + neighbor].RThetaPoint.theta) + sensorPose.x;

				Matrix localPt = new Matrix(4, 1), lastLocalPt = new Matrix(4, 1), nextLocalPt = new Matrix(4, 1);
				localPt[0, 0] = currentX; localPt[1, 0] = currentY; localPt[3, 0] = 1;
				lastLocalPt[0, 0] = lastX; lastLocalPt[1, 0] = lastY; lastLocalPt[3, 0] = 1;
				nextLocalPt[0, 0] = nextX; nextLocalPt[1, 0] = nextY; nextLocalPt[3, 0] = 1;
				Vector4 currentPt = laser2robotDCM * localPt;
				Vector4 lastPt = laser2robotDCM * lastLocalPt;
				Vector4 nextPt = laser2robotDCM * nextLocalPt;

				// check if the z position of the laser return is higher than 10 cm of current z-state
				//if (currentPt.Z > currentPose.z + 0.1)
				//    features.Add(new Vector2(currentPt.X, currentPt.Y));

				// height comparison
				Vector2 neighborPt = FindNeighborToCompare(filteredScan, sensorPose, i, 0.1);
				Matrix neighbotPtM = new Matrix(4, 1);
				neighbotPtM[0, 0] = neighborPt.X; neighbotPtM[1, 0] = neighborPt.Y; neighbotPtM[3, 0] = 1;
				Vector4 neighborPtV = laser2robotDCM * neighbotPtM;

				double yaw = currentPose.yaw - Math.PI / 2;
				double heightDiff = neighborPtV.Z - currentPt.Z;
				if (heightDiff > 0.1)
				{
					features.Add(new Vector2(neighborPtV.X * Math.Cos(yaw) - neighborPtV.Y * Math.Sin(yaw) + currentPose.x,
											neighborPtV.X * Math.Sin(yaw) + neighborPtV.Y * Math.Cos(yaw) + currentPose.y));
				}
				else if (heightDiff < -0.10)
				{
					features.Add(new Vector2(currentPt.X * Math.Cos(yaw) - currentPt.Y * Math.Sin(yaw) + currentPose.x,
											currentPt.X * Math.Sin(yaw) + currentPt.Y * Math.Cos(yaw) + currentPose.y));
				}

			}
			Console.WriteLine("Jump(?) Extraction Time:" + sw.ElapsedMilliseconds);
			return features;
		}

		private static Vector2 FindNeighborToCompare(List<ILidar2DPoint> filteredScan, SensorPose lidarPose, int initialIdx, double threshold)
		{
			double currentX = -filteredScan[initialIdx].RThetaPoint.R * Math.Sin(filteredScan[initialIdx].RThetaPoint.theta) - lidarPose.y;
			double currentY = filteredScan[initialIdx].RThetaPoint.R * Math.Cos(filteredScan[initialIdx].RThetaPoint.theta) + lidarPose.x;
			double neighborX = -filteredScan[filteredScan.Count - 1].RThetaPoint.R * Math.Sin(filteredScan[filteredScan.Count - 1].RThetaPoint.theta) - lidarPose.y;
			double neighborY = filteredScan[filteredScan.Count - 1].RThetaPoint.R * Math.Cos(filteredScan[filteredScan.Count - 1].RThetaPoint.theta) + lidarPose.x;
			Vector2 currentPt = new Vector2(currentX, currentY);
			for (int k = initialIdx; k < filteredScan.Count; k++)
			{
				neighborX = -filteredScan[k].RThetaPoint.R * Math.Sin(filteredScan[k].RThetaPoint.theta) - lidarPose.y;
				neighborY = filteredScan[k].RThetaPoint.R * Math.Cos(filteredScan[k].RThetaPoint.theta) + lidarPose.x;
				Vector2 neighborPt = new Vector2(neighborX, neighborY);
				if (currentPt.DistanceTo(neighborPt) > threshold)
					return neighborPt;
			}
			return new Vector2(neighborX, neighborY);
		}

	}
}
