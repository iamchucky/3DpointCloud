using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Magic.Network
{
	public class GenericMulticastServer<T>
	{
		private IPAddress ip;
		private Int32 port;
		private Socket sock;
		private IMulticastSerializer<T> serializer;
		int totalPackets = 0;
		bool multiplexMode = false;

		/// <summary>
		/// This is the generic UDP Multicast sender interface for messages based on the multicast serializer.
		/// </summary>
		/// <param name="address">The network address of the multicast endpoint</param>
		public GenericMulticastServer(NetworkAddress address, IMulticastSerializer<T> serializer)
		{
			this.ip = address.Address; this.port = address.Port; this.serializer = serializer;
			if (address.Protocol != NetworkAddressProtocol.UDP_MULTI && address.Protocol != NetworkAddressProtocol.ANY)
				Console.WriteLine("Warning: Using non UDP comaptible network address in Multicast Server (" + address.Name + ")");

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

		private void BuildSocket(IPAddress bindingAddress)
		{
			lock (this)
			{
				if (this.sock == null)
				{
					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					this.sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

					//IPAddress wireless = (NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wireless));
					//if (bindingAddress.Equals(wireless) && (wireless.Equals(IPAddress.Any) == false) && MultiplexerUtil.USE_MULTIPLEXER)
					if (NetworkAddress.IsRobotWirelessIP (bindingAddress) && MultiplexerUtil.USE_MULTIPLEXER)
					{
						Console.WriteLine("Using Multiplexer for IP : " + ip);
						//Why is this here?
						//this.sock.Bind(new IPEndPoint(bindingAddress, this.port));
						multiplexMode = true;
					}
					else
					{
						this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 10);
						this.sock.Bind(new IPEndPoint(bindingAddress, this.port));
						this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.ip, bindingAddress));
						multiplexMode = false;
                        //multiplexMode = true;
					}
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
			if (multiplexMode)
			{
				this.sock.SendTo(binary, new IPEndPoint(MultiplexerUtil.MULTIPLEXER_WIRELESS_IP, this.port));
				//Console.WriteLine("using mux");
			}
			else
			{
				this.sock.SendTo(binary, new IPEndPoint(this.ip, this.port));
				//Console.WriteLine("not mux");
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
