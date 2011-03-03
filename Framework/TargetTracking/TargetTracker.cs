using System;
using System.Collections.Generic;
using System.Threading;
using Magic.Common;
using Magic.Common.Mapack;
using Magic.Common.Messages;
using Magic.Common.Sensors;

namespace Magic.TargetTracking
{
	public class TargetTracker : IDisposable
	{
		List<Vector2> currentTargetPose;
		List<TargetTrackingImpl> implList; // for each target
		int robotID;

		// debugging purposes
		public List<Vector2> zSCRMean = new List<Vector2>();
		public List<Matrix> zSCRCov = new List<Matrix>();
		public List<Vector2> xyPosList = new List<Vector2>();

		double cameraTimeStamp = 0.05;
		//int count = 0;
		public List<TargetTrackingImpl> trackerList
		{ get { return implList; } }

		//Messaging messaging;

		double sigma_f;
		string screenSize;
		double staticDistanceThreshold;
		double dynamicDistanceThreshold;
		double cameraProcessingTimeGuess = 0;
		double dT;

		bool isRunning = true;
		object locker = new object();

		/// <summary>
		/// Wrapper class for target tracking
		/// </summary>
		/// <param name="sigma_f">sigma_f = 3</param>
		/// <param name="distanceThreshold">To distinguish between different targets</param>
		/// <param name="cameraSize">either "640x480", "320x240", or "960x240"</param>
		public TargetTracker(double sigma_f, double staticDistanceThreshold, double dynamicDistanceThreshold, double dT, string cameraSize, int robotID)
		{
			this.robotID = robotID;
			this.sigma_f = sigma_f;
			this.screenSize = cameraSize;
			this.staticDistanceThreshold = staticDistanceThreshold;
			this.dynamicDistanceThreshold = dynamicDistanceThreshold;
			this.dT = dT;
			currentTargetPose = new List<Vector2>(100);

			implList = new List<TargetTrackingImpl>();

			Thread t = new Thread(UpdateMOOIs);
			//t.Start();
		}

		/// <summary>
		/// Thread loop for target tracking update for MOOI
		/// </summary>
		void UpdateMOOIs()
		{
			while (isRunning)
			{
				foreach (TargetTrackingImpl impl in implList)
				{
					if (impl.Type == TargetTypes.PotentialMOOI || impl.Type == TargetTypes.ConfirmedMOOI)
					{
						impl.UpdateWithoutMeasurement();
					}
				}
				Thread.Sleep(500);
			}
		}

		/// <summary>
		/// Clear target list so the SensorDirector does not have to restart everytime
		/// </summary>
		public void ClearTargets()
		{
			currentTargetPose.Clear();
			implList.Clear();
		}

