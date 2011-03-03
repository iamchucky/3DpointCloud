using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Robots;
using Magic.Common.Sensors;

namespace Magic.Sensors.Odometry
{
	public class OdometryProcessorTwoWheel : IOdometryProcessor, ITimingProvider
	{
		IRobotTwoWheel robot;
		double prevIntegratedLeft;
		double prevIntegratedRight;
        double prevTS;
		double curTS;
		bool gotFirstUpdate = false;
		RobotPose state = new RobotPose();

		public event EventHandler<TimestampedEventArgs<RobotPose>> OdometryUpdate;
		public event EventHandler<TimestampedEventArgs<OdometryData>> EncoderUpdate;

		public OdometryProcessorTwoWheel(IRobotTwoWheel robot)
		{
			this.robot = robot;
			this.state.Zero();
			robot.WheelPositionUpdate += new EventHandler<TimestampedEventArgs<IRobotTwoWheelStatus>>(wheelPositionUpdate);
		}
		public void Reset()
		{
			gotFirstUpdate = false;
			state = new RobotPose();			
		}

		public void ForceState(RobotPose state)
		{
			gotFirstUpdate = false;
			this.state = state;
		}
		public RobotPose CurrentState
		{ get { return new RobotPose(state); } }
	
		void wheelPositionUpdate(object sender, TimestampedEventArgs<IRobotTwoWheelStatus> e)
		{
			curTS = e.TimeStamp;
			double curLeftPos = e.Message.IntegratedLeftWheelPosition; 
			double curRightPos = e.Message.IntegratedRightWheelPosition;
			double curLeftSpeed = e.Message.LeftWheelSpeed;
			double curRightSpeed = e.Message.RightWheelSpeed;
			double ts = e.TimeStamp;
			
			if (!gotFirstUpdate)
			{

				prevIntegratedLeft = curLeftPos;
				prevIntegratedRight = curRightPos;
                prevTS = curTS;
				gotFirstUpdate = true;
				return;
			}
			//get the current thing
			double diffL = curLeftPos - prevIntegratedLeft;
			double diffR = curRightPos - prevIntegratedRight;
            double deltaT = curTS - prevTS;
			prevIntegratedLeft = curLeftPos;
			prevIntegratedRight = curRightPos;
            prevTS = curTS;

			//from lavalle's book ; http://planning.cs.uiuc.edu/node659.html

			//calculate the derivatives in measurements we have
			double xdot = ((diffL + diffR) / 2.0) * Math.Cos(state.yaw);
			double ydot = ((diffL + diffR) / 2.0) * Math.Sin(state.yaw);
			double headingDot = (diffR - diffL) / robot.TrackWidth;

			//update the state vector
			state.x += xdot;
			state.y += ydot;
			state.yaw += headingDot;
			state.yaw = Pose.pi2pi(state.yaw);
			state.timestamp = ts;

			//update the wheel measurements
				
			if (OdometryUpdate != null) OdometryUpdate(this, new TimestampedEventArgs<RobotPose> (ts,CurrentState));
			if (EncoderUpdate != null) EncoderUpdate(this, new TimestampedEventArgs<OdometryData> (ts, new OdometryData (curLeftPos,curRightPos, diffL,diffR, curLeftSpeed, curRightSpeed, deltaT)));
		}

		#region ITimingProvider Members

		public double GetCurrentTime()
		{
			return curTS;
		}

		#endregion
	}


}
