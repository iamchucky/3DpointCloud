using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;
using Magic.Common;
using Magic.Common.Messages;

namespace Magic.TargetTracking
{
	public class TargetTrackingImpl
	{
		// algorithm
		TargetTrackingAlgorithm algorithm;

		// covariances
		Matrix SPOI = new Matrix(7, 7);
		Matrix Sw = new Matrix(3, 3, 0.001);
		Matrix SSCR = new Matrix(3, 3, Math.Sqrt(10));
		Matrix Sx2 = new Matrix(9, 9);
		Matrix xx2Noisy = new Matrix(9, 1);
		Matrix xx2 = new Matrix(9, 1);
		Matrix scr_noise = new Matrix(2, 1);

		// measurement
		Matrix zSCR = new Matrix(3, 1);
		Matrix xNAV = new Matrix(3, 1);
		Matrix xATT = new Matrix(3, 1);
		Matrix xGIM = new Matrix(3, 1);

		// prediction
		Matrix xPOI = new Matrix(7, 1);
		TargetTypes type;
		public Matrix TargetState { get { return xPOI; } }
		public Matrix TargetCov { get { return SPOI; } }
		public TargetTypes Type { get { return type; } }
		double lastRobotPoseTimestamp = 0;

		public RobotPose LastRobotPose
		{
			get
			{
				RobotPose pose = new RobotPose();
				pose.x = xx2[0, 0]; pose.y = xx2[1, 0]; pose.z = xx2[2, 0];
				pose.roll = xx2[3, 0]; pose.pitch = xx2[4, 0]; pose.yaw = xx2[5, 0];
				pose.timestamp = lastRobotPoseTimestamp;
				return pose;
			}
		}

		// parameter
		double sigma_f;
		MathNet.Numerics.Distributions.NormalDistribution normDist = new MathNet.Numerics.Distributions.NormalDistribution();

		object locker = new object();
		bool isInitialized = false;
		public bool IsInitialized { get { return isInitialized; } }

		string screenSize;
		string cameraType;

		public TargetTrackingImpl(double sigma_f, double dT, string screenSize, string cameraType)
		{
			this.screenSize = screenSize;
			this.sigma_f = sigma_f;
			this.cameraType = cameraType;
			SPOI.Zero();
			Sx2.Zero();
			for (int i = 0; i < 7; i++)
			{
				if (i < 3)
					SPOI[i, i] = 0.5;
				else
					SPOI[i, i] = 0.001;
			}
			for (int i = 0; i < 9; i++)
			{
				if (i < 6)
					Sx2[i, i] = 0.1;
				else
					Sx2[i, i] = 0.01;
			}
			Random r = new Random();
			SSCR[2, 2] = 0.01; // laser covariance
			scr_noise[0, 0] = r.NextDouble() * 10; scr_noise[1, 0] = r.NextDouble() * 10;
			algorithm = new TargetTrackingAlgorithm(dT);
		}

		public void UpdateVehicleState(Matrix xNAV, Matrix xATT, Matrix xGIM, double ts)
		{
			lock (locker)
			{
				this.xNAV = xNAV; this.xATT = xATT; this.xGIM = xGIM;
				//for (int i = 0; i < 3; i++)
				//{
				this.xx2.SetSubMatrix(0, 2, 0, 0, xNAV);
				this.xx2.SetSubMatrix(3, 5, 0, 0, xATT);
				this.xx2.SetSubMatrix(6, 8, 0, 0, xGIM);
				this.lastRobotPoseTimestamp = ts;
				//}
			}
		}

		public void UpdateVehicleState(Matrix Xx2, double ts)
		{
			lock (locker)
			{
				this.xx2 = Xx2;
				this.lastRobotPoseTimestamp = ts;
			}
		}

		public void SetInitialPOIInfo(Matrix xPOI, Matrix SPOI)
		{
			if (!isInitialized)
			{
				this.xPOI = xPOI;
				this.SPOI = SPOI;
				isInitialized = true;
			}
		}

