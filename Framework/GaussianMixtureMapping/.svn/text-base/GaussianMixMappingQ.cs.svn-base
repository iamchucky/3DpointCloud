using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;
using Magic.Common;
using Magic.OccupancyGrid;
using Magic.Common.Sensors;
using Magic.Common.DataTypes;
using Magic.SickInterface;
using System.Diagnostics;
using Magic.UnsafeMath;
using Magic.Common.Shapes;

namespace Magic.Sensor.GaussianMixtureMapping
{
	public class GaussianMixMappingQ
	{
		UMatrix laserToRobotDCM;
		UMatrix robotToGlocalDCM;
		UMatrix covRobotPose;
		UMatrix covLaserPose;
		UMatrix covLaserScan;
		UMatrix covRobotPoseQ;

		double MAXRANGEHokuyo;
		double MINRANGEHokuyo;
		double MAXRANGESick;
		double MINRANGESick;

		int hokuyoStartIdx, hokuyoEndIdx;
		int rangeToApply;
		RobotPose currentRobotPose;

		Dictionary<Index, int> indicesDictionary;
		List<Index> indicesList;
		List<float> heightList;
		List<float> covList;
		List<float> pijSumList;

		Dictionary<int, Dictionary<int, UMatrix>> laser2RobotTransMatrixDictionary;
		Dictionary<int, Dictionary<int, UMatrix>> jacobianLaserPoseDictionary;

		#region accessors
		public OccupancyGrid2D UhatGM
		{
			get { return uhatGM; }
		}

		public OccupancyGrid2D Pij_sum
		{
			get { return pij_sum; }
		}
		public OccupancyGrid2D Pu_hat
		{
			get { return pu_hat; }
		}
		public OccupancyGrid2D Pu_hat_square
		{
			get { return pu_hat_square; }
		}
		public OccupancyGrid2D Psig_u_hat_square
		{
			get { return psig_u_hat_square; }
		}
		public OccupancyGrid2D LaserHit
		{ get { return laserHit; } }
		public OccupancyGrid2D ThresholdedHeightMap
		{ get { return thresholdedHeightMap; } }
		public OccupancyGrid2D SigSqrCov
		{ get { return sigSqrGM; } }
		#endregion
		double[] sinLookupSick, cosLookupSick;
		double[] sinLookupHokuyo, cosLookupHokuyo;

		OccupancyGrid2D pij_sum;
		OccupancyGrid2D pu_hat;
		OccupancyGrid2D pu_hat_square;
		OccupancyGrid2D psig_u_hat_square;
		OccupancyGrid2D uhatGM;
		OccupancyGrid2D sigSqrGM;
		OccupancyGrid2D thresholdedHeightMap;
		OccupancyGrid2D laserHit;
		OccupancyGrid2D resetCountMap;
		OccupancyGrid2D indexMap;
		Object locker = new Object();
		UMatrix JTpl;

		double laserHalfAngleHokuyo;
		double laserHalfAngleSick;

		int numCellX = 0, numCellY = 0;

		public GaussianMixMappingQ(IOccupancyGrid2D ocToUpdate, SensorPose laserRelativePositionToRover)
			: this(ocToUpdate, laserRelativePositionToRover, 10)
		{
		}

		public GaussianMixMappingQ(IOccupancyGrid2D ocToUpdate, SensorPose laserRelativePositionToRover, int cellToUpdate)
		{
			//currentLaserPoseToRobot = laserRelativePositionToRover;
			this.rangeToApply = cellToUpdate;
			Matrix4 laser2RobotDCM = Matrix4.FromPose(laserRelativePositionToRover);
			laserToRobotDCM = new UMatrix(4, 4);
			robotToGlocalDCM = new UMatrix(4, 4);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					laserToRobotDCM[i, j] = laser2RobotDCM[i, j];
				}
			}
			covRobotPose = new UMatrix(6, 6);
			covLaserPose = new UMatrix(6, 6);
			covLaserScan = new UMatrix(2, 2);

			pij_sum = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			pu_hat = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			pu_hat_square = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			psig_u_hat_square = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			uhatGM = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			sigSqrGM = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			laserHit = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			thresholdedHeightMap = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			resetCountMap = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			indexMap = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
			this.numCellX = ocToUpdate.NumCellX;
			this.numCellY = ocToUpdate.NumCellY;
			#region Initial setup

			double[] Qp_robot = new double[36]{0.1, 0, 0, 0, 0, 0,
											   0, 0.1, 0, 0, 0, 0,
											   0, 0, 0.1, 0, 0, 0,
											   0, 0, 0, 0.01, 0, 0,
											   0, 0, 0, 0, 0.01, 0,
											   0, 0, 0, 0, 0, 0.01};


			double[] Qp_laser = new double[36]{0.001, 0, 0, 0, 0, 0,
											   0, 0.001, 0, 0, 0, 0,
											   0, 0, 0.001, 0, 0, 0,
											   0, 0, 0, 0.0001, 0, 0,
											   0, 0, 0, 0, 0.0001, 0,
											   0, 0, 0, 0, 0, 0.0001};

			double[] Qr = new double[4] { .01 * .01, 0, 0, (0.1 * Math.PI / 180.0) * (0.1 * Math.PI / 180.0) };

