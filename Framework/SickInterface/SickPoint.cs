using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common;
using System.Runtime.Serialization;

namespace Magic.SickInterface
{
	[Serializable]
	public class SickPoint : ILidar2DPoint
	{
		public RThetaCoordinate location;
		public SickPoint(RThetaCoordinate location)
		{ this.location = location; }


		#region ILidarPoint Members

		public override RThetaCoordinate RThetaPoint
		{
			get { return location; }
		}
        public override double Reflectivity { get { return 0; } }

		#endregion

    }

}
