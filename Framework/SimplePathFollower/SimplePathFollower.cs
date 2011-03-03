using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Robots;
using System.Threading;
using Magic.Common.Path;

namespace Magic.PathPlanning
{
	public class SimplePathFollower : IDisposable
	{
		object followerLock = new object();

		//indicates the algorithm thread is running
		bool running = true;

		//this makes the robot translate slower when it is turning.
		//The number must be less than one, and is more "effective" when it is closer to 0.
		double velocityTurningDamingFactor = .5;
		// threshold
		//const double distToStopAtTheEndOfPath = 0.1; //point at which to stop, i.e. when a path is finished.
		const double lookAheadDistParam = .24; // distance to find in advance in IPath
		const double epsilonHysterisis = 20 * Math.PI / 180.0; //hysterisis threshold

		//the current point the robot is trying to get to
		Vector2 goalPoint = new Vector2();
		Vector2 startPoint = new Vector2(); // Closest point on path.

		//the current point the robot is at
		RobotPose currentPoint = new RobotPose();

		//the command last calculated by the planner
		RobotTwoWheelCommand command = new RobotTwoWheelCommand(0, 0);

		IPath pathCurrentlyTracked;
		IPathSegment segmentCurrentlyTracked;

		//PointOnPath lastPointOnPath;
		//PointOnPath goalPointOnPath;
		//PointOnPath robotPointOnPath;

		public IPath PathCurrentlyTracked
		{
			get { return pathCurrentlyTracked; }
		}
		
		//gains
		double kPPosition;
		double kIPosition;
		double kDPosition;

		double kPHeading;
		double kIHeading;
		double kDHeading;

		double capVelocity;
		double capHeadingDot;

		double errorYaw;
		double errorYawTangent;
		double averageErrorYawTangent;
		double lastErrorYaw;


		public SimplePathFollower(double transVelMax, double rotVelMax)
		{
			//set the values of our parameters		

			kPPosition = 20;
			kIPosition = 0;
			kDPosition = 0;

			kPHeading = 50;
			kIHeading = 0;
			kDHeading = 100;

			capHeadingDot = rotVelMax;
			capVelocity = transVelMax;

			errorYaw = 0;
			lastErrorYaw = 0;
		}

		private bool hysterisisPositive = false;
		private bool hysterisisNegative = false;


		private static double UnwrapAndCalculateYawError(double errorX, double errorY, ref double currentYaw)
		{
			currentYaw %= (2 * Math.PI); //unwrap (if wrapped)

			while (currentYaw > Math.PI)
				currentYaw -= (2 * Math.PI);
			while (currentYaw < -Math.PI)
				currentYaw += (2 * Math.PI);

			double errorYaw = Math.Atan2(errorY, errorX) - currentYaw;

			while (errorYaw > Math.PI)
				errorYaw -= (2 * Math.PI);
			while (errorYaw < -Math.PI)
				errorYaw += (2 * Math.PI);

			return errorYaw;
		}

		public void UpdatePose(RobotPose pose)
		{
			lock (followerLock)
			{
				this.currentPoint.x = pose.x;
				this.currentPoint.y = pose.y;
				this.currentPoint.yaw = pose.yaw;
			}
		}


		public void UpdateGoal(RobotPose goal)
		{
			lock (followerLock)
			{
				this.goalPoint.X = goal.x;
				this.goalPoint.Y = goal.y;
			}
		}
		public void UpdatePath(IPath path)
		{
			if (path == null || path.Count == 0) return;
			if (PathUtils.CheckPathsEqual(path, pathCurrentlyTracked)) return;
			lock (followerLock)
			{
				pathCurrentlyTracked = path;
				segmentCurrentlyTracked = path[0];
			}
		}

        public RobotTwoWheelCommand GetCommand()
        {
            return GetCommand(capVelocity, capHeadingDot);
        }