		/// <summary>
		/// Update tracker with given input. It will do simple data association with given data (comparing distance between existing targets).
		/// </summary>
		/// <param name="uPixel">x-pixel in a camera coordinate</param>
		/// <param name="vPixel">y-pixel in a camera coordinate</param>
		/// <param name="range">laser range reading</param>
		/// <param name="currentPose"></param>
		/// <returns>target index = targetID</returns>
		public int Update(double uPixel, double vPixel, RobotPose currentPose, SensorPose lidarPose, TargetTypes type, ILidar2DPoint lidarPt, string cameraType, int numCamera)
		{
			Matrix zSCR, Xx2;
			double distance = 0;
			int targetIdx = 0;
			lock (locker)
			{
				// Compute X and Y position of the target based on the passed lidar point
				//Vector2 xyPos = TargetTrackingImpl.FindPosCoord(screenSize, range, uPixel, vPixel, currentPose, null);
				// this X Y coordinate = E N
				double yaw = currentPose.yaw - Math.PI / 2;
				double pitch = currentPose.pitch; double roll = currentPose.roll;
				Matrix R_ENU2R = new Matrix(Math.Cos(yaw), Math.Sin(yaw), 0, -Math.Sin(yaw), Math.Cos(yaw), 0, 0, 0, 1) *
											new Matrix(1, 0, 0, 0, Math.Cos(pitch), -Math.Sin(pitch), 0, Math.Sin(pitch), Math.Cos(pitch)) *
											new Matrix(Math.Cos(roll), 0, Math.Sin(roll), 0, 1, 0, -Math.Sin(roll), 0, Math.Cos(roll));
				Matrix localPt = new Matrix(3, 1); localPt[0, 0] = -lidarPt.RThetaPoint.ToVector2().Y - lidarPose.y; localPt[1, 0] = lidarPt.RThetaPoint.ToVector2().X + lidarPose.x;
				Matrix globalPt = R_ENU2R.Inverse * localPt;
				Vector2 xyPos = new Vector2(globalPt[0, 0] + currentPose.x, globalPt[1, 0] + currentPose.y);
				this.xyPosList.Add(xyPos);
				#region debugging - go away
				// find the closest target
				//if (currentTargetPose.Count == 0)
				//{
				//    distance = double.MaxValue;
				//    targetIdx = 0;
				//}
				//else
				//{
				//    targetIdx = FindClosestTarget(xyPos, ref distance, type);
				//}

				//if (type == TargetTypes.PotentialSOOI || type == TargetTypes.ConfirmedSOOI)
				//{
				//    if (distance > staticDistanceThreshold) // if the distance is larger than the threshold, add as a new target
				//    {
				//        if (currentTargetPose.Count == currentTargetPose.Capacity)
				//            currentTargetPose.RemoveAt(0);
				//        currentTargetPose.Add(xyPos);

				//        implList.Add(new TargetTrackingImpl(sigma_f, dT, screenSize, cameraType));
				//        targetIdx = currentTargetPose.Count - 1;
				//    }
				//}
				//else if (type == TargetTypes.PotentialMOOI || type == TargetTypes.ConfirmedMOOI)
				//{
				//    if (distance > dynamicDistanceThreshold) // if the distance is larger than the threshold, add as a new target
				//    {
				//        if (currentTargetPose.Count == currentTargetPose.Capacity)
				//            currentTargetPose.RemoveAt(0);
				//        currentTargetPose.Add(xyPos);

				//        implList.Add(new TargetTrackingImpl(sigma_f, dT, screenSize, cameraType));
				//        targetIdx = currentTargetPose.Count - 1;
				//    }
				//}
				#endregion

				zSCR = new Matrix(3, 1);
				zSCR[0, 0] = uPixel; zSCR[1, 0] = vPixel; zSCR[2, 0] = lidarPt.RThetaPoint.R;
				Xx2 = new Matrix(9, 1);
				Xx2[0, 0] = currentPose.x; Xx2[1, 0] = currentPose.y; /* Xx2[2, 0] = currentPose.z; */ Xx2[2, 0] = 0;
				Xx2[3, 0] = currentPose.roll; Xx2[4, 0] = currentPose.pitch; Xx2[5, 0] = currentPose.yaw;
				//Xx2[3, 0] = 0; Xx2[4, 0] = 0; Xx2[5, 0] = currentPose.yaw;
				Xx2[6, 0] = 0; Xx2[7, 0] = 0; Xx2[8, 0] = 0;

				// modification for 3 cameras
				if (numCamera == 3)
				{
					if (uPixel > 0 && uPixel <= 320)
					{
						Xx2[6, 0] = 50 * Math.PI / 180;
					}
					else if (uPixel > 320 && uPixel <= 320 * 2)
					{
						zSCR[0, 0] = uPixel - 320; zSCR[1, 0] = vPixel;
						Xx2[6, 0] = 1.5 * Math.PI / 180;
					}
					else if (uPixel > 320 * 2)
					{
						zSCR[0, 0] = uPixel - 320 * 2; zSCR[1, 0] = vPixel;
						Xx2[6, 0] = -47 * Math.PI / 180;
					}
				}

				// find the most associated target
				Matrix Sxx2 = new Matrix(9, 9);
				Sxx2[0, 0] = Math.Sqrt(Math.Abs(currentPose.covariance[0, 0]));
				Sxx2[1, 1] = Math.Sqrt(Math.Abs(currentPose.covariance[1, 1]));
				Sxx2[2, 2] = Math.Sqrt(Math.Abs(currentPose.covariance[2, 2]));
				Sxx2[3, 3] = Math.Sqrt(0.01); // roll
				Sxx2[4, 4] = Math.Sqrt(0.01); // pitch
				Sxx2[5, 5] = Math.Sqrt(0.03); // yaw
				Sxx2[6, 6] = Math.Sqrt(0.01); // pan
				Sxx2[7, 7] = Math.Sqrt(0.01); // tilt
				Sxx2[8, 8] = Math.Sqrt(0.01); // scan

				//targetIdx = FindMostAssociatedTarget(xyPos, type, Xx2, Sxx2, zSCR);
				Matrix zSCRForTest = new Matrix(2, 1); zSCRForTest[0, 0] = lidarPt.RThetaPoint.R; zSCRForTest[1, 0] = lidarPt.RThetaPoint.theta;
				targetIdx = FindMostAssociatedTarget(xyPos, type, Xx2, Sxx2, zSCRForTest);
				if (type == TargetTypes.PotentialSOOI || type == TargetTypes.ConfirmedSOOI)
				{
					if (targetIdx == -1) // if no good associated target found, then add a new one
					{
						implList.Add(new TargetTrackingImpl(sigma_f, dT, screenSize, cameraType));
						targetIdx = implList.Count - 1;
					}
				}
				else if (type == TargetTypes.PotentialMOOI || type == TargetTypes.ConfirmedMOOI)
				{
					if (targetIdx == -1)
					{
						implList.Add(new TargetTrackingImpl(sigma_f, dT, screenSize, cameraType));
						targetIdx = implList.Count - 1;
					}
				}


				Matrix xhatPOI = new Matrix(7, 1); xhatPOI.Zero();
				xhatPOI[0, 0] = xyPos.X; xhatPOI[1, 0] = xyPos.Y; xhatPOI[2, 0] = 0;
				if (!implList[targetIdx].IsInitialized)
					implList[targetIdx].SetInitialPOIInfo(xhatPOI);

			}

			Matrix Sx2 = new Matrix(9, 9);
			Sx2[0, 0] = Math.Sqrt(Math.Abs(currentPose.covariance[0, 0]));
			Sx2[1, 1] = Math.Sqrt(Math.Abs(currentPose.covariance[1, 1]));
			Sx2[2, 2] = Math.Sqrt(Math.Abs(currentPose.covariance[2, 2]));
			//Sx2[0, 0] = Math.Sqrt(0.1);
			//Sx2[1, 1] = Math.Sqrt(0.1);
			//Sx2[2, 2] = Math.Sqrt(0.01);
			Sx2[3, 3] = Math.Sqrt(0.01); // roll
			Sx2[4, 4] = Math.Sqrt(0.01); // pitch
			Sx2[5, 5] = Math.Sqrt(0.01); // yaw
			Sx2[6, 6] = Math.Sqrt(0.01); // pan
			Sx2[7, 7] = Math.Sqrt(0.01); // tilt
			Sx2[8, 8] = Math.Sqrt(0.01); // scan

			// update with correct target
			implList[targetIdx].UpdateZSCR(zSCR);
			implList[targetIdx].UpdateVehicleState(Xx2, currentPose.timestamp);
			implList[targetIdx].UpdateSx2(Sx2);
			if (implList[targetIdx].Type != TargetTypes.ConfirmedSOOI && implList[targetIdx].Type != TargetTypes.ConfirmedMOOI)
				implList[targetIdx].SetTargetType(type);
			implList[targetIdx].Update();
			return targetIdx;
		}

