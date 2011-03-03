using System;
using Magic.Common;
using Magic.Common.DataStructures.Tree;
using System.Threading;
using System.Collections.Generic;
using Magic.Common.Shapes;
using Magic.Common.Path;
using System.Diagnostics;

namespace Magic.PathPlanning
{
    public class RRTPlanner : IDisposable, IPathPlanner
    {
        object plannerLock = new object();
        object pathLock = new object();
        bool running;
        double stepSize;
        double wayPointRadius;
        double pathPointRadius;
        int minX;
        int minY;
        int maxX;
        int maxY;
        List<SimpleTreeNode<Vector2>> nodeList;
        List<SimpleTreeNode<Vector2>> lastPlannedPath;
        List<SimpleTreeNode<Vector2>> outputNodeList;
        List<SimpleTreeNode<Vector2>> userWaypoints;
        List<SimpleTreeNode<Vector2>> activeWaypoints;
        IPath userDefinedPath;
        List<Polygon> obstacles;
        List<Polygon> bloatedObstacles;
        Vector2 currentLocation;
        Random rand;

        double safeVehicleRadius = 0.75;
        double vehicleRadius = 0.5;


        /// <summary>
        /// Creates a new RRT planner with specified extents and stepsize.
        /// </summary>
        public RRTPlanner(int minX, int maxX, int minY, int maxY, double stepSize)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            this.stepSize = stepSize;
            wayPointRadius = 0.4;
            pathPointRadius = 0.4;
            outputNodeList = new List<SimpleTreeNode<Vector2>>();
            userWaypoints = new List<SimpleTreeNode<Vector2>>();
            activeWaypoints = new List<SimpleTreeNode<Vector2>>();
            userDefinedPath = new PointPath();
            obstacles = new List<Polygon>();
            bloatedObstacles = new List<Polygon>();
            currentLocation = new Vector2();
            rand = new Random();
            running = true;

            Thread t = new Thread(new ParameterizedThreadStart(PlannerThread));
            t.Start();
        }

        public List<Polygon> GetObstacles()
        {
            List<Polygon> bloatedObstaclesToReturn;
            lock (plannerLock)
            {
                bloatedObstaclesToReturn = new List<Polygon>();
                foreach (Polygon p in bloatedObstacles)
                    bloatedObstaclesToReturn.Add(p);
            }
            return bloatedObstaclesToReturn;
        }

