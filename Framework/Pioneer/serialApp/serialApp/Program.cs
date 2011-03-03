

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;



namespace serialApp
{
	public class Program
	{

		//sp.WriteTimeout = 500;

		//create an Serial Port object
		SerialPort sp = new SerialPort("COM1",
		9600, Parity.None, 8, StopBits.One);

		byte serialCount = 0;

		bool sync1Received = false;
		//number of bytes recieved thus far
		short syncReceived = 0;
		byte bytesReceived = 0;
		int[] baudRate = new int[5] { 9600, 38400, 19200, 115200, 57600 };

		//receive buffer
		byte[] receiveBuffer = new byte[30];
		byte receivedBufferStart = 0;
		byte receivedBufferEnd = 0;

		//received packet
		const byte MAX_PACKET_SIZE = 207;
		byte[] packetReceived = new byte[207];
		ulong packetCount = 0;

		//max
		const byte MAX_RECEIVE_BUFFER_SIZE = 30;
		//first start byte
		const byte START_BYTE_ONE = 0xFA;
		//second start byte
		const byte START_BYTE_TWO = 0xFB;

		//state of sending
		byte sendState = 0;
		//command packet
		byte[] packetM = new byte[20];

		static void Main(string[] args)
		{
			new Program();


		}

		private Program()
		{
			Console.WriteLine("Incoming Data:");

		//	sp.ReceivedBytesThreshold = 7;
			sp.Open();
			sp.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

			while (true)
			{
				//	sp.Write("a");
				//}
				if (sendState < 3)
				{
					//try ne baudrate
					//if (serialCount > 0)
					Console.WriteLine("sending sync {0,1:d}", (int)syncReceived + 1);
					packetM[0] = 0xfa;
					packetM[1] = 0xfb;
					packetM[2] = 3;
					packetM[3] = (byte)syncReceived;
					packetM[4] = 0;
					packetM[5] = (byte)syncReceived;

					//				sp.BaudRate = baudRate[serialCount];
					//		Console.WriteLine("trying baud:\n{0,8:d}", baudRate[serialCount]);
					//			Console.WriteLine("sending: ");
					//		Console.WriteLine(((char)packetM[0]).ToString());
					sp.Write(new byte[] { 250, 251, 3, (byte)syncReceived, 0, (byte)syncReceived }, 0, 6);
					Thread.Sleep(1000);
					//if(syncReceived>sendState)
					
					//disable iorequest
			/*		packetM[0] = 0xfa;
					packetM[1] = 0xfb;
					packetM[2] = 6;
					packetM[3] = 40;
					packetM[4] = 0x3b;
					packetM[5] = 0x00;
					packetM[6] = 0x00;
					packetM[7] = (byte)((checkSum()) >> 8);
					packetM[8] = (byte)((checkSum()) & 0xff);
					sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
								packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
				*/	
					serialCount++;
					if (serialCount == 5)
						serialCount = 0;
				}
				else
				{
					switch (sendState)
					{
						case 3://open
							Console.WriteLine("sending open: {0,1:d}\n", (int)sendState);
							sp.Write(new byte[] { 250, 251, 3, 1, 
								0, 1 }, 0, 6);
							Thread.Sleep(1000);
							//pulse again
							sp.Write(new byte[] { 250, 251, 3, 0, 
								0, 0 }, 0, 6);
							Thread.Sleep(1000);



							//disable sonar
							packetM[0] = 0xfa;
							packetM[1] = 0xfb;
							packetM[2] = 6;
							packetM[3] = 28;
							packetM[4] = 0x3b;
							packetM[5] = 0x00;
							packetM[6] = 0x00;
							packetM[7] = (byte)((checkSum()) >> 8);
							packetM[8] = (byte)((checkSum()) & 0xff);
							sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
								packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);


							if (syncReceived > 5)
							{
								//	syncReceived = 4;
								//		sendState++;
							}
							break;
						/*		break;
							case 4:
								while (true)
								{
	*/
						case 4:
							//	case 5:
							Console.WriteLine("start motors: {0,1:d}\n", (int)sendState);

							//pulse again
							sp.Write(new byte[] { 250, 251, 3, 0, 
								0, 0 }, 0, 6);

							packetM[0] = 0xfa;
							packetM[1] = 0xfb;
							packetM[2] = 6;
							packetM[3] = 0x04;
							packetM[4] = 0x3b;
							packetM[5] = 0x01;
							packetM[6] = 0x00;
							packetM[7] = (byte)((checkSum()) >> 8);
							packetM[8] = (byte)((checkSum()) & 0xff);
							//				Console.WriteLine("sending: ");
							//			Console.Write(BitConverter.ToString(packetM));

							sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
								packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
							Thread.Sleep(2000);
							/*			sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
										Thread.Sleep(1000);
										sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
										Thread.Sleep(1000);
										sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
										Thread.Sleep(4);
										sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
										Thread.Sleep(4);
										sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
										Thread.Sleep(4);
										sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
										Thread.Sleep(4);
										sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
										Thread.Sleep(4);
								*/
							///			if (syncReceived > 7)
							//		{
							//			syncReceived = 5;
							sendState = 6;
							//		}
							break;
						case 5:
							//		Thread.Sleep(4);
							Console.WriteLine("move motors: {0,1:d}\n", (int)sendState);
							//				sendState++;
							/*
														packetM[0] = 0xfa;
														packetM[1] = 0xfb;
														packetM[2] = 6;
														packetM[3] = 0x04;
														packetM[4] = 0x3b;
														packetM[5] = 0x01;
														packetM[6] = 0x00;
														packetM[7] = (byte)((checkSum()) >> 8);
														packetM[8] = (byte)((checkSum()) & 0xff);
														//				Console.WriteLine("sending: ");
														//			Console.Write(BitConverter.ToString(packetM));

														sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
															packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);

														//set max vel
														packetM[0] = 0xfa;
														packetM[1] = 0xfb;
														packetM[2] = 5;
														packetM[3] = 0x06;
														packetM[4] = 0x3b;
														packetM[5] = 0x00;
														packetM[6] = 0xfe;
														packetM[7] = (byte)((checkSum()) >> 8);
														packetM[8] = (byte)((checkSum()) & 0xff);

														Thread.Sleep(3);
														//move forward
														sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
															packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);

														//set max accel
														packetM[0] = 0xfa;
														packetM[1] = 0xfb;
														packetM[2] = 5;
														packetM[3] = 0x05;
														packetM[4] = 0x3b;
														packetM[5] = 0x00;
														packetM[6] = 0xfe;
														packetM[7] = (byte)((checkSum()) >> 8);
														packetM[8] = (byte)((checkSum()) & 0xff);

														Thread.Sleep(3);
														//move forward
														sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
															packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
							*/
							//			while (true)
							//			{
							/*				//enable motors
											packetM[0] = 0xfa;
											packetM[1] = 0xfb;
											packetM[2] = 6;
											packetM[3] = 0x04;
											packetM[4] = 0x3b;
											packetM[5] = 0x01;
											packetM[6] = 0x00;
											packetM[7] = (byte)((checkSum()) >> 8);
											packetM[8] = (byte)((checkSum()) & 0xff);
											//				Console.WriteLine("sending: ");
											//			Console.Write(BitConverter.ToString(packetM));
									//		Thread.Sleep(3);
											sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);

										packetM[0] = 0xfa;
										packetM[1] = 0xfb;
										packetM[2] = 6;
										packetM[3] = 0x20;
										packetM[4] = 0x3b;
										packetM[5] = 0x20;
										packetM[6] = 0x20;
										packetM[7] = (byte)((checkSum()) >> 8);
										packetM[8] = (byte)((checkSum()) & 0xff);
						
										//	Thread.Sleep(3);
											//move forward
											sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
											packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
									*/
							packetM[0] = 0xfa;
							packetM[1] = 0xfb;
							packetM[2] = 6;
							packetM[3] = 0x08;
							packetM[4] = 0x3b;
							packetM[5] = 0x00;
							packetM[6] = 0x02;
							packetM[7] = (byte)((checkSum()) >> 8);
							packetM[8] = (byte)((checkSum()) & 0xff);

							//							Thread.Sleep(3);
							//move forward
							sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
								packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
							/*		Thread.Sleep(4);
									//disable motors
									packetM[0] = 0xfa;
									packetM[1] = 0xfb;
									packetM[2] = 6;
									packetM[3] = 0x04;
									packetM[4] = 0x3b;
									packetM[5] = 0x00;
									packetM[6] = 0x00;
									packetM[7] = (byte)((checkSum()) >> 8);
									packetM[8] = (byte)((checkSum()) & 0xff);
									//				Console.WriteLine("sending: ");
									//			Console.Write(BitConverter.ToString(packetM));

									sp.Write(new byte[] { packetM[0], packetM[1], packetM[2], packetM[3], 
									packetM[4], packetM[5],packetM[6],packetM[7],packetM[8] }, 0, 9);
							*/
							sendState++;
							//			}

							/*		//turn
										*/
							break;
						case 6:
							Thread.Sleep(3);

							sp.ReceivedBytesThreshold = 40;
							//pulse to keep alive
							sp.Write(new byte[] { 250, 251, 3, 0, 
									0, 0 }, 0, 6);
							Thread.Sleep(1000);
							break;
						default:
							break;
					}
				}

				// sp.Write(new byte[] { 0xfa,0xfb,n,0,0x3b, 255, 251, 3, 0, 0, 0 }, 0, 7);
			}
			sp.Close();
		}