		public int UpdateTypeWithConfirmation(int targetIdx, TargetTypes type)
		{
			if (implList.Count <= 0 || implList[targetIdx] == null)
				return 1;
			implList[targetIdx].SetTargetType(type);
			return 0;
		}

		private int FindMostAssociatedTarget(Vector2 xyPos, TargetTypes type, Matrix Xx2, Matrix Sx2, Matrix zSCR)
		{
			int sigma = 3;
			int targetIdx = -2;
			Dictionary<int, double> validIdxToDistance = new Dictionary<int, double>(); // index of targets that has this point within 3-sigma bound

			//go through every target I have and do the sigma ellipse tseting
			//if (type == TargetTypes.ConfirmedMOOI || type == TargetTypes.PotentialMOOI)
			//{
			//foreach (TargetTrackingImpl impl in implList)
			//{
			//    //if (impl.Type == type || impl.Type == TargetTypes.ConfirmedMOOI || impl.Type == TargetTypes.Junk || impl.Type == TargetTypes.Meta)
			//    //{
			//    // check if the xyPos is within 3-sigma range
			//    Matrix zMinusMu = new Matrix(2, 1);
			//    zMinusMu[0, 0] = xyPos.X - impl.TargetState[0, 0];
			//    zMinusMu[1, 0] = xyPos.Y - impl.TargetState[1, 0];
			//    Matrix cov = impl.TargetCov.Submatrix(0, 1, 0, 1) * impl.TargetCov.Submatrix(0, 1, 0, 1).Transpose();
			//    if ((zMinusMu.Transpose() * cov.Inverse * zMinusMu)[0, 0] - (sigma * sigma) < 0)
			//    {
			//        double distance = Math.Sqrt(zMinusMu[0, 0] * zMinusMu[0, 0] + zMinusMu[1, 0] * zMinusMu[1, 0]);
			//        validIdxToDistance.Add(implList.IndexOf(impl), distance);
			//        targetIdx = implList.IndexOf(impl);
			//    }
			//    //}
			//}
			//}
			/*
		else if (type == TargetTypes.PotentialSOOI || type == TargetTypes.ConfirmedSOOI)
		{
			 * */
			//zSCRMean.Clear(); zSCRCov.Clear();
			foreach (TargetTrackingImpl impl in implList)
			{
				//double distance = Double.MaxValue;
				Matrix mean, cov;
				//if (impl.Type == TargetTypes.PotentialSOOI || impl.Type == TargetTypes.ConfirmedSOOI || impl.Type == TargetTypes.Junk)
				//{
				impl.TestAssociation(Xx2, Sx2, out mean, out cov);
				Vector2 twoDPose = new Vector2();
				twoDPose.X = mean[0, 0]; twoDPose.Y = mean[1, 0];
				Matrix zMinusMu = new Matrix(2, 1);
				zMinusMu[0, 0] = xyPos.X - twoDPose.X;
				zMinusMu[1, 0] = xyPos.Y - twoDPose.Y;
				Matrix targetCov = impl.TargetCov.Submatrix(0, 1, 0, 1) * impl.TargetCov.Submatrix(0, 1, 0, 1).Transpose();
				if ((zMinusMu.Transpose() * targetCov.Inverse * zMinusMu)[0, 0] - (sigma * sigma) < 0)
				{
					double distance = Math.Sqrt(zMinusMu[0, 0] * zMinusMu[0, 0] + zMinusMu[1, 0] * zMinusMu[1, 0]);
					validIdxToDistance.Add(implList.IndexOf(impl), distance);
					targetIdx = implList.IndexOf(impl);
				}


				zSCRMean.Add(twoDPose);
				zSCRCov.Add(cov);

			}
			//}
			//*/
			// if found more than one valid targets, do the mean comparison
			if (validIdxToDistance.Count > 1)
			{
				double min = Double.MaxValue;
				foreach (KeyValuePair<int, double> pair in validIdxToDistance)
				{
					if (pair.Value < min)
					{
						min = pair.Value;
						targetIdx = pair.Key;
					}
				}
			}
			else if (validIdxToDistance.Count == 0) // only one target to associate found
				targetIdx = -1;
			return targetIdx;
		}


