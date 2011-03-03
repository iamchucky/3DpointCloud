using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Magic.Common.Sensors;
using Magic.Network;
using Magic.Common;

namespace Magic.Sensors
{

	public class UDPCamera : ICamera
	{
		private IPAddress ip;
		private Int32 port;
		private byte[] buf;
		private Socket sock;

		private struct UDPCameraMsg
		{
			public int index;
			public double timestamp;
			public int width;
			public int height;
			public int size;
			public int id;
		}

		#region ICamera Members

		public event EventHandler<TimestampedEventArgs<RobotImage>> ImageReceived;

		#endregion

		public UDPCamera(NetworkAddress address)
		{
			this.ip = address.Address; this.port = address.Port;
		}

		public void Start()
		{
			BuildSocket();
		}

		public void Start(IPAddress addr)
		{
			BuildSocket();
		}

		public void Stop()
		{
			//this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(this.ip));
			this.sock = null;
		}

		private void BuildSocket()
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
					this.sock.Bind(new IPEndPoint(IPAddress.Any, this.port));
					this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.ip));
					this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, this);
				}
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				int bytesRead = this.sock.EndReceive(ar);
				if (bytesRead > 0)
				{
					MemoryStream ms = new MemoryStream(buf, 0, bytesRead, false);
					BinaryReader br = new BinaryReader(ms);
					ParseImagePacket(br);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Socket exception! " + ex.Message);

			}
			try
			{
				this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
			}
			catch { }

		}

		private void ParseImagePacket(BinaryReader br)
		{
			UDPCameraMsg msg = new UDPCameraMsg();

			msg.index = br.ReadInt32();
			msg.timestamp = br.ReadDouble();
			msg.width = br.ReadInt32();
			msg.height = br.ReadInt32();
			msg.size = br.ReadInt32();
			msg.id = br.ReadInt32();
			byte[] jpgimg = br.ReadBytes(msg.size);
			MemoryStream ms = new MemoryStream(jpgimg);

			Bitmap bmp = (Bitmap)System.Drawing.Bitmap.FromStream(ms);

			RobotImage ri = new RobotImage(msg.id, bmp, msg.timestamp);
			if (ImageReceived != null)
				ImageReceived(this, new TimestampedEventArgs<RobotImage>(msg.timestamp, ri));


		}

		public SensorPose ToBody { get { return new SensorPose(0, 0, 0, 0, 0, 0, 0); } }


	}
}
