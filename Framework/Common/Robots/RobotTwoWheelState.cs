using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Robots;

namespace Magic.Common.Robots
{
	public class RobotTwoWheelState
	{
		public RobotTwoWheelState(RobotPose initialPose, RobotTwoWheelCommand initialCommand, RobotWheelModel right, RobotWheelModel left)
		{
			this.pose = initialPose; this.command = initialCommand;
			rightWheel = new RobotWheelModel(right);
			leftWheel = new RobotWheelModel(left);
		}


		public RobotTwoWheelState(RobotTwoWheelState toCopy, RobotTwoWheelCommand command)
		{
			//deep copy the class
			this.pose = new RobotPose(toCopy.pose);
			this.rightWheel = new RobotWheelModel(toCopy.rightWheel);
			this.leftWheel = new RobotWheelModel(toCopy.leftWheel);
			this.command = command;
		}

		private RobotPose pose;

		public RobotPose Pose
		{
			get { return pose; }
			set { pose = value; }
		}

		private RobotTwoWheelCommand command;

		public RobotTwoWheelCommand Command
		{
			get { return command; }
		}

		//internal model
		public RobotWheelModel rightWheel;
		public RobotWheelModel leftWheel;
	}

	//this is a second order model of the robot,
	//i.e. we are assuming we can change acceleration immediately.
	//
	public class RobotWheelModel
	{
		public double accel = 0;
		public double vel = 0;
		public double pos = 0;

		/// <summary>
		/// The default constructor
		/// </summary>
		public RobotWheelModel(double pos, double vel, double accel)
		{
			this.pos = pos; this.vel = vel; this.accel = accel;
		}

		public RobotWheelModel(RobotWheelModel toCopy)
		{
			this.accel = toCopy.accel; this.vel = toCopy.vel; this.pos = toCopy.pos;
		}
		public void ApplyCommand(double velocity, double timestep)
		{
			double errVelocity = velocity - vel;
			double adjuster = 40;
			double kp = 0.018 * adjuster;
			double ki = 0 * adjuster;
			double kd = -0.0085 * adjuster;
			if (Math.Abs(velocity) < Math.Abs(vel))
			{
				kp = kp * 6.0;
			}
			accel = errVelocity * kp + 0 * ki + accel * kd;
			vel += accel * timestep;
			pos += vel * timestep;
		}
	}
}
