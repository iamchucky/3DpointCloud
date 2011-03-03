using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class TargetQueryMessage : IRobotMessage
	{
		int robotID;


		public TargetQueryMessage(int robotID)
		{
			this.robotID = robotID;
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
