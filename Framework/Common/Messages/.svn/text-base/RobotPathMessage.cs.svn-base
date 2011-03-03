using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Path;

namespace Magic.Common.Messages
{
	[Serializable]
	public class RobotPathMessage : IRobotMessage
	{
		int robotID;
		IPath path;
		PointOnPath goalPointOnPath;
		PointOnPath robotPointOnPath;

		public PointOnPath GoalPointOnPath
		{
			get { return goalPointOnPath; }			
		}

		public PointOnPath RobotPointOnPath
		{
			get { return robotPointOnPath; }			
		}

		public RobotPathMessage(int robotID, IPath path)
		{
			this.path = path;
			this.robotID = robotID;
			if (path.Count > 0)
			{
				this.goalPointOnPath = path[0].StartPoint;
				this.robotPointOnPath = path[0].StartPoint;
			}
		}

		public RobotPathMessage(int robotID, IPath path, PointOnPath goalPointOnPath, PointOnPath robotPointOnPath) : this(robotID,path)
		{
			this.goalPointOnPath = goalPointOnPath;
			this.robotPointOnPath = robotPointOnPath;
		}

		public IPath Path
		{
			get { return path; }			
		}


		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
