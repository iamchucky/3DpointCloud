using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;
using Magic.Common.DataInterfaces;

namespace Magic.Common
{
	/// <summary>
	/// This represents the full position and orientation of a robot. 		
	/// </summary>
	[Serializable]
	public class RobotPose : Pose, ILoggable, ITimestampedEventQueueItem, ITimeComparable
	{
		public Matrix covariance = new Matrix(6, 6);

		public RobotPose()
		{
			Zero();
		}

		public RobotPose(RobotPose toCopy)
			: base(toCopy)
		{
			this.covariance = new Matrix(toCopy.covariance.Array);
		}

		public RobotPose(double x, double y, double z, double yaw, double pitch, double roll, double timestamp)
			: base(x, y, z, yaw, pitch, roll, timestamp)
		{ }

		public static RobotPose Invalid = new RobotPose(-9999, -9999, -9999, -9999, -9999, -9999, 0);

		public RobotPose DeepCopy()
		{
			return new RobotPose(x, y, z, yaw, pitch, roll, timestamp);
		}

		public static RobotPose operator +(RobotPose left, RobotPose right)
		{
			RobotPose toReturn = new RobotPose();
			toReturn.x = left.x + right.x;
			toReturn.y = left.y + right.y;
			toReturn.z = left.z + right.z;
			toReturn.roll = left.roll + right.roll;
			toReturn.pitch = left.pitch + right.pitch;
			toReturn.yaw = left.yaw + right.yaw;
			toReturn.timestamp = (left.timestamp + right.timestamp) / 2;
			return toReturn;
		}

		public static RobotPose operator -(RobotPose left, RobotPose right)
		{
			RobotPose toReturn = new RobotPose();
			toReturn.x = left.x - right.x;
			toReturn.y = left.y - right.y;
			toReturn.z = left.z - right.z;
			toReturn.roll = left.roll - right.roll;
			toReturn.pitch = left.pitch - right.pitch;
			toReturn.yaw = left.yaw - right.yaw;
			toReturn.timestamp = (left.timestamp + right.timestamp) / 2;
			return toReturn;
		}

		#region ILoggable Members

		public string ToLog()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(timestamp); sb.Append("\t");
			sb.Append(x); sb.Append("\t");
			sb.Append(y); sb.Append("\t");
			sb.Append(z); sb.Append("\t");
			sb.Append(yaw); sb.Append("\t");
			sb.Append(pitch); sb.Append("\t");
			sb.Append(roll); sb.Append("\t");
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 6; j++)
				{
					sb.Append(covariance[i, j]); sb.Append("\t");
				}
			}
			return sb.ToString();
		}

		#endregion

		#region ITimestampedEventQueueItem Members

		public string DataType
		{
			get { return "RobotPose"; }
		}

		#endregion

		#region ITimeComparable Members

		public double TimeStamp
		{
			get { return this.timestamp; }
		}

		public int CompareTo(ITimeComparable obj)
		{
			if (this.timestamp < obj.TimeStamp) return -1;
			if (this.timestamp > obj.TimeStamp) return 1;
			return 0;
		}

		#endregion

		#region ITimeComparable Members

		double ITimeComparable.TimeStamp
		{
			get
			{
				return this.timestamp;
			}
			set
			{
				this.timestamp = value;
			}
		}

		#endregion
	}
}