		void port_DataReceived(object sender,
			SerialDataReceivedEventArgs e)
		{
			
			
			byte[] receivedData = new byte[200];
			if (sendState == 6)
			{
				gotByte();
				receivedData = processPacket();
				if (receivedData != null)
				{
				//	Array.Copy(processPacket(), receivedData, 100);
					string s = System.Text.ASCIIEncoding.ASCII.GetString(receivedData);

					//if (receivedData != null)
					Console.WriteLine("received data: {0,1:d}\n", s);
				}
			}
			syncReceived++;
			switch (sendState)
			{
				case 0:
				case 1:
				case 2:

					if (syncReceived == 1)
					{
						Console.Write("sync1 received: ");
						Console.WriteLine(sp.ReadExisting().ToString());
						Console.WriteLine("\n");

						sendState = 1;
					}
					else if (syncReceived == 2)
					{
						Console.Write("sync2 received: ");
						Console.WriteLine(sp.ReadExisting().ToString());
						Console.WriteLine("\n");
						sendState = 2;
					}
					else if (syncReceived == 10)
					{
						Console.Write("sync3 received: ");
						Console.WriteLine(sp.ReadExisting().ToString());
						Console.WriteLine("\n");

						sendState = 3;
					}
					break;

				case 3:
					//wait for 3 packets
					//	 if (syncReceived == 6)
					// {
					Console.WriteLine("open received: {0,1:d}\n", (int)syncReceived + 1);
					Console.WriteLine(sp.ReadExisting());
					Console.WriteLine("\n");
					sendState++;
					//}
					break;
				default:
					/*				 if(syncReceived >15)
										sp.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
									 else
									{
						*/
					// Show all the incoming data in the port's buffer
					Console.WriteLine("nothing: ");
					Console.WriteLine(sp.ReadExisting());
					Console.WriteLine("\n");
					//		}
					break;
			}
		}
		short chksum;
		int checkSum()
		{
			int i;
			//char *buffer = &packetM[3];
			int c = 0;
			byte n;

			i = 3;
			n = (byte)(packetM[2] - 2);
			while (n > 1)
			{
				c += ((char)packetM[i] << 8) | (char)packetM[i + 1];
				c = c & 0xffff;
				n -= 2;
				i += 2;
				//buffer+=2;
			}
			if (n > 0) c = c ^ (int)((char)packetM[i]);
			return c;
		}