        private void PlannerThread(object o)
        {
            //Declare variables
            List<SimpleTreeNode<Vector2>> waypointsToRemove;
            SimpleTreeNode<Vector2> roverNode;
            List<SimpleTreeNode<Vector2>> nodesToKeep;
            SimpleTreeNode<Vector2> lastSafeWaypoint;
            List<SimpleTreeNode<Vector2>> currentPath;
            List<SimpleTreeNode<Vector2>> newPath;
            List<Vector2> activeWaypointValues;
            while (running)
            {
                Thread.Sleep(50);
                lock (plannerLock)
                {

                    //add ghetto fix to robot inside polygon issue
                    foreach (Polygon poly in bloatedObstacles)
                    {
                        if (poly.IsInside(currentLocation))
                        {
                            bloatedObstacles = new List<Polygon>();
                            foreach (Polygon smallPoly in obstacles)
                            {
                                Polygon vehiclePolygon = Polygon.VehiclePolygonWithRadius(vehicleRadius);
                                bloatedObstacles.Add(Polygon.ConvexMinkowskiConvolution(smallPoly, vehiclePolygon));
                            }
                        }
                    }

                    //Create a copy of the output list of nodes to work on, losing all parent/child relationships in the process
                    lastPlannedPath = new List<SimpleTreeNode<Vector2>>();
                    foreach (SimpleTreeNode<Vector2> node in outputNodeList)
                    {
                        lastPlannedPath.Add(new SimpleTreeNode<Vector2>(node.Value));
                    }

                    //Create a copy of the waypoints list and see any of them are now inside obstacles
                    activeWaypoints = new List<SimpleTreeNode<Vector2>>();
                    foreach (SimpleTreeNode<Vector2> node in userWaypoints)
                    {
                        activeWaypoints.Add(node);
                    }

                    //Check to see if any of the waypoints are inside obstacles, if so remove them from the list
                    waypointsToRemove = new List<SimpleTreeNode<Vector2>>();
                    foreach (SimpleTreeNode<Vector2> waypoint in activeWaypoints)
                    {
                        foreach (Polygon poly in bloatedObstacles)
                        {
                            if (poly.IsInside(waypoint.Value))
                            {
                                waypointsToRemove.Add(waypoint);
                                break;
                            }
                        }
                    }
                    foreach (SimpleTreeNode<Vector2> waypoint in waypointsToRemove)
                    {
                        activeWaypoints.Remove(waypoint);
                    }

                    //Check to see if the active waypoint list is empty. If so, output a empty path
                    //and go to the next iteration of the main planning loop
                    if (activeWaypoints.Count == 0)
                    {
                        UpdateLastPath(new List<SimpleTreeNode<Vector2>>());
                        continue;
                    }

                    //Create a new node at the current rover location, add it to the node list from the last planned path,
                    //and set it as the parent of the first node in that list. Make sure to remove this node form the list
                    //before outputting
                    roverNode = new SimpleTreeNode<Vector2>(currentLocation);
                    lastPlannedPath.Insert(0, roverNode);
                    activeWaypoints.Insert(0, roverNode);

                    //Check to see how much of the last path is still valid
                    nodesToKeep = new List<SimpleTreeNode<Vector2>>();
                    lastSafeWaypoint = lastPlannedPath[0];
                    nodesToKeep.Add(lastPlannedPath[0]);
                    activeWaypointValues = new List<Vector2>();
                    int safeWaypoints = 1; //You always clear the first waypoint, as that is the current rover position
                    foreach (SimpleTreeNode<Vector2> waypoint in activeWaypoints)
                    {
                        activeWaypointValues.Add(waypoint.Value);
                    }
                    if (lastPlannedPath.Count > 1)
                    {
                        if (CheckObstacles(lastPlannedPath[0], lastPlannedPath[1]))
                        {
                            for (int i = 1; i < lastPlannedPath.Count; i++)
                            {
                                if (CheckObstacles(lastPlannedPath[i], lastPlannedPath[i - 1]))
                                {
                                    nodesToKeep.Add(lastPlannedPath[i]);
                                }
                                else break;
                                if (activeWaypointValues.Contains(lastPlannedPath[i].Value))
                                {
                                    lastSafeWaypoint = lastPlannedPath[i];
																		activeWaypointValues.Remove(lastPlannedPath[i].Value);
                                    safeWaypoints++;
                                }
                            }
                        }
                    }

                    //Initialize a new path to plan from by including all of the safe nodes in the previous solution
                    //up to and including the last safe waypoint.
                    currentPath = new List<SimpleTreeNode<Vector2>>();
                    for (int i = 0; i <= lastPlannedPath.IndexOf(lastSafeWaypoint); i++)
                    {
                        currentPath.Add(lastPlannedPath[i]);
                    }

                    //Remove cleared waypoints from active list
                    waypointsToRemove.Clear();
                    for (int i = 0; i < safeWaypoints; i++)
                    {
                        waypointsToRemove.Add(activeWaypoints[i]);
                    }
                    foreach (SimpleTreeNode<Vector2> waypoint in waypointsToRemove)
                    {
                        activeWaypoints.Remove(waypoint);
                    }

                    //plan a path from the last safe waypoint to the end of the active waypoints list
                    //and add it to the current path. Note that noew the active waypoints list starts
                    //at the first waypoint after the last safe one.
                    newPath = FindPath(lastSafeWaypoint, activeWaypoints);
                    if (newPath != null)
                    {
                        currentPath.AddRange(newPath);
                    }

                    //remove the node at the rover's current location and update the last planned path
                    currentPath.RemoveAt(0);
                    UpdateLastPath(currentPath);
                }
            }
        }