		public void SetInitialPOIInfo(Matrix xPOI)
		{
			if (!isInitialized)
			{
				this.xPOI = xPOI;
				isInitialized = true;
			}
		}

		public void UpdateZSCR(Matrix zSCR)
		{ this.zSCR = zSCR; }

		/// <summary>
		/// Run the filter to predict next state. Make sure you update vehicle pose, zSCR, and other covariances before you update for the same time instance.
		/// </summary>
		public void Update()
		{
			lock (locker)
			{
				//for (int i = 0; i < 9; i++)
				//{
				//    normDist.Mu = xx2[i, 0]; normDist.Sigma = Sx2[i, i] * Sx2.Transpose()[i, i];
				//    xx2Noisy[i, 0] = normDist.NextDouble();
				//}
				//xx2Noisy[0, 0] = 1.8162; xx2Noisy[1, 0] = -0.2315; xx2Noisy[2, 0] = -0.0084;
				//xx2Noisy[3, 0] = 0.0239; xx2Noisy[4, 0] = -0.0063; xx2Noisy[5, 0] = 1.5929;
				//xx2Noisy[6, 0] = -4.7106; xx2Noisy[7, 0] = 0.0094; xx2Noisy[8, 0] = -0.0007;
				//zSCR.Zero();
				algorithm.Update(xPOI, xx2, SPOI, Sw, SSCR, Sx2, zSCR, sigma_f, screenSize, cameraType, type);
			}
			xPOI = algorithm.TargetState;
			SPOI = algorithm.STarget;
		}

		// return the ellipse distance from the new measurement and predicted measurement
		public void TestAssociation(Matrix Xx2, Matrix Sx2, out Matrix mean, out Matrix cov)
		{
			double sigma_fff = 3;
			Matrix newSSCR = new Matrix(2, 2);
			newSSCR[0, 0] = 0.001; newSSCR[1, 1] = 0.001; 
			algorithm.PredictLidarMeasurementWithCurrentPose(xPOI, xx2, SPOI, Sw, newSSCR, Sx2, type, sigma_fff, out mean, out cov);
		}

		public void SetTargetType(TargetTypes type)
		{ this.type = type; }

		public Matrix GenerateEllipse(int numPoint, double centerX, double centerY, Matrix sMatrix, int numSigma)
		{
			Matrix ep = new Matrix(numPoint, 2); // ellipse points
			// generate points in a circle
			for (int i = 0; i < numPoint; i++)
			{
				ep[i, 0] = numSigma * Math.Cos(2 * Math.PI / numPoint * i);
				ep[i, 1] = numSigma * Math.Sin(2 * Math.PI / numPoint * i);
			}

			// skew the ellipse
			CholeskyDecomposition chol = new CholeskyDecomposition(sMatrix);
			ep = chol.LeftTriangularFactor.Transpose() * ep.Transpose();
			ep = ep.Transpose();

			// re-center the ellipse
			Matrix ones = new Matrix(numPoint, 2);
			for (int i = 0; i < numPoint; i++)
			{
				ep[i, 0] = ep[i, 0] + centerX;
				ep[i, 1] = ep[i, 1] + centerY;
			}
			return ep;
		}

		/// <summary>
		/// Returns angle measured in radian with respect to the front of the robot. Following right hand convention
		/// </summary>
		/// <param name="focalLength"></param>
		/// <param name="pixel"></param>
		/// <param name="centerPixel"></param>
		/// <param name="robotYaw"></param>
		/// <returns></returns>
		public double FindAngle(double focalLength, int pixel, int centerPixel)
		{
			return Math.Atan2(centerPixel - pixel, focalLength);
		}

