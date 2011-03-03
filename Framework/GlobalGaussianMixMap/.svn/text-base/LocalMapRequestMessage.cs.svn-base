using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;

namespace Magic.Sensor.GlobalGaussianMixMap
{
	[Serializable]
	public class LocalMapRequestMessage
	{
		int robotID;
		public int RobotID { get { return robotID; } }

		RobotPose currentPose;
		public RobotPose CurrentPose { get { return currentPose; } }

		double extentX;
		public double ExtentX { get { return extentX; } }
		
		double extentY;
		public double ExtentY { get { return extentY; } }

		/// <summary>
		/// Create a LocalMapRequestMessage that is sent from robots to the CentralSensorProcessor for a updated local map
		/// </summary>
		/// <param name="robotID">robotID</param>
		/// <param name="currentPose">current position</param>
		/// <param name="extentX">x-size of the local occupancy map</param>
		/// <param name="extentY">y-size of the local occupancy map </param>
		public LocalMapRequestMessage(int robotID, RobotPose currentPose, double extentX, double extentY)
		{
			this.robotID = robotID;
			this.currentPose = currentPose;
			this.extentX = extentX;
			this.extentY = extentY;
		}

	}
}
