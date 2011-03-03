using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.DataStructures;
using Magic.Common.Shapes;
using Magic.Common.Robots;
using Magic.Common.Path;
using System.Diagnostics;
using KDTreeDLL;



namespace Magic.PathPlanning
{
	public class RRTPlannerControl
	{
		// distance around the robot position for sampling a random point
		public double randomSampleRadius = 10.0;
		// flag to find a path

		// velocity and turn rate sigma for Gaussian Distribution (Normal distribution)
		public double vSigma = 3.0;
		public double wSigma = 100;
		public double timeStep = 0.5;
		//1 is weighting towards previous input, 0 is weighitng towards goal
		public double mixingProportion = 0.3;
		public double chanceToSampleRoot = .3;
		int goalPointSamplingRate = 5;

		public int terminationCount = 15000;

		public double MAX_VEL = 3;
		public double MIN_VEL = .1;

		public double MAX_TURN = 10000;
		public double MIN_TURN = -10000;

		int numSlice = 2;
		double goalTolerance = 0.5;

		//double kPwSample = .050;

		KDTree kdTree;



		int numNodesToExtend = 3;

		Random rand = new Random();

		public bool FindPath(ref RRTNode originalRoot, Vector2 goalPoint, List<Polygon> obstacles, out RRTNode goal)
		{
            RRTNode root = new RRTNode(originalRoot.State);
			double distance = goalPoint.DistanceTo(root.State.Pose.ToVector2());
			randomSampleRadius = distance + 15.0;
            timeStep = rand.NextDouble();
            //vSigma = rand.NextDouble() * 5.0;
            numSlice = (int)Math.Round(timeStep / 0.2);
			//if (distance < 2)
			//  timeStep = 0.3;
			//else if (distance > 4)
			//  timeStep = 1.0;
			//else
			//  timeStep = 0.5;

			//numSlice = (int)Math.Round(timeStep / 0.1);
			Stopwatch randomGenerationTimer = new Stopwatch();
			Stopwatch extendingTimer = new Stopwatch();
			Stopwatch closestSearchTimer = new Stopwatch();
			List<Double> randomTime = new List<double>();
			List<Double> extendingTime = new List<double>();
			List<Double> closestSearchTime = new List<double>();
			bool foundPath = false;
			goal = null; //not found yet!
			//RRT is divided into the following steps:
			//0) assume the root node is the first node
			//1) randomly select a sample point in space centered around our robot within some fixed distance. Every 20th can be the goal.
			//2) select the closest node to the sampled point in the existing tree based on xy distance
			//3) generate a control input that drives towards the sample point also biased with our initial control inputs
			//3a)   -Biasing Details:
			//		Select Velocity: Normal Distribution with mean = closest node velocity and sigma = SigmaVelocity
			//		Select Turn Rate: 
			//			Apply the following heuristic:  mean = (atan2(yf-yi,xf-xi) - thetaInit)/(delT)
			//																			sigma = SigmaTurnRate
			//4) Divide the total RRT timestep into smaller sub-steps
			//4a) calculate the trajectory at one substep given the control inputs and closest node initial conditions
			//4b) check at each the linear path between the initial and simulation end does not intersect a polygon
			//4c) if intersects, terminate and go to 1.
			//4d) if not intersects
			//4da) if last substep, add the results of this simulation to the closest node as a child
			//4db) else simulate the next subtime step by going to 4a
			//5) Check if the new node added is within some tolerance of the goal node. If so, mark node as goal and you're done! Else, Goto 1.	

			//----------------------------------------------------------------------------------------------------------------------------------//
			// Declare variables
			int sampleCount = 0; // counter for sample to be biased every 20th time
			int iterationCount = 0; // counter for termination
			kdTree = new KDTree(2);
			kdTree.insert(RRTNode.ToKey(root.State.Pose.x, root.State.Pose.y), root);
			// 0) assume root note is the first node
			while (!foundPath)
			//for (int i = 0; i < 1000; i++)
			{
				//--- Termination ---//
				iterationCount++;
				if (iterationCount > terminationCount)
				{

					Console.WriteLine("//-----------------------------------------------------------------------//");
					Console.WriteLine("Random generation average time: " + (randomTime.Sum() / randomTime.Count) + " ms | total time: " + randomTime.Sum() + " | iteration: " + randomTime.Count);
					Console.WriteLine("Searching closest node average time: " + (closestSearchTime.Sum() / closestSearchTime.Count) + " ms | total time: " + closestSearchTime.Sum() + " | iteration: " + closestSearchTime.Count);
					Console.WriteLine("Extending average time: " + (extendingTime.Sum() / extendingTime.Count) + " ms | total time: " + extendingTime.Sum() + " | iteration: " + extendingTime.Count);
					// Benchmark output
					Console.WriteLine("|--Simulation average time: " + (simulationTime.Sum() / simulationTime.Count) + " ms | total time: " + simulationTime.Sum() + " | iteration: " + simulationTime.Count);
					Console.WriteLine("|--Obstacle checking average time: " + (obstacleTime.Sum() / obstacleTime.Count) + " ms | total time: " + obstacleTime.Sum() + " | iteration: " + obstacleTime.Count);
					Console.WriteLine("//-----------------------------------------------------------------------//");
					simulationTime.Clear();
					obstacleTime.Clear();
					return false;
				}
				//-------------------//

				// 1) randomly select a sample point in space centered around our robot within some fixed distance.
				int actualNumNodesToExtend = numNodesToExtend;
				Vector2 samplePoint;
				if (sampleCount < goalPointSamplingRate)
				{
					if (rand.NextDouble() > chanceToSampleRoot)
					{
						randomGenerationTimer.Start();
						//double randomX = root.State.Pose.x + (rand.NextDouble() - .5) * randomSampleRadius * 2.0;
						//double randomY = root.State.Pose.y + (rand.NextDouble() - .5) * randomSampleRadius * 2.0;
						double randomX = goalPoint.X + (rand.NextDouble() - .5) * randomSampleRadius * 2.0;
						double randomY = goalPoint.Y + (rand.NextDouble() - .5) * randomSampleRadius * 2.0;
						samplePoint = new Vector2(randomX, randomY);
						randomTime.Add(randomGenerationTimer.ElapsedMilliseconds);
						randomGenerationTimer.Reset();
					}
					else
					{
						samplePoint = root.Point;
						actualNumNodesToExtend = 1;
					}
					sampleCount++;
				}
				else
				{
					samplePoint = goalPoint;
					sampleCount = 0;
				}
				closestSearchTimer.Start();
				// 2) select the closest node to the sampled point in the existing tree based on xy distance
				//List<RRTNode> closestNodes = root.FindNClosestNodeInTree(samplePoint, actualNumNodesToExtend);
				List<RRTNode> closestNodes = root.FindNClosestNodeInTreeKDTREE(3, samplePoint, kdTree);
				closestSearchTime.Add(closestSearchTimer.ElapsedMilliseconds);
				closestSearchTimer.Reset();

				extendingTimer.Start();
				foreach (RRTNode closeNode in closestNodes)
				{
					RRTNode newNode = ExtendNode(ref goalPoint, obstacles, ref goal, ref foundPath, ref samplePoint, closeNode, rand);
					if (newNode != null) kdTree.insert(RRTNode.ToKey(newNode.State.Pose.x, newNode.State.Pose.y), newNode);
				}
				extendingTime.Add(extendingTimer.ElapsedMilliseconds);
				extendingTimer.Reset();
			}

			Console.WriteLine("//-----------------------------------------------------------------------//");
			Console.WriteLine("Random generation average time: " + (randomTime.Sum() / randomTime.Count) + " ms | total time: " + randomTime.Sum() + " | iteration: " + randomTime.Count);
			Console.WriteLine("Searching closest node average time: " + (closestSearchTime.Sum() / closestSearchTime.Count) + " ms | total time: " + closestSearchTime.Sum() + " | iteration: " + closestSearchTime.Count);
			Console.WriteLine("Extending average time: " + (extendingTime.Sum() / extendingTime.Count) + " ms | total time: " + extendingTime.Sum() + " | iteration: " + extendingTime.Count);
			// Benchmark output
			Console.WriteLine("|--Simulation average time: " + (simulationTime.Sum() / simulationTime.Count) + " ms | total time: " + simulationTime.Sum() + " | iteration: " + simulationTime.Count);
			Console.WriteLine("|--Obstacle checking average time: " + (obstacleTime.Sum() / obstacleTime.Count) + " ms | total time: " + obstacleTime.Sum() + " | iteration: " + obstacleTime.Count);
			Console.WriteLine("//-----------------------------------------------------------------------//");
			simulationTime.Clear();
			obstacleTime.Clear();

			return true;

		}





