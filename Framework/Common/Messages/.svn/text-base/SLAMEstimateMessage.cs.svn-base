using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;

namespace Magic.Common.Messages
{
	[Serializable]
	public class SLAMEstimateMessage : IRobotMessage
	{
		public SLAMEstimateMessage(int robotID, Matrix mean, Matrix cov) { this.robotID = robotID; this.mean = mean; this.cov = cov; }
		
		int robotID;
		Matrix mean;
		Matrix cov;

		public Matrix Mean
		{
			get { return mean; }
			set { mean = value; }
		}

		public Matrix Covariance
		{
			get { return cov; }
			set { cov = value; }
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { return robotID; }
		}

		#endregion
	}
}
