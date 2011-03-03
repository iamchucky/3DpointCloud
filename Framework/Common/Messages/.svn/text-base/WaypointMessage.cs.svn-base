using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Messages
{
    [Serializable]
    public class WaypointMessage : IRobotMessage
    {
        int robotID;
        List<Waypoint> waypoints;

        public WaypointMessage(int robotID, List<Waypoint> waypoints)
        {
            this.robotID = robotID;
            this.waypoints = waypoints;
        }

        public List<Waypoint> Waypoints
        {
            get { return waypoints; }
        }

        #region IRobotMessage Members

        public int RobotID
        {
            get { return robotID; }
        }

        #endregion
    }
}
