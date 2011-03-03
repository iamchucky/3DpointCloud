using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Network;
using Magic.Common;
using Magic.SickInterface;

namespace LidarProxy
{
	class LidarProxy
	{
		ILidar2D lidar;
		INetworkAddressProvider addrProvider;
		ILidarScan<ILidar2DPoint> currentData;

		public LidarProxy()
		{
			addrProvider = new HardcodedAddressProvider();
			lidar = new SickLMS(addrProvider.GetAddressByName("Sick"), new SensorPose());
			lidar.ScanReceived += new EventHandler<ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>>(lidar_ScanReceived);
		}

		/// <summary>
		/// Starts the lidar
		/// </summary>
		public void Start()
		{
			lidar.Start();
		}

		/// <summary>
		/// Stops the lidar
		/// </summary>
		public void Stop()
		{
			lidar.Stop();
		}

		/// <summary>
		/// Publishes the most recently received scan to the greater network
		/// </summary>
		public void Publish()
		{
			if (currentData != null)
				Console.WriteLine(currentData.Timestamp);
				//Publish stuff
			return;
		}

		private void lidar_ScanReceived(object sender, ILidarScanEventArgs<ILidarScan<ILidar2DPoint>> e)
		{
			currentData = e.Scan;
		}

		#region Destructor

		~LidarProxy()
		{
			lidar.Stop();
		}

		#endregion
	}
}
