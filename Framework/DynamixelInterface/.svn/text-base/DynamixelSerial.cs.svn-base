using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace Magic.Framework.DynamixelInterface
{
	enum DynanmixelStatus
	{
		ALIVE, ERROR, NOT_ALIVE, NOT_AVAILABLE
	}

	public class DynamixelSerial : IDisposable
	{
		SerialPort rxSerial;
		int seriesNumber;
		List<DynanmixelStatus> status;
		bool pingSuccess = false;

		int reading;
		public int Reading
		{ get { return reading; } }


		public DynamixelSerial(string portName, int baudrate, int seriesNumber)
		{
			this.seriesNumber = seriesNumber;
			rxSerial = new SerialPort(portName, baudrate);
			rxSerial.Open();

			status = new List<DynanmixelStatus>(seriesNumber);
			for (int i = 0; i < seriesNumber; i++)
				status.Add(DynanmixelStatus.NOT_AVAILABLE);
		}
				
		public bool Ping()
		{
			List<bool> sucessList = new List<bool>();
			for (int i = 0; i < seriesNumber; i++)
			{
				byte[] pingByte = new byte[6];
				pingByte[0] = 0xFF;
				pingByte[1] = 0xFF;
				pingByte[2] = Byte.Parse(i.ToString());
				pingByte[3] = 0x02;
				pingByte[4] = 0x01;
				pingByte[5] = CalculateCheckSum(i, 2, pingByte[4]);
				rxSerial.Write(pingByte, 0, pingByte.Length);

				Thread.Sleep(150);
				byte[] readByte = new byte[rxSerial.BytesToRead];
				if (rxSerial.BytesToRead == 0)
				{
					Console.WriteLine("No response from DynamixelRX. Check the power connection...and etc!");
					return false;
				}
				rxSerial.Read(readByte, 0, 6);
				if (!HasError(readByte, i))
				{
					sucessList.Add(true);
					status[i] = DynanmixelStatus.ALIVE;
					Console.WriteLine("ID #" + i + " is alive!");
				}
				else
					Console.WriteLine("ID #" + i + " is NOT alive!");
			}
			if (sucessList.Count == seriesNumber)
			{
				pingSuccess = true;
				//rxSerial.DataReceived += new SerialDataReceivedEventHandler(rxSerial_DataReceived);
			}
			return true;
		}

		public void SetGoalPos(int ID, int reading, int speed)
		{
			List<byte> parameters = new List<byte>();
			byte[] message = new byte[11];
			message[0] = 0xFF;
			message[1] = 0xFF;
			message[2] = Byte.Parse(ID.ToString());
			//message[2] = 0xFE;
			message[3] = 0x07; // length
			message[4] = 0x03; // instruction
			message[5] = 0x1E; // address
			message[6] = Byte.Parse((reading % 256).ToString());
			message[7] = Byte.Parse((reading / 256).ToString());
			message[8] = Byte.Parse((speed % 256).ToString());
			message[9] = Byte.Parse((speed / 256).ToString());
			parameters.Add(message[5]); 
			parameters.Add(message[6]); 
			parameters.Add(message[7]);
			parameters.Add(message[8]);
			parameters.Add(message[9]);
			message[10] = CalculateCheckSum(ID, message[3], message[4], parameters);
			rxSerial.Write(message, 0, message.Length);

			Thread.Sleep(150);
			int byteCount = rxSerial.BytesToRead;
			byte[] bytes = new byte[byteCount];
			rxSerial.Read(bytes, 0, byteCount);
		}

		[Obsolete]
		public void SetGoalPos(List<int> IDs, List<int> readings)
		{
			int n = 6; // index of message for address
			List<byte> parameters = new List<byte>();
			byte[] message = new byte[2 * IDs.Count + 8];
			message[0] = 0xFF;
			message[1] = 0xFF;
			message[2] = 0xFE;
			message[3] = Byte.Parse((2 * IDs.Count + 4).ToString()); // length
			message[4] = 0x83; // instruction
			message[5] = 0x1E; // address
			message[6] = 0x01; // number of input for each
			parameters.Add(message[5]);
			parameters.Add(message[6]);
			for (int i = 0; i < IDs.Count; i++)
			{
				message[(i * 2) + n + 1] = 0;// Byte.Parse(IDs[i].ToString());
				message[(i * 2) + n + 2] = 1;// Byte.Parse((readings[i] % 256).ToString());
				//message[(i * 3) + n + 3] = Byte.Parse((readings[i] / 256).ToString());
				//parameters.Add(Byte.Parse(IDs[i].ToString()));
				//parameters.Add(Byte.Parse((readings[i] % 256).ToString()));
				//parameters.Add(Byte.Parse((readings[i] / 256).ToString()));
				parameters.Add((byte)0);
				parameters.Add((byte)1);
			}
			message[2 * IDs.Count + 8 - 1] = CalculateCheckSum(int.Parse(message[2].ToString()), message[3], message[4], parameters);
			rxSerial.Write(message, 0, message.Length);

			Thread.Sleep(150);
			int byteCount = rxSerial.BytesToRead;
			byte[] bytes = new byte[byteCount];
			rxSerial.Read(bytes, 0, byteCount);
		}

		public int GetCurrentPos(int ID)
		{
			byte[] message = new byte[8];
			List<byte> parameters = new List<byte>();
			message[0] = 0xFF;
			message[1] = 0xFF;
			message[2] = Byte.Parse(ID.ToString());
			message[3] = 0x04;
			message[4] = 0x02;
			message[5] = 0x24;
			message[6] = 0x02;
			parameters.Add(message[5]);
			parameters.Add(message[6]);
			message[7] = CalculateCheckSum(ID, message[3], message[4], parameters);
			if (status[ID] == DynanmixelStatus.ALIVE)
				rxSerial.Write(message, 0, message.Length);

			Thread.Sleep(10);
			int byteCount = rxSerial.BytesToRead;
			byte[] bytes = new byte[byteCount];
			rxSerial.Read(bytes, 0, byteCount);
			HasError(bytes, bytes[2]); // check error
			return 256 * bytes[6] + bytes[5];
		}

		private string GetIntBinaryString(int n)
		{
			char[] b = new char[8];
			int pos = 7;
			int i = 0;

			while (i < 8)
			{
				if ((n & (1 << i)) != 0)
				{
					b[pos] = '1';
				}
				else
					b[pos] = '0';
				pos--;
				i++;
			}
			return new string(b);
		}

		private bool HasError(byte[] byteRespond, int ID)
		{
			// first 2 bytes are always 0xFF
			if (!(byteRespond[0] == 0xFF && byteRespond[1] == 0xFF))
			{
				Console.WriteLine("Failing receiving data...");
				return true;
			}
			else if (byteRespond[4] != 0x00)
			{
				string binary = GetIntBinaryString(byteRespond[4]);
				if (binary[1] == '1')
				{
					status[ID] = DynanmixelStatus.ERROR;
					throw new InvalidOperationException("Instruction Error");
				}
				else if (binary[2] == '1')
				{
					status[ID] = DynanmixelStatus.ERROR;
					throw new InvalidOperationException("Overload Error");
				}
				else if (binary[3] == '1')
				{
					status[ID] = DynanmixelStatus.ERROR;
					throw new InvalidOperationException("Checksum Error");
				}
				else if (binary[4] == '1')
				{
					status[ID] = DynanmixelStatus.ERROR;
					throw new InvalidOperationException("Range Error");
				}
				else if (binary[5] == '1')
				{
					status[ID] = DynanmixelStatus.ERROR;
					throw new InvalidOperationException("Overheating Error");
				}
				else if (binary[6] == '1')
				{
					status[ID] = DynanmixelStatus.ERROR;
					throw new InvalidOperationException("Angle Limit Error");
				}
				else if (binary[7] == '1')
				{
					status[ID] = DynanmixelStatus.ERROR;
					throw new InvalidOperationException("Input Voltage Error");
				}

			}
			return false;
		}

		private byte CalculateCheckSum(int ID, int length, byte instruction)
		{
			return CalculateCheckSum(ID, length, instruction, new List<byte>());
		}

		private byte CalculateCheckSum(int ID, int length, byte instruction, List<byte> parameter)
		{
			byte sumParameter = 0x00;
			if (parameter != null)
			{
				foreach (byte b in parameter)
				{
					sumParameter += b;
				}
			}
			return Byte.Parse((0xFF - ((Byte.Parse(ID.ToString()) + Byte.Parse(length.ToString()) + instruction + sumParameter) % 255)).ToString());
		}




		#region IDisposable Members

		public void Dispose()
		{
			rxSerial.Close();
		}

		#endregion
	}
}
