using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Network;
using Magic.Common.Messages;
using Magic.Common.Sensors;
using Magic.Common;
using System.Drawing;
using System.Threading;
using Magic.Common.Mapack;
using System.IO;

namespace Magic.TargetListImages
{
	public class TargetListQuery
	{
		class TargetListPackage // for convenience & looks organized lol
		{
			public int robotID;
			public Dictionary<int, RobotImage> robotImage;
			public Dictionary<int, Vector2> pixelLTCorner;
			public Dictionary<int, Vector2> pixelRBCorner;
			public Dictionary<int, double> timeStamp;
			public Dictionary<int, TargetTypes> targetType;
			public Dictionary<int, ILidarScan<ILidar2DPoint>> lidarScan;

			public TargetListPackage()
			{
				robotImage = new Dictionary<int, RobotImage>();
				pixelLTCorner = new Dictionary<int, Vector2>();
				pixelRBCorner = new Dictionary<int, Vector2>();
				timeStamp = new Dictionary<int, double>();
				targetType = new Dictionary<int, TargetTypes>();
				lidarScan = new Dictionary<int, ILidarScan<ILidar2DPoint>>();
			}

			public RobotImage GetUnconfirmedRobotImage(ref int index, List<int> missingIDs)
			{
				foreach (KeyValuePair<int, TargetTypes> pair in targetType)
				{
					if (pair.Value == TargetTypes.Unconfirmed && !missingIDs.Contains(pair.Key))
					{
						index = pair.Key;
						if (robotImage.ContainsKey(pair.Key))
							return robotImage[pair.Key];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Information kept internally
		/// </summary>
		class TargetInformation
		{
			public int targetID;
			public RobotPose targetState;
			public Matrix targetCov;
			public RobotPose detectedPose;
			public TargetTypes type = TargetTypes.Unconfirmed;

			public TargetInformation(int targetID, RobotPose targetState, Matrix targetCov, RobotPose detectedPose, TargetTypes type)
			{
				this.targetID = targetID;
				this.targetState = targetState;
				this.targetCov = targetCov;
				this.detectedPose = detectedPose;
				this.type = type;
			}
		}

		INetworkAddressProvider addressProvider;
		GenericMulticastServer<TargetListMessage> targetListServer;
		GenericMulticastClient<TargetListNoImageMessage> targetNoImageClient;
		GenericMulticastClient<TargetQueryMessage> queryClient;
		GenericMulticastClient<TargetMessage> indivTargetClient;
		GenericMulticastServer<TargetList2CentralNodeMessage> targetToCentralServer;
		GenericMulticastClient<ConfirmOOIMessage> confirmOOIClient;
		GenericMulticastClient<ClearTargetListMessage> clearTargetListClient;
		GenericMulticastServer<UnconfirmedTargetNumberMessage> unconfirmedTargetNoServer;
		GenericMulticastServer<LidarPoseTargetMessage> detectionStateScanServer;

		ImageQueue imageQueue;
		Dictionary<int, TargetListPackage> robotIDToPackage;
		Dictionary<int, TargetInformation> associatedTargetID2Information;
		Dictionary<int, TargetInformation> targetIDToInformation;
		Dictionary<int, int> associationDictionary;
		List<int> targetIDfromTracker;
		List<int> missingTargets;

		bool isRunning = true;
		Random r = new Random();
		object locker = new object(); // locker

		public TargetListQuery()
		{
			InitializeNetwork();
			InitializeAlgorithm();
			Thread t = new Thread(UpdateDetectedPicture);
			t.Start();
		}

		#region Initialize Algorithm & Network
		void InitializeAlgorithm()
		{
			imageQueue = new ImageQueue(); // it has its own network setting for camera images broadcasted.
			robotIDToPackage = new Dictionary<int, TargetListPackage>();
			targetIDToInformation = new Dictionary<int, TargetInformation>();
			associatedTargetID2Information = new Dictionary<int, TargetInformation>();
			targetIDfromTracker = new List<int>();
			missingTargets = new List<int>();
			associationDictionary = new Dictionary<int, int>();
		}

		void InitializeNetwork()
		{
			addressProvider = new HardcodedAddressProvider();
			targetListServer = new GenericMulticastServer<TargetListMessage>(addressProvider.GetAddressByName("TargetList"), new CSharpMulticastSerializer<TargetListMessage>(true));
			targetNoImageClient = new GenericMulticastClient<TargetListNoImageMessage>(addressProvider.GetAddressByName("TargetListNoImg"), new CSharpMulticastSerializer<TargetListNoImageMessage>(true));
			queryClient = new GenericMulticastClient<TargetQueryMessage>(addressProvider.GetAddressByName("TargetListQuery"), new CSharpMulticastSerializer<TargetQueryMessage>(true));
			indivTargetClient = new GenericMulticastClient<TargetMessage>(addressProvider.GetAddressByName("TrackedTarget"), new CSharpMulticastSerializer<TargetMessage>(true));
			targetToCentralServer = new GenericMulticastServer<TargetList2CentralNodeMessage>(addressProvider.GetAddressByName("TargetListToCentral"), new CSharpMulticastSerializer<TargetList2CentralNodeMessage>(true));
			confirmOOIClient = new GenericMulticastClient<ConfirmOOIMessage>(addressProvider.GetAddressByName("ConfirmOOIMessage"), new CSharpMulticastSerializer<ConfirmOOIMessage>(true));
			clearTargetListClient = new GenericMulticastClient<ClearTargetListMessage>(addressProvider.GetAddressByName("ClearTarget"), new CSharpMulticastSerializer<ClearTargetListMessage>(true));
			unconfirmedTargetNoServer = new GenericMulticastServer<UnconfirmedTargetNumberMessage>(addressProvider.GetAddressByName("UnconfirmedTargetNumber"), new CSharpMulticastSerializer<UnconfirmedTargetNumberMessage>(true));
			detectionStateScanServer = new GenericMulticastServer<LidarPoseTargetMessage>(addressProvider.GetAddressByName("TargetDetectionLaserScan"), new CSharpMulticastSerializer<LidarPoseTargetMessage>(true));


			targetListServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
			targetToCentralServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
			targetNoImageClient.Start();
			queryClient.Start();
			indivTargetClient.Start();
			confirmOOIClient.Start();
			clearTargetListClient.Start();
			unconfirmedTargetNoServer.Start();
			detectionStateScanServer.Start();

			targetNoImageClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<TargetListNoImageMessage>>(targetNoImageClient_MsgReceived);
			queryClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<TargetQueryMessage>>(queryClient_MsgReceived);
			indivTargetClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<TargetMessage>>(indivTargetClient_MsgReceived);
			confirmOOIClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<ConfirmOOIMessage>>(confirmOOIClient_MsgReceived);
			clearTargetListClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<ClearTargetListMessage>>(clearTargetListClient_MsgReceived);
		}

		void clearTargetListClient_MsgReceived(object sender, MsgReceivedEventArgs<ClearTargetListMessage> e)
		{
			if (e.message.RobotID == 0)
			{
				robotIDToPackage.Clear();
				targetIDToInformation.Clear();
				associatedTargetID2Information.Clear();
                missingTargets.Clear();
				Console.WriteLine("Clear Targets message received! and cleared!");
			}
			else
				Console.WriteLine("Clear Targets message received! but didn't do anything.");
		}
		#endregion

		void confirmOOIClient_MsgReceived(object sender, MsgReceivedEventArgs<ConfirmOOIMessage> e)
		{
            lock (locker)
            {
                // when confirmation gets received, update the target information we have in TargetListPackage
                if (robotIDToPackage.ContainsKey(e.message.RobotID) && robotIDToPackage[e.message.RobotID].targetType.ContainsKey(e.message.TargetId))
                {
                    targetIDToInformation[e.message.TargetId].type = e.message.TargetType;
                    robotIDToPackage[e.message.RobotID].targetType[e.message.TargetId] = e.message.TargetType; // update the information
                }
                // also update the associated targetID with the confirmation
                if (associationDictionary.ContainsKey(e.message.TargetId))
                {
                    int associatedTargetIdx = associationDictionary[e.message.TargetId] % 1000;
                    int robotID = associationDictionary[e.message.TargetId] / 1000;
                    robotIDToPackage[robotID].targetType[associatedTargetIdx] = e.message.TargetType;
                }
            }
		}

		void indivTargetClient_MsgReceived(object sender, MsgReceivedEventArgs<TargetMessage> e)
		{
            //if (e.message.RobotID == 3) return;
			for (int i = 0; i < e.message.TargetState.Count; i++)
			{
				int targetID = e.message.RobotID * 1000 + i;
                lock (locker)
                {
                    if (!targetIDToInformation.ContainsKey(targetID))
                    {
                        targetIDToInformation.Add(targetID, new TargetInformation(targetID, e.message.TargetState[i],
                                                                    e.message.TargetCov[i], e.message.LastRobotPose[i], e.message.TargetTypes[i]));
                    }
                    else
                    {
                        targetIDToInformation[targetID].targetState = e.message.TargetState[i];
                        targetIDToInformation[targetID].targetCov = e.message.TargetCov[i];
                        targetIDToInformation[targetID].detectedPose = e.message.LastRobotPose[i];
                        if (targetIDToInformation[targetID].type == TargetTypes.Unconfirmed)
                            targetIDToInformation[targetID].type = e.message.TargetTypes[i];
                    }
                }
			}
		}

		void UpdateDetectedPicture(Object o)
		{
			int count = 0;
			while (isRunning)
			{
				Dictionary<int, TargetListPackage> copy = new Dictionary<int, TargetListPackage>(robotIDToPackage);
				foreach (KeyValuePair<int, TargetListPackage> pair in copy)
				{
					Dictionary<int, double> target2tsDict;
					lock (locker)
					{ target2tsDict = new Dictionary<int, double>(pair.Value.timeStamp); }
					foreach (KeyValuePair<int, double> target2ts in target2tsDict)
					{
						// make sure the detection time is already within the image queue
						double ts = imageQueue.GetMostRecentTimestamp(pair.Key);
						if (ts != -1 && target2ts.Value < ts)
						{
							RobotImage img = imageQueue.QueueClosestImage(pair.Key, target2ts.Value);
							if (img != null)
								pair.Value.robotImage[target2ts.Key] = img;
						}
					}
				}

				// existing targets association
				Dictionary<int, TargetInformation> dict = new Dictionary<int, TargetInformation>();
				if (targetIDToInformation.Count != 0)
				{
					//dict.Add(targetIDToInformation.Keys.ToList()[0], targetIDToInformation.Values.ToList()[0]);
                    Dictionary<int, TargetInformation> targetIDInformationCopy;
                    lock (locker)
                    {
                        targetIDInformationCopy = new Dictionary<int, TargetInformation>(targetIDToInformation);
                    }
					foreach (KeyValuePair<int, TargetInformation> pair in targetIDInformationCopy)
					{
						// find missing targets
						if (!targetIDfromTracker.Contains(pair.Key) && targetIDfromTracker.Count > 0 && !missingTargets.Contains(pair.Key))
							missingTargets.Add(pair.Key);
						if (targetIDfromTracker.Contains(pair.Key) && missingTargets.Contains(pair.Key))
							missingTargets.Remove(pair.Key);

						int dictLength = dict.Count; bool isWithinRange = false;
						int i = 0;
						for (i = 0; i < dictLength; i++) // go through the new list creating and check the distance
						{
							if (dict[dict.Keys.ToList()[i]].Equals(pair.Value) || missingTargets.Contains(pair.Key)) // if you're looking at the same object, ignore
								continue;
							double dist = dict[dict.Keys.ToList()[i]].targetState.ToVector2().DistanceTo(pair.Value.targetState.ToVector2());
							if (dist < 0.5 || dist == 0)
							{
								isWithinRange = true;
								break; // if you find anything within this range, break and skip this thing (do not add to the list)
							}
						}
						if (!isWithinRange)
							dict.Add(pair.Key, pair.Value);
						else // if the target is inside the range...
						{
							if (!associationDictionary.ContainsKey(pair.Key))
								associationDictionary.Add(pair.Key, dict.Keys.ToList()[i]); // make a dictionary that tells which targetID is which targetID
							// from another robot.
						}
					}
					associatedTargetID2Information = dict;
				}


                if (count >= 10)
                {
                    if (associatedTargetID2Information.Count != 0)
                    {
                        List<RobotPose> targetState = new List<RobotPose>(), detectedPose = new List<RobotPose>();
                        List<int> targetID = new List<int>(); ; List<Matrix> targetCov = new List<Matrix>(); List<TargetTypes> ooiTypes = new List<TargetTypes>();
                        foreach (KeyValuePair<int, TargetInformation> pair in associatedTargetID2Information)
                        {
                            if (pair.Value.type != TargetTypes.Junk)
                            {
                                targetState.Add(pair.Value.targetState);
                                targetCov.Add(pair.Value.targetCov);
                                targetID.Add(pair.Key);
                                detectedPose.Add(pair.Value.detectedPose);
                                ooiTypes.Add(pair.Value.type);
                            }
                        }
                        targetToCentralServer.SendUnreliably(new TargetList2CentralNodeMessage(targetState, targetCov, targetID, detectedPose, ooiTypes));
                        unconfirmedTargetNoServer.SendUnreliably(new UnconfirmedTargetNumberMessage(FindUnconfirmedTargetNumber(associatedTargetID2Information.Values.ToList()) - missingTargets.Count));
                        count = 0;
                    }
                    else
                    {
                        List<RobotPose> targetState = new List<RobotPose>(), detectedPose = new List<RobotPose>();
                        List<int> targetID = new List<int>(); ; List<Matrix> targetCov = new List<Matrix>(); List<TargetTypes> ooiTypes = new List<TargetTypes>();
                        targetToCentralServer.SendUnreliably(new TargetList2CentralNodeMessage(targetState, targetCov, targetID, detectedPose, ooiTypes));
                        unconfirmedTargetNoServer.SendUnreliably(new UnconfirmedTargetNumberMessage(FindUnconfirmedTargetNumber(associatedTargetID2Information.Values.ToList()) - missingTargets.Count));
                        count = 0;
                    }
                }
				count++;
				Thread.Sleep(10);
			}
		}

		private int FindUnconfirmedTargetNumber(List<TargetInformation> list)
		{
			int count = 0;
			foreach (TargetInformation info in list)
			{
				if (!(info.type == TargetTypes.ConfirmedMOOI || info.type == TargetTypes.ConfirmedSOOI || info.type == TargetTypes.Junk || info.type == TargetTypes.Meta))
					count++;
			}
			return count;
		}


		void targetNoImageClient_MsgReceived(object sender, MsgReceivedEventArgs<TargetListNoImageMessage> e)
		{
			int robotID = e.message.RobotID;
            //if (robotID == 3) return;
			if (!robotIDToPackage.ContainsKey(robotID))
				robotIDToPackage.Add(robotID, new TargetListPackage());

			for (int i = 0; i < e.message.TargetIDs.Count; i++)
			{
				lock (locker)
				{
					int targetID = e.message.TargetIDs[i];
					if (!robotIDToPackage[robotID].timeStamp.ContainsKey(targetID)) // if data doesn't exist yet
					{
						robotIDToPackage[robotID].timeStamp.Add(targetID, e.message.TimeStamp);
						robotIDToPackage[robotID].pixelLTCorner.Add(targetID, e.message.PixelLTCorner[i]);
						robotIDToPackage[robotID].pixelRBCorner.Add(targetID, e.message.PixelRBCorner[i]);
						robotIDToPackage[robotID].targetType.Add(targetID, TargetTypes.Unconfirmed);
						robotIDToPackage[robotID].robotID = robotID;
						robotIDToPackage[robotID].lidarScan.Add(targetID, e.message.LidarScan);
					}
					robotIDToPackage[robotID].timeStamp[targetID] = e.message.TimeStamp;
					robotIDToPackage[robotID].pixelLTCorner[targetID] = e.message.PixelLTCorner[i];
					robotIDToPackage[robotID].pixelRBCorner[targetID] = e.message.PixelRBCorner[i];
					robotIDToPackage[robotID].robotID = robotID;
					robotIDToPackage[robotID].lidarScan[targetID] = e.message.LidarScan;

					// checking missing stuff
					if (!targetIDfromTracker.Contains(targetID))
						targetIDfromTracker.Add(targetID);
				}
			}
			#region oldcode - remove when done
			//if (!robotIDToPackage.ContainsKey(robotID))
			//{
			//    TargetListPackage package = new TargetListPackage();
			//    package.robotID = e.message.RobotID;
			//    package.pixelLTCorner = e.message.PixelLTCorner;
			//    package.pixelRBCorner = e.message.PixelRBCorner;
			//    package.timeStamp = e.message.TimeStamp;
			//    robotIDToPackage.Add(robotID, package);
			//}
			//else
			//{
			//    robotIDToPackage[robotID].robotID = e.message.RobotID;
			//    robotIDToPackage[robotID].pixelLTCorner = e.message.PixelLTCorner;
			//    robotIDToPackage[robotID].pixelRBCorner = e.message.PixelRBCorner;
			//    robotIDToPackage[robotID].timeStamp = e.message.TimeStamp; 
			//}

			//List<int> targetID = new List<int>(e.message.PixelLTCorner.Count);
			//for (int i = 0; i < targetID.Capacity; i++)
			//    targetID.Add(robotID * 1000 + i); // target ID will be assigned as robotNumber * 1000 + localTargetID. i.e) for target 3 for Robot2 = 2003
			//robotIDToPackage[robotID].targetID = targetID;
			#endregion
		}

		void queryClient_MsgReceived(object sender, MsgReceivedEventArgs<TargetQueryMessage> e)
		{
			int robotIDRequested = e.message.RobotID; // this would be just zero... now let's reassign this.
			Bitmap gg;
			if (robotIDToPackage.Count == 0) // send anything. The robot ID would be zero.
			//if (!robotIDToPackage.ContainsKey(robotIDRequested))
			{
				Console.WriteLine("No detection found yet");
				string fileLocation = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()) + "\\noDetection.png";
				gg = new Bitmap(fileLocation);
				RobotImage noImg = new RobotImage(robotIDRequested, gg, 0.0);
				targetListServer.SendUnreliably(new TargetListMessage(robotIDRequested, noImg, 0.0, -1, TargetTypes.Junk));
				return;
			}
			int targetID = 0;
            //robotIDRequested = (int)Math.Ceiling(r.NextDouble() * robotIDToPackage.Count);
            //List<int> robotIDList = robotIDToPackage.Keys.ToList();
            robotIDRequested = robotIDToPackage.Keys.ToList()[(int)Math.Floor(r.NextDouble() * robotIDToPackage.Count)];
			RobotImage img = robotIDToPackage[robotIDRequested].GetUnconfirmedRobotImage(ref targetID, missingTargets);
			if (targetID == 0) Console.WriteLine("TargetID is not valid.");
			if (img == null)
			{
				Console.WriteLine("No Image Queue to process OR no more unconfirmed image");
				string fileLocation = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()) + "\\allConfirmed.png";
				gg = new Bitmap(fileLocation);
				RobotImage noImg = new RobotImage(robotIDRequested, gg, 0.0);
				targetListServer.SendUnreliably(new TargetListMessage(robotIDRequested, noImg, 0.0, -1, TargetTypes.Junk));
				return;
			}
			else
				gg = new Bitmap(img.image.Clone() as Image);

			//if (targetIDToInformation[targetID].targetCov[0, 0] > 0.5 && targetIDToInformation[targetID].targetCov[1, 1] > 0.5)
			//{
			//    Console.WriteLine("No Good Target with small covariance");
			//    string fileLocation = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()) + "\\noGoodTarget.png";
			//    gg = new Bitmap(fileLocation);
			//    RobotImage noImg = new RobotImage(robotIDRequested, gg, 0.0);
			//    targetListServer.SendUnreliably(new TargetListMessage(robotIDRequested, noImg, 0.0, -1));
			//    return;
			//}

			Graphics g = Graphics.FromImage(gg);

			Vector2 RBCorner = robotIDToPackage[robotIDRequested].pixelRBCorner[targetID];
			Vector2 LTCorner = robotIDToPackage[robotIDRequested].pixelLTCorner[targetID];
			//if (e.message.RobotID == 1)
			g.DrawRectangle(new Pen(Color.Red, 3.0f), new Rectangle((int)(LTCorner.X / 2 * 1) - 10, (int)((LTCorner.Y / 2 * 1) - 10),
								(int)(RBCorner.X / 2 - LTCorner.X / 2) * 1 + 10, (int)((RBCorner.Y - LTCorner.Y) / 2 * 1 + 10)));
			/*
			 * else
				g.DrawRectangle(new Pen(Color.Red, 3.0f), new Rectangle((int)LTCorner.X - 10, (int)LTCorner.Y - 10,
									(int)(RBCorner.X - LTCorner.X) + 10, (int)(RBCorner.Y - LTCorner.Y) + 10));
			*/
			Bitmap toSend = new Bitmap(160, 40);
			Graphics gr = Graphics.FromImage(toSend);
			gr.DrawImage(gg, 0, 0, 160, 40);

			RobotImage rImg = new RobotImage(img.robotID, toSend, img.timeStamp);
			targetListServer.SendUnreliably(new TargetListMessage(robotIDRequested, rImg, robotIDToPackage[robotIDRequested].timeStamp[targetID], targetID, targetIDToInformation[targetID].targetState.ToVector2(), targetIDToInformation[targetID].targetCov, targetIDToInformation[targetID].type));
			detectionStateScanServer.SendUnreliably(new LidarPoseTargetMessage(robotIDRequested, targetID, targetIDToInformation[targetID].detectedPose, robotIDToPackage[robotIDRequested].lidarScan[targetID]));


		}
	}
}
