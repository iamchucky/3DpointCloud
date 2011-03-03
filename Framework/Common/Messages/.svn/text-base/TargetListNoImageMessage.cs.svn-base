using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;

namespace Magic.Common.Messages
{
    [Serializable]
    public class TargetListNoImageMessage : IRobotMessage
    {
        int robotID;
        List<Vector2> pixelLTCorner;
        List<Vector2> pixelRBCorner;
        List<int> targetID;
        double timeStamp;
        ILidarScan<ILidar2DPoint> lidarScan;

        public List<int> TargetIDs
        { get { return targetID; } }
        public List<Vector2> PixelLTCorner
        { get { return pixelLTCorner; } }
        public List<Vector2> PixelRBCorner
        { get { return pixelRBCorner; } }
        public double TimeStamp
        { get { return timeStamp; } }
        public ILidarScan<ILidar2DPoint> LidarScan
        { get { return lidarScan; } }

        public TargetListNoImageMessage(int robotID, List<Vector2> pixelLTCorner, List<Vector2> pixelRBCorner, List<int> targetIDs, ILidarScan<ILidar2DPoint> lidarScan, double timeStamp)
        {
            int numTarget = targetIDs.Count;
            //this.pixelLTCorner = new List<Vector2>(); this.pixelRBCorner = new List<Vector2>();
            //for (int i = 0; i < numTarget; i++)
            //{
            //    this.pixelLTCorner.Add(pixelLTCorner[i]);
            //    this.pixelRBCorner.Add(pixelRBCorner[i]);
            //}
            this.robotID = robotID;
            this.pixelLTCorner = pixelLTCorner;
            this.pixelRBCorner = pixelRBCorner;
            this.timeStamp = timeStamp;
            this.targetID = targetIDs;
            this.lidarScan = lidarScan;
        }

        #region IRobotMessage Members

        public int RobotID
        {
            get { return robotID; }
        }

        #endregion
    }
}