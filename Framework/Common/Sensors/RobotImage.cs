using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Magic.Common.Sensors
{
    [Serializable]
    public class RobotImage
    {
        public int robotID;
        public Image image;
				public double timeStamp;
				public RobotImage(int robotID, Image image, double timeStamp) { this.robotID = robotID; this.image = image; this.timeStamp = timeStamp; }
    }
}
