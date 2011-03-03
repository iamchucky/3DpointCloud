using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Network;
using Magic.SickInterface;
using Magic.Common;
using Magic.Common.Messages;

namespace ProxyComponents
{
	public class LidarProxy
	{
		ILidar2D lidar;
		INetworkAddressProvider addressProvider;
		ILidarScan<ILidar2DPoint> currentData;
		SensorPose lidarSensorPose;

		public LidarProxy()
		{
			lidarSensorPose = new SensorPose(0, 0, 0.5, 0, 0, 0, 0);
			addressProvider = new HardcodedAddressProvider();
			lidar = new SickLMS(addressProvider.GetAddressByName("Sick"), lidarSensorPose);
			lidar.ScanReceived += new EventHandler<ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>>(lidar_ScanReceived);
		}

		public void Start()
		{
			lidar.Start();
		}

		public void Stop()
		{
			lidar.Stop();
		}

		public void Publish()
		{

		}

		void lidar_ScanReceived(object sender, ILidarScanEventArgs<ILidarScan<ILidar2DPoint>> e)
		{
			currentData = e.Scan;
			Messaging.lidarScanServer.SendUnreliably(new LidarScanMessage(Messaging.robotID, e.Scan));
		}

	}
}
