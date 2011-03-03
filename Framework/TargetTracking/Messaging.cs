using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Network;
using Magic.Common.Messages;
using Magic.Common;
using Magic.Common.Mapack;

namespace Magic.TargetTracking
{
	public class Messaging
	{
		INetworkAddressProvider addressProvider;
		
		public GenericMulticastClient<ClearTargetListMessage> clearTargetClient;
		
		public Messaging()
		{
			addressProvider = new HardcodedAddressProvider();
//			targetServer = new GenericMulticastServer<TargetMessage>(addressProvider.GetAddressByName("TrackedTarget"), new CSharpMulticastSerializer<TargetMessage>(true));
			clearTargetClient = new GenericMulticastClient<ClearTargetListMessage>(addressProvider.GetAddressByName("ClearTarget"), new CSharpMulticastSerializer<ClearTargetListMessage>(true));
//			targetServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wireless));
			clearTargetClient.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wireless));
		}

		public void SendTargets(int robotID, List<RobotPose> poseList, List<Matrix> covList, List<RobotPose> lastRobotPose, List<TargetTypes> types)
		{
//			targetServer.SendUnreliably(new TargetMessage(robotID, poseList, covList, lastRobotPose, types));
		}
	}
}
