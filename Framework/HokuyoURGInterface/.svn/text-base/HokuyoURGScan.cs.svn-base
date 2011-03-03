using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;

namespace Magic.HokuyoURGInterface
{
    [Serializable]
    public class HokuyoURGScan : ILidarScan<ILidar2DPoint>
    {
        public int packetNum;   // packet counter	  
        public double timestamp;
        public int scannerID;   // Our internal ID of the source scanner
        public List<ILidar2DPoint> points;


        #region ILidarScan<ILidar2DPoint> Members

        public int ScannerID
        {
            get { return scannerID; }
        }

        public double Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        public List<ILidar2DPoint> Points
        {
            get { return points; }
			set { points = value; }
        }

        #endregion

        #region ILoggable Members

        public string ToLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(timestamp.ToString() + "\t" + scannerID.ToString() + "\t" + points.Count + "\t");

            foreach (ILidar2DPoint pt in points)
            {
                sb.Append(pt.RThetaPoint.R + "\t" + pt.RThetaPoint.theta + "\t");
            }
            return sb.ToString();
        }

        #endregion

        #region ITimestampedEventQueueItem Members

        public string DataType
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region ITimeComparable Members

        public double TimeStamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                timestamp = value;
            }
        }

        public int CompareTo(Magic.Common.DataInterfaces.ITimeComparable obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