			// assign to our UMatrix
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 6; j++)
				{
					covRobotPose[j, i] = Qp_robot[j * 6 + i];// *1e-6;
					covLaserPose[j, i] = Qp_laser[j * 6 + i];// *1e-6;
				}
			}
			covLaserScan[0, 0] = Qr[0];
			covLaserScan[0, 1] = 0;
			covLaserScan[1, 0] = 0;
			covLaserScan[1, 1] = Qr[3];
			#endregion

			int k = 0;

			sinLookupHokuyo = new double[682];
			cosLookupHokuyo = new double[682];
			for (double i = -120; i <= 120; i += 0.35190615835777126099706744868035)
			{
				sinLookupHokuyo[k] = (Math.Sin(i * Math.PI / 180.0));
				cosLookupHokuyo[k] = (Math.Cos(i * Math.PI / 180.0));
				k++;
			}
			laserHalfAngleHokuyo = 120;
			MAXRANGEHokuyo = 5.0;
			MINRANGEHokuyo = 0.5;

			k = 0;
			sinLookupSick = new double[361];
			cosLookupSick = new double[361];
			for (double i = -90; i <= 90; i += .5)
			{
				sinLookupSick[k] = (Math.Sin(i * Math.PI / 180.0));
				cosLookupSick[k] = (Math.Cos(i * Math.PI / 180.0));
				k++;
			}
			laserHalfAngleSick = 90;
			MAXRANGESick = 30.0;
			MINRANGESick = 0.5;

			hokuyoStartIdx = 100;
			hokuyoEndIdx = 500;

			indicesList = new List<Index>();
			heightList = new List<float>();
			covList = new List<float>();
			pijSumList = new List<float>();

			laser2RobotTransMatrixDictionary = new Dictionary<int, Dictionary<int, UMatrix>>();
			jacobianLaserPoseDictionary = new Dictionary<int, Dictionary<int, UMatrix>>();

			indicesDictionary = new Dictionary<Index, int>();
		}

		public void SetPijSum(double x, double y, double value)
		{
			pij_sum.SetCell(x, y, value);
		}

		public void SetPuHat(double x, double y, double value)
		{
			pu_hat.SetCell(x, y, value);
		}

		public void SetPuHatSquare(double x, double y, double value)
		{
			pu_hat_square.SetCell(x, y, value);
		}

		public void SetPsigUhatSquare(double x, double y, double value)
		{
			psig_u_hat_square.SetCell(x, y, value);
		}

		public void UpdateHeight(double x, double y)
		{
			double largeU = pu_hat.GetCell(x, y) / pij_sum.GetCell(x, y);
			double largeSig = (psig_u_hat_square.GetCell(x, y) + pu_hat_square.GetCell(x, y)) / pij_sum.GetCell(x, y) - largeU * largeU;

			uhatGM.SetCell(x, y, largeU);
			sigSqrGM.SetCell(x, y, largeSig);
		}

		private void UpdateCovariance(Matrix robotCov)
		{
			lock (locker)
			{
				UMatrix abc = new UMatrix(6, 6);
				for (int i = 0; i < 6; i++)
				{
					for (int j = 0; j < 6; j++)
					{
						abc[i, j] = robotCov[i, j]; // this is a heck !!!!!
					}

					if (abc[i, i] < 0)
						abc[i, i] = Math.Abs(abc[i, i]);
				}
				if (abc[2, 2] == 0) abc[2, 2] = 0.01;
				if (abc[4, 4] == 0) abc[4, 4] = 0.01;
				if (abc[5, 5] == 0) abc[5, 5] = 0.01;
				covRobotPose = abc;
			}
		}

		private void UpdateCovarianceQ(Matrix state)
		{
			UMatrix abc = new UMatrix(7, 7);
			for (int i = 0; i < 7; i++)
			{
				for (int j = 0; j < 7; j++)
					abc[i, j] = state[i, j];
			}
			covRobotPoseQ = abc;
		}


		/// <summary>
		/// Update robot to global coordination DCM with a Pose
		/// </summary>
		/// <param name="robotPosition"></param>
		public void UpdatePose(RobotPose robotPosition)
		{
			this.currentRobotPose = robotPosition;
			Matrix4 robot2GlocalDCM = Matrix4.FromPose(robotPosition);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					robotToGlocalDCM[i, j] = robot2GlocalDCM[i, j];
				}
			}
		}

		public void UpdateSensorPose(SensorPose sensorPose)
		{
			Matrix4 laser2RobotDCM = Matrix4.FromPose(sensorPose);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					laserToRobotDCM[i, j] = laser2RobotDCM[i, j];
				}
			}
		}

		/// <summary>
		/// Update OccupancyGrid based on lidarScan and robotPose received
		/// </summary>
		/// <param name="lidarScan"></param>
		/// <param name="currentRobotPose"></param>
		public void UpdateOccupancyGrid(ILidarScan<ILidar2DPoint> lidarScan, int robotID, int scannerID, PoseFilterState currentRobotPose, SensorPose lidarPose, List<Polygon> dynamicObstacles)
		{
			if (robotID == 1)
			{
				MAXRANGESick = 7.0;
			}
			else if (robotID == 3)
			{
				MAXRANGESick = 30.0;
			}

			if (lidarPose == null)
			{
				lidarPose = new SensorPose(0, 0, 0.5, 0, 0 * Math.PI / 180.0, 0, 0);
			}
			if (laser2RobotTransMatrixDictionary.ContainsKey(robotID))
			{
				if (laser2RobotTransMatrixDictionary[robotID].ContainsKey(scannerID))
				{
					JTpl = jacobianLaserPoseDictionary[robotID][scannerID];
					laserToRobotDCM = laser2RobotTransMatrixDictionary[robotID][scannerID];
				}
				else
				{
					Matrix4 laser2RobotDCM = Matrix4.FromPose(lidarPose);
					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							laserToRobotDCM[i, j] = laser2RobotDCM[i, j];
						}
					}
					laser2RobotTransMatrixDictionary[robotID].Add(scannerID, laserToRobotDCM);
					jacobianLaserPoseDictionary[robotID].Add(scannerID, ComputeJacobian(lidarPose.yaw, lidarPose.pitch, lidarPose.roll));
					JTpl = jacobianLaserPoseDictionary[robotID][scannerID];
				}
			}
			else
			{
				laser2RobotTransMatrixDictionary.Add(robotID, new Dictionary<int, UMatrix>());
				jacobianLaserPoseDictionary.Add(robotID, new Dictionary<int, UMatrix>());
				Matrix4 laser2RobotDCM = Matrix4.FromPose(lidarPose);
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						laserToRobotDCM[i, j] = laser2RobotDCM[i, j];
					}
				}
				laser2RobotTransMatrixDictionary[robotID].Add(scannerID, new UMatrix(laserToRobotDCM));
				jacobianLaserPoseDictionary[robotID].Add(scannerID, ComputeJacobian(lidarPose.yaw, lidarPose.pitch, lidarPose.roll));
				JTpl = jacobianLaserPoseDictionary[robotID][scannerID];
			}

			// calculate robot2global transformation matrix
			if (currentRobotPose == null) return;
			Matrix4 robot2GlocalDCM = Matrix4.FromPose(currentRobotPose);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					robotToGlocalDCM[i, j] = robot2GlocalDCM[i, j];
				}
			}

			if (lidarScan == null) return;

			UMatrix JTpr = ComputeJacobianQ(currentRobotPose.q1, currentRobotPose.q2, currentRobotPose.q3, currentRobotPose.q4);
			List<UMatrix> JfPrCubixLaserToRobotDCM = new List<UMatrix>(6);
			List<UMatrix> RobotToGlocalDCMJfPlCubix = new List<UMatrix>(7);
			for (int i = 0; i < 7; i++)
			{
				//derivative of the robot transform matrtix w.r.t. some element of the robot psoe
				UMatrix j = new UMatrix(4, 4);
				j[0, 0] = JTpr[0, i]; j[1, 0] = JTpr[1, i]; j[2, 0] = JTpr[2, i]; j[3, 0] = JTpr[3, i];
				j[0, 1] = JTpr[4, i]; j[1, 1] = JTpr[5, i]; j[2, 1] = JTpr[6, i]; j[3, 1] = JTpr[7, i];
				j[0, 2] = JTpr[8, i]; j[1, 2] = JTpr[9, i]; j[2, 2] = JTpr[10, i]; j[3, 2] = JTpr[11, i];
				j[0, 3] = JTpr[12, i]; j[1, 3] = JTpr[13, i]; j[2, 3] = JTpr[14, i]; j[3, 3] = JTpr[15, i];
				JfPrCubixLaserToRobotDCM.Add(j * laserToRobotDCM);

				if (i == 7) continue; // same as break
				UMatrix tempJacobianPl = new UMatrix(4, 4);
				tempJacobianPl[0, 0] = JTpl[0, i]; tempJacobianPl[1, 0] = JTpl[1, i]; tempJacobianPl[2, 0] = JTpl[2, i]; tempJacobianPl[3, 0] = JTpl[3, i];
				tempJacobianPl[0, 1] = JTpl[4, i]; tempJacobianPl[1, 1] = JTpl[5, i]; tempJacobianPl[2, 1] = JTpl[6, i]; tempJacobianPl[3, 1] = JTpl[7, i];
				tempJacobianPl[0, 2] = JTpl[8, i]; tempJacobianPl[1, 2] = JTpl[9, i]; tempJacobianPl[2, 2] = JTpl[10, i]; tempJacobianPl[3, 2] = JTpl[11, i];
				tempJacobianPl[0, 3] = JTpl[12, i]; tempJacobianPl[1, 3] = JTpl[13, i]; tempJacobianPl[2, 3] = JTpl[14, i]; tempJacobianPl[3, 3] = JTpl[15, i];
				RobotToGlocalDCMJfPlCubix.Add(robotToGlocalDCM * tempJacobianPl);
			}
			UMatrix laserToENU = robotToGlocalDCM * laserToRobotDCM;
			//UMatrix pijCell = new UMatrix(rangeToApply * 2 + 1, rangeToApply * 2 + 1);
			// update covariance
			UpdateCovarianceQ(currentRobotPose.Covariance);

			//SickPoint p = new SickPoint(new RThetaCoordinate(1.0f, 0.0f));
			for (int laserIndex = 0; laserIndex < lidarScan.Points.Count; laserIndex++)
			{
				lock (locker)
				{
					ILidar2DPoint p = lidarScan.Points[laserIndex];
					if (scannerID == 1)
					{
						if (p.RThetaPoint.R >= MAXRANGESick || p.RThetaPoint.R <= MINRANGESick)
							continue;
					}
					else if (scannerID == 2)
					{
						if (p.RThetaPoint.R >= MAXRANGEHokuyo || p.RThetaPoint.R <= MINRANGEHokuyo || laserIndex < hokuyoStartIdx || laserIndex > hokuyoEndIdx)
							continue;
					}
					bool hitDynamicObstacles = false;
					// figure out if this lidar point is hitting other robot

					// find laser points in 3D space
					// first define 2D point on the laser plane
					UMatrix laserPoint = new UMatrix(4, 1);

					double deg = (p.RThetaPoint.theta * 180.0 / Math.PI);
					int thetaDegIndex = 0;
					if (scannerID == 2) // hokuyo
						thetaDegIndex = (int)Math.Round((deg + laserHalfAngleHokuyo) * 2.84);// % 360;
					else if (scannerID == 1) // sick
						thetaDegIndex = (int)Math.Round((deg + laserHalfAngleSick) * 2) % 360;

					double cosTheta = 0, sinTheta = 0;

					if (scannerID == 1)
					{
						cosTheta = cosLookupSick[thetaDegIndex];
						sinTheta = sinLookupSick[thetaDegIndex];
					}
					else if (scannerID == 2)
					{
						cosTheta = cosLookupHokuyo[thetaDegIndex];
						sinTheta = sinLookupHokuyo[thetaDegIndex];
					}

					// initial laser points
					laserPoint[0, 0] = p.RThetaPoint.R * cosTheta;
					laserPoint[1, 0] = p.RThetaPoint.R * sinTheta;
					laserPoint[2, 0] = 0;
					laserPoint[3, 0] = 1;

					//calculate r_hat_ENU
					UMatrix r_hat_ENU = laserToENU * laserPoint;

					foreach (Polygon dp in dynamicObstacles)
					{
						if (dp.IsInside(new Vector2(r_hat_ENU[0, 0], r_hat_ENU[1, 0])))
						{
							hitDynamicObstacles = true;
							break;
						}
					}
					if (hitDynamicObstacles) continue;

					//-------------------------------//
					// COVARIANCE UMatrix CALCULATION //
					//-------------------------------//		
					UMatrix JRr = new UMatrix(4, 2);
					JRr.Zero();
					JRr[0, 0] = cosTheta;
					JRr[0, 1] = -p.RThetaPoint.R * sinTheta;
					JRr[1, 0] = sinTheta;
					JRr[1, 1] = p.RThetaPoint.R * cosTheta;

					UMatrix Jfr = new UMatrix(3, 2); // 3x2 								
					Jfr = (laserToENU * JRr).Submatrix(0, 2, 0, 1);	 // 4x4 * 4x4 * 4x2			

					UMatrix Jfpr = new UMatrix(3, 7);
					UMatrix Jfpl = new UMatrix(3, 6);

					for (int i = 0; i < 7; i++) //for each state var (i.e. x,y,z,y,p,r)
					{
						UMatrix JfprTemp = (JfPrCubixLaserToRobotDCM[i]) * laserPoint; // 4 by 1 UMatrix					
						Jfpr[0, i] = JfprTemp[0, 0];
						Jfpr[1, i] = JfprTemp[1, 0];
						Jfpr[2, i] = JfprTemp[2, 0];

						//UMatrix JfplTemp = (RobotToGlocalDCMJfPlCubix[i]) * laserPoint; // 4 by 1 UMatrix
						//Jfpl[0, i] = JfplTemp[0, 0];
						//Jfpl[1, i] = JfplTemp[1, 0];
						//Jfpl[2, i] = JfplTemp[2, 0];
					}
					UMatrix JfprQprJfprT = new UMatrix(3, 3);
					UMatrix JfplQplJfplT = new UMatrix(3, 3);
					UMatrix JfrQrJfrT = new UMatrix(3, 3);
					JfprQprJfprT = (Jfpr * covRobotPoseQ) * Jfpr.Transpose();
					//JfplQplJfplT = (Jfpl * covLaserPose) * Jfpl.Transpose(); // not doing because covariance of laser point is so small
					JfrQrJfrT = (Jfr * covLaserScan) * Jfr.Transpose();

					// add above variables together and get the covariance
					UMatrix covRHatENU = JfprQprJfprT + /*JfplQplJfplT +*/ JfrQrJfrT; // 3x3 UMatrix
					//-----------------------------//
					// FIND WHICH CELLS TO COMPUTE //
					//-----------------------------//

					// find out cells around this laser point
					int laserPointIndexX = 0;
					int laserPointIndexY = 0;
					//this is used just to do the transformation from reaal to grid and visa versa
					psig_u_hat_square.GetIndicies(r_hat_ENU[0, 0], r_hat_ENU[1, 0], out laserPointIndexX, out laserPointIndexY); // get cell (r_hat_ENU_X, r_hat_ENU_y)
					if ((laserPointIndexX < 0 || laserPointIndexX >= numCellX) || (laserPointIndexY < 0 || laserPointIndexY >= numCellY))
						continue;

					int rangeToApplyX = (int)Math.Round(Math.Sqrt(covRHatENU[0, 0]) / (pij_sum.ResolutionX * 2)) * 2;
					int rangeToApplyY = (int)Math.Round(Math.Sqrt(covRHatENU[1, 1]) / (pij_sum.ResolutionY * 2)) * 2;
					//-----------------------------------------//
					// COMPUTE THE DISTRIBUTION OF UNCERTAINTY //
					//-----------------------------------------//
					UMatrix pijCell = new UMatrix(rangeToApplyY * 2 + 1, rangeToApplyX * 2 + 1);

					double sigX = Math.Sqrt(covRHatENU[0, 0]);
					double sigY = Math.Sqrt(covRHatENU[1, 1]);
					double rho = covRHatENU[1, 0] / (sigX * sigY);
					double logTerm = Math.Log(2 * Math.PI * sigX * sigY * Math.Sqrt(1 - (rho * rho)));
					double xTermDenom = (1 - (rho * rho));
					//for (int i = -rangeToApply; i <= rangeToApply; i++) // row
					for (int i = -rangeToApplyX; i <= rangeToApplyX; i++) // row
					{
						//for (int j = -rangeToApplyX; j <= rangeToApplyX; j++) // column
						for (int j = -rangeToApplyY; j <= rangeToApplyY; j++) // column
						{
							if (laserPointIndexX + i < 0 || laserPointIndexX + i >= numCellX || laserPointIndexY + j < 0 || laserPointIndexY + j >= numCellY) continue;
							// estimate using Bivariate Normal Distribution
							double posX = 0; double posY = 0;
							psig_u_hat_square.GetReals(laserPointIndexX + i, laserPointIndexY + j, out posX, out posY);
							posX += psig_u_hat_square.ResolutionX / 2;
							posY += psig_u_hat_square.ResolutionY / 2;
							double x = posX - r_hat_ENU[0, 0];
							double y = posY - r_hat_ENU[1, 0];
							double z = (x * x) / (sigX * sigX) -
												(2 * rho * x * y / (sigX * sigY)) +
												 (y * y) / (sigY * sigY);
							double xTerm = -0.5 * z / xTermDenom;
							// chi2 test
							//if ((2 * (1 - (rho * rho))) * ((x * x) / (sigX * sigX) + (y * y) / (sigY * sigY) - (2 * rho * x * y) / (sigX * sigY)) > 15.2)
							//  continue;						
							pijCell[j + rangeToApplyY, i + rangeToApplyX] = Math.Exp(xTerm - logTerm) * psig_u_hat_square.ResolutionX * psig_u_hat_square.ResolutionY;
							laserHit.SetCellByIdx(laserPointIndexX + i, laserPointIndexY + j, laserHit.GetCellByIdxUnsafe(laserPointIndexX + i, laserPointIndexY + j) + 1);
						}
					}

					//---------------------------//
					// COMPUTE HEIGHT ESTIMATION //
					//---------------------------//				
					UMatrix PEN = covRHatENU.Submatrix(0, 1, 0, 1);

					UMatrix PENInv = PEN.Inverse2x2;

					UMatrix PuEN = new UMatrix(1, 2);
					UMatrix PENu = new UMatrix(2, 1);
					UMatrix PuENPENInv = PuEN * PENInv;
					UMatrix uHatMatrix = new UMatrix(rangeToApplyY * 2 + 1, rangeToApplyX * 2 + 1);
					UMatrix sigUHatMatrix = new UMatrix(rangeToApplyY * 2 + 1, rangeToApplyX * 2 + 1);

					PuEN[0, 0] = covRHatENU[2, 0];
					PuEN[0, 1] = covRHatENU[2, 1];

					PENu[0, 0] = covRHatENU[0, 2];
					PENu[1, 0] = covRHatENU[1, 2];

					double sig_u_hat_product = (PuENPENInv * PENu)[0, 0]; // output = 1x1 UMatrix

					for (int i = -rangeToApplyX; i <= rangeToApplyX; i++) // row
					{
						for (int j = -rangeToApplyY; j <= rangeToApplyY; j++) // column
						{
							UMatrix ENmr_EN = new UMatrix(2, 1);
							double posX = 0; double posY = 0;
							psig_u_hat_square.GetReals(laserPointIndexX + i, laserPointIndexY + j, out posX, out posY);
							ENmr_EN[0, 0] = posX - r_hat_ENU[0, 0];
							ENmr_EN[1, 0] = posY - r_hat_ENU[1, 0];
							double u_hat_product = (PuENPENInv * (ENmr_EN))[0, 0]; // output = 1x1 UMatrix
							uHatMatrix[j + rangeToApplyY, i + rangeToApplyX] = r_hat_ENU[2, 0] + u_hat_product;
							sigUHatMatrix[j + rangeToApplyY, i + rangeToApplyX] = covRHatENU[2, 2] - sig_u_hat_product;
						}
					}

					//-------------------------------------------//
					// ASSIGN FINAL VALUES TO THE OCCUPANCY GRID //
					//-------------------------------------------//
					for (int i = -rangeToApplyX; i <= rangeToApplyX; i++)
					{
						for (int j = -rangeToApplyY; j <= rangeToApplyY; j++)
						{
							int indexXToUpdate = laserPointIndexX + i;
							int indexYToUpdate = laserPointIndexY + j;
							// if the cell to update is out of range, continue
							if (!psig_u_hat_square.CheckValidIdx(indexXToUpdate, indexYToUpdate))
								continue;

							pij_sum.SetCellByIdx(indexXToUpdate, indexYToUpdate,
											pijCell[j + rangeToApplyY, i + rangeToApplyX] + pij_sum.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate));
							pu_hat.SetCellByIdx(indexXToUpdate, indexYToUpdate,
											pijCell[j + rangeToApplyY, i + rangeToApplyX] * uHatMatrix[j + rangeToApplyY, i + rangeToApplyX] + pu_hat.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate));
							pu_hat_square.SetCellByIdx(indexXToUpdate, indexYToUpdate,
											pijCell[j + rangeToApplyY, i + rangeToApplyX] * uHatMatrix[j + rangeToApplyY, i + rangeToApplyX] * uHatMatrix[j + rangeToApply, i + rangeToApply] + pu_hat_square.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate));
							psig_u_hat_square.SetCellByIdx(indexXToUpdate, indexYToUpdate,
											pijCell[j + rangeToApplyY, i + rangeToApplyX] * sigUHatMatrix[j + rangeToApplyY, i + rangeToApplyX] + psig_u_hat_square.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate));
							uhatGM.SetCellByIdx(indexXToUpdate, indexYToUpdate,
															(pu_hat.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate) / pij_sum.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate)));

							double largeU = (pu_hat.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate) / pij_sum.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate));
							double largeSig = (psig_u_hat_square.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate) + pu_hat_square.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate)) / pij_sum.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate) - largeU * largeU;
							if (pij_sum.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate) > 1)
								thresholdedHeightMap.SetCellByIdx(indexXToUpdate, indexYToUpdate, largeU);//pij_sum.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate) / laserHit.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate));

							uhatGM.SetCellByIdx(indexXToUpdate, indexYToUpdate, largeU);
							//sigSqrGM.SetCellByIdx(indexXToUpdate, indexYToUpdate, largeU + 2 * Math.Sqrt(largeSig));

							if (indexMap.GetCellByIdxUnsafe(indexXToUpdate, indexYToUpdate) != 1.0)
							{
								Index index = new Index(indexXToUpdate, indexYToUpdate);
								indicesDictionary.Add(index, indicesDictionary.Count);
								indexMap.SetCellByIdx(indexXToUpdate, indexYToUpdate, 1.0);
							}
						}
					}

				} // end foreach

				//Console.WriteLine("1: " + sw1.ElapsedMilliseconds +
				//                                    " 2: " + sw2.ElapsedMilliseconds +
				//                                    " 3: " + sw3.ElapsedMilliseconds +
				//                                    " 4: " + sw4.ElapsedMilliseconds +
				//                                    " 5: " + sw5.ElapsedMilliseconds +
				//                                    " 6: " + sw6.ElapsedMilliseconds +
				//                                    " TOTAL: " + (sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds + sw3.ElapsedMilliseconds + sw4.ElapsedMilliseconds + sw5.ElapsedMilliseconds + sw6.ElapsedMilliseconds).ToString());
			} // end function
		}

		/// <summary>
		/// Returns arrays to send out through messaging
		/// </summary>
		/// <param name="largeUDiff"></param>
		/// <param name="largeSigDiff"></param>
		/// <param name="pijDiff"></param>
		/// <param name="colIdx"></param>
		/// <param name="rowIdx"></param>
		public void GetArraysToSend(out List<Index> indexList, out List<float> heightList, out List<float> covList, out List<float> pijSumList, out List<float> laserHit)
		{
			int index = 0;
			if (indicesDictionary == null)
			{
				indexList = new List<Index>();
				heightList = new List<float>();
				covList = new List<float>();
				pijSumList = new List<float>();
				laserHit = new List<float>();
				return;
			}
			else
			{
				indexList = new List<Index>(indicesDictionary.Count);
				heightList = new List<float>(indicesDictionary.Count);
				covList = new List<float>(indicesDictionary.Count);
				pijSumList = new List<float>(indicesDictionary.Count);
				laserHit = new List<float>(indicesDictionary.Count);
			}

			lock (locker)
			{
				foreach (KeyValuePair<Index, int> pair in indicesDictionary)
				{
					indexList.Add(pair.Key);
					heightList.Add((float)uhatGM.GetCellByIdxUnsafe(pair.Key.Col, pair.Key.Row));
					//heightList.Add((float)thresholdedHeightMap.GetCellByIdxUnsafe(pair.Key.Col, pair.Key.Row));
					covList.Add((float)sigSqrGM.GetCellByIdxUnsafe(pair.Key.Col, pair.Key.Row));
					pijSumList.Add((float)pij_sum.GetCellByIdxUnsafe(pair.Key.Col, pair.Key.Row));
					laserHit.Add((float)LaserHit.GetCellByIdxUnsafe(pair.Key.Col, pair.Key.Row));
					indexMap.SetCellByIdx(pair.Key.Col, pair.Key.Row, 0.0); // reset the index map
				}
				indicesDictionary.Clear();

			}
		}

		
		# region Calculation Helper

		UMatrix ComputeJacobianQ(double q1, double q2, double q3, double q4)
		{
			UMatrix toReturn = new UMatrix(16, 7);
			toReturn.Zero();

			toReturn[12, 0] = 1;
			toReturn[13, 1] = 1;
			toReturn[14, 2] = 1;

			toReturn[0, 3] = 2 * q1;
			toReturn[1, 3] = 2 * q2;
			toReturn[2, 3] = 2 * q3;
			toReturn[4, 3] = 2 * q2;
			toReturn[5, 3] = -2 * q1;
			toReturn[6, 3] = 2 * q4;
			toReturn[8, 3] = 2 * q3;
			toReturn[9, 3] = -2 * q4;
			toReturn[10, 3] = -2 * q1;

			toReturn[0, 4] = -2 * q2;
			toReturn[1, 4] = 2 * q1;
			toReturn[2, 4] = -2 * q4;
			toReturn[4, 4] = 2 * q1;
			toReturn[5, 4] = 2 * q2;
			toReturn[6, 4] = 2 * q3;
			toReturn[8, 4] = 2 * q4;
			toReturn[9, 4] = 2 * q3;
			toReturn[10, 4] = -2 * q2;

			toReturn[0, 5] = -2 * q3;
			toReturn[1, 5] = 2 * q4;
			toReturn[2, 5] = 2 * q1;
			toReturn[4, 5] = -2 * q4;
			toReturn[5, 5] = -2 * q3;
			toReturn[6, 5] = 2 * q2;
			toReturn[8, 5] = 2 * q1;
			toReturn[9, 5] = 2 * q2;
			toReturn[10, 5] = 2 * q3;

			toReturn[0, 6] = 2 * q4;
			toReturn[1, 6] = 2 * q3;
			toReturn[2, 6] = -2 * q2;
			toReturn[4, 6] = -2 * q3;
			toReturn[5, 6] = 2 * q4;
			toReturn[6, 6] = 2 * q1;
			toReturn[8, 6] = 2 * q2;
			toReturn[9, 6] = -2 * q1;
			toReturn[10, 6] = 2 * q4;

			return toReturn;
		}


		public UMatrix ComputeJacobian(double yaw, double pitch, double roll)
		{
			UMatrix m = new UMatrix(16, 6);
			m.Zero();

			double cp = Math.Cos(pitch);
			double sp = Math.Sin(pitch);
			double cy = Math.Cos(yaw);
			double sy = Math.Sin(yaw);
			double cr = Math.Cos(roll);
			double sr = Math.Sin(roll);

			m[0, 3] = -cp * sy;
			m[0, 4] = -sp * cy;
			m[1, 3] = -sp * sr * sy - cy * cr;
			m[1, 4] = cp * sr * cy;
			m[1, 5] = sp * cr * cy + sy * sr;

			m[2, 3] = -sp * cr * sy + cy * sr;
			m[2, 4] = cp * cr * cy;
			m[2, 5] = -sp * sr * cy + sy * cr;

			m[4, 3] = cp * cy;
			m[4, 4] = -sp * sy;
			m[5, 3] = sp * sr * cy - sy * cr;
			m[5, 4] = cp * sr * sy;
			m[5, 5] = sp * cr * sy - cy * sr;
			m[6, 3] = sp * cr * cy + sy * sr;
			m[6, 4] = cp * cr * sy;
			m[6, 5] = -sp * sr * sy - cy * cr;

			m[8, 4] = -cp;
			m[9, 4] = -sp * sr;
			m[9, 5] = cp * cr;
			m[10, 4] = -sp * cr;
			m[10, 5] = -cp * sr;

			m[12, 0] = 1;
			m[13, 1] = 1;
			m[14, 2] = 1;

			return m;
		}


		private UMatrix ComputeJacobianZYX(double yaw, double pitch, double roll)
		{
			UMatrix m = new UMatrix(16, 6);
			m.Zero();

			m[12, 0] = 1;

			m[13, 1] = 1;

			m[14, 2] = 1;

			m[1, 3] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Sin(roll) * Math.Sin(yaw);
			m[2, 3] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Cos(roll) * Math.Sin(yaw);
			m[5, 3] = Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Sin(roll) * Math.Cos(yaw);
			m[6, 3] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Cos(roll) * Math.Cos(yaw);
			m[9, 3] = Math.Cos(roll) * Math.Cos(pitch);
			m[10, 3] = -Math.Sin(roll) * Math.Cos(pitch);

			m[0, 4] = -Math.Sin(pitch) * Math.Cos(yaw);
			m[1, 4] = Math.Sin(roll) * Math.Cos(pitch) * Math.Cos(yaw);
			m[2, 4] = Math.Cos(roll) * Math.Cos(pitch) * Math.Cos(yaw);
			m[4, 4] = -Math.Sin(pitch) * Math.Sin(yaw);
			m[5, 4] = Math.Sin(roll) * Math.Cos(pitch) * Math.Sin(yaw);
			m[6, 4] = Math.Cos(roll) * Math.Cos(pitch) * Math.Sin(yaw);
			m[8, 4] = -Math.Cos(pitch);
			m[9, 4] = -Math.Sin(roll) * Math.Sin(pitch);
			m[10, 4] = -Math.Cos(roll) * Math.Sin(pitch);

			m[0, 5] = -Math.Cos(pitch) * Math.Sin(yaw);
			m[1, 5] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Cos(roll) * Math.Cos(yaw);
			m[2, 5] = -Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Sin(roll) * Math.Cos(yaw);
			m[4, 5] = Math.Cos(pitch) * Math.Cos(yaw);
			m[5, 5] = Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Cos(roll) * Math.Sin(yaw);
			m[6, 5] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Sin(roll) * Math.Sin(yaw);

			return m;
		}

		private UMatrix ComputeJacobianXYZ(double roll, double pitch, double yaw)
		{
			UMatrix m = new UMatrix(16, 6);
			m.Zero();

			m[12, 0] = 1;

			m[13, 1] = 1;

			m[14, 2] = 1;

			m[1, 3] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Sin(roll) * Math.Sin(yaw);
			m[2, 3] = Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Cos(roll) * Math.Sin(yaw);
			m[5, 3] = -Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Sin(roll) * Math.Cos(yaw);
			m[6, 3] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Cos(roll) * Math.Cos(yaw);
			m[9, 3] = -Math.Cos(roll) * Math.Cos(pitch);
			m[10, 3] = -Math.Sin(roll) * Math.Cos(pitch);

			m[0, 4] = -Math.Sin(pitch) * Math.Cos(yaw);
			m[1, 4] = Math.Sin(roll) * Math.Cos(pitch) * Math.Cos(yaw);
			m[2, 4] = -Math.Cos(roll) * Math.Cos(pitch) * Math.Cos(yaw);
			m[4, 4] = Math.Sin(pitch) * Math.Sin(yaw);
			m[5, 4] = -Math.Sin(roll) * Math.Cos(pitch) * Math.Sin(yaw);
			m[6, 4] = Math.Cos(roll) * Math.Cos(pitch) * Math.Sin(yaw);
			m[8, 4] = Math.Cos(pitch);
			m[9, 4] = Math.Sin(roll) * Math.Sin(pitch);
			m[10, 4] = -Math.Cos(roll) * Math.Sin(pitch);

			m[0, 5] = -Math.Cos(pitch) * Math.Sin(yaw);
			m[1, 5] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Cos(roll) * Math.Cos(yaw);
			m[2, 5] = Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Sin(roll) * Math.Cos(yaw);
			m[4, 5] = -Math.Cos(pitch) * Math.Cos(yaw);
			m[5, 5] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Cos(roll) * Math.Sin(yaw);
			m[6, 5] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Sin(roll) * Math.Sin(yaw);

			return m;
		}
		#endregion




		//public static void ConvertToOcGrid(float[] incomingMessage, ref OccupancyGrid2D largeU, ref OccupancyGrid2D largeSig, ref OccupancyGrid2D pij)
		//{
		//  int msgCount = (int)incomingMessage[0];
		//  int eachLength = (int)incomingMessage[1];

		//  for (int i = 2; i < eachLength + 2; i++)
		//  {
		//    float largeUVal = incomingMessage[i];
		//    //float largeSigVal = incomingMessage[i + eachLength];
		//    //float pijVal = incomingMessage[i + 2 * eachLength];
		//    int colIdx = (int)incomingMessage[i + 1 * eachLength];
		//    int rowIdx = (int)incomingMessage[i + 2 * eachLength];

		//    //Console.WriteLine("[" + rowIdx + ", " + colIdx + "]");

		//    largeU.SetCellByIdx(colIdx, rowIdx, largeUVal);
		//    //largeSig.SetCellByIdx(colIdx, rowIdx, largeSigVal);
		//    //pij.SetCellByIdx(colIdx, rowIdx, pijVal);
		//  }
		//}

		//public static void ConvertToOcGrid(ref OccupancyGrid2D globalHeightMap, ref OccupancyGrid2D globalSigMap, ref OccupancyGrid2D globalPijMap,
		//  List<float> largeU, List<float> largeSig, List<float> pij, List<int> colIdx, List<int> rowIdx)
		//{
		//  for (int i = 0; i < largeU.Count; i++)
		//  {
		//    globalHeightMap.SetCellByIdx(rowIdx[i], colIdx[i], largeU[i]);
		//    globalSigMap.SetCellByIdx(rowIdx[i], colIdx[i], largeSig[i]);
		//    globalPijMap.SetCellByIdx(rowIdx[i], colIdx[i], pij[i]);
		//  }
		//}

		//public static void ConvertToOcGrid(ref OccupancyGrid2D globalHeightMap, ref OccupancyGrid2D globalSigMap, ref OccupancyGrid2D globalPijMap, List<Index> indexList, List<float> heightList, List<float> covList, List<float> pijSumList)
		//{
		//  for (int i = 0; i < indexList.Count; i++)
		//  {
		//    int row = indexList[i].Row;
		//    int col = indexList[i].Col;
		//    globalHeightMap.SetCellByIdx(col, row, heightList[i]);
		//    globalSigMap.SetCellByIdx(col, row, covList[i]);
		//    globalPijMap.SetCellByIdx(col, row, pijSumList[i]);
		//  }
		//}

	}
}
