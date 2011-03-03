using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Sensors;
using Magic.Common.Mapack;

namespace Magic.Sensor.LidarTargetDetector
{
	public class LidarTargetDetector
	{
		double thresholdHeight; // above 5 cm
		Matrix4 laserToRobot;

		public LidarTargetDetector(double thresholdHeight, SensorPose laserPose, bool forCamera)
		{
			this.thresholdHeight = thresholdHeight;
			laserToRobot = Matrix4.FromPose(laserPose);
			if (forCamera)
				laserToRobot[2, 3] = 0;
			//laserToRobot[0, 2] = laserPose.x;
			//laserToRobot[1, 2] = laserPose.y;
			//laserToRobot[2, 2] = laserPose.z;
			//laserToRobot[3, 3] = 1.0;
		}

		public void SetSensorPose(SensorPose laserPose, bool forCamera)
		{
			laserToRobot = Matrix4.FromPose(laserPose);
			if (forCamera)
				laserToRobot[2, 3] = 0;
			//laserToRobot[0, 2] = laserPose.x;
			//laserToRobot[1, 2] = laserPose.y;
			//laserToRobot[2, 2] = 0;
			//laserToRobot[3, 3] = 1.0;
		}


		/// <summary>
		/// Find targets above grounds and return in global and robot coordinate
		/// </summary>
		/// <param name="robotPose"></param>
		/// <param name="laserScan"></param>
		/// <param name="localPoints"></param>
		/// <param name="globalPoints"></param>
		public void DetectTarget(RobotPose robotPose, ILidarScan<ILidar2DPoint> laserScan, out List<Vector3> laserPoints, out List<Vector3> localPoints, bool lidarUpsideDown)
		{
			// initialization
			localPoints = new List<Vector3>();
			laserPoints = new List<Vector3>();
			for (int i = 0; i < laserScan.Points.Count; i++)
			{
				Vector4 localPt = laserToRobot * laserScan.Points[i].RThetaPoint.ToVector4();
				Vector4 newLocalPt;

				if (lidarUpsideDown)
					newLocalPt = new Vector4(-localPt.Y, localPt.X, localPt.Z, localPt.W);
				else
					newLocalPt = new Vector4(localPt.Y, localPt.X, localPt.Z, localPt.W);

				localPoints.Add(new Vector3(newLocalPt.X, newLocalPt.Y, newLocalPt.Z));
				if (lidarUpsideDown)
					laserPoints.Add(new Vector3(laserScan.Points[i].RThetaPoint.R, laserScan.Points[i].RThetaPoint.theta, laserScan.Points[i].RThetaPoint.thetaDeg));
				else
					laserPoints.Add(new Vector3(laserScan.Points[laserScan.Points.Count - i - 1].RThetaPoint.R,
																   laserScan.Points[laserScan.Points.Count - i - 1].RThetaPoint.theta,
																   laserScan.Points[laserScan.Points.Count - i - 1].RThetaPoint.thetaDeg));
			}
		}
	}
}