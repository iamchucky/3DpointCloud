using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;

namespace Magic.Common.Messages
{
	[Serializable]
	public class LidarPoseTargetMessage
	{
		int targetID;
		int robotID;
		RobotPose detectionPose;
		ILidarScan<ILidar2DPoint> scan;

		public int TargetID { get { return targetID; } }
		public int RobotID { get { return robotID; } }
		public RobotPose DetectionPose { get { return detectionPose; } }
		public ILidarScan<ILidar2DPoint> LidarScan { get { return scan; } }

		public LidarPoseTargetMessage(int robotID, int targetID, RobotPose detectionPose, ILidarScan<ILidar2DPoint> scan)
		{
			this.robotID = robotID;
			this.targetID = targetID;
			this.detectionPose = detectionPose;
			this.scan = scan;
		}

	}
}
