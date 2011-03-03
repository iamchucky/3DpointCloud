using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Magic.Network;
using Magic.Common;
using Magic.Common.Sensors;
using Magic.Common.Mapack;

namespace Magic.SickInterface
{

	public class SickLMS : ILidar2D
	{

		private const int STX = 0x02;
		private const int LIDAR_ADDR = 0x80;
		private const int LIDAR_MAXVAL_ERROR = 8183;
		private const int LIDAR_DAZZLING_ERROR = 8190;

		private IPAddress ip;
		private Int32 port;
		private byte[] buf;
		private Socket sock;
		SensorPose toBodyTransform;
		private bool upsideDown = false;

		public bool IsUpsideDown
		{ get { return upsideDown; } }
		public event EventHandler<ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>> ScanReceived;

		int totalPackets = 0;
		int totalPacketsResetable = 0;

		public SickLMS(NetworkAddress na, SensorPose toBodyTransform, bool upsideDown)
		{
			this.ip = na.Address; this.port = na.Port; this.upsideDown = upsideDown;
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
			try
			{
				int bytesReceived = this.sock.EndReceive(ar);
				if (bytesReceived > 0)
				{
					int ret = ProcessLidarPacket(buf);
					if (ret == 0)
					{
						totalPackets++;
						totalPacketsResetable++;
					}
					else
						Console.WriteLine("BAD sick packet!!!");
				}
			}
			catch (SocketException ex)
			{
				Console.WriteLine("Socket exception! " + ex.Message);
				return;
			}

			this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
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



		private int ProcessLidarPacket(Byte[] rx)
		{
			SickScan scan = new SickScan();
			scan.points = new List<ILidar2DPoint>();
			MemoryStream ms = new MemoryStream(rx);
			int id = ms.ReadByte();
			//ghetto:
			//check for 180s 
			bool is180 = false;
			if (id == 1)
				is180 = true;

			int mode = ms.ReadByte();//Remove id/mode
			int secs = (ms.ReadByte() << 8) + ms.ReadByte();
			int ticks = (ms.ReadByte() << 24) + (ms.ReadByte() << 16) + (ms.ReadByte() << 8) + (ms.ReadByte());
			if (ms.ReadByte() != STX) //STX
				return -1;
			if (ms.ReadByte() != LIDAR_ADDR) //Address
				return -2;
			int length = ms.ReadByte() + (ms.ReadByte() * 256); //message len excluding CRC
			int cmd = ms.ReadByte(); //command
			ms.ReadByte(); //boolshit 2 bytes
			ms.ReadByte();
			if (is180)
			{
				for (float i = -90; i <= 90; i += 0.5f)
				{
					int r = (ms.ReadByte() + (ms.ReadByte() * 256));
					double theta = ((i) * (Math.PI / 180)); //radians
					double range = (double)r * .01; //range in meters

					//	double rawY = -1 * Math.Sin(theta) * range;
					//	double rawX = Math.Cos(theta) * range;
					if (upsideDown)
						theta *= -1;
					scan.points.Add(new SickPoint(new RThetaCoordinate((float)range, (float)theta, i)));

				}
			}
			else
			{
				for (float i = -45; i <= 45; i += 0.5f)
				{
					int r = (ms.ReadByte() + (ms.ReadByte() * 256));
					double theta = ((i) * (Math.PI / 180)); //radians
					double range = (double)r * .01; //range in meters
					//	double rawY = -1 * Math.Sin(theta) * range;
					//	double rawX = Math.Cos(theta) * range;
					if (upsideDown) theta *= -1;
					scan.points.Add(new SickPoint(new RThetaCoordinate((float)range, (float)theta, i)));
				}
			}

			scan.packetNum = totalPackets;
			scan.scannerID = id;
			scan.timestamp = (double)secs + (double)ticks / 10000.0;


			if (ScanReceived != null)
				ScanReceived(this, new ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>(scan));
			return 0;
		}

	}
}
