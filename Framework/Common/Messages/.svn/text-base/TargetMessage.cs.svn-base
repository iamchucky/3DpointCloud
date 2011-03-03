using System;
using System.Collections.Generic;
using Magic.Common.Mapack;

namespace Magic.Common.Messages
{
    [Serializable]
    public class TargetMessage : IRobotMessage
    {
        List<RobotPose> targetState;
        List<RobotPose> lastRobotPose;
        List<Matrix> targetCov;
        List<TargetTypes> targetTypes;
        int robotID;

        public List<RobotPose> TargetState
        { get { return targetState; } }

        public List<Matrix> TargetCov
        { get { return targetCov; } }

        public List<RobotPose> LastRobotPose
        { get { return lastRobotPose; } }

        public List<TargetTypes> TargetTypes
        { get { return targetTypes; } }

        public TargetMessage(int robotID, List<RobotPose> targetPose, List<Matrix> targetCov, List<RobotPose> lastRobotPose, List<TargetTypes> types)
        {
            this.robotID = robotID;
            this.targetState = targetPose;
            this.targetCov = targetCov;
            this.lastRobotPose = lastRobotPose;
            this.targetTypes = types;
        }

        #region IRobotMessage Members

        public int RobotID
        {
            get { return robotID; }
        }

        #endregion
    }
}
