using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Path;
using Magic.Common.Shapes;
using Magic.Common.Robots;

namespace Magic.PathPlanning
{
	public class RRTPlannerImpl : IPathPlanner
	{
		RobotTwoWheelStateProvider stateProvider;
		RobotTwoWheelCommand lastCommand = new RobotTwoWheelCommand(0, 0);

		public RobotTwoWheelCommand LastCommand
		{
			get { return lastCommand; }
			set { lastCommand = value; }
		}
		RRTPlannerControl rrt;
		List<Polygon> obstacles;
		RobotPose currentPose;
		IPath waypoints;
		IPath lastWaypoints = new PointPath();

		Object locker = new object();
		IPathSegment currentSegment;

		public RRTPlannerImpl(RobotTwoWheelStateProvider stateProvider)
		{
			rrt = new RRTPlannerControl();
			obstacles = new List<Polygon>();
			currentPose = new RobotPose();
			waypoints = new PointPath();
			this.stateProvider = stateProvider;
		}


		/// <summary>
		/// Find the path from the goal to the starting point
		/// </summary>
		/// <param name="goal"></param>
		/// <returns>path from the start point to the goal point</returns>
		private IPath GetFinalPath(RRTNode goal)
		{
			IPath pathToGoal = new PointPath();
			GetToRoot(goal, ref pathToGoal);
			return pathToGoal;
		}


		/// <summary>
		/// Starting from the goal, recursively follow its parent while inserting it into the path, because it's moving backward
		/// </summary>
		/// <param name="goal"></param>
		/// <param name="pathToAdd"></param>
		private void GetToRoot(RRTNode goal, ref IPath pathToAdd)
		{
			if (goal.IsRoot == false)
			{
				IPathSegment segment = new LinePathSegment(goal.Parent.Point, goal.Point);
				pathToAdd.Insert(0, segment);
				GetToRoot((RRTNode)goal.Parent, ref pathToAdd);
			}
		}

        public volatile bool receivedNewPath = false;
		public RRTNode GetNodeToGoal()
		{
			if (waypoints.Length == 0.0)
				return null;
			lock (locker)
			{
				RobotTwoWheelState currentState = stateProvider.GetCurrentState(lastCommand);
				IPath path = new PointPath();
				RRTNode startingNode = new RRTNode(currentState);
				RRTNode goalNode;
				//foreach (IPathSegment segment in waypoints)
				//{
				bool foundPath = false;

				Console.WriteLine("Finding a path");
				do
                {
                    startingNode = new RRTNode(currentState);
					foundPath = rrt.FindPath(ref startingNode, waypoints[1].End, obstacles, out goalNode);
				}
				while (!foundPath && receivedNewPath);
                receivedNewPath = false;
				return goalNode;
			}
		}

		#region IPathPlanner Members

		public IPath GetPathToGoal()
		{
			if (waypoints.Length == 0.0)
				return null;
			lock (locker)
			{
				RobotTwoWheelState currentState = stateProvider.GetCurrentState(lastCommand);
				IPath path = new PointPath();
				RRTNode startingNode = new RRTNode(currentState);
				RRTNode goalNode;
				//foreach (IPathSegment segment in waypoints)
				//{
                while (rrt.FindPath(ref startingNode, waypoints[1].End, obstacles, out goalNode) == false)
                {
                    continue;
                }
				//startingNode = goalNode;
				IPath segmentToGoal = GetFinalPath(goalNode);
				// add each small segment of each segment to the path to return
				//foreach (IPathSegment seg in segmentToGoal)
				//path.Add(seg);
				//}
				return segmentToGoal;
			}
		}

		public void UpdateObstacles(List<Polygon> obstacles)
		{
			lock (locker)
			{
				this.obstacles.Clear();
				Polygon safeVehiclePolygon = Polygon.VehiclePolygonWithRadius(0.8);
				foreach (Polygon poly in obstacles)
				{
					Polygon convolutedPoly = Polygon.ConvexMinkowskiConvolution(poly, safeVehiclePolygon);
					this.obstacles.Add(convolutedPoly);
				}
			}
		}

		public void UpdatePose(RobotPose pose)
		{
			lock (locker)
			{
				this.currentPose = pose;
			}
		}

		/// <summary>
		/// Update waypoints and return false if given path is same as before
		/// </summary>
		/// <param name="givenWaypoints"></param>
		/// <returns></returns>
		public bool UpdateWaypoints(List<Vector2> waypoints)//IPath givenWaypoints)
		{
			/*if (PathUtils.CheckPathsEqual(givenWaypoints, lastWaypoints)) return false;
			lastWaypoints.Clear();
			foreach (IPathSegment segment in givenWaypoints)
			{
				lastWaypoints.Add(segment);
			}

			lock (locker)
			{
				this.waypoints = givenWaypoints;
				IPathSegment currentToFirstPoint = new LinePathSegment(currentPose.ToVector2(), givenWaypoints[0].StartPoint.pt);
				this.waypoints.Insert(0, currentToFirstPoint);
			}*/
			return true;
		}

		public List<Polygon> GetObstacles()
		{
			lock (locker)
				return this.obstacles;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
