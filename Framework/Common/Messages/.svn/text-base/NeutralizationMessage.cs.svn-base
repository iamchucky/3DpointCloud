using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class NeutralizationMessage : IRobotMessage
	{
		private int robotID;
		private int targetID;

		public NeutralizationMessage(int robotID, int targetID)
		{
			this.robotID = robotID;
			this.targetID = targetID;
		}

		public int TargetID
		{
			get { return targetID; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
