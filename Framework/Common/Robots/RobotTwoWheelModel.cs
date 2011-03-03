using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Robots;

namespace Magic.Common.Robots
{
	public class RobotTwoWheelModel
	{
		private static double trackWidth = .4625;

		public static RobotTwoWheelState Simulate(RobotTwoWheelCommand cmd, RobotTwoWheelState inititalState, double timestep)
		{
			//copy the initial state first!
			RobotTwoWheelState final = new RobotTwoWheelState(inititalState,cmd);			

			double prevIntegratedLeft = inititalState.leftWheel.pos;
			double prevIntegratedRight = inititalState.rightWheel.pos;

			if (cmd.velocity > 2) cmd.velocity = 2;
			else if (cmd.velocity < -2) cmd.velocity = -2;
			if (cmd.turn > 360) cmd.turn = 360;
			else if (cmd.turn < -360) cmd.turn = -360;
			cmd.turn = cmd.turn * 0.5;

			double cmdLeftWheelVel = cmd.velocity - 0.5 * trackWidth * (cmd.turn * Math.PI / 180);
			double cmdRightWheelVel = cmd.velocity + 0.5 * trackWidth * (cmd.turn * Math.PI / 180);

			final.leftWheel.ApplyCommand(cmdLeftWheelVel, timestep);
			final.rightWheel.ApplyCommand(cmdRightWheelVel, timestep);

			//now figure out the updated pose given the new state of our robot..
			double diffL = final.leftWheel.pos - prevIntegratedLeft;
			double diffR = final.rightWheel.pos - prevIntegratedRight;
			prevIntegratedLeft = final.leftWheel.pos;
			prevIntegratedRight = final.rightWheel.pos;

			//from lavalle's book ; http://planning.cs.uiuc.edu/node659.html

			//calculate the derivatives in measurements we have
			double xdot = ((diffL + diffR) / 2.0) * Math.Cos(inititalState.Pose.yaw);
			double ydot = ((diffL + diffR) / 2.0) * Math.Sin(inititalState.Pose.yaw);
			double headingDot = (diffR - diffL) / trackWidth;

			//update the state vector
			final.Pose.x = final.Pose.x + xdot;
			final.Pose.y = final.Pose.y + ydot;
			final.Pose.yaw = final.Pose.yaw + headingDot;
			while (final.Pose.yaw > Math.PI)
				final.Pose.yaw = final.Pose.yaw - 2 * Math.PI;
			while (final.Pose.yaw < -Math.PI)
				final.Pose.yaw = final.Pose.yaw + 2 * Math.PI;
			final.Pose.timestamp = final.Pose.timestamp + timestep;
			return final;
		}
		
	}
}
