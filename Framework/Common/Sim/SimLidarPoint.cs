using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common;

namespace Magic.Common.Sim
{
	[Serializable]
	public class SimLidarPoint : ILidar2DPoint
	{
		public RThetaCoordinate location;

		public SimLidarPoint(RThetaCoordinate loc)
		{
			location = loc;
		}

		public override RThetaCoordinate RThetaPoint
		{
			get { return location; }
		}

		public override double Reflectivity
		{
			get { return 0; }
		}
	}
}
