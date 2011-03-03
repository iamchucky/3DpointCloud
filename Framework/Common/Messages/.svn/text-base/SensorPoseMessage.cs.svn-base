using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class SensorPoseMessage : IRobotMessage
	{
		public SensorPoseMessage(int robotID, SensorPose pose) { this.robotID = robotID; this.pose = pose; }
		int robotID;

		SensorPose pose;

		public SensorPose Pose
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
