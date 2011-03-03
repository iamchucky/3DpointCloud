using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Network;
using Magic.Common.Messages;

namespace ProxyComponents
{
	static class Messaging
	{
		public static INetworkAddressProvider addressProvider;
		public static GenericMulticastServer<LidarScanMessage> lidarScanServer;

		static bool isStarted = false;
		public static int robotID;
		public static bool IsStarted
		{
			get { return isStarted; }
		}

		static Messaging()
		{
			addressProvider = new HardcodedAddressProvider();
			robotID = NetworkAddress.GetRobotIDByHostname();
			lidarScanServer = new GenericMulticastServer<LidarScanMessage>(addressProvider.GetAddressByName("LidarScan"), new CSharpMulticastSerializer<LidarScanMessage>(true));
		}

		public static void Start()
		{
			lidarScanServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wireless));
		}
	}
}
