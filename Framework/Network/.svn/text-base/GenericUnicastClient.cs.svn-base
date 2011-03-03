using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Magic.Network
{
	public class GenericUnicastClient<T>
	{
		public event EventHandler<MsgReceivedEventArgs<T>> MsgReceived;

		private IPAddress ip;
		private Int32 port;
		private byte[] buf;
		private Socket sock;

		private IMulticastSerializer<T> serializer;
		int totalPackets = 0;
		int totalPacketsResetable = 0;

		public GenericUnicastClient(NetworkAddress address, IMulticastSerializer<T> serializer)
		{
			this.ip = address.Address;
			this.port = address.Port;
			this.serializer = serializer;
			if (address.Protocol != NetworkAddressProtocol.UDP_UNI)
				Console.WriteLine("Warning: Using non UDP comaptible network address in Unicast Client (" + address.Name + ")");
		}

		public void Start()
		{
			BuildSocket(IPAddress.Any);
		}

		public void Start(IPAddress bindingAddress)
		{
			if (!NetworkAddress.IsValidBindingAddress(bindingAddress))
				Console.WriteLine("Warning: " + bindingAddress.ToString() + " may not be a valid binding address.");
			BuildSocket(bindingAddress);
		}

		public void Stop()
		{
			if (this.sock == null) return; //already stopped
			this.sock = null;
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				int bytesReceived = this.sock.EndReceive(ar);
				if (bytesReceived > 0)
				{
					MemoryStream ms = new MemoryStream(buf, 0, bytesReceived);
					int ret = ProcessPacket(ms);
					totalPackets++;
					totalPacketsResetable++;
				}
			}
			catch (SocketException ex)
			{
				Console.WriteLine("Socket exception on Callback! " + ex.Message);
				return;
			}

			this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
		}

		private void BuildSocket(IPAddress bindingAddress)
		{
			lock (this)
			{
				if (this.sock == null)
				{
					if (this.buf == null) this.buf = new byte[65536]; //max udp frame size
					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					this.sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
					
					this.sock.Bind(new IPEndPoint(bindingAddress, this.port));
					this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
				}
			}
		}

		private int ProcessPacket(MemoryStream ms)
		{
			T msg = default(T);
			try
			{
				msg = serializer.Deserialize(ms);
			}
			catch (Exception ex)
			{
				Console.WriteLine("WARNING: Exception Generic Unicast Client: (" + typeof(T).ToString() + ") :" + ex.Message);
				return 1;
			}

			if (MsgReceived != null)
				MsgReceived(this, new MsgReceivedEventArgs<T>(msg, this));
			return 0;
		}
	}
}