		/// <summary>
		/// Returns lidar scan projection into a image plane
		/// </summary>
		/// <param name="lidarScan"></param>
		/// <param name="cameraSize">"320x240" or "640x480"</param>
		/// <param name="camType">"Fire-i" or "FireFly"</param>
		/// <param name="camPose">Your camera pose relative to the lidar</param>
		/// <returns>List of pixel values for each lidar point</returns>
		static public List<Vector2> FindLidarProjection(ILidarScan<ILidar2DPoint> lidarScan, string cameraSize, string camType, SensorPose camPose)
		{
			Matrix DCM4D = new Matrix(4, 4, 1.0);
			double[] fc = new double[2];
			double[] cc = new double[2];
			if (cameraSize.Equals("320x240"))
			{
				//for 320 x 240 image with Unibrain Fire-i camera
				if (camType.Equals("Fire-i"))
				{
					fc[0] = 384.4507; fc[1] = 384.1266;
					cc[0] = 155.1999; cc[1] = 101.5641;
				}

				// Fire-Fly MV
				else if (camType.Equals("FireFly"))
				{
					fc[0] = 345.26498; fc[1] = 344.99438;
					cc[0] = 159.36854; cc[1] = 118.26944;
				}
			}
			else if (cameraSize.Equals("640x480"))
			{
				// for 640 x 480 image with Unibrain Fire-i camera
				if (camType.Equals("Fire-i"))
				{
					fc[0] = 763.5805; fc[1] = 763.8337;
					cc[0] = 303.0963; cc[1] = 215.9287;
				}
				// for Fire-Fly MV (Point Gray)
				else if (camType.Equals("FireFly"))
				{
					fc[0] = 691.09778; fc[1] = 690.70187;
					cc[0] = 324.07388; cc[1] = 234.22204;
				}
			}
			double alpha_c = 0;

			// camera matrix
			Matrix KK = new Matrix(fc[0], alpha_c * fc[0], cc[0], 0, fc[1], cc[1], 0, 0, 1);


			// update DCM for point transformation
			DCM4D[0, 3] = camPose.x; DCM4D[1, 3] = camPose.y; DCM4D[2, 3] = camPose.z;
			DCM4D[0, 0] = Math.Cos(camPose.yaw); DCM4D[1, 1] = Math.Cos(camPose.yaw);
			DCM4D[0, 1] = Math.Sin(camPose.yaw); DCM4D[1, 0] = -Math.Sin(camPose.yaw);
			List<Vector2> pixelList = new List<Vector2>(lidarScan.Points.Count);
			foreach (ILidar2DPoint pt in lidarScan.Points)
			{
				Matrix point = new Matrix(4, 1);
				point[0, 0] = -pt.RThetaPoint.ToVector4().Y;
				point[1, 0] = pt.RThetaPoint.ToVector4().X;
				point[2, 0] = pt.RThetaPoint.ToVector4().Z;
				point[3, 0] = 1;
				Matrix transPt = DCM4D * point;
				Matrix ptImgPlane = new Matrix(3, 1);
				ptImgPlane[0, 0] = transPt[0, 0] / transPt[1, 0];
				ptImgPlane[1, 0] = -transPt[2, 0] / transPt[1, 0];
				ptImgPlane[2, 0] = transPt[1, 0] / transPt[1, 0];
				ptImgPlane = KK * ptImgPlane;
				pixelList.Add(new Vector2(ptImgPlane[0, 0], ptImgPlane[1, 0]));
			}
			return pixelList;
		}


