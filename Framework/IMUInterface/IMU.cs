using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using System.Net;
using Magic.Common;
using System.Net.Sockets;
using Magic.Network;
using System.IO;

namespace IMUInterface
{
	public class IMU : IIMU
	{
		private IPAddress ip;
		private Int32 port;
		private byte[] buf;
		private byte[] temp = { 0, 0, 0, 0 };
		private Int32 sum;
		private Socket sock;
		SensorPose toBodyTransform;

		int totalPackets = 0;
		int totalPacketsResetable = 0;
		bool showDebugMessages = false;
		public IMU(NetworkAddress na, SensorPose toBodyTransform)
		{
			this.ip = na.Address; this.port = na.Port;
			this.toBodyTransform = toBodyTransform;
		}

		public void Start()
		{
			BuildSocket(IPAddress.Any);
		}

		public void Start(IPAddress bindAddress)
		{
			BuildSocket(bindAddress);
		}
		public void Stop()
		{
			if (this.sock == null) return; //already stopped
			this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(this.ip));
			this.sock = null;
		}

		public SensorPose ToBody
		{
			get { return toBodyTransform; }
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			if (this.sock == null) return;
			int bytesReceived = this.sock.EndReceive(ar);
			if (bytesReceived > 0)
			{
				MemoryStream ms = new MemoryStream(buf, 0, bytesReceived, false);
				BinaryReader br = new BinaryReader(ms);
				ParsePacket(br, bytesReceived);
				totalPackets++;
				totalPacketsResetable++;
			}

			this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
		}

		private void ParsePacket(BinaryReader br, int bytesReceived)
		{
			UInt16 secs = Endian.BigToLittle(br.ReadUInt16());
			UInt32 ticks = Endian.BigToLittle(br.ReadUInt32());
			double ts = secs + ticks / 10000.0;

			byte packetid = br.ReadByte();
			UInt16 len = Endian.BigToLittle(br.ReadUInt16());
			switch (packetid)
			{
				case 0xCC:
					ProcessIMUPacket(br, bytesReceived - 9, ts);
					break;
				case 0xFD:
					ProcessIMUErrorPacket(br, bytesReceived - 9, ts);
					break;
			}
		}

		private void ProcessIMUErrorPacket(BinaryReader br, int packetLen, double ts)
		{
			Console.WriteLine("IMU ERROR! @" + ts + ": CODE " + br.ReadByte().ToString("x") + " len: " + packetLen);
		}

		private void ProcessIMUPacket(BinaryReader br, int packetLen, double ts)
		{
			byte id = br.ReadByte();

			sum = 0;
            if (packetLen >= 40)
            {
                if (id == 0xaa)
                {
                    for (int i = 1; i < 8; i++)
                        sum = sum + br.ReadByte();

                    ProcessIMUc2(br, ts);
                }
            }
            /*else
            {
                Console.WriteLine("IMU ERROR! @" + ts + ": CODE " + id.ToString("x") + " len: " + packetLen);
            }*/
		}

		private void BuildSocket(IPAddress bindAddress)
		{
			lock (this)
			{
				if (this.sock == null)
				{
					if (this.buf == null)
						this.buf = new byte[65536];

					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					this.sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
					this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 10);
					this.sock.Bind(new IPEndPoint(bindAddress, this.port));
					this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.ip, bindAddress));
					this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
				}
			}
		}

		private void addsum(BinaryReader br)
		{
			for (int i = 3; i >= 0; i--)
			{
				temp[i] = br.ReadByte();
				sum = sum + temp[i];
			}
		}

		#region Messages
		//Acceleration and Angular Rate (0xC2)
		void ProcessIMUc2(BinaryReader br, double ts)
		{
			IMUData imuData = new IMUData();

			addsum(br);
			float accelX = BitConverter.ToSingle(temp, 0);
			addsum(br);
			float accelY = BitConverter.ToSingle(temp, 0);
			addsum(br);
			float accelZ = BitConverter.ToSingle(temp, 0);
			addsum(br);
			float angRateX = BitConverter.ToSingle(temp, 0);
			addsum(br);
			float angRateY = BitConverter.ToSingle(temp, 0);
			addsum(br);
			float angRateZ = BitConverter.ToSingle(temp, 0);
			addsum(br);
			//float timer = BitConverter.ToSingle(temp, 0); //time since system power-up
			uint timer = BitConverter.ToUInt32(temp, 0); //time since system power-up

			br.ReadBytes(2);
			UInt16 checksum = Endian.BigToLittle(br.ReadUInt16());
			if (sum == checksum)
			{
				imuData.xAccel = accelX;
				imuData.yAccel = accelY;
				imuData.zAccel = accelZ;
				imuData.xRate = angRateX;
				imuData.yRate = angRateY;
				imuData.zRate = angRateZ;
				imuData.DataType = "IMU";
				imuData.timer = timer;
			}
			else //if (showDebugMessages)
			{
				Console.Write("IMU Msg: @" + ts.ToString("F4") + ":::didn't pass checksum.");
				Console.WriteLine();
			}
			sum = 0;
			if (IMUUpdate != null) IMUUpdate(this, new TimestampedEventArgs<IMUData>(ts, imuData));
			if (showDebugMessages) Console.WriteLine("Accel: ( " + accelX.ToString("F8") + ", " + accelY.ToString("F8") + ", " + accelZ.ToString("F8") + " )" + "AngRate: ( " + angRateX.ToString("F8") + ", " + angRateY.ToString("F8") + ", " + angRateZ.ToString("F8") + " ) Timer: " + timer);
		}
		#endregion

		#region ISensor Members



		#endregion


		#region IIMU Members

		public event EventHandler<TimestampedEventArgs<IMUData>> IMUUpdate;

		public void Reset()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
