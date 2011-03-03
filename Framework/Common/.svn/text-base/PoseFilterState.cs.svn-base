using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;
using Magic.Common;

namespace Magic.Common
{
	/// <summary>
	/// This represents the full position and orientation of a robot. 		
	/// </summary>
	[Serializable]
	public class PoseFilterState : Pose, ILoggable, IComparable, IComparable<PoseFilterState>
	{
		public Matrix Covariance = new Matrix(7, 7);
		public Matrix DCM = new Matrix(3, 3);

		//Quaternion for attitude representation
		public double q1;
		public double q2;
		public double q3;
		public double q4;

        //Velocity Estimates
        public double vx;
        public double vy;
        public double vz;
        //Rotation Rates
        public double wx;
        public double wy;
        public double wz;

		public PoseFilterState()
		{
			Zero();
            q1 = q2 = q3 = q4 = vx = vy = vz = wx = wy = wz = 0;
		}

		public PoseFilterState(PoseFilterState toCopy)
			: base(toCopy)
		{
			this.Covariance = new Matrix(toCopy.Covariance.Array);
			this.DCM = new Matrix(toCopy.DCM.Array);
			this.q1 = toCopy.q1;
			this.q2 = toCopy.q2;
			this.q3 = toCopy.q3;
			this.q4 = toCopy.q4;
            this.vx = toCopy.vx;
            this.vy = toCopy.vy;
            this.vz = toCopy.vz;
            this.wx = toCopy.wx;
            this.wy = toCopy.wy;
            this.wz = toCopy.wz;

		}

