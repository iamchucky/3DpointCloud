using System;
using System.Collections.Generic;
using System.Text;

namespace Magic.Common
{
	[Obsolete]
	public class SensorLocation
	{
		public double xFromIMU;
		public double yFromIMU;
		public double zFromIMU;
		public double yaw;
		public double pitch;
		public double roll;
		public SensorLocation(double xFromIMU, double yFromIMU, double yaw)
		{
			this.xFromIMU = xFromIMU; this.yaw = yaw; this.yFromIMU = yFromIMU;
		}
		public SensorLocation()
		{ }
	}
}
