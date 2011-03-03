using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common.Mapack;

namespace Magic.Common.Messages
{
    [Serializable]
    public class TargetListWithImageMessage : IRobotMessage
    {
        int robotID;
        List<RobotPose> state, lastPose;
        List<Matrix> cov;
        List<Vector2> pixelLTCorner;
        List<Vector2> pixelRBCorner;
        List<int> targetID;
        double timeStamp;
        RobotImage img;

        public List<RobotPose> TargetState { get { return state; } }
        public List<RobotPose> LastRobotPose { get { return lastPose; } }
        public List<Matrix> TargetCov { get { return cov; } }
        public List<int> TargetIDs
        { get { return targetID; } }
        public List<Vector2> PixelLTCorner
        { get { return pixelLTCorner; } }

        public List<Vector2> PixelRBCorner
        { get { return pixelRBCorner; } }

        public RobotImage Image
        { get { return img; } }

        public double TimeStamp
        { get { return timeStamp; } }

        public TargetListWithImageMessage(int robotID, RobotImage img, List<RobotPose> targetState, List<RobotPose> lastPose, List<Matrix> targetCov,
                                                        List<Vector2> pixelLTCorner, List<Vector2> pixelRBCorner, List<int> targetIDs, double timeStamp)
        {
            this.robotID = robotID;
            this.pixelLTCorner = pixelLTCorner;
            this.pixelRBCorner = pixelRBCorner;
            this.timeStamp = timeStamp;
            this.targetID = targetIDs;
            this.img = img;
            this.state = targetState;
            this.cov = targetCov;
            this.lastPose = lastPose;
        }

        #region IRobotMessage Members

        public int RobotID
        {
            get { return robotID; }
        }

        #endregion
    }
}