		/// <summary>
		/// 
		/// </summary>
		/// <param name="goalPoint"></param>
		/// <param name="obstacles"></param>
		/// <param name="goal"></param>
		/// <param name="foundPath"></param>
		/// <param name="samplePoint"></param>
		/// <param name="closestNode"></param>
		/// <returns>true if the node was added to the tree (i.e. didnt hit crap)</returns>
		private RRTNode ExtendNode(ref Vector2 goalPoint, List<Polygon> obstacles, ref RRTNode goal, ref bool foundPath, ref Vector2 samplePoint, RRTNode closestNode, Random rand)
		{
			//3) generate a control input that drives towards the sample point also biased with our initial control inputs
			//3a)   -Biasing Details:
			//		Select Velocity: Normal Distribution with mean = closest node velocity and sigma = SigmaVelocity
			//		Select Turn Rate: 
			//			Apply the following heuristic:  mean = (atan2(yf-yi,xf-xi) - thetaInit)/(delT)
			//																			sigma = SigmaTurnRate

			// velocity distribution

			MathNet.Numerics.Distributions.NormalDistribution vDist = new MathNet.Numerics.Distributions.NormalDistribution(closestNode.State.Command.velocity, vSigma);

			// turn-rate biased
			double mixingSample = rand.NextDouble();
			double wMean = 0;
			if (mixingSample > mixingProportion)
			{
				double angleToClosestNode = Math.Atan2((samplePoint.Y - closestNode.Point.Y), (samplePoint.X - closestNode.Point.X));
				//wMean = -kPwSample * angleToClosestNode;
				wMean = ((angleToClosestNode - closestNode.State.Pose.yaw)) * 180.0 / Math.PI / timeStep;
				if (wMean > MAX_TURN - 20)
					wMean = MAX_TURN - 20;
				if (wMean < MIN_TURN + 20)
					wMean = MIN_TURN + 20;
			}
			else
				wMean = 0;

			MathNet.Numerics.Distributions.NormalDistribution wDist = new MathNet.Numerics.Distributions.NormalDistribution(wMean, wSigma);

			double velSampled = vDist.NextDouble();
			double wSampled = wDist.NextDouble();
			while (velSampled > MAX_VEL || velSampled < MIN_VEL)
				velSampled = vDist.NextDouble();
			while (wSampled > MAX_TURN || wSampled < MIN_TURN)
				wSampled = wDist.NextDouble();

			// 4) Predict a node
			RRTNode predictedNode = CalculateNextNode(closestNode, velSampled, wSampled, obstacles);
			if (predictedNode != null)
			{
				closestNode.AddChild(predictedNode);
				//5) Check if the new node added is within some tolerance of the goal node. If so, mark node as goal and you're done! Else, Goto 1.
				//Polygon goalPolygon = Polygon.VehiclePolygonWithRadius(0.5, goalPoint);
				Circle c = new Circle(.5, goalPoint);
				LineSegment nodeToParent = new LineSegment(predictedNode.Point, predictedNode.Parent.Point);
				Vector2[] pts = new Vector2[2];
				if (c.Intersect(nodeToParent, out pts))
				{
					foundPath = true;
					goal = predictedNode;
				}
				//if (predictedNode.DistanceTo(goalPoint) < goalTolerance)
				//{
				//    foundPath = true;
				//    goal = predictedNode;
				//}
				return predictedNode;
			}
			return null;
		}

