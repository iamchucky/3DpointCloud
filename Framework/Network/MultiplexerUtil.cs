using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Magic.Network
{
	/// <summary>
	/// These are essentially just global settings that enable the multiplexer everywhere
	/// in the networking code.
	/// </summary>
	static class MultiplexerUtil
	{
		public static IPAddress MULTIPLEXER_WIRELESS_IP = IPAddress.Parse("10.0.0.8");
		public static IPAddress MULTIPLEXER_WIRED_IP = IPAddress.Parse("10.0.0.7");
		public static bool USE_MULTIPLEXER = true;

		static MultiplexerUtil()
		{
			if (USE_MULTIPLEXER)
				Console.WriteLine("Using Multiplexer @ Wireless " + MULTIPLEXER_WIRELESS_IP + " @ Wireed " + MULTIPLEXER_WIRED_IP);
		}
	}
}
