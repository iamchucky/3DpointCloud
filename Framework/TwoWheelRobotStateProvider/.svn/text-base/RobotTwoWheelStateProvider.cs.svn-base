using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Robots;
using Magic.Common.Sensors;
using Magic.Common;

namespace Magic
{
	public class RobotTwoWheelStateProvider
	{
		double rWheelPos = 0;
		double lWheelPos = 0;

		double velTS = 0;
		double rWheelVel = 0;
		double lWheelVel = 0;
		double lastRWheelVel = 0;
		double lastLWheelVel = 0;
		double lastVelTS = 0;

		double rWheelAccel = 0;
		double lWheelAccel = 0;

		IRobotTwoWheel robot;
		RobotPose curPose = RobotPose.Invalid;

		public RobotTwoWheelStateProvider(IRobotTwoWheel robot, IPoseProvider poseProvider)
		{
			this.robot = robot;
			robot.WheelSpeedUpdate += new EventHandler<Magic.Common.TimestampedEventArgs<IRobotTwoWheelStatus>>(robot_WheelSpeedUpdate);
			robot.WheelPositionUpdate += new EventHandler<Magic.Common.TimestampedEventArgs<IRobotTwoWheelStatus>>(robot_WheelPositionUpdate);
			poseProvider.NewPoseAvailable += new EventHandler<NewPoseAvailableEventArgs>(poseProvider_NewPoseAvailable);
		}

		void poseProvider_NewPoseAvailable(object sender, NewPoseAvailableEventArgs e)
		{
			curPose = e.Pose;
		}

		void robot_WheelPositionUpdate(object sender, Magic.Common.TimestampedEventArgs<IRobotTwoWheelStatus> e)
		{
			lWheelPos = e.Message.IntegratedLeftWheelPosition;
			rWheelPos = e.Message.IntegratedRightWheelPosition;
		}

		void robot_WheelSpeedUpdate(object sender, Magic.Common.TimestampedEventArgs<IRobotTwoWheelStatus> e)
		{
			lastVelTS = velTS;
			lastRWheelVel = rWheelVel;
			lastLWheelVel = lWheelVel;
			rWheelVel = e.Message.RightWheelSpeed;
			lWheelVel = e.Message.LeftWheelSpeed;
			velTS = e.TimeStamp;

			if ((velTS - lastVelTS) > 0)
			{
				rWheelAccel = (rWheelVel - lastRWheelVel) / (velTS - lastVelTS);
				lWheelAccel = (lWheelVel - lastLWheelVel) / (velTS - lastVelTS);
			}
			else
			{
				rWheelAccel = 0;
				lWheelAccel = 0;
			}

		}

		public RobotTwoWheelState GetCurrentState(RobotTwoWheelCommand currentCommand)
		{
			RobotWheelModel rightW = new RobotWheelModel(rWheelPos, rWheelVel, rWheelAccel);
			RobotWheelModel leftW = new RobotWheelModel(lWheelPos, lWheelVel, lWheelAccel);
			RobotTwoWheelState state = new RobotTwoWheelState(curPose, currentCommand, rightW, leftW);
			return new RobotTwoWheelState(state, currentCommand);
		}
	}
}