		public RobotTwoWheelCommand GetCommand(double transMax, double turnMax)
		{
			lock (followerLock)
			{
				if (pathCurrentlyTracked == null)
				{
					Console.WriteLine("Null path tracked!");
					return new RobotTwoWheelCommand(0, 0);
				}

				//we are really far off the path, just get on the stupid path
				//mark sucks and wants this behavior
				if (pathCurrentlyTracked.Length == 0) //single waypoint case
				{
					goalPoint = pathCurrentlyTracked.EndPoint.pt;
					startPoint = pathCurrentlyTracked.EndPoint.pt;
				}
				//else if (segmentCurrentlyTracked.DistanceOffPath(currentPoint.ToVector2()) > lookAheadDistParam / 2)
				//{
				//    //Console.WriteLine("2");
				//    double lookaheadRef = 0;
				//    PointOnPath pTemp = segmentCurrentlyTracked.StartPoint;
				//    goalPoint = pathCurrentlyTracked.AdvancePoint(pTemp, ref lookaheadRef).pt;
				//}
				else
				{
					//Console.WriteLine("1");
					//see if we can track the next segment and if so, update that...
					PointOnPath currentPointOnPath = segmentCurrentlyTracked.ClosestPoint(currentPoint.ToVector2());
					double lookaheadRef = lookAheadDistParam;
					PointOnPath lookaheadPointOnPath = pathCurrentlyTracked.AdvancePoint(currentPointOnPath, ref lookaheadRef);
					if (segmentCurrentlyTracked != lookaheadPointOnPath.segment)
						segmentCurrentlyTracked = lookaheadPointOnPath.segment;
					goalPoint = lookaheadPointOnPath.pt;
					startPoint = currentPointOnPath.pt;
				}

				// Calculate errors
				double errorX = (goalPoint.X - currentPoint.x);
				double errorY = (goalPoint.Y - currentPoint.y);
				Vector2 verr = new Vector2(errorX, errorY);
				

				double tangentX = (goalPoint.X - startPoint.X);
				double tangentY = (goalPoint.Y - startPoint.Y);
				Vector2 tangent = new Vector2(tangentX, tangentY);

				double errorDistance = currentPoint.ToVector2().DistanceTo(startPoint);
				double currentYaw = currentPoint.yaw;
				double tempCurrentYaw = currentPoint.yaw;

				errorYawTangent = UnwrapAndCalculateYawError(tangentX, tangentY, ref tempCurrentYaw);
				errorYaw = UnwrapAndCalculateYawError(errorX, errorY, ref currentYaw);

				double tangentEquation = (goalPoint.Y - startPoint.Y) * currentPoint.x / (goalPoint.X - startPoint.Y)
					- (goalPoint.Y - startPoint.Y) * goalPoint.X / (goalPoint.X - startPoint.Y) + startPoint.Y;

				//Console.Clear();
				//Console.WriteLine("Current yaw is: " + currentYaw);
				

				if (tangentEquation < currentPoint.y)
				{
					if (Math.PI/2 <= Math.Abs(currentYaw) && Math.Abs(currentYaw) <= Math.PI)
					{
						//Console.WriteLine("Above line, pointing left");
						errorDistance *= -1;
					}
					//else Console.WriteLine("Above line, pointing right");
				}
				else
				{
					if (0 <= Math.Abs(currentYaw) && Math.Abs(currentYaw) <= Math.PI/2)
					{
						//Console.WriteLine("Below line, pointing right");
						errorDistance *= -1;
					}
					//else Console.Write("Below line, pointing left");
				}

				//if we are really close to last waypoint, make the robot just stop
				if ((segmentCurrentlyTracked == pathCurrentlyTracked[pathCurrentlyTracked.Count - 1]) &&
							 (PathCurrentlyTracked.EndPoint.pt - currentPoint.ToVector2()).Length < lookAheadDistParam)
				{
					command = new RobotTwoWheelCommand(0, 0);
					//Console.WriteLine("Acheived!");
				}
				else
				{
					//the idea here is we want to make the velocity (which is a derivative) be proportional to the error in our actual
					//position

					double unsignedYawErrorNormalizeToOne = Math.Abs(errorYaw) / Math.PI;
					double velocityPercentage = Math.Abs((Math.PI - Math.Abs(errorYawTangent)) / Math.PI);
					double commandedVelocity = (velocityPercentage < 0.5) ? 0.0 : transMax * velocityPercentage;
					//double commandedHeading = 200 * errorYawTangent + 100 * errorYaw + 300 * errorDistance;
					double commandedHeading = 150 * errorYawTangent + 100 * errorYaw + 200 * errorDistance;

					if (Math.Abs(commandedVelocity) > transMax) commandedVelocity = Math.Sign(commandedVelocity) * transMax;

					if (Math.Abs(commandedHeading) > turnMax)
					{
						commandedHeading = Math.Sign(commandedHeading) * turnMax;
						commandedVelocity = 0.0;
					}

					/*if (unsignedYawErrorNormalizeToOne > .1)
						commandedVelocity *= (1 - Math.Pow(unsignedYawErrorNormalizeToOne, velocityTurningDamingFactor));*/

					command = new RobotTwoWheelCommand(commandedVelocity, commandedHeading);

					//Console.WriteLine(errorYaw + ", " + errorYawTangent + ", " + errorDistance);
				}
				return command;
			}
		}


		public void Reset()
		{

		}

		#region IDisposable Members
		/// <summary>
		/// Clean up anything left over when this class is no longer needed
		/// </summary>
		public void Dispose()
		{
			running = false;
		}

		#endregion


	}
}