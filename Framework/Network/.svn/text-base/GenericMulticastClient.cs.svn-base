using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Magic.Network
{
	/// <summary>
	/// Allows receiving any single tyep of MulticastMessageType Message over Multicast UDP
	/// This also 
	/// </summary>
	/// <typeparam name="T">The type of the message to receive</typeparam>	
	public class GenericMulticastClient<T>
	{
		public event EventHandler<MsgReceivedEventArgs<T>> MsgReceived;

		private IPAddress ip;
		private Int32 port;
		private byte[] buf;
		private Socket sock;

		private IMulticastSerializer<T> serializer;
		int totalPackets = 0;
		int totalPacketsResetable = 0;
		/// <summary>
		/// This is the generic UDP Multicast receiver interface for messages based on a Multiast Serializer.
		/// </summary>
		/// <param name="address">The network address of the multicast endpoint</param>
		public GenericMulticastClient(NetworkAddress address, IMulticastSerializer<T> serializer)
		{
			this.ip = address.Address; this.port = address.Port; this.serializer = serializer;
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


					//IPAddress wireless = (NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wireless));
					//if (bindingAddress.Equals(wireless) && (wireless.Equals(IPAddress.Any) == false) && MultiplexerUtil.USE_MULTIPLEXER)
					if (NetworkAddress.IsRobotWirelessIP(bindingAddress) && MultiplexerUtil.USE_MULTIPLEXER)
					{
						Console.WriteLine("USING MULTIPLEXER for " + ip);
						this.sock.Bind(new IPEndPoint(bindingAddress, this.port));
						this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
					}
					else
					{
						this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 10);
						this.sock.Bind(new IPEndPoint(bindingAddress, this.port));
						this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.ip, bindingAddress));
						this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
					}
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
				Console.WriteLine("WARNING: Exception Generic Multicast Client: (" + typeof(T).ToString() + ") :" + ex.Message);
				return 1;
			}

			if (MsgReceived != null)
				MsgReceived(this, new MsgReceivedEventArgs<T>(msg, this));
			return 0;
		}
	}
}
