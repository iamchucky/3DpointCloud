using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;

namespace Magic.Common.Messages
{
	[Serializable]
	public class LidarPosePackageMessage : IRobotMessage
	{
		public LidarPosePackageMessage(int robotID, RobotPose pose, SensorPose sensorPose, ILidarScan<ILidar2DPoint> scan) 
		{ 
			this.robotID = robotID; 
			this.pose = pose;
			this.lidarScan = scan;
			this.sensorPose = sensorPose;
		}
		int robotID;

		RobotPose pose;
		ILidarScan<ILidar2DPoint> lidarScan;
		SensorPose sensorPose;

		public SensorPose SensorPose
		{
			get { return sensorPose; }
			set { sensorPose = value; }
		}

		public ILidarScan<ILidar2DPoint> LidarScan
		{
			get { return lidarScan; }
			set { lidarScan = value; }
		}

		public RobotPose Pose
		{
			get { return pose; }
			set { pose = value; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