		public void SetTimeStep(double tStep)
		{
			this.timeStep = tStep;
		}

		public void SetDistance(double dist)
		{
			this.randomSampleRadius = dist;
		}



		Stopwatch obstacleTimer = new Stopwatch();
		Stopwatch simulationTimer = new Stopwatch();
		List<Double> obstacleTime = new List<double>();
		List<Double> simulationTime = new List<double>();

		/// <summary>
		/// Calculate a point starting from a RRTNode, while avoiding obstacles
		/// </summary>
		/// <param name="startNode">The node that path predicted from</param>
		/// <param name="vInput">Velocity control input</param>
		/// <param name="wInput">Turn Rate control input</param>
		/// <param name="obstacles">Obstalces (List of Polygon)</param>
		/// <returns>a RRTNode that has startNode as its parent. Returns null if fails</returns>
		private RRTNode CalculateNextNode(RRTNode startNode, double vInput, double wInput, List<Polygon> obstacles)
		{

			//4) Divide the total RRT timestep into smaller sub-steps
			//4a) calculate the trajectory at one substep given the control inputs and closest node initial conditions
			//4b) check at each the linear path between the initial and simulation end does not intersect a polygon
			//4c) if intersects, terminate and go to 1.
			//4d) if not intersects
			//4da) if last substep, add the results of this simulation to the closest node as a child
			//4db) else simulate the next subtime step by going to 4a

			RobotTwoWheelState lastSliceState = startNode.State;
			RobotTwoWheelState curSliceState = startNode.State;
			List<Vector2> simulationPoints = new List<Vector2>();
			for (int i = 0; i < numSlice; i++)
			{
				simulationTimer.Start();
				curSliceState = RobotTwoWheelModel.Simulate(new RobotTwoWheelCommand(vInput, wInput), lastSliceState, (timeStep / numSlice));
				simulationTime.Add(simulationTimer.ElapsedMilliseconds);
				simulationTimer.Reset();

				obstacleTimer.Start();
				if (IsHittingObstacle(lastSliceState.Pose.ToVector2(), curSliceState.Pose.ToVector2(), obstacles))
					return null;
				obstacleTime.Add(obstacleTimer.ElapsedMilliseconds);
				obstacleTimer.Reset();
				simulationPoints.Add(lastSliceState.Pose.ToVector2());
				lastSliceState = curSliceState;
			}
			simulationPoints.Add(lastSliceState.Pose.ToVector2());
			RRTNode r = new RRTNode(curSliceState, startNode);
			r.simulationPoints = simulationPoints;
			return r;
			//return new RRTNode(rOut, vOut, wOut, 0, startNode);
		}


