using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;

namespace Magic.Common.Messages
{
    [Serializable]
    public class LidarFilterPackageMessage : IRobotMessage
    {
        public LidarFilterPackageMessage(int robotID, PoseFilterState state, SensorPose sensorPose, ILidarScan<ILidar2DPoint> scan)
        {
            this.robotID = robotID;
            this.state = state;
            this.lidarScan = scan;
            this.sensorPose = sensorPose;
        }
        int robotID;

        PoseFilterState state;
        ILidarScan<ILidar2DPoint> lidarScan;
        SensorPose sensorPose;

        public SensorPose SensorPose
        {
            get { return sensorPose; }
            set { sensorPose = value; }
        }

        public ILidarScan<ILidar2DPoint> LidarScan
        {
            get { return lidarScan; }
            set { lidarScan = value; }
        }

        public PoseFilterState Pose
        {
            get { return state; }
            set { state = value; }
        }

        #region IRobotMessage Members

        public int RobotID
        {
            get { return robotID; }
        }

        #endregion
    }
}
