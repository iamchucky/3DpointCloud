using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Path;

namespace Magic.Common.Messages
{
	[Serializable]
	public class PlannerStatusMessage : IRobotMessage
	{
		int robotID;
		bool plannerWorking;

		public PlannerStatusMessage(int robotID, bool plannerStatus)
		{
			this.robotID = robotID;
			this.plannerWorking = plannerStatus;
		}

        public bool PlannerWorking
        {
            get { return plannerWorking; }
        }

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}