using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class ClearTargetListMessage
	{
		int robotID; public int RobotID { get { return robotID; } }

		public ClearTargetListMessage(int robotID)
		{
			this.robotID = robotID;
		}

		public ClearTargetListMessage()
		{
			this.robotID = 0;
		}
	}
}
