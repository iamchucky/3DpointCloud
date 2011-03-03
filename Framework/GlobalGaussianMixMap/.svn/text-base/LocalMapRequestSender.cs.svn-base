using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Network;
using Magic.Common;

namespace Magic.Sensor.GlobalGaussianMixMap
{
	public class LocalMapRequestSender
	{
		GenericMulticastServer<LocalMapRequestMessage> localMapRequestServer;
		INetworkAddressProvider addressProvider;
		int robotID;

		public LocalMapRequestSender(int robotID)
		{
			this.robotID = robotID;
			addressProvider = new HardcodedAddressProvider();
			localMapRequestServer = new GenericMulticastServer<LocalMapRequestMessage>(addressProvider.GetAddressByName("LocalMapRequest"), new CSharpMulticastSerializer<LocalMapRequestMessage>(true));
			localMapRequestServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wireless));
		}

		public void SendLocalMapRequest(RobotPose currentPose, double extentX, double extentY)
		{
			localMapRequestServer.SendUnreliably(new LocalMapRequestMessage(robotID, currentPose, extentX, extentY));
		}
	}
}
