using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using System.IO.Ports;
using Magic.Common;
using System.IO;

namespace SeptentrioInterface
{
	/// <summary>
	/// Provides an interface to the Septentrio AsteRX class receivers over RS232
	/// </summary>
	public class SeptentrioSerial : IGPS
	{
		SerialPort rxSerial;
		SensorPose sensorPose;
		ITimingProvider timingProvider;
		public SeptentrioSerial(string comPort, int baud, SensorPose sensorPose, ITimingProvider timingProvider)
		{
			rxSerial = new SerialPort(comPort, baud);
			foreach (string p in SerialPort.GetPortNames())
				Console.WriteLine(p);
			rxSerial.DataReceived += new SerialDataReceivedEventHandler(rxSerial_DataReceived);
			this.sensorPose = sensorPose;
			this.timingProvider = timingProvider;
		}

		void rxSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if (e.EventType == SerialData.Chars)
			{
				int bytecount = rxSerial.BytesToRead;
				byte[] bytes = new byte[bytecount];
				rxSerial.Read(bytes, 0, bytecount);
				foreach (byte b in bytes)
					OnNewByte(b);
			}
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

		private void OnNewByte(byte b)
		{
			switch (rxState)
			{
				//wait for start byte 0
				case 0:
					if (b == 0x24)
					{
						curMsgTimestamp = timingProvider.GetCurrentTime();
						rxState = 1;
					}
					else
					{
						Console.WriteLine("Septentrio Serial Parser: Resync");
					}
					break;

				//wait for start byte 1
				case 1:
					if (b == 0x40)
					{
						headerByteCount = 0;
						rxState = 2;
					}
					else if (b == 0x24) rxState = 1;
					else rxState = 0;
					break;

				//the next 6 bytes are part of the fixed header.
				case 2:
					headerBytes[headerByteCount] = b;
					headerByteCount++;
					if (headerByteCount == 6)
					{
						BinaryReader br = new BinaryReader(new MemoryStream(headerBytes));
						curMsgCRC = br.ReadUInt16();
						UInt16 temp = br.ReadUInt16();
						curMsgLen = br.ReadUInt16();
						curMsgID = (UInt16)(temp & 0x1FFF);
						curMsgIDRev = (Byte)(temp >> 13);
						//for the unique case of a 0 length message
						if (curMsgLen > 0)
						{
							//allocate the message and copy the header bytes
							curMsg = new byte[curMsgLen];
							curMsg[0] = 0x24; curMsg[1] = 0x40;
							Array.Copy(headerBytes, 0, curMsg, 2, 6);
							curMsgByteNum = 8;
							rxState = 3;
						}
						else
						{
							Console.WriteLine("Septentrio Serial Parser: Warning: zero length message!");
							rxState = 0;
						}
					}
					break;

				//we have the complete header, now begin receiving bytes until we reach message length
				case 3:
					curMsg[curMsgByteNum] = b;
					curMsgByteNum++;
					if (curMsgByteNum == curMsgLen)
					{
						if (CheckCRC(curMsg, curMsgCRC))
							ProcessPacket(curMsgID, curMsgIDRev, curMsg, curMsgTimestamp);
						else
							Console.WriteLine("Septentrio Serial Parser: CRC Failure. Message Dropped!");
						rxState = 0;
					}
					break;

				default:
					throw new InvalidOperationException("Septentrio Serial Parser: Fell threw state machine");
					break;
			}
		}
		int i = 0;
		private void ProcessPacket(ushort id, byte idRev, byte[] msg, double ts)
		{
			Console.WriteLine("Got: " +  id  + " or " + Endian.BigToLittle (id) + " " +i++);
			switch (id)
			{
				case 4007:
					ParsePVTGeodetic(idRev, msg, ts); break;
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

		private void ParsePVTGeodetic(byte idRev, byte[] msg, double ts)
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
			if (PositionMeasurementReceived != null) PositionMeasurementReceived(this,new TimestampedEventArgs<GPSPositionData> (ts,posData ));
			
		}



		private bool CheckCRC(byte[] msg, UInt16 CRC)
		{
			//IMPORTANT: TODO
			return true;
		}

		#region IGPS Members

		public event EventHandler<TimestampedEventArgs<GPSPositionData>> PositionMeasurementReceived;

		public event EventHandler<TimestampedEventArgs<GPSVelocityData>> VelocityMeasurementReceived;

		public event EventHandler<TimestampedEventArgs<GPSErrorData>> ErrorMeasurementReceived;


		#endregion

		#region ISensor Members

		public void Start()
		{
			if (rxSerial.IsOpen == false)
				rxSerial.Open();
		}

		public void Start(System.Net.IPAddress localBind)
		{
			Start();
		}

		public void Stop()
		{
			rxSerial.Close();
		}

		public SensorPose ToBody
		{
			get { return sensorPose; }
		}

		#endregion
	}
}
