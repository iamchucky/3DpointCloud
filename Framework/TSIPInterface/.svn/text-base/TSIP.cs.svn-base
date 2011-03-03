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

namespace Magic.Sensors
{
	public class TSIPGPS : IGPS
	{
		private IPAddress ip;
		private Int32 port;
		private byte[] buf;
		private Socket sock;        
		SensorPose toBodyTransform;

		int totalPackets = 0;
		int totalPacketsResetable = 0;
        bool showDebugMessages = false;
        GPSFixType fixType = GPSFixType.NotAvailable;
		public TSIPGPS(NetworkAddress na, SensorPose toBodyTransform)
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
				case 0xFE:
					ProcessTSIPPacket(br, bytesReceived - 9, ts);
					break;
				case 0xFD:
					ProcessTSIPErrorPacket(br, bytesReceived - 9, ts);
					break;
			}
		}

		private void ProcessTSIPErrorPacket(BinaryReader br, int packetLen, double ts)
		{
			Console.WriteLine("TSIP ERROR! @" + ts + ": CODE " + br.ReadByte().ToString("x") + " len: " + packetLen);
		}

		private void ProcessTSIPPacket(BinaryReader br, int packetLen, double ts)
		{
			byte id = br.ReadByte();

			switch (id)
			{
				case 0x41:
                    ProcessTSIP41(br, ts);
					break;
				case 0x46:
                    ProcessTSIP46(br, ts);
					break;
				case 0x4a:
                    ProcessTSIP4A(br, ts);
					break;
				case 0x4b:
                    ProcessTSIP4B(br, ts);
					break;
                case 0x56:
                    ProcessTSIP56(br, ts);
                    break;
				case 0x6d:
					ProcessTSIP6D(br,ts);
					break;

				case 0x82:
					ProcessTSIP82(br,ts);
					break;
                case 0x84:
                    ProcessTSIP84(br,ts);
                    break;
				default:
                    if (showDebugMessages)
                    {
                        Console.Write("TSIP Msg: @" + ts.ToString("F4") + " ID : " + id.ToString("x02") + " len: " + packetLen.ToString("00") + ":::");
                        for (int i = 1; i < packetLen; i++)
                        {
                            Console.Write(" " + br.ReadByte().ToString("x02"));
                        }
                        Console.WriteLine();
                    }
					break;
			}

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

		#region Messages
		//Velocity ENU
		void ProcessTSIP56(BinaryReader br, double ts)
		{
            GPSVelocityData velData = new GPSVelocityData();
            float eastVel = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
            float northVel = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
            float upVel = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
            float clockBias = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
            float TimeOfFix = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);

            velData.eastVelocity = eastVel;
            velData.northVelocity = northVel;
            velData.upVelocity = upVel;
            velData.timestamp = ts;
            velData.GPSTime = TimeOfFix;
            if (VelocityMeasurementReceived != null) VelocityMeasurementReceived(this, new TimestampedEventArgs<GPSVelocityData>(ts, velData));
            if (showDebugMessages) Console.WriteLine("Velocity ENU string: " + eastVel.ToString("F8") + " " + northVel.ToString("F8") + " " + upVel.ToString("F8") + " TOF:" + TimeOfFix);

		}

		//Allinview Satellites, DOP's, FixMode	
		void ProcessTSIP6D(BinaryReader br, double ts)
		{
            GPSErrorData errData = new GPSErrorData();
			byte stat = br.ReadByte();
			byte dimension = (byte)(stat & 0x03);
			byte mode = (byte)((stat & 0x04) >> 2);
			byte numSV = (byte)((stat & 0xF0) >> 4);
			float PDOP = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			float HDOP = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			float VDOP = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			float TDOP = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			List<byte> svPRN = new List<byte>();
			for (int i = 0; i < numSV; i++)
				svPRN.Add(br.ReadByte());            
            errData.numSatellites = numSV;
            errData.HDOP = HDOP;
            errData.VDOP = VDOP;
            errData.PDOP = PDOP;
            errData.fixType = fixType;
            errData.timestamp = ts;
            if(ErrorMeasurementReceived != null) ErrorMeasurementReceived(this, new TimestampedEventArgs<GPSErrorData>(ts, errData));
            if (showDebugMessages)
            {
                Console.Write("All In View: dim:" + dimension + " mode:" + mode + " numSV:" + numSV +
                    " PDOP: " + PDOP + " HDOP: " + HDOP + " VDOP:" + VDOP + " TDOP:" + TDOP);
                for (int i = 0; i < numSV; i++)
                    Console.Write(" svPRN:" + svPRN[i]);
                Console.WriteLine();
            }
		}

		//DGPS Fix Mode
		enum DGPSFixMode : byte
		{
			ManualDiffOff = 0,
			ManualDiffOn = 1,
			AutoDiffOff = 2,
			AutoDiffOn = 3
		}
		void ProcessTSIP82(BinaryReader br, double ts)
		{
            DGPSFixMode fixMode = (DGPSFixMode)br.ReadByte();
            if (fixMode == DGPSFixMode.AutoDiffOff || fixMode == DGPSFixMode.ManualDiffOff)
                fixType = GPSFixType.Autonomous;
            else
                fixType = GPSFixType.Differential;
            if(showDebugMessages) Console.WriteLine("Fix Mode: " + fixMode);

		}


		void ProcessTSIP84(BinaryReader br, double ts)
		{
            GPSPositionData posData = new GPSPositionData();
			double lat = BitConverter.ToDouble(br.ReadBytes(8).Reverse().ToArray(), 0);
			double lon = BitConverter.ToDouble(br.ReadBytes(8).Reverse().ToArray(), 0);
			double alt = BitConverter.ToDouble(br.ReadBytes(8).Reverse().ToArray(), 0);
			double clockBias = BitConverter.ToDouble(br.ReadBytes(8).Reverse().ToArray(), 0);
			float TimeOfFix = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			double latDeg = 180.0 / Math.PI * lat;
			double lonDeg = 180.0 / Math.PI * lon;
            posData.position.lat = latDeg;
            posData.position.lon = lonDeg;
            posData.position.alt = alt;
            posData.timeOfFix = TimeOfFix;
            posData.timestamp = ts;
            if (PositionMeasurementReceived != null) PositionMeasurementReceived(this, new TimestampedEventArgs<GPSPositionData>(ts, posData));
            if (showDebugMessages) Console.WriteLine("Position LLA dbl: " + latDeg.ToString("F8") + " " + lonDeg.ToString("F8") + " " + alt.ToString("F8") + " TOF:" + TimeOfFix);

		}
		//Position (single precision)
		void ProcessTSIP4A(BinaryReader br,double ts)
		{
            GPSPositionData posData = new GPSPositionData();
			float lat = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			float lon = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			float alt = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			float clockBias = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			float TimeOfFix = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			double latDeg = 180.0 / Math.PI * lat;
			double lonDeg = 180.0 / Math.PI * lon;
            posData.position.lat = latDeg;
            posData.position.lon = lonDeg;
            posData.position.alt = alt;
            posData.timestamp = ts;
            posData.timeOfFix = TimeOfFix;
            if (PositionMeasurementReceived != null) PositionMeasurementReceived(this, new TimestampedEventArgs<GPSPositionData>(ts, posData));
            if (showDebugMessages)	Console.WriteLine("Position LLA sing: " + latDeg.ToString("F8") + " " + lonDeg.ToString("F8") + " " + alt.ToString("F8") + " TOF:" + TimeOfFix);

		}

		//Receiver Health (5 seconds)
		enum TSIPStatusCode : byte
		{
			DoingPositionFixes = 0,
			NoGPSTime = 1,
			NeedInitialization = 2,
			PDOPTooHigh = 3,
			NoSatellites = 8,
			OnlyOneSat = 9,
			OnlyTwoSat = 0xA,
			OnlyThreeSat = 0xB,
			ChosenSatUnavailable = 0xC
		}
		void ProcessTSIP46(BinaryReader br,double ts)
		{
			TSIPStatusCode code = (TSIPStatusCode)br.ReadByte();
			Byte t = br.ReadByte();
			bool battOK = ((t & 0x01) == 0x00);
			bool antOK = ((t & 0x08) == 0x00);
			bool openDetected = ((t & 0x10) == 0x00);
            if (showDebugMessages)
			Console.WriteLine("Status: " + code + " batt: " + battOK + " ant: " + antOK + " short:" + openDetected);
		}

		//Machine Status (5 seconds)
		void ProcessTSIP4B(BinaryReader br,double ts)
		{
			byte machineID = br.ReadByte();
			byte status1 = br.ReadByte();
			byte status2 = br.ReadByte();
			Console.WriteLine("Machine ID: " + machineID.ToString("x02") + " stat1: " + status1.ToString("x02") + " stat2: " + status2.ToString("x02"));
		}

		//GPS Time (5 seconds)
		void ProcessTSIP41(BinaryReader br,double ts)
		{
			float timeOfWeek = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			Int16 weekNumber = BitConverter.ToInt16(br.ReadBytes(2).Reverse().ToArray(), 0);
			float GPSUTCOffset = BitConverter.ToSingle(br.ReadBytes(4).Reverse().ToArray(), 0);
			//Note – UTC time lags behind GPS time by an integer number of 
			//seconds; UTC = (GPS time) - (GPS UTC offset).
			//Warning – GPS week number runs from 0 to 1023 and then cycles back 
			//to week #0. week # 0 began January 6, 1980. The first cycle back to week 
			//#0 was on August 22, 1999. The extended GPS week number however, 
			//does not cycle back to 0. For example: the week # for August 22, 1999 = 
			//1024; the Week # for April 1, 2002 = 1160.
		}
		#endregion		

		#region ISensor Members



		#endregion

        #region IGPS Members

        public event EventHandler<TimestampedEventArgs<GPSPositionData>> PositionMeasurementReceived;

        public event EventHandler<TimestampedEventArgs<GPSVelocityData>> VelocityMeasurementReceived;

        public event EventHandler<TimestampedEventArgs<GPSErrorData>> ErrorMeasurementReceived;

        #endregion
    }
}