		/// <summary>
		/// Check if the line between two points are hitting any obstacle
		/// </summary>
		/// <param name="v1">Point1</param>
		/// <param name="v2">Point2</param>
		/// <param name="obstacles">List of Polygon</param>
		/// <returns>true if hitting, false if not</returns>
		private bool IsHittingObstacle(Vector2 v1, Vector2 v2, List<Polygon> obstacles)
		{
			foreach (Polygon p in obstacles)
			{
				//if (p.IsInside(new LineSegment(v1, v2)))
				if (p.ConvexDoesIntersect(v1, v2))
					return true;
			}
			return false;
		}
	}


	public class RRTNode : IAsPoint, ITreeNode
	{
		RobotTwoWheelState state;
		public List<Vector2> simulationPoints = new List<Vector2>();
		public List<Vector2> SimulationPoints { get { return simulationPoints; } }
		public RobotTwoWheelState State
		{
			get { return state; }
		}

		List<RRTNode> children = new List<RRTNode>();
		RRTNode parent;
		public static double[] ToKey(double x, double y)
		{
			double[] arr = new double[2];
			arr[0] = x;
			arr[1] = y;
			return arr;
		}
		public List<ITreeNode> Children
		{
			get
			{
				//lame
				lock (this)
				{
					List<ITreeNode> ret = new List<ITreeNode>(children.Count);
					foreach (RRTNode n in children) ret.Add(n);
					return ret;
				}
			}
		}

		public ITreeNode Parent
		{
			get { return parent; }
		}

		public bool IsRoot { get { return this.parent == this; } }

		public RRTNode(RobotTwoWheelState state, RRTNode parent)
		{
			this.state = state;
			this.parent = parent;
		}

		public RRTNode(RobotTwoWheelState state)
		{
			this.state = state;
			this.parent = this;
		}

		public void AddChild(RRTNode node)
		{
			lock (this)
			{
				children.Add(node);
			}
		}

		public bool RemoveChild(RRTNode node)
		{
			lock (this)
			{
				if (children.Contains(node))
				{
					children.Remove(node); return true;
				}
			}
			return false;
		}

