using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Messages;
using Magic.Network;
using Magic.OccupancyGrid;
using Magic.Common.Shapes;

namespace Magic.Sensors.GlobalObstaclesMap
{
	public class GlobalObstaclesMap
	{
		// classes & variables
		OccupancyGrid2D ocGrid, ocGrid4Poly;
		OccupancyGrid2DLogOdds ocGridLogOdd;
		Sensors.OcGrid2Poly.OcGrid2Poly ocGrid2Poly;
		
		// Network stuff
		INetworkAddressProvider addressProvider;
		GenericMulticastServer<GlobalObstaclesMessage> globalObstacleServer;

		public GlobalObstaclesMap(GenericMulticastClient<LidarFilterPackageMessage> filterLidarClient, double extent, double resolution)
		{
			this.ocGrid = new OccupancyGrid2D(resolution, resolution, extent, extent);
			ocGridLogOdd = new OccupancyGrid2DLogOdds(this.ocGrid);
			ocGrid4Poly = new OccupancyGrid2D(this.ocGrid);
			ocGrid2Poly = new Sensors.OcGrid2Poly.OcGrid2Poly(this.ocGrid4Poly, 0.1);
			
			// network stuff
			addressProvider = new HardcodedAddressProvider();
			globalObstacleServer = new GenericMulticastServer<GlobalObstaclesMessage>(addressProvider.GetAddressByName("GlobalObstacles"), new CSharpMulticastSerializer<GlobalObstaclesMessage>(true));
			globalObstacleServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));

			filterLidarClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<LidarFilterPackageMessage>>(filterLidarClient_MsgReceived);
			ocGridLogOdd.NewGridAvailable += new EventHandler<Magic.Common.Sensors.NewOccupancyGrid2DAvailableEventArgs>(ocGridLogOdd_NewGridAvailable);
		}

		void ocGridLogOdd_NewGridAvailable(object sender, Magic.Common.Sensors.NewOccupancyGrid2DAvailableEventArgs e)
		{
			ocGrid2Poly.UpdateOccupancyGrid(e.OccupancyGrid);
			List<Polygon> polyList = ocGrid2Poly.FindPolygons();
			globalObstacleServer.SendUnreliably(new GlobalObstaclesMessage(polyList));
		}

		void filterLidarClient_MsgReceived(object sender, MsgReceivedEventArgs<LidarFilterPackageMessage> e)
		{
			// only update with SICK lidar, which has ID = 1
			if (e.message.LidarScan.ScannerID != 1)
				return;
			// Update
			ocGridLogOdd.SetRobotPose(e.message.Pose.ToRobotPose());
			ocGridLogOdd.SetLidarPose(e.message.SensorPose);
			ocGridLogOdd.UpdateLidarScan(e.message.LidarScan);
		}
	}
}
