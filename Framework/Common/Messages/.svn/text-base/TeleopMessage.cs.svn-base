using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Robots;

namespace Magic.Common.Messages
{
	[Serializable]
	public class TeleopMessage : IRobotMessage	
	{
		int robotID;
		RobotTwoWheelCommand cmd;

		public TeleopMessage(int robotID, RobotTwoWheelCommand cmd) { this.robotID = robotID; this.cmd = cmd; }

		public RobotTwoWheelCommand Command
		{
			set { cmd = value; }
			get { return cmd; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}



		#endregion
	}
}