		static public double FindTargetDistance(ILidarScan<ILidar2DPoint> lidarScan, double u, double v, Dictionary<int, int> colorPix, Vector2 TLCorner, Vector2 RBCorner,
												 string cameraSize, string camType, RobotPose camPose, TargetTypes type, out ILidar2DPoint lidarPt, ref List<Vector2> ptInBox)
		{
			#region camera stuff
			Matrix DCM4D = new Matrix(4, 4, 1.0);
			double[] fc = new double[2];
			double[] cc = new double[2];
			if (cameraSize.Equals("320x240"))
			{
				//for 320 x 240 image with Unibrain Fire-i camera
				if (camType.Equals("Fire-i"))
				{
					fc[0] = 384.4507; fc[1] = 384.1266;
					cc[0] = 155.1999; cc[1] = 101.5641;
				}

				// Fire-Fly MV
				else if (camType.Equals("FireFly"))
				{
					fc[0] = 345.26498; fc[1] = 344.99438;
					cc[0] = 159.36854; cc[1] = 118.26944;
				}
			}
			else if (cameraSize.Equals("640x480"))
			{
				// for 640 x 480 image with Unibrain Fire-i camera
				if (camType.Equals("Fire-i"))
				{
					fc[0] = 763.5805; fc[1] = 763.8337;
					cc[0] = 303.0963; cc[1] = 215.9287;
				}
				// for Fire-Fly MV (Point Gray)
				else if (camType.Equals("FireFly"))
				{
					fc[0] = 691.09778; fc[1] = 690.70187;
					cc[0] = 324.07388; cc[1] = 234.22204;
				}
			}
			double alpha_c = 0;

			// camera matrix
			Matrix KK = new Matrix(fc[0], alpha_c * fc[0], cc[0], 0, fc[1], cc[1], 0, 0, 1);
			#endregion

			// update DCM for point transformation
			DCM4D[0, 3] = camPose.x; DCM4D[1, 3] = camPose.y; DCM4D[2, 3] = camPose.z;
			DCM4D[0, 0] = Math.Cos(camPose.yaw); DCM4D[1, 1] = Math.Cos(camPose.yaw);
			DCM4D[0, 1] = Math.Sin(camPose.yaw); DCM4D[1, 0] = -Math.Sin(camPose.yaw);
			List<Vector2> pixelList = new List<Vector2>(lidarScan.Points.Count);
			List<ILidar2DPoint> lidarScanInBox = new List<ILidar2DPoint>();
			foreach (ILidar2DPoint pt in lidarScan.Points)
			{
				Matrix point = new Matrix(4, 1);
				point[0, 0] = -pt.RThetaPoint.ToVector4().Y;
				point[1, 0] = pt.RThetaPoint.ToVector4().X;
				point[2, 0] = pt.RThetaPoint.ToVector4().Z;
				point[3, 0] = 1;
				Matrix transPt = DCM4D * point;
				Matrix ptImgPlane = new Matrix(3, 1);
				ptImgPlane[0, 0] = transPt[0, 0] / transPt[1, 0];
				ptImgPlane[1, 0] = -transPt[2, 0] / transPt[1, 0];
				ptImgPlane[2, 0] = transPt[1, 0] / transPt[1, 0];
				ptImgPlane = KK * ptImgPlane;
				pixelList.Add(new Vector2(ptImgPlane[0, 0], ptImgPlane[1, 0]));
				if (ptImgPlane[0, 0] >= TLCorner.X && ptImgPlane[0, 0] <= RBCorner.X && ptImgPlane[1, 0] >= TLCorner.Y && ptImgPlane[1, 0] <= RBCorner.Y)
				{
					if (colorPix.Count > 0)
					{
						if (colorPix.ContainsKey((int)ptImgPlane[0, 0]) && colorPix[(int)ptImgPlane[0, 0]] == 255)
						{
							lidarScanInBox.Add(pt);
							ptInBox.Add(new Vector2(ptImgPlane[0, 0], ptImgPlane[1, 0]));
						}
					}
					else
					{
						lidarScanInBox.Add(pt);
						ptInBox.Add(new Vector2(ptImgPlane[0, 0], ptImgPlane[1, 0]));
					}
				}

			}
			if (lidarScanInBox.Count == 0)
			{
				lidarPt = null;
				return -1;
			}

			lidarPt = FineTargetDistanceClusterBased(lidarScanInBox);
			if (lidarPt == null)
				return -1;
			return lidarPt.RThetaPoint.R;
		}

