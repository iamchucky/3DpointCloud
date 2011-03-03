using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Path;
using Magic.Common.Robots;

namespace Magic.PathPlanning
{
	public class MonteCarloFollower : IDisposable
	{
		double DISTANCE_WEIGHT, VELOCITY_WEIGHT, TURN_WEIGHT;
		double lookAhead, maxVelocity, maxTurn;
		IPath path;
		RobotTwoWheelState state;

		public MonteCarloFollower(double lookAhead, double maxVelocity, double maxTurn)
		{
			DISTANCE_WEIGHT = 25;
			VELOCITY_WEIGHT = 1;
			TURN_WEIGHT = 1;

			this.lookAhead = lookAhead;
			this.maxVelocity = maxVelocity;
			this.maxTurn = maxTurn;
			state = new RobotTwoWheelState(new RobotPose(), new RobotTwoWheelCommand(0, 0), new RobotWheelModel(0, 0, 0), new RobotWheelModel(0, 0, 0));
		}

		public RobotTwoWheelCommand GetCommand()
		{
			Random rand = new Random();
			double lowestCost = double.MaxValue;
			double tempCost;
			RobotTwoWheelCommand lowestCommand, tempCommand;

			lowestCommand = new RobotTwoWheelCommand(0, 0);

			for (int i = 0; i < 1000; i++)
			{
				tempCommand = new RobotTwoWheelCommand(rand.NextDouble() * maxVelocity, rand.NextDouble() * 2 * maxTurn - maxTurn);

				tempCost = GetCost(tempCommand);
				if (tempCost < lowestCost)
				{
					lowestCommand = tempCommand;
					lowestCost = tempCost;
				}
			}

			Console.WriteLine("Command: " + lowestCommand.velocity + ", " + lowestCommand.turn + " Cost: " + lowestCost);

			return lowestCommand;
		}

		public void UpdatePose(RobotPose pose)
		{
			state.Pose = pose;
		}

		public void UpdatePath(IPath path)
		{
			this.path = path;
		}

		public IPath PathCurrentlyTracked
		{
			get { return path; }
		}

		private double GetCost(RobotTwoWheelCommand command)
		{
			double lookAheadRemaining = lookAhead;
			double cost = 0;

			PointOnPath closestPoint = path.GetClosest(state.Pose.ToVector2());
			PointOnPath lookAheadPoint = path.AdvancePoint(closestPoint, ref lookAheadRemaining);

			RobotTwoWheelState newState = RobotTwoWheelModel.Simulate(command, state, (lookAhead - lookAheadRemaining) / command.velocity);

			return newState.Pose.ToVector2().DistanceTo(lookAheadPoint.pt) * DISTANCE_WEIGHT;// - command.velocity * VELOCITY_WEIGHT - command.turn * TURN_WEIGHT;
		}

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion
	}
}
