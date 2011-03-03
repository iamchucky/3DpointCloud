using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Magic.Network;
using Magic.Common;
using Magic.Common.Sensors;
using System.Threading;

namespace Magic.ViconInterface
{
	public class ViconTarsus : ISensor, IDisposable
	{
		private IPAddress ip;
		private Int32 port;
		private TcpClient tcp;
		private bool isRunning = true;

		private List<String> allChannelNames;
		private List<int> wantedStreamingChannels;
		private Thread streamingThread;

		public event EventHandler<IViconFrameEventArgs> DataReceived;

		public ViconTarsus(NetworkAddress na)
		{
			this.ip = na.Address;
			this.port = na.Port;
		}

		public ViconTarsus(String ip, Int32 port)
		{
			this.ip = IPAddress.Parse(ip);
			this.port = port;
		}

		public ViconTarsus()
		{
			this.ip = IPAddress.Parse("10.0.0.102");
			this.port = 800;
		}

		public List<double> GetValues(List<int> channels)
		{
			if (tcp == null)
			{
				Console.WriteLine("Null socket!");
				return null;
			}

			tcp.GetStream().Write(ViconRequest.Data, 0, ViconRequest.RequestSize);

			return ProcessValueData(channels);
		}


		byte[] processValueDataBuffer = new byte[60000];
		private List<double> ProcessValueData(List<int> channels)
		{
			List<double> values = new List<double>(channels.Count);
			int responseKind, responseType, channelCount;


			ReadBytes(processValueDataBuffer, 12);

			responseKind = BitConverter.ToInt32(processValueDataBuffer, 0);
			responseType = BitConverter.ToInt32(processValueDataBuffer, 4);
			channelCount = BitConverter.ToInt32(processValueDataBuffer, 8);

			if (responseKind != 2 || responseType != 1 || channelCount != allChannelNames.Count)
			{
				Console.WriteLine("Bad Vicon packet!");
				return null;
			}

			ReadBytes(processValueDataBuffer, 8 * allChannelNames.Count);

			for (int i = 0; i < channels.Count; i++)
				values.Add(BitConverter.ToDouble(processValueDataBuffer, channels.ElementAt(i) * 8));

			return values;
		}

		private void ReadBytes(byte[] buffer, int numberToRead)
		{
			int sum = 0;
			while (sum < numberToRead)
			{
				int bytesRead = tcp.GetStream().Read(buffer, sum, numberToRead - sum);
				if (bytesRead == 0) throw new Exception("Socket most likely toast.");
				sum += bytesRead;
			}

			//while (tcp.Available < numberToRead) Thread.Sleep(1);
			//tcp.GetStream().Read(buffer, 0, numberToRead);


		}

		public List<int> GetChannelNumbers(List<String> names)
		{
			List<int> channels = new List<int>(names.Count);

			RequestChannels();

			for (int i = 0; i < names.Count; i++)
			{
				for (int j = 0; j < allChannelNames.Count; j++)
				{
					if (allChannelNames.ElementAt(j) == names.ElementAt(i))
					{
						channels.Insert(i, j);
						break;
					}
				}
			}

			return channels;
		}

		public void SetStreamingChannels(List<int> channels)
		{
			wantedStreamingChannels = channels;
		}

		public void SetStreaming(bool TurnOn)
		{
			if (tcp == null)
			{
				Console.WriteLine("Null socket!");
				return;
			}

			if (TurnOn == true)
			{
				streamingThread = new Thread(StreamingReceiveThread);
				streamingThread.Start();
			}
			else tcp.GetStream().Write(ViconRequest.StreamingOff, 0, ViconRequest.RequestSize);
		}

		private void StreamingReceiveThread()
		{

			List<double> values = new List<double>(wantedStreamingChannels.Count);
			tcp.GetStream().Write(ViconRequest.StreamingOn, 0, ViconRequest.RequestSize);

			while (isRunning)
			{
				values = ProcessValueData(wantedStreamingChannels);

				if (DataReceived != null)
					DataReceived(this, new IViconFrameEventArgs(values));
			}
		}

		private void RequestChannels()
		{
			if (tcp == null)
			{
				Console.WriteLine("Null socket!");
				return;
			}

			tcp.GetStream().Write(ViconRequest.Info, 0, ViconRequest.RequestSize);
			ProcessChannelData();
		}


		private void ProcessChannelData()
		{
			int responseKind, responseType, channelCount, letterCount;

			byte[] buffer = new byte[65000];
			ReadBytes(buffer, 12);

			responseKind = BitConverter.ToInt32(buffer, 0);
			responseType = BitConverter.ToInt32(buffer, 4);
			channelCount = BitConverter.ToInt32(buffer, 8);

			if ((responseKind == 1 || responseKind == 2) && responseType == 1)
			{
				allChannelNames = new List<String>(channelCount);

				for (int i = 0; i < channelCount; i++)
				{
					ReadBytes(buffer, 4);
					letterCount = BitConverter.ToInt32(buffer, 0);

					ReadBytes(buffer, letterCount);
					allChannelNames.Insert(i, (ASCIIEncoding.ASCII.GetString(buffer, 0, letterCount)));
				}
			}
		}

		private void BuildSocket()
		{
			lock (this)
			{
				if (tcp == null)
				{
					try
					{
						tcp = new TcpClient();
						tcp.Connect(new IPEndPoint(ip, port));
					}
					catch (SocketException e)
					{
						Console.WriteLine("Socket exception!" + e.Message);
					}
				}
			}
		}

		#region OTB Members

		public void test()
		{
			SetStreaming(false);
		}

		#endregion

		#region ISensor Members

		public void Start()
		{
			BuildSocket();
			//SetStreaming(false);
			//RequestChannels();
		}

		public void Start(IPAddress bindAddresss)
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			if (tcp == null) return;

			tcp.GetStream().Write(ViconRequest.Close, 0, ViconRequest.RequestSize);
			tcp.Close();
			tcp = null;
		}

		public SensorPose ToBody
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			isRunning = false;
		}

		#endregion
	}
}
