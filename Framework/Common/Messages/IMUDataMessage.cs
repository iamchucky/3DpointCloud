using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;

namespace Magic.Common.Messages
{
    [Serializable]
	public class IMUDataMessage : IRobotMessage	
	{
        public IMUDataMessage(int robotID, IMUData data) { this.robotID = robotID; this.data = data; }
		int robotID;

		IMUData data;

		public IMUData Data
		{
			get { return data; }
			set { data = value; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