		public double DistanceTo(Vector2 target)
		{
			return this.State.Pose.ToVector2().DistanceTo(target);
		}

		/// <summary>
		/// Find the closest RRTNode in tree from given Vector2 point
		/// </summary>
		/// <param name="point"></param>
		/// <returns>Closest RRTNode in the tree</returns>
		public RRTNode FindClosestNodeInTree(Vector2 point)
		{
			/*double minDistance = 999999.99;
			RRTNode minNode = null;

			foreach (RRTNode node in children)
			{
				if (minDistance > node.DistanceTo(point))
				{
					minNode = node;
					minDistance = node.DistanceTo(point);
				}
			}

			return minNode;*/

			Dictionary<RRTNode, Double> dictionary = new Dictionary<RRTNode, Double>();
			ReadTreeAndCalculateDistance(this, point, dictionary);
			if (dictionary.Count != 0)
			{
				double minDistance = 999999.99;
				RRTNode minNode = null;
				foreach (KeyValuePair<RRTNode, Double> kvp in dictionary)
				{
					// distance recorded in dictionary
					double comparerDistance = kvp.Value;
					if (minDistance > comparerDistance) // find the minimum node & distance
					{
						minNode = kvp.Key;
						minDistance = comparerDistance;
					}
				}
				return minNode;
			}
			return null;
		}


		/// <summary>
		/// Find the closest RRTNode in tree from given Vector2 point
		/// </summary>
		/// <param name="point"></param>
		/// <returns>Closest RRTNode in the tree</returns>
		public List<RRTNode> FindNClosestNodeInTree(Vector2 point, int n)
		{
			List<RRTNode> ret = new List<RRTNode>();
			Dictionary<RRTNode, Double> dictionary = new Dictionary<RRTNode, Double>();
			ReadTreeAndCalculateDistance(this, point, dictionary);
			IOrderedEnumerable<KeyValuePair<RRTNode, Double>> sorted = dictionary.OrderBy(item => item.Value);  //sick			
			int nBest = 3;
			for (int i = 0; i < Math.Min(n, (int)Math.Ceiling((double)dictionary.Count / nBest)); i++)
				ret.Add(sorted.ElementAt(i * 3).Key);
			return ret;
		}

		public List<RRTNode> FindNClosestNodeInTreeKDTREE(int n, Vector2 target, KDTree kdTree)
		{
			int modulus = 3;
			Object[] objectList = kdTree.nearest(ToKey(target.X, target.Y), Math.Min(n * modulus, (int)Math.Ceiling((double)kdTree.Count)));
			List<RRTNode> listToReturn = new List<RRTNode>();
			for (int i = 0; i < objectList.Length; i += modulus)
			{
				listToReturn.Add(objectList[i] as RRTNode);
			}
			return listToReturn;


		}

		//private void ReadTreeAndSaveList(RRTNode parentNode, KDTree tree)
		//{
		//    try
		//    {
		//        tree.insert(RRTNode.ToKey(parentNode.state.Pose.x, parentNode.state.Pose.y), parentNode);
		//    }
		//    catch { }
		//    foreach (RRTNode n in parentNode.children)
		//    {
		//        ReadTreeAndSaveList(n, tree);
		//    }


		//}



		/// <summary>
		/// Recursively go through every child in the tree, and record [Node, distance between the node and the vector point] into the passed dictionary
		/// </summary>
		/// <param name="parentNode">(RRTNode) parent node </param>
		/// <param name="samplePoint">(Vector2) sampled point</param>
		/// <param name="dictionary">(Dictionary[RRTNode, Double] to insert the child nodes</param>
		private void ReadTreeAndCalculateDistance(RRTNode parentNode, Vector2 samplePoint, Dictionary<RRTNode, Double> dictionary)
		{
			dictionary.Add(parentNode, parentNode.DistanceTo(samplePoint));
			foreach (RRTNode n in parentNode.children)
			{
				ReadTreeAndCalculateDistance(n, samplePoint, dictionary);
			}
		}
		#region IAsPoint Members

		public Vector2 Point
		{
			get { return State.Pose.ToVector2(); }
		}

		#endregion

		public RRTNode GetParent()
		{
			return parent;
		}

	}
}
