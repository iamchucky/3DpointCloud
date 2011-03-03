using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Network;
using System.Net;
using System.Net.Sockets;

namespace Magic.LittleLaserInterface
{
	public class LittleLaser
	{
		private IPAddress ip;
		private int port = 20;
		private UdpClient udp;

		private byte[] onMsg = { 0xF8, 0x01 };
		private byte[] offMsg = { 0xF8, 0x00 };

		public LittleLaser(NetworkAddress na)
		{
			ip = na.Address;
			port = na.Port;
			udp = new UdpClient();
		}

		public void TurnOn()
		{
			udp.Send(onMsg, 2, new IPEndPoint(ip, port));
		}

		public void TurnOff()
		{
			udp.Send(offMsg, 2, new IPEndPoint(ip, port));
		}
	}
}