		private static ILidar2DPoint FineTargetDistanceClusterBased(List<ILidar2DPoint> lidarScanInBox)
		{
			// lidar data clusering
			double lastRange = -1;
			List<List<ILidar2DPoint>> cluster = new List<List<ILidar2DPoint>>();
			foreach (ILidar2DPoint pt in lidarScanInBox)
			{
				if (lidarScanInBox.IndexOf(pt) == 0) continue;
				if (Math.Abs(pt.RThetaPoint.R - lastRange) > 0.2)
				{
					cluster.Add(new List<ILidar2DPoint>());
					cluster[cluster.Count - 1].Add(pt);
				}
				else
					cluster[cluster.Count - 1].Add(pt);
				lastRange = pt.RThetaPoint.R;
			}
			// find the largest cluster
			List<ILidar2DPoint> largestCluster = new List<ILidar2DPoint>();
			int maxNum = Int16.MinValue;
			foreach (List<ILidar2DPoint> list in cluster)
			{
				if (list.Count > maxNum)
				{
					largestCluster = list;
					maxNum = list.Count;
				}
			}
			ILidar2DPoint toReturn;
			if (largestCluster.Count == 0) return null; // if no largest cluster, return null - highly unlikely to happen
			if (largestCluster.Count % 2 == 1)
				toReturn = largestCluster[largestCluster.Count / 2];
			else
				toReturn = largestCluster[largestCluster.Count / 2 - 1];
			return toReturn;

		}

