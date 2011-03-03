using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common
{
	[Serializable]
	public class SensorPose : Pose
	{
		public SensorPose(double x, double y, double z, double yaw, double pitch, double roll, double timestamp)
			: base(x, y, z, yaw, pitch, roll, timestamp)
		{}

		public SensorPose()
		{
			Zero();
		}

        public SensorPose(SensorPose p) : base(p) { }
		
	}
}
