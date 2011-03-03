using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Network;
using Magic.Common.Messages;
using Magic.Common;
using Magic.Common.Sensors;
using System.Threading;
using Magic.OccupancyGrid;
using Magic.Sensor.GaussianMixtureMapping;
using Magic.Common.Shapes;
using System.Diagnostics;
using Magic.Common.Sim;
using Magic.Common.Mapack;

namespace Magic.Sensor.GlobalGaussianMixMap
{
	public class GlobalGaussianMixMapQ : IDisposable
	{
		INetworkAddressProvider addressProvider;
		GenericMulticastServer<GlobalMapCell> globalGaussianMixMapServer;
		GenericMulticastClient<LocalMapRequestMessage> localMapRequestClient;
		GenericMulticastServer<UpdateMapDataMessage> localMapResponseServer;

		// global occupancy grid updated by every robot
		OccupancyGrid2D globalOcGrid;
		// gaussian mix map algorithm for global occupancy grid
		GaussianMixMappingQ gaussianMixMapAlgorithm;
		// global occupancy grid updated by only one robot
		Dictionary<int, OccupancyGrid2D> globalOcGridByEachRobot;
		// gaussian mix map algorithm for each global map (updated by only one robot)
		Dictionary<int, GaussianMixMappingQ> globalOcGridByEachRobotAlgorithm;
		SensorPose laserToRobot;

		Dictionary<int, PoseFilterState> robotIDToPose;
		Dictionary<int, ILidarScan<ILidar2DPoint>> robotIDToScan;
		Dictionary<int, SensorPose> robotIDToSensorPose;
		Dictionary<int, List<Polygon>> robotIDToDynamicObstacles;
		Dictionary<int, double> robotIDToTimestamp;
		Dictionary<int, double> robotIDToPastTimestamp;
		Dictionary<int, Polygon> otherRobotPolygon;

		bool isRunning = true;
		int sendCounter = 0;
		Object locker = new Object();
		public GlobalGaussianMixMapQ(OccupancyGrid2D globalOcGrid, GenericMulticastClient<LidarFilterPackageMessage> lidarFilterClient)
		{
			addressProvider = new HardcodedAddressProvider();
			//lidarFilterPackageClient = new GenericMulticastClient<LidarFilterPackageMessage>(addressProvider.GetAddressByName("LidarFilterPackage"), new CSharpMulticastSerializer<LidarFilterPackageMessage>(true));
			
			globalGaussianMixMapServer = new GenericMulticastServer<GlobalMapCell>(addressProvider.GetAddressByName("GlobalGaussianMixMap"), new CSharpMulticastSerializer<GlobalMapCell>(true));
			globalGaussianMixMapServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));

			robotIDToPose = new Dictionary<int, PoseFilterState>();
			robotIDToScan = new Dictionary<int, ILidarScan<ILidar2DPoint>>();
			robotIDToSensorPose = new Dictionary<int, SensorPose>();
			robotIDToTimestamp = new Dictionary<int, double>();
			robotIDToPastTimestamp = new Dictionary<int, double>();
			globalOcGridByEachRobot = new Dictionary<int, OccupancyGrid2D>();
			robotIDToDynamicObstacles = new Dictionary<int, List<Polygon>>();
			otherRobotPolygon = new Dictionary<int, Polygon>();

			this.globalOcGrid = new OccupancyGrid2D(globalOcGrid);
			laserToRobot = new SensorPose(0, 0, 0.5, 0, 0 / 180.0, 0, 0);
			gaussianMixMapAlgorithm = new GaussianMixMappingQ(globalOcGrid, laserToRobot);
			globalOcGridByEachRobotAlgorithm = new Dictionary<int, GaussianMixMappingQ>();

			//lidarFilterPackageClient.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
			lidarFilterClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<LidarFilterPackageMessage>>(lidarFilterPackageClient_MsgReceived);

