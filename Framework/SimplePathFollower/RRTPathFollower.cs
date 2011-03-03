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
    public class RRTPathFollower : IDisposable
    {
        object followerLock = new object();

        //indicates the algorithm thread is running
        bool running = true;

        //this makes the robot translate slower when it is turning.
        //The number must be less than one, and is more "effective" when it is closer to 0.
        double velocityTurningDamingFactor = .35;
        // threshold
        //const double distToStopAtTheEndOfPath = 0.1; //point at which to stop, i.e. when a path is finished.
        const double lookAheadDistParam = 0.25; // distance to find in advance in IPath
        const double epsilonHysterisis = 20 * Math.PI / 180.0; //hysterisis threshold

        //the current point the robot is trying to get to
        Vector2 goalPoint = new Vector2();

        //the current point the robot is at
        RobotPose currentPoint = new RobotPose();

        //the command last calculated by the planner
        RobotTwoWheelCommand command = new RobotTwoWheelCommand(0, 0);

        // List of command from GoalNode to RootNode
        List<RobotTwoWheelCommand> inputList;


        IPath pathCurrentlyTracked;
        IPathSegment segmentCurrentlyTracked;
        IPathSegment nextSegmentToTrack;

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


        public RRTPathFollower()
        {
            kPPosition = 40;
            kIPosition = 0;
            kDPosition = 0;

            kPHeading = 400;
            kIHeading = 0;
            kDHeading = 0;

            capHeadingDot = 1000;
            capVelocity = 4;
            Thread t = new Thread(new ParameterizedThreadStart(PlannerThread));
            t.Start();
        }

        public void PlannerThread(object o)
        {
            while (running)
            {
                lock (followerLock)
                {
                    PlanPurePursuit();
                }
                Thread.Sleep(5);
            }
        }

        private void PlanPurePursuit()
        {
            if (pathCurrentlyTracked == null) return;

            //we are really far off the path, just get on the stupid path
            //mark sucks and wants this behavior
            if (pathCurrentlyTracked.Length == 0)
            {
                goalPoint = pathCurrentlyTracked.EndPoint.pt;
            }
            else if (segmentCurrentlyTracked.DistanceOffPath(currentPoint.ToVector2()) > lookAheadDistParam / 2)
            {
                //Console.WriteLine("2");
                double lookaheadRef = 0;
                PointOnPath pTemp = segmentCurrentlyTracked.StartPoint;
                goalPoint = pathCurrentlyTracked.AdvancePoint(pTemp, ref lookaheadRef).pt;
            }
            else
            {
                //Console.WriteLine("1");
                //see if we can track the next segment and if so, update that...
                PointOnPath currentPointOnPath = segmentCurrentlyTracked.ClosestPoint(currentPoint.ToVector2());
                double lookaheadRef = lookAheadDistParam;
                PointOnPath lookaheadPointOnPath = pathCurrentlyTracked.AdvancePoint(currentPointOnPath, ref lookaheadRef);
                //the path lookahead point is at the end, and we cant do antyhing

                segmentCurrentlyTracked = lookaheadPointOnPath.segment;
                goalPoint = lookaheadPointOnPath.pt;
            }

            double errorX = (goalPoint.X - currentPoint.x);
            double errorY = (goalPoint.Y - currentPoint.y);

            Vector2 verr = new Vector2(errorX, errorY);

            //Console.WriteLine(errorX + " | " + errorY);
            double errorDistance = verr.Length;
            double currentYaw = currentPoint.yaw;
            double errorYaw = UnwrapAndCalculateYawError(errorX, errorY, ref currentYaw);


            //Console.Write("neg Hyst: " + hysterisisNegative + " pos Hyst: " + hysterisisPositive + " yawError" +  errorYaw * 180.0/ Math.PI+ " ||");
            //the reason for this is that our angles are defined from 0 to 360
            //but the PID controller expects angle to be -180 to 180


            //calculate the control outputs
            //the idea here is we want to make the velocity (which is a derivative) be proportional to the error in our actual
            //position

            double unsignedYawErrorNormalizeToOne = Math.Abs(errorYaw) / Math.PI;
            double commandedVelocity = (kPPosition * errorDistance);
            double commandedHeading = kPHeading * errorYaw;

            if (Math.Abs(commandedVelocity) > capVelocity) commandedVelocity = Math.Sign(commandedVelocity) * capVelocity;
            if (Math.Abs(commandedHeading) > capHeadingDot) commandedHeading = Math.Sign(commandedHeading) * capHeadingDot;

            //if (unsignedYawErrorNormalizeToOne > .1)

            // find which input to use
            RobotTwoWheelCommand currentInput = inputList[pathCurrentlyTracked.IndexOf(segmentCurrentlyTracked)];

            //commandedVelocity *= (1 - Math.Pow(unsignedYawErrorNormalizeToOne, velocityTurningDamingFactor));
            //command = new RobotTwoWheelCommand(commandedVelocity, commandedHeading);
            if (pathCurrentlyTracked.EndPoint.pt.DistanceTo(currentPoint.ToVector2()) < .2)
                command = new RobotTwoWheelCommand(0, 0);
            else
            {
                if (currentInput.velocity < 0.3)
                    command = new RobotTwoWheelCommand(0.4, commandedHeading);
                else
                    command = new RobotTwoWheelCommand(currentInput.velocity, commandedHeading);
            }
            //if (reachedGoal)
            //{
            //  command.velocity = 0;
            //  command.turn = 0;
            //}
            //Console.WriteLine("Current: " + currentPoint.x + " " + currentPoint.y + " " + currentPoint.yaw + " | " + errorDistance + " | " + errorYaw + " | " + commandedVelocity + " " + commandedHeading);
            //Console.WriteLine();
        }

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
            lock (followerLock)
            {
                if (PathUtils.CheckPathsEqual(path, pathCurrentlyTracked)) return;
                pathCurrentlyTracked = path;
                segmentCurrentlyTracked = path[0];
                nextSegmentToTrack = path[1];
            }
        }

        public void UpdatePath(RRTNode goalNode)
        {
            //create IPath from rootNode
            IPath newNodePath = new PointPath();
            inputList = new List<RobotTwoWheelCommand>();
            ConvertNodeToPath(newNodePath, goalNode, inputList);
            lock (followerLock)
            {
                UpdatePath(newNodePath);
            }
        }

        private void ConvertNodeToPath(IPath newNodePath, RRTNode goalNode, List<RobotTwoWheelCommand> inputList)
        {
            if (goalNode.IsRoot) return;
            IPathSegment segment = new LinePathSegment(goalNode.Parent.Point, goalNode.Point);
            newNodePath.Insert(0, segment);
            inputList.Insert(0, new RobotTwoWheelCommand(goalNode.State.Command.velocity, goalNode.State.Command.turn));
            ConvertNodeToPath(newNodePath, goalNode.GetParent(), inputList);
        }


        public RobotTwoWheelCommand GetCommand()
        {
            lock (followerLock)
                return command;
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