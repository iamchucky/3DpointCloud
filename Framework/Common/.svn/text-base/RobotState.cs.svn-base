using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Path;
using Magic.Common.Robots;

namespace Magic.Common
{
	[Serializable]
	public class RobotState
	{
		public enum RobotMode { Starting, Exiting, Paused, EStopped, Waypoint, Direct, Neutralize, Test };
        public enum RobotSubMode { None, WaypointStandard, WaypointGap };
        public enum RobotNeutralizeStatus { None, Searching, Locked, InProgress };

		public int RobotID { get; set; }
		public RobotMode Mode { get; set; }
        public RobotSubMode SubMode { get; set; }
		public RobotPose Pose { get; set; }
		public RobotPose rPose { get; set; } // Previous pose ('r' denotes value at previous tick)
		public bool PoseWatchdogFail { get; set; }
		public bool PlannerFail { get; set; }
		public bool InObstacle { get; set; }
		public bool WpInObstacle { get; set; }
		public bool IsLockedOn { get; set; }
        public RobotNeutralizeStatus NeutralizeStatus { get; set; }
		public List<Vector2> Waypoints { get; set; }
		public double AngleWhenDone { get; set; }
		public IPath TrackedPath { get; set; }
		public IPath SparsePath { get; set; }
		public RobotTwoWheelCommand Cmd { get; set; }
        public DateTime CmdTime { get; set; }
		public int WaypointsCompleted { get; set; }

		public RobotState()
		{
			RobotID = 0;
			Mode = RobotMode.Starting;
            SubMode = RobotSubMode.None;
			Pose = new RobotPose();
			rPose = new RobotPose();
			PoseWatchdogFail = true;
			PlannerFail = false;
			InObstacle = false;
			WpInObstacle = false;
			IsLockedOn = false;
            NeutralizeStatus = RobotNeutralizeStatus.None;
			Waypoints = null;
			AngleWhenDone = 0;
			TrackedPath = null;
			SparsePath = null;
			Cmd = new RobotTwoWheelCommand(0, 0);
            CmdTime = DateTime.Now;
			WaypointsCompleted = 0;
		}

		public RobotState(RobotState toCopy)
			: this(toCopy.RobotID, toCopy.Mode, toCopy.SubMode, toCopy.Pose, toCopy.rPose, toCopy.PoseWatchdogFail,
                toCopy.PlannerFail, toCopy.InObstacle, toCopy.WpInObstacle, toCopy.IsLockedOn, toCopy.NeutralizeStatus, 
				toCopy.Waypoints, toCopy.AngleWhenDone, toCopy.TrackedPath, toCopy.SparsePath, toCopy.Cmd,
                toCopy.CmdTime, toCopy.WaypointsCompleted)
		{
		}

		public RobotState(int robotID, RobotMode mode, RobotSubMode subMode, RobotPose pose, RobotPose rpose,
            bool poseWatchdogFail, bool plannerFail, bool inObstacle, bool wpInObstacle, bool isLockedOn, 
            RobotNeutralizeStatus neutralizeStatus, List<Vector2> waypoints, double angleWhenDone,
			IPath path, IPath sparsePath, RobotTwoWheelCommand cmd, DateTime cmdTime, int waypointsCompleted)
		{
			RobotID = robotID;
			Mode = mode;
            SubMode = subMode;
			Pose = pose.DeepCopy();
			rPose = rpose.DeepCopy();
			PoseWatchdogFail = poseWatchdogFail;
			PlannerFail = plannerFail;
			InObstacle = inObstacle;
			WpInObstacle = wpInObstacle;
			IsLockedOn = isLockedOn;
            NeutralizeStatus = neutralizeStatus;
			Waypoints = new List<Vector2>(waypoints);
			AngleWhenDone = angleWhenDone;
			TrackedPath = path.Clone();
			SparsePath = sparsePath.Clone();
			Cmd = new RobotTwoWheelCommand(cmd.velocity, cmd.turn);
            CmdTime = new DateTime(cmdTime.Ticks);
			WaypointsCompleted = waypointsCompleted;
		}

		public RobotState Clone()
		{
			return new RobotState(RobotID, Mode, SubMode, Pose, rPose, PoseWatchdogFail, PlannerFail, InObstacle, WpInObstacle, IsLockedOn, 
                                  NeutralizeStatus, Waypoints, AngleWhenDone, TrackedPath, SparsePath, Cmd, CmdTime, WaypointsCompleted);
		}
	}
}
