using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;

namespace Magic.Common.Messages
{
    [Serializable]
    public class TargetList2CentralNodeMessage
    {
        List<RobotPose> targetState;
        List<Matrix> targetCov;
        List<int> targetID;
        List<RobotPose> detectedPose;
        List<TargetTypes> ooiType;

        public List<RobotPose> TargetState { get { return targetState; } }
        public List<Matrix> TargetCov { get { return targetCov; } }
        public List<int> TargetID { get { return targetID; } }
        public List<RobotPose> DetectionPose { get { return detectedPose; } }
		public List<TargetTypes> OOIType { get { return ooiType; } }

        public TargetList2CentralNodeMessage(List<RobotPose> targetState, List<Matrix> targetCov, List<int> targetID, List<RobotPose> detectedPose, List<TargetTypes> targetTypes)
        {
            this.targetState = targetState;
            this.targetCov = targetCov;
            this.targetID = targetID;
            this.detectedPose = detectedPose;
            this.ooiType = targetTypes;
        }
    }
}
