using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Magic.Network;
using System.IO;

namespace Magic.Network
{
	/// <summary>
	/// Listens for byte streams on the network
	/// </summary>
	public class MulticastBytePacketClient
	{
		public event EventHandler<MsgReceivedEventArgs<MemoryStream>> MsgReceived;

		public IPAddress ip;
		public UInt16 port;
		private byte[] buf;
		private Socket sock;
		private string name;
		private int lastBytes = 0;
		int totalPackets = 0;
		int totalPacketsResetable = 0;

		public MulticastBytePacketClient(NetworkAddress address)
		{
			this.ip = address.Address;
			this.port = (UInt16)address.Port;
			this.name = address.Name;
			if (address.Protocol != NetworkAddressProtocol.UDP_MULTI && address.Protocol != NetworkAddressProtocol.ANY)
				Console.WriteLine("Warning: Using non UDP comaptible network address in Multicast Client (" + address.Name + ")");

		}

		/// <summary>
		/// Initializes listening on the port/ip given in the constructor. 
		/// Note that this particular call binds to the "most appropriate" endpoint
		/// which may be incorrect if you have multiple network cards
		/// </summary>
		public void Start()
		{
			BuildSocket(IPAddress.Any);
		}

		/// <summary>
		/// Initializes listening on the port/ip given in the constructor. 		
		/// </summary>
		/// <param name="bindingAddress">Allows specifying the binding address of the local adapter on which we should listen. See GetAllBindingAddresses(static) for values.</param>
		public void Start(IPAddress bindingAddress)
		{
			if (!NetworkAddress.IsValidBindingAddress(bindingAddress))
				Console.WriteLine("Warning: " + bindingAddress.ToString() + " may not be a valid binding address.");
			BuildSocket(bindingAddress);
		}

		/// <summary>
		/// Closes the underlying socket and drops multicast group memebership.
		/// </summary>
		public void Stop()
		{
			if (this.sock == null) return; //already stopped

			this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(this.ip));
			this.sock = null;
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				int bytesReceived = this.sock.EndReceive(ar);
				lastBytes = bytesReceived;
				if (bytesReceived > 0)
				{
					MemoryStream ms = new MemoryStream(buf);
					ms.SetLength(bytesReceived);
					int ret = ProcessPacket(ms);
					totalPackets++;
					totalPacketsResetable++;
				}

				this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
			}
			catch (NullReferenceException)
			{
				//Socket already closed
			}
		}
		private void BuildSocket(IPAddress bindingAddress)
		{
			lock (this)
			{
				if (this.sock == null)
				{
					if (this.buf == null)
						this.buf = new byte[65536]; //max udp frame size

					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					this.sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
					this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 10);
					this.sock.Bind(new IPEndPoint(bindingAddress, this.port));
					this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.ip, bindingAddress));
					this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
				}
			}
		}
		/// <summary>
		/// Triggers the MsgReceived event
		/// </summary>
		/// <param name="ms"></param>
		/// <returns></returns>
		private int ProcessPacket(MemoryStream ms)
		{
			if (MsgReceived != null)
				MsgReceived(this, new MsgReceivedEventArgs<MemoryStream>(ms, this));

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>A formatted string of relevant information</returns>
		public string GetInfo()
		{
			return String.Format("{0,-25}{1,-8}{2,17}  :{3,7}", name, lastBytes, ip, port);  // name + "   " + lastBytes + " " + ip + " : " + port;
		}
	}
}