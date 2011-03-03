using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{

	[Serializable]
	public class EstopMessage : IRobotMessage
	{
		public EstopMessage(int robotID, EstopType type) { this.robotID = robotID; this.type = type; }
		int robotID;
		EstopType type;

		public EstopType Type
		{
			get { return type; }
			set { type = value; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}

	public enum EstopType
	{
		ESTOP,
		FREEZE,
		UNFREEZE
	}
}