			// local map request from robots
			localMapRequestClient = new GenericMulticastClient<LocalMapRequestMessage>(addressProvider.GetAddressByName("LocalMapRequest"), new CSharpMulticastSerializer<LocalMapRequestMessage>(true));
			localMapRequestClient.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
			localMapRequestClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<LocalMapRequestMessage>>(localMapRequestClient_MsgReceived);

			// local map update sender
			localMapResponseServer = new GenericMulticastServer<UpdateMapDataMessage>(addressProvider.GetAddressByName("LocalMapResponse"), new CSharpMulticastSerializer<UpdateMapDataMessage>(true));
			localMapResponseServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));

			Thread t = new Thread(new ParameterizedThreadStart(UpdateGlobalMap));
			t.Start();
			//Thread t2 = new Thread(new ParameterizedThreadStart(SendGlobalUpdate));
			//t2.Start();
		}

		void localMapRequestClient_MsgReceived(object sender, MsgReceivedEventArgs<LocalMapRequestMessage> e)
		{
			if (e.message == null)
				return;
			UpdateMapDataMessage mapToSend = Diff(e.message.RobotID, gaussianMixMapAlgorithm.UhatGM, e.message.CurrentPose, e.message.ExtentX, e.message.ExtentY);
			if (mapToSend != null)
			{
				int segmentLength = 2000;
				int numIteration = mapToSend.CellData.Count / segmentLength;
				int numLeftOver = mapToSend.CellData.Count - (numIteration * segmentLength);
				int lastIndex = 0;
				for (int i = 0; i < numIteration + 1; i++)
				{
					if (i == numIteration)
					{
						localMapResponseServer.SendUnreliably(new UpdateMapDataMessage(mapToSend.CellData.GetRange(lastIndex, numLeftOver)));
					}
					else
					{
						localMapResponseServer.SendUnreliably(new UpdateMapDataMessage(mapToSend.CellData.GetRange(lastIndex, segmentLength)));
						lastIndex += segmentLength;
					}
				}
			}
		}

		void lidarFilterPackageClient_MsgReceived(object sender, MsgReceivedEventArgs<LidarFilterPackageMessage> e)
		{
			// measurement rejection for unreasonable covariance matrix.
            //if (e.message.Pose.Covariance.Determinant <= 0)
            //{
            //    Console.WriteLine("GlobalGaussianMixMapQ.cs: Invalid Covariance..!! Ignore.");
            //    return;
            //}
			// measurement update
			lock (locker)
			{
				if (e.message.Pose != null && e.message.LidarScan != null && e.message.SensorPose != null)
				{
					if (!robotIDToSensorPose.ContainsKey(e.message.RobotID))
					{
						robotIDToPose.Add(e.message.RobotID, e.message.Pose);
						//gaussianMixMapAlgorithm.UpdateCovariance(e.message.Pose.covariance);
						robotIDToSensorPose.Add(e.message.RobotID, e.message.SensorPose);
						robotIDToScan.Add(e.message.RobotID, e.message.LidarScan);
						robotIDToTimestamp.Add(e.message.RobotID, e.message.LidarScan.Timestamp);
						robotIDToPastTimestamp.Add(e.message.RobotID, 0);

						// each robot's global OCgrid
						globalOcGridByEachRobot.Add(e.message.RobotID, new OccupancyGrid2D(globalOcGrid));
						globalOcGridByEachRobotAlgorithm.Add(e.message.RobotID, new GaussianMixMappingQ(globalOcGridByEachRobot[e.message.RobotID], e.message.SensorPose, 4));
					}
					robotIDToSensorPose[e.message.RobotID] = e.message.SensorPose;
					robotIDToPose[e.message.RobotID] = e.message.Pose;
					robotIDToScan[e.message.RobotID] = e.message.LidarScan;
					robotIDToTimestamp[e.message.RobotID] = e.message.Pose.timestamp;
					//gaussianMixMapAlgorithm.UpdateCovariance(e.message.Pose.covariance);
				}
				if (!otherRobotPolygon.ContainsKey(e.message.RobotID))
					otherRobotPolygon.Add(e.message.RobotID, Polygon.VehiclePolygonWithRadius(0.5, e.message.Pose.ToVector2()));
				else
					otherRobotPolygon[e.message.RobotID] = Polygon.VehiclePolygonWithRadius(0.5, e.message.Pose.ToVector2());
			}
		}

		public void UpdateDynamicObstacles(int robotID, List<Polygon> obstacles)
		{
			if (!robotIDToDynamicObstacles.ContainsKey(robotID))
			{
				robotIDToDynamicObstacles.Add(robotID, obstacles);
			}
			robotIDToDynamicObstacles[robotID] = obstacles;
		}

		void UpdateGlobalMap(Object o)
		{
			while (isRunning)
			{
				int minRobotNum = Math.Min(robotIDToPose.Count, robotIDToScan.Count);
				if (minRobotNum != robotIDToPose.Count)
				{
					List<int> abc = robotIDToScan.Keys.ToList<int>();
					Console.WriteLine("ERROR: The number of Pose and LidarScan is different");
					//Console.WriteLine("Pose Available for: " +  + " and Lidar Available for: " + robotIDToScan.Keys);
				}
				lock (locker)
				{
					foreach (KeyValuePair<int, PoseFilterState> pair in robotIDToPose)
					{
						if (robotIDToScan.ContainsKey(pair.Key))
						{
							if (!robotIDToDynamicObstacles.ContainsKey(pair.Key))
								robotIDToDynamicObstacles.Add(pair.Key, new List<Polygon>());
							List<Polygon> obstacles = otherRobotPolygon.Values.ToList<Polygon>();
							obstacles.AddRange(robotIDToDynamicObstacles[pair.Key]);
							// check if the data is updated - if not ignore
							if (robotIDToTimestamp[pair.Key] != robotIDToPastTimestamp[pair.Key])
							{
								gaussianMixMapAlgorithm.UpdateOccupancyGrid(robotIDToScan[pair.Key], pair.Key, robotIDToScan[pair.Key].ScannerID, pair.Value, robotIDToSensorPose[pair.Key], obstacles);
								globalOcGridByEachRobotAlgorithm[pair.Key].UpdateOccupancyGrid(robotIDToScan[pair.Key], pair.Key, robotIDToScan[pair.Key].ScannerID, pair.Value, robotIDToSensorPose[pair.Key], robotIDToDynamicObstacles[pair.Key]);
								robotIDToPastTimestamp[pair.Key] = robotIDToTimestamp[pair.Key];
							}
						}
					}
				}
				if (sendCounter >= 10)
				{
					SendGlobalUpdate();
					sendCounter = 0;
				}
				Thread.Sleep(10);
				sendCounter++;
			}
		}

		void SendGlobalUpdate()
		{
			//while (isRunning)
			//{
			List<Index> indexList; List<float> heightList; List<float> covList; List<float> pijSumList; List<float> laserHitList;
			gaussianMixMapAlgorithm.GetArraysToSend(out indexList, out heightList, out covList, out pijSumList, out laserHitList);
			if (indexList.Count == 0)
				return;
			int lastIndex = 0;
			int segmentLength = 3000;
			int numIteration = indexList.Count / segmentLength;
			int numLeftOver = indexList.Count - (numIteration * segmentLength);
			Stopwatch sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < numIteration + 1; i++)
			{
				if (i == numIteration)
				{

					globalGaussianMixMapServer.SendUnreliably(new GlobalMapCell(globalOcGrid.ResolutionX, globalOcGrid.ExtentX, indexList.GetRange(lastIndex, numLeftOver),
						heightList.GetRange(lastIndex, numLeftOver), covList.GetRange(lastIndex, numLeftOver), pijSumList.GetRange(lastIndex, numLeftOver), laserHitList.GetRange(lastIndex, numLeftOver)));
				}
				else
				{
					globalGaussianMixMapServer.SendUnreliably(new GlobalMapCell(globalOcGrid.ResolutionX, globalOcGrid.ExtentX, indexList.GetRange(lastIndex, segmentLength),
						heightList.GetRange(lastIndex, segmentLength), covList.GetRange(lastIndex, segmentLength), pijSumList.GetRange(lastIndex, segmentLength), laserHitList.GetRange(lastIndex, segmentLength)));
					lastIndex += segmentLength;
				}
				//Thread.Sleep(100);
			}
			Console.WriteLine("Sending time: " + sw.ElapsedMilliseconds + " ms");

			//}
		}

		#region occupancy grid stuff

		/// <summary>
		/// Get difference of two occupancy map
		/// </summary>
		/// <param name="globalMulti">ocGrid1</param>
		/// <param name="globalSingle">ocGrid2</param>
		/// <param name="currentPose">current position</param>
		/// <param name="extentX">x-length of the comparison</param>
		/// <param name="extentY">y-length of the comparison</param>
		/// <returns>list of index classes - has ocGrid1 value</returns>
		public UpdateMapDataMessage Diff(int robotID, OccupancyGrid2D globalMulti, RobotPose currentPose, double extentX, double extentY)
		{
			if (!globalOcGridByEachRobotAlgorithm.ContainsKey(robotID)) return null;
			OccupancyGrid2D globalSingle = globalOcGridByEachRobotAlgorithm[robotID].UhatGM;
			//OccupancyGrid2D globalSingle = globalOcGridByEachRobot[robotID];
			//List<Index> diffIndexToSend = new List<Index>();
			List<Position> diffPositionToSend = new List<Position>();
			//List<float> heightList = new List<float>();
			//List<float> covList = new List<float>();
			//List<float> pijList = new List<float>();

			List<float> pijSum = new List<float>();
			List<float> puHat = new List<float>();
			List<float> puHatSquare = new List<float>();
			List<float> pSigUhateSquare = new List<float>();

			int numCellXHalf = (int)(extentX / globalMulti.ResolutionX);
			int numCellYHalf = (int)(extentY / globalMulti.ResolutionY);
			int currentCellX, currentCellY;
			globalMulti.GetIndicies(currentPose.x, currentPose.y, out currentCellX, out currentCellY);
			int comparisonCellX, comparisonCellY;
			for (int i = 0; i < numCellYHalf * 2; i++) // [i, j] = [column, row]
			{
				for (int j = 0; j < numCellXHalf * 2; j++)
				{
					comparisonCellX = currentCellX - numCellXHalf + j;
					comparisonCellY = currentCellY - numCellYHalf + i;
					if (globalMulti.GetCellByIdx(comparisonCellX, comparisonCellY) != globalSingle.GetCellByIdx(comparisonCellX, comparisonCellY))
					{
						double x, y; globalMulti.GetReals(comparisonCellX, comparisonCellY, out x, out y);
						diffPositionToSend.Add(new Position((float)x, (float)y));
						//heightList.Add((float)gaussianMixMapAlgorithm.UhatGM.GetCellByIdx(j, i));
						//covList.Add((float)gaussianMixMapAlgorithm.Psig_u_hat_square.GetCellByIdx(j, i));
						//pijList.Add((float)gaussianMixMapAlgorithm.Pij_sum.GetCellByIdx(j, i));
						pijSum.Add((float)gaussianMixMapAlgorithm.Pij_sum.GetCell(x, y));
						puHat.Add((float)gaussianMixMapAlgorithm.Pu_hat.GetCell(x, y));
						puHatSquare.Add((float)gaussianMixMapAlgorithm.Pu_hat_square.GetCell(x, y));
						pSigUhateSquare.Add((float)gaussianMixMapAlgorithm.Psig_u_hat_square.GetCell(x, y));
					}
				}
			}
			return new UpdateMapDataMessage(robotID, diffPositionToSend, pijSum, puHat, puHatSquare, pSigUhateSquare);
		}

		/// <summary>
		/// Return only height map difference
		/// </summary>
		/// <param name="robotID">Robot ID</param>
		/// <param name="globalMulti">global Map to compare</param>
		/// <param name="currentPose">robot's current position</param>
		/// <param name="extentX">extent x</param>
		/// <param name="extentY">extent y</param>
		/// <returns>UpdateMapDataMessage with only height information</returns>
		//public UpdateMapDataMessage HeightDiff(int robotID, OccupancyGrid2D globalMulti, RobotPose currentPose, double extentX, double extentY)
		//{
		//    if (!globalOcGridByEachRobotAlgorithm.ContainsKey(robotID)) return null;
		//    OccupancyGrid2D globalSingle = globalOcGridByEachRobotAlgorithm[robotID].UhatGM;
		//    //List<Index> diffIndexToSend = new List<Index>();
		//    List<Position> diffPositionToSend = new List<Position>();
		//    List<float> heightList = new List<float>();
		//    int numCellXHalf = (int)(extentX / globalMulti.ResolutionX);
		//    int numCellYHalf = (int)(extentY / globalMulti.ResolutionY);
		//    int currentCellX, currentCellY;
		//    globalMulti.GetIndicies(currentPose.x, currentPose.y, out currentCellX, out currentCellY);
		//    int comparisonCellX, comparisonCellY;
		//    for (int i = 0; i < numCellYHalf * 2; i++) // [i, j] = [column, row]
		//    {
		//        for (int j = 0; j < numCellXHalf * 2; j++)
		//        {
		//            comparisonCellX = currentCellX - numCellXHalf + j;
		//            comparisonCellY = currentCellY - numCellYHalf + i;
		//            if (comparisonCellX < 0 || comparisonCellX > globalSingle.NumCellX || comparisonCellY < 0 || comparisonCellY > globalSingle.NumCellY)
		//                continue;
		//            if (globalMulti.GetCellByIdx(comparisonCellX, comparisonCellY) != globalSingle.GetCellByIdx(comparisonCellX, comparisonCellY))
		//            {
		//                double x, y; globalMulti.GetReals(comparisonCellX, comparisonCellY, out x, out y);
		//                diffPositionToSend.Add(new Position((float)x, (float)y));
		//                heightList.Add((float)gaussianMixMapAlgorithm.UhatGM.GetCellByIdx(comparisonCellX, comparisonCellY));
		//            }
		//        }
		//    }
		//    return new UpdateMapDataMessage(robotID, diffPositionToSend, heightList);
		//}


		/// <summary>
		/// Converts GlobalMapCell data into a occupancy grid for global map
		/// </summary>
		/// <param name="globalHeightMap">returned global height map</param>
		/// <param name="globalSigMap">returned global covariance map</param>
		/// <param name="globalPijMap">returned laser hitting (kinda) map</param>
		/// <param name="globalMapDataList">incoming message from network</param>
		public static void ConvertToOcGrid(ref OccupancyGrid2D globalHeightMap, ref OccupancyGrid2D globalSigMap, ref OccupancyGrid2D globalPijMap, ref OccupancyGrid2D laserHitMap, List<GlobalMapCell.GlobalMapCellData> globalMapDataList)
		{
			foreach (GlobalMapCell.GlobalMapCellData cell in globalMapDataList)
			{
				if (cell.Equals(globalMapDataList[0]))
					continue;
				int col = cell.colIdx; int row = cell.rowIdx;
				globalHeightMap.SetCellByIdx(col, row, cell.largeU);
				globalSigMap.SetCellByIdx(col, row, cell.largeSig);
				globalPijMap.SetCellByIdx(col, row, cell.pij);
				laserHitMap.SetCellByIdx(col, row, cell.laserHit);
			}
		}

		#endregion
		#region IDisposable Members

		public void Dispose()
		{
			isRunning = false;
		}

		#endregion
	}
}