        /// <summary>
        /// Plan a path starting at a specified node through a list of waypoint nodes
        /// </summary>
        /// <returns>A list of nodes that includes the last waypoint reached but does not include the initial node.
        /// Returs null if the first waypoint is not reachable after a fixed number of iterations.</returns>
        private List<SimpleTreeNode<Vector2>> FindPath(SimpleTreeNode<Vector2> startNode, List<SimpleTreeNode<Vector2>> waypointList)
        {
            //Declare local variables
            int pointCounter;
            int iterationCounter;
            bool foundPath;
            bool pathFinderFailed;
            List<SimpleTreeNode<Vector2>> path = new List<SimpleTreeNode<Vector2>>();

            //If there are no active waypoints, return null
            if (waypointList.Count == 0) return null;

            //Find a path between the startNode and the first waypoint
            pointCounter = 20;
            iterationCounter = 0;
            foundPath = false;
            nodeList = new List<SimpleTreeNode<Vector2>>();
            nodeList.Add(startNode); //we have to add the startNode so that the nearest neighbor search can initialise. Remove this node later.
            while (!foundPath)
            {
                //Every 20 iterations replace the random point with the first waypoint to bias the search
                if (pointCounter < 20)
                {
                    Vector2 randomPoint = new Vector2((double)rand.Next(minX, maxX), (double)rand.Next(minY, maxY));
                    foundPath = Extend(randomPoint, waypointList[0], nodeList);
                    pointCounter++;
                    iterationCounter++;
                }
                else
                {
                    foundPath = Extend(waypointList[0].Value, waypointList[0], nodeList);
                    pointCounter = 0;
                    iterationCounter++;
                }
                //Stop if it takes too long to plan a path to the first waypoint
                if (iterationCounter > 1000) return null;
            }
            //build the path between the startNode and the first waypoint and add it to the output
            path.AddRange(BuildPath(nodeList));

            //If there is more tha one waypoint, repeat this process for all remaining waypoints
            if (activeWaypoints.Count > 1)
            {
                for (int i = 0; i < activeWaypoints.Count - 1; i++)
                {
                    pointCounter = 20;
                    iterationCounter = 0;
                    foundPath = false;
                    pathFinderFailed = false;
                    nodeList.Clear();
                    nodeList.Add(activeWaypoints[i]);
                    while (!foundPath)
                    {
                        //Every 20 iterations replace the random point with the first waypoint to bias the search
                        if (pointCounter < 20)
                        {
                            Vector2 randomPoint = new Vector2((double)rand.Next(minX, maxX), (double)rand.Next(minY, maxY));
                            foundPath = Extend(randomPoint, waypointList[i + 1], nodeList);
                            pointCounter++;
                            iterationCounter++;
                        }
                        else
                        {
                            foundPath = Extend(waypointList[i + 1].Value, waypointList[i + 1], nodeList);
                            pointCounter = 0;
                            iterationCounter++;
                        }
                        //Stop if it takes too long to plan a path to the waypoint
                        if (iterationCounter > 1000)
                        {
                            pathFinderFailed = true;
                            break;
                        }
                    }
                    if (pathFinderFailed) break; //Stop the for loop if no path ca be found between waypoints
                    //build the path between the waypoints and add it to the output
                    path.AddRange(BuildPath(nodeList));
                }
            }
            return path;
        }

        /// <summary>
        /// Grow tree towards a specified point
        /// </summary>
        private bool Extend(Vector2 extendPoint, SimpleTreeNode<Vector2> goalNode, List<SimpleTreeNode<Vector2>> nodeList)
        {
            bool foundPath = false;
            double distance;
            SimpleTreeNode<Vector2> newNode = new SimpleTreeNode<Vector2>();
            SimpleTreeNode<Vector2> neighbor = FindNearestNeighbor(extendPoint, out distance, nodeList);
            if (distance > stepSize)
            {
                newNode.Value = neighbor.Value + stepSize * (extendPoint - neighbor.Value) / distance;
            }
            else
            {
                newNode.Value = extendPoint;
            }
            if (CheckObstacles(newNode, neighbor))
            {
                neighbor.Children.Add(newNode);
                nodeList.Add(newNode);
                if (newNode.Value == goalNode.Value)
                {
                    foundPath = true;
                }
            }
            return foundPath;
        }

        /// <summary>
        /// Find the nearest neighbor on the tree for a new point by checking every node on the tree in turn
        /// </summary>
        private SimpleTreeNode<Vector2> FindNearestNeighbor(Vector2 point, out double distance, List<SimpleTreeNode<Vector2>> nodeList)
        {
            double newDistance;
            distance = double.PositiveInfinity;
            SimpleTreeNode<Vector2> nearestNode = new SimpleTreeNode<Vector2>();
            foreach (SimpleTreeNode<Vector2> node in nodeList)
            {
                newDistance = (node.Value - point).Length;
                if (newDistance < distance)
                {
                    distance = newDistance;
                    nearestNode = node;
                }
            }
            return nearestNode;
        }

        /// <summary>
        /// Check obstacle avoidance by intersecting the line segment between two nodes
        /// </summary>
        private bool CheckObstacles(SimpleTreeNode<Vector2> nodeOne, SimpleTreeNode<Vector2> nodeTwo)
        {
            bool isSafe = true;
            LineSegment ls = new LineSegment(nodeOne.Value, nodeTwo.Value);
            foreach (Polygon poly in bloatedObstacles)
            {
                if (poly.DoesIntersect(ls))
                {
                    isSafe = false;
                    break;
                }
            }
            return isSafe;
        }

        /// <summary>
        /// Construct a path by working backwards from the last node until you reach the first node in the list
        /// </summary>
        private List<SimpleTreeNode<Vector2>> BuildPath(List<SimpleTreeNode<Vector2>> nodeList)
        {
            List<SimpleTreeNode<Vector2>> newPath = new List<SimpleTreeNode<Vector2>>();
            SimpleTreeNode<Vector2> endNode = nodeList[nodeList.Count - 1];
            SimpleTreeNode<Vector2> node = endNode;
            while (node != nodeList[0])
            {
                newPath.Insert(0, node);
                node = node.Parent;
            }
            return newPath;
        }


