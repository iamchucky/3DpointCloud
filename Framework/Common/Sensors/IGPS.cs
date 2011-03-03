using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Magic.Common.Mapack;
using Magic.Common.DataInterfaces;

namespace Magic.Common.Sensors
{
	public enum GPSFixType
	{
		NotAvailable, //no gps signal
		Autonomous,		//normal gps case
		Differential  //using differential correction
	}

	public enum GPSCoordinateFrame
	{
		ENU, ECEF, UTM
	}
	public interface IGPS : ISensor
	{
		event EventHandler<TimestampedEventArgs<GPSPositionData>> PositionMeasurementReceived;
		event EventHandler<TimestampedEventArgs<GPSVelocityData>> VelocityMeasurementReceived;
		event EventHandler<TimestampedEventArgs<GPSErrorData>> ErrorMeasurementReceived;
	}

	/// <summary>
	/// GPS data struct.
	/// </summary>
	public class GPSPositionData : ILoggable, ITimestampedEventQueueItem
	{
		/// <summary>
		/// Unified  Timestamp of measurement
		/// </summary>
		public double timestamp;
		/// <summary>
		/// GPS Time of the time of fix
		/// </summary>
		public double timeOfFix;
		/// <summary>
		/// Best gues of our current position
		/// </summary>
		public LLACoord position;

		string dataType;


		#region ILoggable Members

		public string ToLog()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(timestamp); sb.Append("\t");
			sb.Append(timeOfFix); sb.Append("\t");
			sb.Append(position.lat); sb.Append("\t");
			sb.Append(position.lon); sb.Append("\t");
			sb.Append(position.alt);
			return sb.ToString();
		}
		#endregion

		#region ITimeComparable Members

		public int CompareTo(ITimeComparable obj)
		{
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


	/// <summary>
	/// GPS Errror Struct
	/// </summary>
	public struct GPSErrorData : ILoggable, ITimeComparable
	{
		/// <summary>
		/// Unified  Timestamp of measurement
		/// </summary>
		public double timestamp;
		public int numSatellites;
		public GPSFixType fixType;
		/// <summary>
		/// Horizonntal Diluotiion of Precision
		/// </summary>
		public double HDOP;
		/// <summary>
		/// Vertical Dilution of Precision
		/// </summary>
		public double VDOP;
		/// <summary>
		/// Position Diliution of Prevcision
		/// </summary>
		public double PDOP;

		#region ILoggable Members

		public string ToLog()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(timestamp); sb.Append("\t");
			sb.Append(numSatellites); sb.Append("\t");
			sb.Append(fixType); sb.Append("\t");
			sb.Append(HDOP); sb.Append("\t");
			sb.Append(VDOP); sb.Append("\t");
			sb.Append(PDOP);
			return sb.ToString();
		}

		#endregion

		#region ITimeComparable Members

		public int CompareTo(ITimeComparable obj)
		{
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
	}

	/// <summary>
	/// This is the covariance matrix of the position solution, which for gps is LLA+T
	/// It is in general 4x4 matrix that is symmetric. It is in units of meters^2
	/// </summary>
	public struct GPSPositionCovariance : ILoggable
	{
		Matrix posCovariance;
		#region ILoggable Members

		string ILoggable.ToLog()
		{
			return posCovariance.ToLog();
		}

		#endregion
	}
	public struct GPSVelocityData : ILoggable, ITimeComparable
	{
		/// <summary>
		/// Unified  Timestamp of measurement
		/// </summary>
		public double timestamp;
		public double GPSTime;
		public double eastVelocity;
		public double northVelocity;
		public double upVelocity;
		public Vector3 ToVector3()
		{
			return new Vector3(eastVelocity, northVelocity, upVelocity);
		}
		#region ILoggable Members

		public string ToLog()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(timestamp); sb.Append("\t");
			sb.Append(eastVelocity); sb.Append("\t");
			sb.Append(northVelocity); sb.Append("\t");
			sb.Append(upVelocity);
			return sb.ToString();
		}

		#endregion

		#region ITimeComparable Members

		public int CompareTo(ITimeComparable obj)
		{
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
	}
}