		public void GetTargetsToSend(out List<RobotPose> state, out List<Matrix> cov, out List<RobotPose> lastPose, out List<TargetTypes> type)
		{
			List<RobotPose> trackedState = new List<RobotPose>(implList.Count);
			List<RobotPose> lastRobotPose = new List<RobotPose>(implList.Count);
			List<Matrix> trackedCov = new List<Matrix>(implList.Count);
			List<TargetTypes> targetTypes = new List<TargetTypes>(implList.Count);
			foreach (TargetTrackingImpl impl in implList)
			{
				trackedState.Add(new RobotPose(impl.TargetState[0, 0], impl.TargetState[1, 0], impl.TargetState[2, 0], 0, 0, 0, impl.LastRobotPose.timestamp));
				lastRobotPose.Add(impl.LastRobotPose);
				trackedCov.Add(impl.TargetCov);
				targetTypes.Add(impl.Type);
			}
			state = trackedState; cov = trackedCov; lastPose = lastRobotPose; type = targetTypes;
			//messaging.SendTargets(robotID, trackedState, trackedCov, lastRobotPose, targetTypes);
		}

		//public void GetTargetInfo(out List<RobotPose> state, out List<RobotPose> lastPose, out List<Matrix> cov)
		//{
		//    List<RobotPose> trackedState = new List<RobotPose>(currentTargetPose.Count);
		//    List<RobotPose> lastRobotPose = new List<RobotPose>(currentTargetPose.Count);
		//    List<Matrix> trackedCov = new List<Matrix>(currentTargetPose.Count);
		//    foreach (TargetTrackingImpl impl in implList)
		//    {
		//        trackedState.Add(new RobotPose(impl.TargetState[0, 0], impl.TargetState[1, 0], impl.TargetState[2, 0], 0, 0, 0, currentPose.timestamp));
		//        lastRobotPose.Add(impl.LastRobotPose);	
		//        trackedCov.Add(impl.TargetCov);
		//    }
		//    state = trackedState;
		//    lastPose = lastRobotPose;
		//    cov = trackedCov;
		//    //messaging.SendTargets(robotID, trackedState, trackedCov, lastRobotPose);
		//}

		private int FindClosestTarget(Vector2 xyPos, ref double distance, TargetTypes type)
		{
			//Vector2 closestTarget = xyPos.ClosestInList(currentTargetPose, ref distance);
			Vector2 closestTarget = ClosestInListByKind(xyPos, currentTargetPose, ref distance, type);
			return currentTargetPose.IndexOf(closestTarget);
		}

		/// <summary>
		/// Return the closest point in a list from this point
		/// </summary>
		/// <param name="list">list of points</param>
		/// <param name="distance">distance to the closest point</param>
		/// <returns>Closest point of the list</returns>
		private Vector2 ClosestInListByKind(Vector2 pt, List<Vector2> list, ref double distance, TargetTypes type)
		{
			double minDist = Double.MaxValue;
			Vector2 toReturn = new Vector2();
			foreach (Vector2 v in list)
			{
				if (implList[list.IndexOf(v)].Type == type && v.DistanceTo(pt) < minDist)
				{
					minDist = v.DistanceTo(pt);
					toReturn = v;
				}
			}
			distance = minDist;
			return toReturn;
		}


		#region IDisposable Members

		public void Dispose()
		{
			isRunning = false;
		}

		#endregion
	}
}
