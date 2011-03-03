using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common
{
    [Serializable]
	public class Waypoint
	{
		private Vector2 coordinate;
        private double terrainCost;
		private bool achieved;
		private bool userWaypoint;
        private bool critical;

		public Waypoint(double x, double y, bool userWaypoint, double terrainCost)
		{
			coordinate = new Vector2(x, y);
			achieved = false;
            critical = false;
            this.terrainCost = terrainCost;
			this.userWaypoint = userWaypoint;
		}

		public Waypoint(Vector2 xy, bool userWaypoint, double terrainCost)
		{
			coordinate = new Vector2(xy.X, xy.Y);
			achieved = false;
            critical = false;
            this.terrainCost = terrainCost;
			this.userWaypoint = userWaypoint;
		}

		public Vector2 Coordinate
		{
			get { return coordinate; }
			set { coordinate = value; }
		}

		public bool Achieved
		{
			get { return achieved; }
			set { achieved = value; }
		}

		public bool UserWaypoint
		{
			get { return userWaypoint; }
			set { userWaypoint = value; }
		}

        public bool Critical
        {
            get { return critical; }
            set { critical = value; }
        }

        public double TerrainCost
        {
            get { return terrainCost; }
            set { terrainCost = value; }
        }
	}
}