		/// <summary>
		/// Return X, Y coordinate of a pixel given Z and focal length information
		/// </summary>
		/// <param name="focalLength"></param>
		/// <param name="range"></param>
		/// <param name="pixel"></param>
		/// <returns></returns>
		public static Vector2 FindPosCoord(string screenSize, double range, double pixelX, double pixelY, RobotPose robotPose, SensorPose lidarPose)
		{
			double focalLength = 0;
			double centerPixX = 0;
			double centerPixY = 0;
			if (screenSize.Equals("320x240"))
			{
				focalLength = (384.4507 + 384.1266) / 2;
				centerPixX = 160; centerPixY = 120;
			}
			else if (screenSize.Equals("640x480"))
			{
				focalLength = (763.5805 + 763.8337) / 2;
				centerPixX = 320; centerPixY = 240;
			}
			else if (screenSize.Equals("960x240"))
			{
				focalLength = (345.26498 + 344.99438) / 2;
				centerPixX = 960 / 2; centerPixY = 480 / 2;
			}
			double angle = Math.PI / 2 - Math.Atan2(pixelX - centerPixX, focalLength);
			Matrix localPt = new Matrix(3, 1);
			localPt[0, 0] = range * Math.Cos(angle); localPt[1, 0] = range * Math.Sin(angle);
			double yaw = robotPose.yaw - Math.PI / 2; double pitch = robotPose.pitch; double roll = robotPose.roll;
			Matrix R_ENU2R = new Matrix(Math.Cos(yaw), Math.Sin(yaw), 0, -Math.Sin(yaw), Math.Cos(yaw), 0, 0, 0, 1) *
											 new Matrix(1, 0, 0, 0, Math.Cos(pitch), -Math.Sin(pitch), 0, Math.Sin(pitch), Math.Cos(pitch)) *
											 new Matrix(Math.Cos(roll), 0, Math.Sin(roll), 0, 1, 0, -Math.Sin(roll), 0, Math.Cos(roll));
			Matrix globalPt = R_ENU2R.Inverse * localPt;
			return new Vector2(globalPt[0, 0] + robotPose.x, globalPt[1, 0] + robotPose.y);
		}

		/// <summary>
		/// Return a 2D vector point in EN framework given two robot pose and pixel reading
		/// </summary>
		/// <param name="r1">Robot1 pose</param>
		/// <param name="r2">Robot2 pose</param>
		/// <param name="pixelR1">pixel reading from robot1</param>
		/// <param name="pixelR2">pixel reading from robot2</param>
		/// <param name="focalLength">focal length of the camera you're using</param>
		/// <returns>2D point of target for initialization</returns>
		public static Vector2 ComputeInitialPoint(RobotPose r1, RobotPose r2, Vector2 pixelR1, Vector2 pixelR2, double centerPixX, double focalLength)
		{
			double u1 = pixelR1.X; double u2 = pixelR2.X;
			double gAngle1 = r1.yaw - Math.Atan2(u1 - centerPixX, focalLength);
			double gAngle2 = r2.yaw - Math.Atan2(u2 - centerPixX, focalLength);
			double xGoal = ((r2.y - r1.y) - Math.Tan(gAngle2) * r2.x + Math.Tan(gAngle1) * r1.x) / (Math.Tan(gAngle1) - Math.Tan(gAngle2));
			double yGoal = Math.Tan(gAngle1) * (xGoal - r1.x) + r1.y;
			return new Vector2(xGoal, yGoal);
		}



		#region Updating Covarainces

		public void UpdateSPOI(Matrix SPOI)
		{ this.SPOI = SPOI.Clone(); }

		public void UpdateSx2(Matrix Sx2)
		{ this.Sx2 = Sx2.Clone(); }

		public void UpdateSw(Matrix Sw)
		{ this.Sw = Sw.Clone(); }

		public void UpdateSSCR(Matrix SSCR)
		{ this.SSCR = SSCR.Clone(); }

		public void updateSigmaF(double sigma_f)
		{ this.sigma_f = sigma_f; }

		# endregion


		internal void UpdateWithoutMeasurement()
		{
			algorithm.UpdateWithoutMeasurement();
			this.SPOI = algorithm.STarget;
			this.xPOI = algorithm.TargetState;
		}
	}
}
