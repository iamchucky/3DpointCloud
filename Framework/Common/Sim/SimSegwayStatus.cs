using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Robots;

namespace Magic.Common.Sim
{
	[Serializable]
	public class SimSegwayStatus : IRobotTwoWheelStatus
	{
		private double leftWheelSpeed, rightWheelSpeed, integratedLeftWheelPos, integratedRightWheelPos;

		public SimSegwayStatus(double lws, double rws, double ilwp, double irwp)
		{
			leftWheelSpeed = lws;
			rightWheelSpeed = rws;
			integratedLeftWheelPos = ilwp;
			integratedRightWheelPos = irwp;
		}

		#region IRobotTwoWheelStatus Members

		public double LeftWheelSpeed
		{
			get { return leftWheelSpeed; }
		}

		public double RightWheelSpeed
		{
			get { return rightWheelSpeed; }
		}

		public double IntegratedLeftWheelPosition
		{
			get { return integratedLeftWheelPos; }
		}

		public double IntegratedRightWheelPosition
		{
			get { return integratedRightWheelPos; }
		}

		public string StatusString
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
