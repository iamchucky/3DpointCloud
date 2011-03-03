using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Magic.Network
{
	public class GenericUnicastServer<T>
	{
		private IPAddress remoteIP;
		private Int32 port;
		private Socket sock;
		private IMulticastSerializer<T> serializer;
		int totalPackets = 0;

		public GenericUnicastServer(IPAddress remoteAddress, NetworkAddress address, IMulticastSerializer<T> serializer)
		{
			this.remoteIP = remoteAddress;
			this.port = address.Port;
			this.serializer = serializer;
		}

		public void Start()
		{
			BuildSocket();
		}

		public void Start(IPAddress bindingAddress)
		{
			BuildSocket();
		}

		public void Stop()
		{
			if (this.sock == null) return; //already stopped
			this.sock = null;
		}

		private void BuildSocket()
		{
			lock (this)
			{
				if (this.sock == null)
				{
					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					this.sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				}
			}
		}

		public bool SendUnreliably(T item)
		{
			if (sock == null) return false;
			//send the shit over udp
			byte[] binary = serializer.Serialize(item);
			if (binary.Length > 60000)
			{
				Console.WriteLine("The message attempted to be sent is too large. It is : " + binary.Length + " bytes.");
				return false;
			}
			else
			{
				this.sock.SendTo(binary, new IPEndPoint(this.remoteIP, this.port));
			}
			totalPackets++;
			return true;
		}

		public void SendReliably(T message)
		{
			if (serializer.SupportsReliable)
			{
				throw new NotImplementedException();
			}
			else
			{
				throw new NotSupportedException();
			}
		}
	}
}
