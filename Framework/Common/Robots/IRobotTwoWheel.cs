using System;
using Magic.Common.Sensors;

namespace Magic.Common.Robots
{
	public interface IRobotTwoWheel : IRobot, ISensor
	{
		void SetVelocityTurn(RobotTwoWheelCommand command, bool inclined);
		void SetVelocityTurn(RobotTwoWheelCommand command);
		void GetWheelPositions(out double leftPosition, out double rightPosition, out double Timestamp);
		double TrackWidth { get; }
		IRobotTwoWheelStatus Status { get; }

		event EventHandler Shutdown;
		event EventHandler StatusUpdated;
		event EventHandler<TimestampedEventArgs<IRobotTwoWheelStatus>> WheelPositionUpdate;
		event EventHandler<TimestampedEventArgs<IRobotTwoWheelStatus>> WheelSpeedUpdate;
		double ForwardVelocity { get; }
		double RotationRate { get; }
	}

	[Serializable]
	public class RobotTwoWheelCommand
	{
		public RobotTwoWheelCommand(double velocity, double turn)
		{
			this.velocity = velocity;
			this.turn = turn;
		}

		public double velocity;
		public double turn;
		
		public override bool Equals(object obj)
		{
			if (obj is RobotTwoWheelCommand == false)
				return false;
			RobotTwoWheelCommand other = obj as RobotTwoWheelCommand;
			return (other.turn == this.turn && other.velocity == this.velocity);
		}
		public override int GetHashCode()
		{
			return velocity.GetHashCode() ^ turn.GetHashCode ();
		}
		public override string ToString()
		{
			return "Velo: " + velocity + " turn: " + turn;
		}
	}

	public interface IRobotTwoWheelStatus
	{
		double LeftWheelSpeed { get; }
		double RightWheelSpeed { get; }

		double IntegratedLeftWheelPosition { get; }
		double IntegratedRightWheelPosition { get; }

		string StatusString { get; }
	}
}
