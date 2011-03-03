using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Magic.Network
{
	/// <summary>
	/// This is the global "configuation" of all local ip addresses and ports on the network
	/// </summary>
	public class HardcodedAddressProvider : INetworkAddressProvider
	{
		List<String> messagesMulticast = new List<String>();
		List<NetworkAddress> addresses = new List<NetworkAddress>();
		public HardcodedAddressProvider()
		{
			#region what's this?
			/*addresses.Add(new NetworkAddress("SegwayControl", IPAddress.Parse("192.168.1.20"), 20, NetworkAddressProtocol.UDP_UNI));
			addresses.Add(new NetworkAddress("SegwayFeedback", IPAddress.Parse("239.132.1.20"), 30020, NetworkAddressProtocol.UDP_MULTI));

			//Local Addresses on the robot local network
			#region Local Addresses

			//messagesMulticast.Add("SegwayFeedback");
			messagesMulticast.Add("Sick");
			messagesMulticast.Add("SeptentrioData");
			messagesMulticast.Add("MessageExample");
			messagesMulticast.Add("LidarScan");
			messagesMulticast.Add("RobotBehavioralPath");
			messagesMulticast.Add("LocalObstacles");
			messagesMulticast.Add("RTKCorrections");
			messagesMulticast.Add("DynamicObstacles");
			messagesMulticast.Add("OccupancyGrid");
			messagesMulticast.Add("UDPCamera");
			messagesMulticast.Add("RobotPose");
			messagesMulticast.Add("RobotState");
			messagesMulticast.Add("LidarPosePackage");
			messagesMulticast.Add("PossibleOOI");
			messagesMulticast.Add("PlannerStatus");
			messagesMulticast.Add("Teleop");
			messagesMulticast.Add("SetRobotMode");

			#endregion

			#region OOI Communications

			messagesMulticast.Add("PossibleOOIPoints");
			messagesMulticast.Add("OOITargets");
			messagesMulticast.Add("OOIPoints");

			#endregion

			#region HRI

			messagesMulticast.Add("RobotPath");
			messagesMulticast.Add("Estop");
			messagesMulticast.Add("HRIPDFMessage");
			messagesMulticast.Add("ConfirmOOIMessage");

			#endregion

			#region Pose

			messagesMulticast.Add("RobotPoseGPS");
			messagesMulticast.Add("RobotPoseODOM");
			messagesMulticast.Add("RobotPoseTRUTH");
			messagesMulticast.Add("RobotPoseFILTER");
			messagesMulticast.Add("PoseFilterState");

			#endregion

			#region Target Tracking

			messagesMulticast.Add("TrackedTarget");
			
			#endregion

			#region Global Map, Central Node, Central Sensor

			messagesMulticast.Add("GlobalPDF");
			messagesMulticast.Add("Waypoints");
			messagesMulticast.Add("GlobalGaussianMixMap");
			messagesMulticast.Add("LocalMapRequest");
			messagesMulticast.Add("LocalMapResponse");

			#endregion

			#region Sim

			messagesMulticast.Add("RobotSimCommands");
			messagesMulticast.Add("SimLidar");
			messagesMulticast.Add("SimPose");
			messagesMulticast.Add("SimLidarScan");
			messagesMulticast.Add("SimBehavioralPath");
			messagesMulticast.Add("SimObstacles");
			messagesMulticast.Add("SimLidarPosePackage");
			messagesMulticast.Add("SimSegwayFeedback");

			#endregion

			for (int i = 0; i < messagesMulticast.Count; i++)
			{
				bool addressFound = false;

				foreach (NetworkAddress na in addresses)
				{
					if (na.Port == 30000 + i)
					{
						addressFound = true;
						break;
					}
				}
				
				if (!addressFound)
					addresses.Add(new NetworkAddress(messagesMulticast[i], IPAddress.Parse("239.132.1." + i.ToString()), 30000 + i, NetworkAddressProtocol.UDP_MULTI));
			}*/

			#endregion
			//it is EXTREMELY important that the ports are unique in addition to the IP adddresses. 
			//this is an assumption used in the udp multicast multiplexer
			addresses.Add(new NetworkAddress("SegwayFeedback", IPAddress.Parse("239.132.1.20"), 30020, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SegwayControl", IPAddress.Parse("192.168.1.20"), 20, NetworkAddressProtocol.UDP_UNI));
			addresses.Add(new NetworkAddress("IMU", IPAddress.Parse("239.132.1.22"), 30022, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("Sick", IPAddress.Parse("239.132.1.16"), 30016, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SeptentrioData", IPAddress.Parse("239.132.1.17"), 30017, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("MessageExample", IPAddress.Parse("239.132.2.1"), 30001, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("LidarScan", IPAddress.Parse("239.132.1.101"), 30101, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotBehavioralPath", IPAddress.Parse("239.132.1.104"), 30104, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("LocalObstacles", IPAddress.Parse("239.132.1.105"), 30105, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("BloatedObstacles", IPAddress.Parse("239.132.1.135"), 30135, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RTKCorrections", IPAddress.Parse("239.132.1.113"), 30113, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("DynamicObstacles", IPAddress.Parse("239.132.1.115"), 30115, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("OccupancyGrid", IPAddress.Parse("239.132.1.21"), 30021, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("UDPCamera", IPAddress.Parse("239.132.1.99"), 30099, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("UDPCameraColor", IPAddress.Parse("239.132.1.98"), 30098, NetworkAddressProtocol.UDP_MULTI));
            addresses.Add(new NetworkAddress("SharedMemoryCamera", IPAddress.Parse("239.132.1.199"), 30199, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("DetectionImage", IPAddress.Parse("239.132.1.97"), 30097, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotPose", IPAddress.Parse("239.132.1.103"), 30103, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotPoseRebroadcast", IPAddress.Parse("239.132.1.136"), 30136, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotState", IPAddress.Parse("239.132.1.23"), 30023, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("LidarPosePackage", IPAddress.Parse("239.132.1.116"), 30116, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("PossibleOOI", IPAddress.Parse("239.132.1.117"), 30117, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("PlannerStatus", IPAddress.Parse("239.132.1.118"), 30118, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("Teleop", IPAddress.Parse("239.132.1.121"), 30121, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SetRobotMode", IPAddress.Parse("239.132.1.124"), 30124, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotCommand", IPAddress.Parse("239.132.1.128"), 30128, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("LidarFilterPackage", IPAddress.Parse("239.132.1.129"), 30129, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("DriveMap", IPAddress.Parse("239.132.1.131"), 30131, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotSparsePath", IPAddress.Parse("239.132.1.137"), 30137, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("HRIRobotCommand", IPAddress.Parse("239.132.1.138"), 30138, NetworkAddressProtocol.UDP_MULTI));
            addresses.Add(new NetworkAddress("HRICloseAll", IPAddress.Parse("239.132.1.141"), 30141, NetworkAddressProtocol.UDP_MULTI));

			//TESTING
			//addresses.Add(new NetworkAddress("UDPCamera", IPAddress.Parse("10.0.0.9"), 30099, NetworkAddressProtocol.UDP_MULTI));
			//messagesMulticast.Add("UDPCamera");

			//OOI communications
			addresses.Add(new NetworkAddress("PossibleOOIPoints", IPAddress.Parse("239.132.2.2"), 30002, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("OOITargets", IPAddress.Parse("239.132.2.3"), 30003, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("OOIPoints", IPAddress.Parse("239.132.2.4"), 30004, NetworkAddressProtocol.UDP_MULTI));

			//HRI
			addresses.Add(new NetworkAddress("RobotPath", IPAddress.Parse("239.132.1.102"), 30102, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("Estop", IPAddress.Parse("239.132.1.100"), 30100, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("HRIPDFMessage", IPAddress.Parse("239.132.1.112"), 30112, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("ConfirmOOIMessage", IPAddress.Parse("239.132.1.133"), 30133, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("TargetDetectionLaserScan", IPAddress.Parse("239.132.1.142"), 30142, NetworkAddressProtocol.UDP_MULTI));

			//Pose UI
			addresses.Add(new NetworkAddress("RobotPoseGPS", IPAddress.Parse("239.132.1.106"), 30106, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotPoseODOM", IPAddress.Parse("239.132.1.107"), 30107, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotPoseTRUTH", IPAddress.Parse("239.132.1.108"), 30108, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("RobotPoseFILTER", IPAddress.Parse("239.132.1.109"), 30109, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("PoseFilterState", IPAddress.Parse("239.132.1.122"), 30122, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("IMUData", IPAddress.Parse("239.132.1.134"), 30134, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SLAMEstimate", IPAddress.Parse("239.132.1.221"), 30221, NetworkAddressProtocol.UDP_MULTI));

			//Target tracking stuff
			addresses.Add(new NetworkAddress("TrackedTarget", IPAddress.Parse("239.132.1.123"), 30123, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("TargetList", IPAddress.Parse("239.132.1.125"), 30125, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("TargetListNoImg", IPAddress.Parse("239.132.1.126"), 30126, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("TargetListWithImg", IPAddress.Parse("239.132.1.132"), 30132, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("TargetListQuery", IPAddress.Parse("239.132.1.127"), 30127, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("TargetListToCentral", IPAddress.Parse("239.132.1.130"), 30130, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("ClearTarget", IPAddress.Parse("239.132.1.139"), 30139, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("UnconfirmedTargetNumber", IPAddress.Parse("230.132.1.140"), 30140, NetworkAddressProtocol.UDP_MULTI));

			//Global Map & Central Node & Central sensor stuff
			addresses.Add(new NetworkAddress("GlobalPDF", IPAddress.Parse("239.132.1.110"), 30110, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("Waypoints", IPAddress.Parse("239.132.1.111"), 30111, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("GlobalGaussianMixMap", IPAddress.Parse("239.132.1.114"), 30114, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("LocalMapRequest", IPAddress.Parse("239.132.1.119"), 30119, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("LocalMapResponse", IPAddress.Parse("239.132.1.120"), 30120, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("GlobalObstacles", IPAddress.Parse("239.132.1.144"), 30144, NetworkAddressProtocol.UDP_MULTI));

			//Sim Addresses
			addresses.Add(new NetworkAddress("RobotSimCommands", IPAddress.Parse("239.132.1.208"), 30208, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimLidar", IPAddress.Parse("239.132.1.209"), 30209, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimPose", IPAddress.Parse("239.132.1.210"), 30210, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimLidarScan", IPAddress.Parse("239.132.1.211"), 30211, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimBehavioralPath", IPAddress.Parse("239.132.1.212"), 30212, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimObstacles", IPAddress.Parse("239.132.1.213"), 30213, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimLidarPosePackage", IPAddress.Parse("239.132.1.214"), 30214, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimSegwayFeedback", IPAddress.Parse("239.132.1.215"), 30215, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("SimIMU", IPAddress.Parse("239.132.1.216"), 30216, NetworkAddressProtocol.UDP_MULTI));

			// Neutralization communication between CN and HRI
			addresses.Add(new NetworkAddress("NeutralizationStarted", IPAddress.Parse("239.132.1.217"), 30217, NetworkAddressProtocol.UDP_MULTI));
			addresses.Add(new NetworkAddress("NeutralizationEnded", IPAddress.Parse("239.132.1.218"), 30218, NetworkAddressProtocol.UDP_MULTI));
            addresses.Add(new NetworkAddress("NeutralizeWithLaser", IPAddress.Parse("239.132.1.143"), 30143, NetworkAddressProtocol.UDP_MULTI));

			// Targets and blast radii as polygons
			addresses.Add(new NetworkAddress("BlastAreas", IPAddress.Parse("239.132.1.219"), 30219, NetworkAddressProtocol.UDP_MULTI));

			// Height Features
			addresses.Add(new NetworkAddress("HeightFeatures", IPAddress.Parse("239.132.1.220"), 30220, NetworkAddressProtocol.UDP_MULTI));
		}

		#region INetworkAddressProvider Members

		public NetworkAddress GetAddressByName(string name)
		{
			foreach (NetworkAddress na in addresses)
				if (na.Name.ToLower().Equals(name.ToLower())) return na;
			throw new NetworkAddressProviderNotFoundException(name);
		}

		public bool NameExists(string name)
		{
			foreach (NetworkAddress na in addresses)
				if (na.Name.ToLower().Equals(name.ToLower())) return true;
			return false;
		}

		public List<NetworkAddress> NetworkAddresses
		{
			get { return addresses; }
		}

		public string GetIdentifierString { get { return "Hardcoded Network Address Provider"; } }
		#endregion
	}
}
