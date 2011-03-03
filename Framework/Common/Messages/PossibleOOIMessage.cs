using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
    /// <summary>
    /// This is the output of Tsung's algorithm from Sensor Director to HRI
    /// All points included here are possible targets needed to be confirmed by the human
    /// </summary>
    [Serializable]
    public class PossibleOOIMessage : IRobotMessage
    {
        int robotID;
        double timestamp;
        Vector2 point;
        TargetTypes ooiType;

        public double TimeStamp { get { return timestamp; } }
        public Vector2 Point { get { return point; } }
        public TargetTypes OOIType { get { return ooiType; } }

        public PossibleOOIMessage(int robotID, double ts, TargetTypes s)
        {
            this.timestamp = ts;
            this.ooiType = s;
            this.robotID = robotID;
            this.point = new Vector2();
        }

        public PossibleOOIMessage(Vector2 point, double ts, TargetTypes s)
        {
            this.point = point;
            this.timestamp = ts;
            this.ooiType = s;
            this.robotID = -1;
        }

        public PossibleOOIMessage(int robotID, Vector2 point, double ts, TargetTypes s)
        {
            this.point = point;
            this.timestamp = ts;
            this.ooiType = s;
            this.robotID = robotID;
        }

        #region IRobotMessage Members

        public int RobotID
        {
            get { return robotID; }
        }

        #endregion
    }
}
