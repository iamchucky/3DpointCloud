using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Magic.Network
{
	/// <summary>
	/// Provides a IP address, port and protocol which defines an entity on the network
	/// </summary>
	public class NetworkAddress
	{
		public const int ROBOT_IP_OFFSET = 90; //i.e. robot 1 is 10.0.0.91 and 192.168.1.91
        public const int ROBOT_IP_OFFSET2 = 100;
		public const int NUMBER_ROBOTS = 5;
		/// <summary>
		/// The starting ID  for sim robots on this machine
		/// </summary>
		private static int startingID = 86;

		/// <summary>
		/// This process's id when running the sim
		/// </summary>
		private static int simRobotID;

		/// <summary>
		/// Hold the mutex
		/// </summary>
		private static Mutex shizazzleMutex;

		/// <summary>
		/// The maximum number of robots to run on one machine
		/// </summary>
		private static int maxMutexes = 3;

		public static bool printedDebugOnce = true;
		/// <summary>
		/// Returns all the possible binding addresses of the computer.
		/// </summary>
		/// <returns></returns>
		static public ICollection<IPAddress> GetAllBindingAddresses()
		{
			// Get host name
			String strHostName = Dns.GetHostName();
			if (!printedDebugOnce) Console.WriteLine("Host Name: " + strHostName);

			// Find host by name
			IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
			// Enumerate IP addresses
			int nIP = 0;
			List<IPAddress> l = new List<IPAddress>();
			foreach (IPAddress ipaddress in iphostentry.AddressList)
			{
				if (!printedDebugOnce) Console.WriteLine("IP #" + ++nIP + ": " + ipaddress.ToString());
				l.Add(ipaddress);
			}
			l.Add(IPAddress.Any);
			printedDebugOnce = true;
			return l;

		}

		/// <summary>
		/// Gives the local binding address associated with the first two octects.
		/// For example, if the octect1= 192 and octet2 = 168, and the computer has an IP
		/// of 192.168.1.3, it will return 192.168.1.3. 
		/// If no match is found, it will return IPAddress.Any
		/// </summary>
		/// <param name="octect1">The first octet (i.e. 192 in 192.168.1.1)</param>
		/// <param name="octet2">The second octet (i.e. 168 in 192.168.1.1)</param>
		/// <returns></returns>
		static public IPAddress GetBindingAddressByOctet(byte octet1, byte octet2)
		{
			IPAddress ret = IPAddress.Any;
			List<IPAddress> candidatesMatch1 = new List<IPAddress>();
			List<IPAddress> candidatesMatch2 = new List<IPAddress>();
			foreach (IPAddress ip in GetAllBindingAddresses())
			{
				byte[] arr = ip.GetAddressBytes();
				if (arr[0] == octet1 && arr[1] == octet2)
					candidatesMatch2.Add(ip);
				else if (arr[0] == octet1)
					candidatesMatch1.Add(ip);
			}
			if (candidatesMatch2.Count > 0)
				return candidatesMatch2[0];
			if (candidatesMatch1.Count > 0)
				return candidatesMatch1[0];
			else
				return ret;
		}

		public static bool IsValidBindingAddress(IPAddress address)
		{
			foreach (IPAddress ip in GetAllBindingAddresses())
			{
				if (ip.Equals(address)) return true;
			}
			return false;
		}
		public enum BindingType
		{
			Wired, Wireless
		}
		static public IPAddress GetBindingAddressByType(BindingType type)
		{
			switch (type)
			{
				case BindingType.Wired: return GetBindingAddressByOctet(192, 168);
				case BindingType.Wireless: return GetBindingAddressByOctet(10, 0);
				default: return GetBindingAddressByOctet(0, 0);
			}
		}

	    public static IPAddress GetRobotWirelessIPAddressByNumber(int robotID)
		{
			int x = ROBOT_IP_OFFSET + robotID;
			return IPAddress.Parse("10.0.0." + x.ToString());
		}

        public static IPAddress GetRobotWirelessIPAddressByNumber2(int robotID)
        {
            int x = ROBOT_IP_OFFSET2 + robotID;
            return IPAddress.Parse("10.0.0." + x.ToString());
        }

		public static bool IsRobotWirelessIP(IPAddress ip)
		{
			bool ret = false;
			for (int i = 1; i <= NUMBER_ROBOTS; i++)
			{
                if (GetRobotWirelessIPAddressByNumber(i).Equals(ip) || GetRobotWirelessIPAddressByNumber2(i).Equals(ip)) return true;
			}
			return ret;
		}
		private IPAddress address;

		public IPAddress Address
		{
			get { return address; }
			set { address = value; }
		}

		private int port;

		public int Port
		{
			get { return port; }
			set { port = value; }
		}

        private int altPort;

        public int AltPort
        {
            get { return altPort; }
            set { altPort = value; }
        }

		/// <summary>
		/// This makes the very strong assumption that the computer's last two numbers are corresponding the robotID.
		/// i.e. spider01 will be robot 1. use this improperly, and nyk will die.
		/// </summary>
		/// <returns></returns>
		public static int GetRobotIDByHostname()
		{
			String strHostName = Dns.GetHostName();
			String shizzazzle = strHostName.Substring(strHostName.Length - 2, 2);
			//Console.WriteLine("attempting to extract from " + shizzazzle);
			int robotID = 0;
			if (Int32.TryParse(shizzazzle, out robotID))
				return robotID;

			return 0;
			/*else if (hostNamesToIDs.ContainsKey(strHostName))
				return hostNamesToIDs[strHostName];

			//Set the starting id to your ip address
			String addr = GetBindingAddressByOctet(10, 0).ToString();
			startingID = Int32.Parse(addr.Substring(addr.LastIndexOf('.') + 1));
			hostNamesToIDs.Add(strHostName, startingID);
			return startingID++;*/
		}

		/// <summary>
		/// Returns a unique id for multiple simulated robots on the same machine
		/// </summary>
		/// <param name="processName">The name of the process attemtping to sim. Eg. "behavioral"</param>
		/// <returns></returns>
		public static int GetSimRobotID(string processName)
		{
			if (shizazzleMutex == null)
			{
				for (int i = 0; i < maxMutexes; i++)
				{
					bool created;
					Mutex attempt = new Mutex(true, processName + "shizazzle" + (i + startingID), out created);
					if (created)
					{
						shizazzleMutex = attempt;
						simRobotID = i + startingID;
						break;
					}
				}
				//Too many robots!! Adjust the maxMutexes field to enable more per machine
				if (shizazzleMutex == null)
					System.Environment.Exit(0);
			}
			return simRobotID;
		}

		private NetworkAddressProtocol protocol;

		public NetworkAddressProtocol Protocol
		{
			get { return protocol; }
			set { protocol = value; }
		}

		private string name;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

        public NetworkAddress(string name, IPAddress address, int port, NetworkAddressProtocol protocol)
            : this(name, address, port, port, protocol)
        {
        }

		public NetworkAddress(string name, IPAddress address, int port, int altPort, NetworkAddressProtocol protocol)
		{
            this.address = address; this.port = port; this.altPort = altPort; this.protocol = protocol; this.name = name;
		}

		public override string ToString()
		{
			return name + " (" + protocol.ToString() + ")";
		}

		public IPEndPoint ToIPEndPoint()
		{
			return new IPEndPoint(address, port);
		}
	}

	/// <summary>
	/// Defines the type of protocol used on this Network Address. Warning: this may not necessairly be obeyed by a communicator
	/// </summary>
	public enum NetworkAddressProtocol
	{
		TCP,
		UDP_MULTI,
		UDP_UNI,
		ANY
	}
}
