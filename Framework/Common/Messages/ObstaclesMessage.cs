using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;

namespace Magic.Common.Messages
{
	[Serializable]
	public class ObstaclesMessage : IRobotMessage 
	{
		int robotID;
		List<Polygon> obstacles;
		double timestamp;

		public double Timestamp
		{
			get { return timestamp; }			
		}

		public List<Polygon> Obstacles
		{
			get { return obstacles; }			
		}
		public ObstaclesMessage(int robotID, List<Polygon> obstacles, double timestamp)
		{
			this.robotID = robotID;
			this.obstacles = obstacles;
			this.timestamp = timestamp;
		}
		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
