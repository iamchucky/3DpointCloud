using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Net;
using Magic.Common.Sensors;
using Magic.Network;

namespace Magic.Common.Sim
{
	public class SimLidar : ILidar2D
	{
		private GenericMulticastClient<SimMessage<ILidarScan<ILidar2DPoint>>> simLidarClient;
		private SensorPose toBodyTransform;
		private int robotID;

		public SimLidar(SensorPose toBodyTransform, int id)
		{
			INetworkAddressProvider addressProvider = new HardcodedAddressProvider();
			simLidarClient = new GenericMulticastClient<SimMessage<ILidarScan<ILidar2DPoint>>>(addressProvider.GetAddressByName("SimLidar"), new CSharpMulticastSerializer<SimMessage<ILidarScan<ILidar2DPoint>>>(true));
			simLidarClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<SimMessage<ILidarScan<ILidar2DPoint>>>>(simLidarClient_MsgReceived);
			this.toBodyTransform = toBodyTransform;
			robotID = id;
		}

		void simLidarClient_MsgReceived(object sender, MsgReceivedEventArgs<SimMessage<ILidarScan<ILidar2DPoint>>> e)
		{
			if (e.message.RobotID == robotID)
			{
				ScanReceived.Invoke(sender, new ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>(e.message.Message));
			}
		}

		#region ILidar2D Members

		public event EventHandler<ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>> ScanReceived;

		#endregion

		#region ISensor Members

		public void Start()
		{
			simLidarClient.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
		}

		public void Start(System.Net.IPAddress localBind)
		{
			simLidarClient.Start(localBind);
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public SensorPose ToBody
		{
			get { return toBodyTransform; }
		}

		#endregion
	}
}
