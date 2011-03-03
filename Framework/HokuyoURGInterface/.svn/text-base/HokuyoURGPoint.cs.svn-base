using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common;
using System.Runtime.Serialization;

namespace Magic.HokuyoURGInterface
{
    [Serializable]
    public class HokuyoURGPoint : ILidar2DPoint
    {
        public RThetaCoordinate location;
        public HokuyoURGPoint(RThetaCoordinate location)
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
