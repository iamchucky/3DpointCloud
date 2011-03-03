using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
	[Serializable]
	public class NeutralizeWithLaserMessage : IRobotMessage
	{
		private int robotID;
		private int targetID;
        private NeutralizeCommand cmd;

        public enum NeutralizeCommand { Start, Abort, SuccessfulFinish };

		public NeutralizeWithLaserMessage(int robotID, int targetID, NeutralizeCommand cmd)
		{
			this.robotID = robotID;
			this.targetID = targetID;
            this.cmd = cmd;
		}

		public int TargetID
		{
			get { return targetID; }
		}

        public NeutralizeCommand Command
        {
            get { return cmd; }
            set { cmd = value; }
        }

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
