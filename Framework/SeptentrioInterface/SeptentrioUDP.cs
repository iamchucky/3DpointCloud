using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common;
using Magic.Network;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace SeptentrioInterface
{
	public class SeptentrioUDP : IGPS
	{
		private IPAddress ip;
		private Int32 port;
		private byte[] buf;
		private Socket sock;
		SensorPose toBodyTransform;
		
		int ID;

		public SeptentrioUDP(NetworkAddress na, SensorPose toBodyTransform, int id)
		{
			this.ip = na.Address; this.port = na.Port;	
			this.toBodyTransform = toBodyTransform;
			this.ID = id;
		}
		
		private int rxState = 0;
		byte[] headerBytes = new byte[6];
		int headerByteCount = 0;

		UInt16 curMsgCRC;
		UInt16 curMsgID;
		Byte curMsgIDRev;
		UInt16 curMsgLen = 0;
		UInt16 curMsgByteNum = 0;
		byte[] curMsg;
		double curMsgTimestamp = 0;


        private void ProcessPacket(byte septID, ushort id, byte idRev, byte[] msg, double ts)
		{
			//Console.WriteLine("Got: " +  id  + " @ " + ts + " ");
			switch (id)
			{
				case 4007:
					ParsePVTGeodetic(idRev, msg, ts, septID); break;
				case 5906:
					ParsePosCovGeodetic(idRev, msg, ts); break;
				case 5908:
					ParseVelCovGeodetic(idRev, msg, ts); break;
				case 4001:
					ParseDOP(idRev, msg, ts); break;
				case 5921:
					ParseEndOfPVT(idRev, msg, ts); break;
			}

		}

		private void ParseEndOfPVT(byte idRev, byte[] msg, double ts)
		{
		}

		private void ParseDOP(byte idRev, byte[] msg, double ts)
		{
		}

		private void ParseVelCovGeodetic(byte idRev, byte[] msg, double ts)
		{
		}

		private  void ParsePosCovGeodetic(byte idRev, byte[] msg, double ts)
		{
		
		}

		private void ParsePVTGeodetic(byte idRev, byte[] msg, double ts, byte septID)
		{
			GPSPositionData posData = new GPSPositionData();
			BinaryReader br = new BinaryReader(new MemoryStream(msg));
			br.ReadBytes(8); //knock off the first 8 bytes
			double TOW = br.ReadUInt32() * .001; //seconds
			double WNc = br.ReadUInt16(); //week number
			byte mode = br.ReadByte();
			byte error = br.ReadByte();
			double latitude = br.ReadDouble(); //in radians!
			double longitude = br.ReadDouble();
			double height = br.ReadDouble();
			double undulation = br.ReadDouble();
			double velN = br.ReadDouble();
			double velE = br.ReadDouble();
			double velU = br.ReadDouble();

			posData.position.alt = height;
			posData.position.lat = latitude * 180.0 / Math.PI;
			posData.position.lon = longitude * 180.0 / Math.PI;
			posData.timeOfFix = TOW;
			posData.timestamp = ts;
            if (PositionMeasurementReceived != null)
            {
                TimestampedEventArgs<GPSPositionData> GPSData = new TimestampedEventArgs<GPSPositionData>(ts, posData);
                if (septID == 0)
                    GPSData.Message.DataType = "frontGPS";
                else if (septID == 1)
                    GPSData.Message.DataType = "rearGPS";
                PositionMeasurementReceived(this, GPSData);
            }
			
		}



		private bool CheckCRC(byte[] msg, UInt16 CRC)
		{
			//IMPORTANT: TODO
			return true;
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			if (this.sock == null) return;
			int bytesReceived = this.sock.EndReceive(ar);
			if (bytesReceived > 0)
			{
				MemoryStream ms = new MemoryStream(buf, 0, bytesReceived, false);
				BinaryReader br = new BinaryReader(ms);
				byte septID = br.ReadByte();
				UInt16 secs = Endian.BigToLittle(br.ReadUInt16());
				UInt32 ticks = Endian.BigToLittle(br.ReadUInt32());
				double ts = secs + ticks / 10000.0;
				br.ReadByte(); br.ReadByte();
				UInt16 CRC = br.ReadUInt16();

				UInt16 temp = br.ReadUInt16();
				UInt16 curMsgID = (UInt16)(temp & 0x1FFF);
				Byte curMsgIDRev = (Byte)(temp >> 13);
				curMsgLen = br.ReadUInt16();
                br.BaseStream.Position = 7;
				byte[] msg = br.ReadBytes (curMsgLen);                    
				//if (septID == ID) 
					ProcessPacket(septID, curMsgID, 0, msg, ts);

			}

			this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
		}

		#region IGPS Members

		public event EventHandler<TimestampedEventArgs<GPSPositionData>> PositionMeasurementReceived;

		public event EventHandler<TimestampedEventArgs<GPSVelocityData>> VelocityMeasurementReceived;

		public event EventHandler<TimestampedEventArgs<GPSErrorData>> ErrorMeasurementReceived;


		#endregion



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

		#region ISensor Members

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

		#endregion
	}


}
