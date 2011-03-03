using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class ConfirmOOIMessage : IRobotMessage
	{
		int targetID;
		TargetTypes targetType;
		int robotID;

		public ConfirmOOIMessage(int targetId, TargetTypes targetType, int robotId)
		{
			this.targetID = targetId;
			this.targetType = targetType;
			robotID = robotId;
		}

		public int TargetId
		{
			get { return targetID; }
		}

		public TargetTypes TargetType
		{
			get { return targetType; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}

	public enum TargetTypes
	{
		PotentialMOOI,
		PotentialSOOI,
		ConfirmedMOOI,
		ConfirmedSOOI,
		Meta,
		Junk,
		Unconfirmed
	}
}
