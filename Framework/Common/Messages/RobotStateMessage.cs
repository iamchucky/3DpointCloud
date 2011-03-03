using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class RobotStateMessage : IRobotMessage
	{
		int robotID;
		public RobotState State { get; set; }

		public RobotStateMessage(int robotID, RobotState state)
		{
			this.robotID = robotID;
			State = state;
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
