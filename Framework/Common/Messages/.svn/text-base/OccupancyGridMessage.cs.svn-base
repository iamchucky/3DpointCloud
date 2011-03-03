using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataTypes;

namespace Magic.Common.Messages
{
	[Serializable]
	public class OccupancyGrid2DMessage : IRobotMessage
	{
		int robotID;
		IOccupancyGrid2D grid;

		public OccupancyGrid2DMessage(int robotID, IOccupancyGrid2D occupancyGrid2D)
		{
			this.robotID = robotID;
			this.grid = occupancyGrid2D;
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		public IOccupancyGrid2D Grid
		{
			get { return grid; }
		}

		#endregion
	}
}
