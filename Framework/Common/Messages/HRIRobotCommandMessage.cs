using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Robots;

namespace Magic.Common.Messages
{
	[Serializable]
	public class HRIRobotCommandMessage : IRobotMessage
	{
		public enum Sender { CameraForm, MapForm, None};
		public enum TeleopMode { Start, Finish, None, CloseForm};
		public Sender Controller { get; set; }
		public TeleopMode Mode { get; set; }
		public int RobotID { get; set; }

		public HRIRobotCommandMessage()
		{
			RobotID = 0;
			Controller = Sender.None;
			Mode = TeleopMode.None;
		}

		public HRIRobotCommandMessage(int robotID, TeleopMode mode, Sender sender)
		{
			RobotID = robotID;
			Mode = mode;
			Controller = sender;
		}
	}
}