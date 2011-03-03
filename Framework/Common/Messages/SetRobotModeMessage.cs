using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class SetRobotModeMessage : IRobotMessage
	{
		int robotID;
		public RobotState.RobotMode Mode { get; set; }

		public SetRobotModeMessage(int robotID, RobotState.RobotMode mode)
		{
			this.robotID = robotID;
			Mode = mode;
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
