using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;

namespace Magic.Common.Messages
{
	[Serializable]
	public class LidarScanMessage : IRobotMessage
	{
		int robotID;
		ILidarScan<ILidar2DPoint> scan;

		public ILidarScan<ILidar2DPoint> Scan
		{
			get { return scan; }			
		}

		public LidarScanMessage(int robotID, ILidarScan<ILidar2DPoint> scan)
		{
			this.scan = scan;
			this.robotID = robotID;
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
