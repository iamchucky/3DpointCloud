using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common.DataInterfaces;

namespace Magic.SickInterface
{

	[Serializable]
	public struct SickScan : ILidarScan<ILidar2DPoint>, ITimestampedEventQueueItem
	{
		public int packetNum;   // packet counter	  
		public double timestamp;
		public int scannerID;   // Our internal ID of the source scanner
		public List<ILidar2DPoint> points;
		public string ToLog()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(timestamp.ToString() + "\t" + scannerID.ToString() + "\t" + points.Count + "\t");

			foreach (ILidar2DPoint pt in points)
			{
				sb.Append(pt.RThetaPoint.R + "\t" + pt.RThetaPoint.theta + "\t");
			}
			return sb.ToString();
		}
		#region ILidarScan Members

		public int ScannerID
		{
			get { return scannerID; }
		}

		public double Timestamp
		{
			get { return timestamp; }
			set { timestamp = value; }
		}

		public List<ILidar2DPoint> Points
		{
			get { return points; }
		}


		#endregion

		#region ITimestampedEventQueueItem Members

		public string DataType
		{
			get { return "LidarScan"; }
		}

		#endregion

		#region ITimeComparable Members

		public double TimeStamp
		{
			get { return this.timestamp; }
			set { this.timestamp = value; }
		}

		public int CompareTo(ITimeComparable obj)
		{
			if (obj == null) { return -1; }
			if (this.timestamp < obj.TimeStamp) return -1;
			if (this.timestamp > obj.TimeStamp) return 1;
			return 0;
		}

		#endregion
	}

}
