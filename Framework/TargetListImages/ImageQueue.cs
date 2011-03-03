using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Sensors;
using Magic.Network;
using Magic.Common.Sensors;
using System.Threading;

namespace Magic.TargetListImages
{
	public class ImageQueue
	{
		UDPCamera camera;
		INetworkAddressProvider addressProvider;
		Dictionary<int, List<RobotImage>> robotIDToImageList;
		Dictionary<int, RobotImage> robotImageDict;
		Dictionary<int, double> mostRecentTimeStamp;
		bool isRunning = true;

		public ImageQueue()
		{
			InitializeNetwork();
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			robotIDToImageList = new Dictionary<int, List<RobotImage>>();
			robotImageDict = new Dictionary<int, RobotImage>();
			mostRecentTimeStamp = new Dictionary<int, double>();
		}

		private void InitializeNetwork()
		{
			addressProvider = new HardcodedAddressProvider();
			camera = new UDPCamera(addressProvider.GetAddressByName("UDPCamera"));
			camera.ImageReceived += new EventHandler<Magic.Common.TimestampedEventArgs<Magic.Common.Sensors.RobotImage>>(camera_ImageReceived);
			camera.Start();
		}

		void camera_ImageReceived(object sender, Magic.Common.TimestampedEventArgs<Magic.Common.Sensors.RobotImage> e)
		{
			int robotID = e.Message.robotID;
			if (robotID >= 5)
				robotID -= 4;

			// e.Timestamp is not the actual time stamp you want !!
			if (!robotIDToImageList.ContainsKey(robotID))
			{
				robotIDToImageList.Add(robotID, new List<RobotImage>(200));
				mostRecentTimeStamp.Add(robotID, e.Message.timeStamp);
			}

			if (robotIDToImageList[robotID].Count == robotIDToImageList[robotID].Capacity)
				robotIDToImageList[robotID].RemoveAt(0);
			robotIDToImageList[robotID].Add(e.Message);

			if (!robotImageDict.ContainsKey(robotID))
				robotImageDict.Add(robotID, robotIDToImageList[robotID][robotIDToImageList[robotID].Count - 1]);
			mostRecentTimeStamp[robotID] = e.Message.timeStamp; // keep track of the most recent timestamp
		}

		public double GetMostRecentTimestamp(int robotID)
		{
			if (mostRecentTimeStamp != null && mostRecentTimeStamp.ContainsKey(robotID))
				return mostRecentTimeStamp[robotID];
			else return -1;
		}

		public RobotImage QueueClosestImage(int robotID, double timeStamp)
		{
			if (!robotIDToImageList.ContainsKey(robotID))
				return null;
			int closestIndex = 0;
			List<RobotImage> imageList = null;
			try
			{
				imageList = new List<RobotImage>(robotIDToImageList[robotID]);
				while (imageList == null)
				{
					imageList = new List<RobotImage>(robotIDToImageList[robotID]);
					Console.WriteLine("NO GOOD STUFF");
				}
				double diff = double.MaxValue;
				foreach (RobotImage img in imageList)
				{
					if (img == null) continue;
					if (Math.Abs(img.timeStamp - timeStamp) < diff)
					{
						diff = Math.Abs(img.timeStamp - timeStamp);
						closestIndex = imageList.IndexOf(img);
					}
				}
				if (closestIndex == 0)
				{
                    //return robotImageDict[robotID];
                    return null;
				}
			}
			catch { return null; }
			robotImageDict[robotID] = imageList[closestIndex];
			return imageList[closestIndex];


		}

	}
}
