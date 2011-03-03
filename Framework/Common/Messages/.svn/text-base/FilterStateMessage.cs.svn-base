using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class FilterStateMessage : IRobotMessage
	{
		PoseFilterState state;
		int robotID;
		public FilterStateMessage(int robotID, PoseFilterState state) { this.robotID = robotID; this.state = state; }

		public PoseFilterState State
		{
			get { return state; }
			set { state = value; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
