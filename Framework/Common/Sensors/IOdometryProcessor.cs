using System;
using Magic.Common;
using System.Collections.Generic;
using System.Text;
using Magic.Common.DataInterfaces;

namespace Magic.Common.Sensors
{
	public interface IOdometryProcessor
	{
		RobotPose CurrentState { get; }
		event EventHandler<TimestampedEventArgs<RobotPose>> OdometryUpdate;
		event EventHandler<TimestampedEventArgs<OdometryData>> EncoderUpdate;
		void Reset();
	}

	[Serializable]
	public class OdometryData : ILoggable, ITimestampedEventQueueItem
	{
		public OdometryData(double l, double r, double ls, double rs)
		{
			this.wheelLCounts = l; this.wheelRCounts = r; this.wheelLSpeed = ls;
			this.wheelRSpeed = rs; this.timestamp = 0.0; this.dataType = "odometry";
			this.integratedLWheel = 0; this.integratedRWheel = 0;
		}
		public OdometryData(double l, double r, double ls, double rs, double deltaT)
		{
			this.wheelLCounts = l; this.wheelRCounts = r; this.wheelLSpeed = ls;
			this.wheelRSpeed = rs; this.timestamp = 0.0; this.dataType = "odometry";
			this.integratedLWheel = 0; this.integratedRWheel = 0; this.deltaT = deltaT;
		}
		public OdometryData(double il, double ir, double l, double r, double ls, double rs, double deltaT)
		{
			this.wheelLCounts = l; this.wheelRCounts = r; this.wheelLSpeed = ls;
			this.wheelRSpeed = rs; this.timestamp = 0.0; this.dataType = "odometry";
			this.integratedLWheel = il; this.integratedRWheel = ir; this.deltaT = deltaT;
		}


		public double integratedLWheel;
		public double integratedRWheel;
		public double wheelLCounts;
		public double wheelRCounts;
		public double wheelLSpeed;
		public double wheelRSpeed;
		public double deltaT;
		public double timestamp;
		public string dataType;



		#region ILoggable Members

		public string ToLog()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(wheelLCounts); sb.Append("\t");
			sb.Append(wheelRCounts); sb.Append("\t");
			sb.Append(wheelLSpeed); sb.Append("\t");
			sb.Append(wheelRSpeed);
			return sb.ToString();
		}

		#endregion

		#region ITimeComparable Members

		public int CompareTo(ITimeComparable obj)
		{
			if (obj == null) { return -1; }
			if (this.timestamp < obj.TimeStamp) return -1;
			if (this.timestamp > obj.TimeStamp) return 1;
			return 0;
		}

		public double TimeStamp
		{
			get { return this.timestamp; }
			set { this.timestamp = value; }
		}

		#endregion

		#region ITimestampedEventQueueItem Members

		public string DataType
		{
			get { return dataType; }
			set { dataType = value; }
		}

		#endregion
	}
}
