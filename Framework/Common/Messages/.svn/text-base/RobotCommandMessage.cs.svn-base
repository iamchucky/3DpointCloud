using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Robots;

namespace Magic.Common.Messages
{	
	[Serializable]
	public class RobotCommandMessage : IRobotMessage
	{
		public enum RobotCommandType { Waypoint, Direct, SetMode }

		private int robotID;

		public RobotCommandType Type { get; set; }
		public List<Vector2> Waypoints { get; set; } // Set to null if not used.
		public RobotTwoWheelCommand DirectCmd { get; set; } // Set to null if not used.
		public double AngleWhenDone { get; set; } // Set to NaN if not used.
		public RobotState.RobotMode Mode { get; set; } // Set to NA if not used.

		public RobotCommandMessage(int robotID, RobotCommandType type, List<Vector2> waypoints)
		{
			this.robotID = robotID;
			Type = type;
			Waypoints = waypoints;
			AngleWhenDone = Double.NaN;
		}

		public RobotCommandMessage(int robotID, RobotCommandType type, List<Vector2> waypoints, double angleWhenDone)
		{
			this.robotID = robotID;
			Type = type;
			Waypoints = waypoints;
			AngleWhenDone = angleWhenDone;
		}

		public RobotCommandMessage(int robotID, RobotCommandType type, RobotTwoWheelCommand cmd)
		{
			this.robotID = robotID;
			Type = type;
			DirectCmd = cmd;
		}

		public RobotCommandMessage(int robotID, RobotCommandType type, RobotState.RobotMode mode)
		{
			this.robotID = robotID;
			Type = type;
			Mode = mode;
		}

		public RobotCommandMessage(int robotID, RobotCommandType type, List<Vector2> waypoints, RobotTwoWheelCommand directCmd,
			double angleWhenDone, RobotState.RobotMode mode)
		{
			this.robotID = robotID;
			Type = type;
			Waypoints = waypoints;
			DirectCmd = directCmd;
			AngleWhenDone = angleWhenDone;
			Mode = mode;
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
