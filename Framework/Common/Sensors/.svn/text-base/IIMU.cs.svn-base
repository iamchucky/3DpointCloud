using System;
using Magic.Common;
using System.Collections.Generic;
using System.Text;
using Magic.Common.DataInterfaces;

namespace Magic.Common.Sensors
{
	public interface IIMU : ISensor
	{
		event EventHandler<TimestampedEventArgs<IMUData>> IMUUpdate;
		void Reset();
	}

	[Serializable]
	public class IMUData : ILoggable, ITimestampedEventQueueItem
	{
		public IMUData()
		{
			this.xRate = 0;
			this.yRate = 0;
			this.zRate = 0;
			this.xAccel = 0;
			this.yAccel = 0;
			this.zAccel = 0;
			this.timer = 0;
		}
		public IMUData(double wx, double wy, double wz, double ax, double ay, double az, uint timer)
		{
			this.xRate = wx;
			this.yRate = wy;
			this.zRate = wz;
			this.xAccel = ax;
			this.yAccel = ay;
			this.zAccel = az;
			this.timer = timer;
		}

		public double xRate;
		public double yRate;
		public double zRate;
		public double xAccel;
		public double yAccel;
		public double zAccel;
		public uint timer;

		override public string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("xRate: ");
			sb.Append(xRate); sb.Append("\n");
			sb.Append("yRate: ");
			sb.Append(yRate); sb.Append("\n");
			sb.Append("zRate: ");
			sb.Append(zRate); sb.Append("\n");
			sb.Append("xAccel: ");
			sb.Append(xAccel); sb.Append("\n");
			sb.Append("yAccel: ");
			sb.Append(yAccel); sb.Append("\n");
			sb.Append("zAccel: ");
			sb.Append(zAccel); sb.Append("\n");
			sb.Append("Timer: ");
			sb.Append(timer);
			return sb.ToString();
		}


		#region ILoggable Members

		public string ToLog()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(xRate); sb.Append("\t");
			sb.Append(yRate); sb.Append("\t");
			sb.Append(zRate); sb.Append("\t");
			sb.Append(xAccel); sb.Append("\t");
			sb.Append(yAccel); sb.Append("\t");
			sb.Append(zAccel);
			return sb.ToString();
		}

		#endregion

		#region ITimestampedEventQueueItem Members

		string dataType;
		public string DataType
		{
			get { return dataType; }
			set { dataType = value; }
		}

		#endregion

		#region ITimeComparable Members

		double timestamp;
		public double TimeStamp
		{
			get { return this.timestamp; }
			set { this.timestamp = value; }
		}

		public int CompareTo(ITimeComparable obj)
		{
			if (this.timestamp < obj.TimeStamp) return -1;
			if (this.timestamp > obj.TimeStamp) return 1;
			return 0;
		}

		#endregion
	}

}