        /// <summary>
        /// Replace the last path planned with a new path
        /// </summary>
        private void UpdateLastPath(List<SimpleTreeNode<Vector2>> newPath)
        {
            lock (pathLock)
            {
                outputNodeList.Clear();
                foreach (SimpleTreeNode<Vector2> node in newPath)
                {
                    outputNodeList.Add(node);
                }
            }
        }

        #region IPathPlanner Members

        /// <summary>
        ///  Output the last planned path
        /// </summary>
        public IPath GetPathToGoal()
        {
            lock (pathLock)
            {
                PointPath newPath = new PointPath();
                if (outputNodeList.Count == 1)
                {
                    //If there is only one ode in the planned path, return an IPath with a degenerate segment
                    newPath.Add(new LinePathSegment(outputNodeList[0].Value, outputNodeList[0].Value));
                }
                else if (outputNodeList.Count > 1)
                {
                    //Build an IPath based on the values of each node in the last planned path
                    int segmentCount = outputNodeList.Count - 1;
                    for (int i = 0; i < segmentCount; i++)
                    {
                        newPath.Add(new LinePathSegment(outputNodeList[i].Value, outputNodeList[i + 1].Value));
                    }
                }
                return newPath;
            }
        }

        /// <summary>
        /// Update Obstacle Map with a new list of polygons
        /// </summary>
        public void UpdateObstacles(List<Polygon> obstacles)
        {
            lock (plannerLock)
            {
                this.obstacles = new List<Polygon>();
                this.bloatedObstacles = new List<Polygon>();
                Polygon safeVehiclePolygon = Polygon.VehiclePolygonWithRadius(safeVehicleRadius);
                foreach (Polygon poly in obstacles)
                {
                    this.obstacles.Add(poly);
                    Polygon convolutedPoly = Polygon.ConvexMinkowskiConvolution(poly, safeVehiclePolygon);
                    this.bloatedObstacles.Add(convolutedPoly);
                }
            }
        }

        /// <summary>
        /// Update rover position and remove nodes from waypoints and planned path if the rover approaches them
        /// </summary>
        /// <param name="pose"></param>
        public void UpdatePose(RobotPose pose)
        {
            lock (plannerLock)
            {
                List<SimpleTreeNode<Vector2>> nodesToRemove = new List<SimpleTreeNode<Vector2>>();
                currentLocation = pose.ToVector2();
                //Remove waypoints from the list as the rover nears them
                nodesToRemove.Clear();
                foreach (SimpleTreeNode<Vector2> node in userWaypoints)
                {
                    if (currentLocation.DistanceTo(node.Value) < wayPointRadius)
                    {
                        nodesToRemove.Add(node);
                    }
                    else break;
                }
                foreach (SimpleTreeNode<Vector2> node in nodesToRemove)
                {
                    userWaypoints.Remove(node);
                }

                //Remove nodes from the planned path as the rover approaches them
                lock (pathLock)
                {
                    nodesToRemove.Clear();
                    foreach (SimpleTreeNode<Vector2> node in outputNodeList)
                    {
                        if (currentLocation.DistanceTo(node.Value) < pathPointRadius)
                        {
                            nodesToRemove.Add(node);
                        }
                        else break;
                    }
                    foreach (SimpleTreeNode<Vector2> node in nodesToRemove)
                    {
                        outputNodeList.Remove(node);
                    }
                }
            }
        }

        /// <summary>
        /// Update list of points to reach
        /// </summary>
        public bool UpdateWaypoints(IPath waypoints)
        {
            lock (plannerLock)
            {
                //If the new user path is the same as the old one, ignore it
                if (!PathUtils.CheckPathsEqual(waypoints, userDefinedPath))
                {
                    userDefinedPath.Clear();
                    activeWaypoints.Clear();
                    userWaypoints.Clear();
                    //Add the beginning of the path as a new node
                    userWaypoints.Add(new SimpleTreeNode<Vector2>(waypoints.StartPoint.pt));
                    foreach (IPathSegment segment in waypoints)
                    {
                        userDefinedPath.Add(segment);
                        //If the segment has the same start and endpoints ignore it, this deals with the case where
                        //the HRI outputs a single segment with identical start and endpoints.
                        if (segment.StartPoint.pt != segment.EndPoint.pt)
                        {
                            userWaypoints.Add(new SimpleTreeNode<Vector2>(segment.EndPoint.pt));
                        }
                    }
                    //If there is a new user path, then the last planned path is now invalid and should be reset to empty
                    //until a new planning loop is finished
                    UpdateLastPath(new List<SimpleTreeNode<Vector2>>());
                }
            }
            return false;
        }

				public bool UpdateWaypoints(List<Vector2> points)
				{
					return false;
				}

        /// <summary>
        /// Stop running the planner
        /// </summary>
        public void Dispose()
        {
            running = false;
        }

        #endregion


    }
}