		int checksumAccum = 0;
		int checksum = 0;
		byte packetLength = 0;
		byte[] processPacket()
		{
			byte input;

			//track routine
			byte count = 0;

			//if(tempArr.Length>2)
			//check for empty buffer
			while (receivedBufferEnd != receivedBufferStart)
			{
				input = receiveBuffer[receivedBufferStart];
				receivedBufferStart++;
				bytesReceived--;
				if (receivedBufferStart >= MAX_RECEIVE_BUFFER_SIZE)
					receivedBufferStart = 0;

				if (count == 0 && input == START_BYTE_ONE)
					count = 1;
				else if (count == 1)
				{
					if (input == START_BYTE_TWO)
						count = 2;
					else if (input == START_BYTE_ONE)
						count = 1;
					else count = 0;
				}
				else if (count == 2)
				{
					//get length
					packetLength = input;
					//checksumAccum = input;
					//count++;

					if (packetLength < MAX_PACKET_SIZE)
						count++;
					else
					{
						if (input == START_BYTE_ONE)
							count = 1;
						else
							count = 0;
					}
				}
				else if (count < 3);
				else if (count < packetLength + 3 - 2)
				//last 2 bytes for checksum
				{
					checksumAccum += input;
					//get data
					packetReceived[count - 3] = input;
					count++;
				}
				else if (count < packetLength + 3 - 1)
				{
					checksum = ((int)input) << 8;
					count++;
				}
				else
				{
					checksum |= input;
					packetCount++;
					if (checksum == checksumAccum)
						return packetReceived;
					else
						return null;
				}


			}
			return null;
		}
		byte[] tempArr;
		void gotByte()
		{
byte inputByte = (byte)sp.ReadByte();
			//tempArr = sp.ReadExisting();
			int byteCount = sp.BytesToRead;
			
			
			
			bytesReceived++;
			for(int i=0;i<byteCount;i++)
		  //while(inputByte != null)
			{
				receiveBuffer[receivedBufferEnd++] =
					inputByte;
				if (receivedBufferEnd == MAX_RECEIVE_BUFFER_SIZE)
					receivedBufferEnd = 0;
				inputByte = (byte)sp.ReadByte();
				
			}
		}

	}
}
