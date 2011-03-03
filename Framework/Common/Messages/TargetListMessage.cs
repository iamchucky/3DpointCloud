using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using System.Drawing;
using Magic.Common.Mapack;

namespace Magic.Common.Messages
{
	[Serializable]
	public class TargetListMessage : IRobotMessage
	{
		int robotID;
		RobotImage image;
		double timeStamp;
		int targetID;
		Vector2 targetPose;
		TargetTypes type;
		Matrix cov;

		public RobotImage Image { get { return image; } }
		public double TimeStamp { get { return timeStamp; } }
		public int TargetID { get { return targetID; } }
		public Vector2 TargetPose { get { return targetPose; } }
		public TargetTypes TargetType { get { return type; } }
		public Matrix TargetCov { get { return cov; } }

		public TargetListMessage(int robotID, RobotImage image, double timeStamp, int targetID, TargetTypes type)
		{
			this.robotID = robotID;
			this.image = image;
			this.timeStamp = timeStamp;
			this.targetID = targetID;
			this.type = type;
		}
		public TargetListMessage(int robotID, RobotImage image, double timeStamp, int targetID, Vector2 targetPose, Matrix targetCov, TargetTypes type)
		{
			this.robotID = robotID;
			this.image = image;
			this.timeStamp = timeStamp;
			this.targetID = targetID;
			this.targetPose = targetPose;
			this.type = type;
			this.cov = targetCov;
		}
		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