		public PoseFilterState(double x, double y, double z, double q1, double q2, double q3, double q4, double timestamp)
		{
			this.x = x;
			this.y = y;
			this.z = z;

			this.q1 = q1;
			this.q2 = q2;
			this.q3 = q3;
			this.q4 = q4;

            this.vx = 0;
            this.vy = 0;
            this.vz = 0;
            this.wx = 0;
            this.wy = 0;
            this.wz = 0;

			this.timestamp = timestamp;
			this.pitch = pi2pi(Math.Asin(2 * (q4 * q2 - q1 * q3)));
			if (Math.Cos(this.pitch) != 0)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 - q2) + Math.Atan2(q3 - q1, q4 + q2));
				this.roll = pi2pi(Math.Atan2(q3 + q1, q4 - q2) - Math.Atan2(q3 - q1, q4 + q2));
			}
			else if (this.pitch == Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 - q1, q4 - q2));
				this.roll = 0;
			}
			else if (this.pitch == -Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 + q2));
				this.roll = 0;
			}
			DCM[0, 0] = q1 * q1 - q2 * q2 - q3 * q3 + q4 * q4;
			DCM[0, 1] = 2 * (q1 * q2 - q3 * q4);
			DCM[0, 2] = 2 * (q1 * q3 + q2 * q4);
			DCM[1, 0] = 2 * (q1 * q2 + q3 * q4);
			DCM[1, 1] = -1 * q1 * q1 + q2 * q2 - q3 * q3 + q4 * q4;
			DCM[1, 2] = 2 * (q2 * q3 - q1 * q4);
			DCM[2, 0] = 2 * (q1 * q3 - q2 * q4);
			DCM[2, 1] = 2 * (q1 * q4 + q2 * q3);
			DCM[2, 2] = -1 * q1 * q1 - q2 * q2 + q3 * q3 + q4 * q4;
		}

		public PoseFilterState(double x, double y, double z, double q1, double q2, double q3, double q4, double timestamp, Matrix covariance)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		
			this.q1 = q1;
			this.q2 = q2;
			this.q3 = q3;
			this.q4 = q4;

            this.vx = 0;
            this.vy = 0;
            this.vz = 0;
            this.wx = 0;
            this.wy = 0;
            this.wz = 0;

			this.timestamp = timestamp;
			this.pitch = pi2pi(Math.Asin(2 * (q4 * q2 - q1 * q3)));
			if (Math.Cos(this.pitch) != 0)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 - q2) + Math.Atan2(q3 - q1, q4 + q2));
				this.roll = pi2pi(Math.Atan2(q3 + q1, q4 - q2) - Math.Atan2(q3 - q1, q4 + q2));
			}
			else if (this.pitch == Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 - q1, q4 - q2));
				this.roll = 0;
			}
			else if (this.pitch == -Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 + q2));
				this.roll = 0;
			}
			DCM[0, 0] = q1 * q1 - q2 * q2 - q3 * q3 + q4 * q4;
			DCM[0, 1] = 2 * (q1 * q2 - q3 * q4);
			DCM[0, 2] = 2 * (q1 * q3 + q2 * q4);
			DCM[1, 0] = 2 * (q1 * q2 + q3 * q4);
			DCM[1, 1] = -1 * q1 * q1 + q2 * q2 - q3 * q3 + q4 * q4;
			DCM[1, 2] = 2 * (q2 * q3 - q1 * q4);
			DCM[2, 0] = 2 * (q1 * q3 - q2 * q4);
			DCM[2, 1] = 2 * (q1 * q4 + q2 * q3);
			DCM[2, 2] = -1 * q1 * q1 - q2 * q2 + q3 * q3 + q4 * q4;
			this.Covariance = covariance;
		}

		public PoseFilterState(double x, double y, double z, double q1, double q2, double q3, double q4, Matrix DCM, double timestamp)
		{
			this.x = x;
			this.y = y;
			this.z = z;
	
			this.q1 = q1;
			this.q2 = q2;
			this.q3 = q3;
			this.q4 = q4;

            this.vx = 0;
            this.vy = 0;
            this.vz = 0;
            this.wx = 0;
            this.wy = 0;
            this.wz = 0;

			this.pitch = pi2pi(Math.Asin(2 * (q4 * q2 - q1 * q3)));
			if (Math.Cos(this.pitch) != 0)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 - q2) + Math.Atan2(q3 - q1, q4 + q2));
				this.roll = pi2pi(Math.Atan2(q3 + q1, q4 - q2) - Math.Atan2(q3 - q1, q4 + q2));
			}
			else if (this.pitch == Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 - q1, q4 - q2));
				this.roll = 0;
			}
			else if (this.pitch == -Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 + q2));
				this.roll = 0;
			}
			this.DCM = DCM;

            this.timestamp = timestamp;
		}

		public PoseFilterState(double x, double y, double z, double q1, double q2, double q3, double q4, Matrix DCM, Matrix Covariance, double timestamp)
		{
			this.x = x;
			this.y = y;
			this.z = z;
	
			this.q1 = q1;
			this.q2 = q2;
			this.q3 = q3;
			this.q4 = q4;

            this.vx = 0;
            this.vy = 0;
            this.vz = 0;
            this.wx = 0;
            this.wy = 0;
            this.wz = 0;

			this.timestamp = timestamp;
			this.pitch = pi2pi(Math.Asin(2 * (q4 * q2 - q1 * q3)));
			if (Math.Cos(this.pitch) != 0)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 - q2) + Math.Atan2(q3 - q1, q4 + q2));
				this.roll = pi2pi(Math.Atan2(q3 + q1, q4 - q2) - Math.Atan2(q3 - q1, q4 + q2));
			}
			else if (this.pitch == Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 - q1, q4 - q2));
				this.roll = 0;
			}
			else if (this.pitch == -Math.PI / 2)
			{
				this.yaw = pi2pi(Math.Atan2(q3 + q1, q4 + q2));
				this.roll = 0;
			}
			this.DCM = DCM;
			this.Covariance = Covariance;
		}

		public PoseFilterState(double x, double y, double z, double yaw, double pitch, double roll, double timestamp)
			: base(x, y, z, yaw, pitch, roll,timestamp)
		{
			DCM[0, 0] = Math.Cos(pitch) * Math.Cos(yaw);  
			DCM[0, 1] = Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Cos(roll) * Math.Sin(yaw);
			DCM[0, 2] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Sin(roll) * Math.Sin(yaw);
			DCM[1, 0] = Math.Cos(pitch) * Math.Sin(yaw);
			DCM[1, 1] = Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Cos(roll) * Math.Cos(yaw);
			DCM[1, 2] = Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Sin(roll) * Math.Cos(yaw);
			DCM[2, 0] = -1 * Math.Sin(pitch);
			DCM[2, 1] = Math.Cos(pitch) * Math.Sin(roll);
			DCM[2, 2] = Math.Cos(pitch) * Math.Cos(roll);

			q3 = Math.Cos(roll / 2) * Math.Cos(pitch / 2) * Math.Sin(yaw / 2) - Math.Sin(roll / 2) * Math.Sin(pitch / 2) * Math.Cos(yaw / 2);
			q2 = Math.Cos(roll / 2) * Math.Sin(pitch / 2) * Math.Cos(yaw / 2) + Math.Sin(roll / 2) * Math.Cos(pitch / 2) * Math.Sin(yaw / 2);
			q1 = Math.Sin(roll / 2) * Math.Cos(pitch / 2) * Math.Cos(yaw / 2) - Math.Cos(roll / 2) * Math.Sin(pitch / 2) * Math.Sin(yaw / 2);
			q4 = Math.Cos(roll / 2) * Math.Cos(pitch / 2) * Math.Cos(yaw / 2) + Math.Sin(roll / 2) * Math.Sin(pitch / 2) * Math.Sin(yaw / 2);

            this.vx = 0;
            this.vy = 0;
            this.vz = 0;
            this.wx = 0;
            this.wy = 0;
            this.wz = 0;
		}

		public RobotPose ToRobotPose()
		{
			RobotPose pose = new RobotPose(x, y, z, yaw, pitch, roll, timestamp);
			pose.covariance[0, 0] = Covariance[0, 0];
			pose.covariance[0, 1] = Covariance[0, 1];
			pose.covariance[0, 2] = Covariance[0, 2];
			pose.covariance[1, 0] = Covariance[1, 0];
			pose.covariance[1, 1] = Covariance[1, 1];
			pose.covariance[1, 2] = Covariance[1, 2];
			pose.covariance[2, 0] = Covariance[2, 0];
			pose.covariance[2, 1] = Covariance[2, 1];
			pose.covariance[2, 2] = Covariance[2, 2];
			return pose;
		}

		public static PoseFilterState Invalid = new PoseFilterState(-9999, -9999, -9999, -9999, -9999, -9999, -9999, 0);

		#region ILoggable Members

		public string ToLog()
		{
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(timestamp); sb.Append("\t");
				sb.Append(x); sb.Append("\t");
				sb.Append(y); sb.Append("\t");
				sb.Append(z); sb.Append("\t");
				sb.Append(q1); sb.Append("\t");
				sb.Append(q2); sb.Append("\t");
				sb.Append(q3); sb.Append("\t");
				sb.Append(q4); sb.Append("\t");
				for (int i = 0; i < 7; i++)
				{
					for (int j = 0; j < 7; j++)
					{
						sb.Append(Covariance[i, j]); sb.Append("\t");
					}
				}
				return sb.ToString();
			}
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (!(obj is PoseFilterState))
				throw new InvalidOperationException("CompareTo: Not a PoseFilterState");
				return CompareTo((PoseFilterState) obj);
		}

		#endregion

		#region IComparable<PoseFilterState> Members

		public int CompareTo(PoseFilterState other)
		{
			if (this.timestamp < other.timestamp) return -1;
			if (this.timestamp > other.timestamp) return 1;
			return 0;
		}

		#endregion
	}
}
