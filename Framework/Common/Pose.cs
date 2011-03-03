using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common
{
	[Serializable]
	public abstract class Pose
	{

		public Pose()
		{
			this.Zero();
		}

		public Pose(Pose p)
		{
			x = p.x;
			y = p.y;
			z = p.z;
			yaw = p.yaw;
			pitch = p.pitch;
			roll = p.roll;
			timestamp = p.timestamp;
		}
		/// <summary>
		/// Represents the position and orientation of a robot
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="yaw"></param>
		/// <param name="pitch"></param>
		/// <param name="roll"></param>
		public Pose(double x, double y, double z, double yaw, double pitch, double roll)
		{
			this.x = x; this.y = y; this.z = z;
			this.yaw = yaw; this.pitch = pitch; this.roll = roll;
			this.timestamp = 0;
		}
		public Pose(double x, double y, double z, double yaw, double pitch, double roll, double timestamp)
		{
			this.x = x; this.y = y; this.z = z;
			this.yaw = yaw; this.pitch = pitch; this.roll = roll;
			this.timestamp = timestamp;
		}

		/// <summary>
		/// Left
		/// </summary>
		public double x;

		/// <summary>
		/// Forward
		/// </summary>
		public double y;

		/// <summary>
		/// Up
		/// </summary>
		public double z;

		/// <summary>
		/// Rotation around the x axis
		/// </summary>
		public double roll;

		/// <summary>
		/// Rotations around the y axis
		/// </summary>
		public double pitch;

		/// <summary>
		/// Rotation around z axis
		/// </summary>
		public double yaw;

		/// <summary>
		/// For most people, this is in seconds. For Aaron, it's "rotation around z axis"
		/// </summary>
		public double timestamp;


		/// <summary>
		/// Resets all parameters to 0
		/// </summary>
		public void Zero()
		{
			x = y = z = roll = pitch = yaw = timestamp = 0;
		}

		public override string ToString()
		{
			return "X:" + x.ToString("G4") + " Y:" + y.ToString("G4") + " Z:" + z.ToString("G4") + "\n" + 
							 " Yaw:" + Pose.pi2pi(yaw).ToString("G4") + " Pitch:" + pitch.ToString("G4") + " Roll:" + roll.ToString("G4") + "\n" + 
							 "@ " + timestamp.ToString("F6");
		}

		public string ToStringThreeLine()
		{
			return "X:" + x.ToString("G4") + "\tY:" + y.ToString("G4") + "\tZ:" + z.ToString("G4") + 
						 "\nYaw:" + Pose.pi2pi(yaw).ToString("G4") + "\tPitch:" + pitch.ToString("G4") + "\tRoll:" + roll.ToString("G4") +
						 "\n@ " + timestamp.ToString("F6");
		}

		public Vector2 ToVector2()
		{
			return new Vector2(x, y);
		}

		public static double pi2pi(double angIn)
		{
			return angIn + Math.PI - Math.Floor((angIn + Math.PI) / (2.0 * Math.PI)) * 2.0 * Math.PI - Math.PI;
		}

	}
}
