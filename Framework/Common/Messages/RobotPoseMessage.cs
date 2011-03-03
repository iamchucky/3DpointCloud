using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class RobotPoseMessage : IRobotMessage	
	{
		public RobotPoseMessage(int robotID, RobotPose pose) { this.robotID = robotID; this.pose = pose; }
		int robotID;

		RobotPose pose;

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
