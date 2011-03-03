using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Network;

namespace Magic.Common.Sim
{
	public class SimIMU : IIMU
	{
		private GenericMulticastClient<SimMessage<IMUData>> simIMUClient;
		private SensorPose toBodyTransform;

		public SimIMU(NetworkAddress na, SensorPose sp)
		{
			simIMUClient = new GenericMulticastClient<SimMessage<IMUData>>(na, new CSharpMulticastSerializer<SimMessage<IMUData>>(false));
			simIMUClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<SimMessage<IMUData>>>(simIMUClient_MsgReceived);
			toBodyTransform = sp;
		}

		void simIMUClient_MsgReceived(object sender, MsgReceivedEventArgs<SimMessage<IMUData>> e)
		{
			e.message.Message.DataType = "IMU";
			if (IMUUpdate != null) IMUUpdate(sender, new TimestampedEventArgs<IMUData>(e.message.Timestamp, e.message.Message));
		}

		#region IIMU Members

		public event EventHandler<TimestampedEventArgs<IMUData>> IMUUpdate;

		public void Reset()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ISensor Members

		public void Start()
		{
			Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
		}

		public void Start(System.Net.IPAddress localBind)
		{
			simIMUClient.Start(localBind);
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
