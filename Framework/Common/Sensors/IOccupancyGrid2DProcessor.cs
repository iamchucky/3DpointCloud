using System;
using Magic.Common.DataTypes;
using Magic.Common.Sensors;
using Magic.Common;
namespace Magic.Common.Sensors
{
    /// <summary>
    /// Defines an algorithm that gives you a 2d occupancy grid given lidar scans, lidar pose, and robot pose.
    /// </summary>
    public interface IOccupancyGrid2DProcessor
    {
        IOccupancyGrid2D GetOccupancyGrid();
        void SetLidarScan(ILidarScan<ILidar2DPoint> s);
        void SetLidarPose(SensorPose p);     
        void SetRobotPose(RobotPose p);

        event EventHandler<NewOccupancyGrid2DAvailableEventArgs> NewGridAvailable;
    }

    public class NewOccupancyGrid2DAvailableEventArgs : EventArgs
    {
        private IOccupancyGrid2D occupancyGrid;
				private double timestamp;

				public double Timestamp
				{
					get { return timestamp; }					
				}
        public IOccupancyGrid2D OccupancyGrid
        {
            get { return occupancyGrid; }
            set { occupancyGrid = value; }
        }

        public NewOccupancyGrid2DAvailableEventArgs(IOccupancyGrid2D occupancyGrid, double timestamp)
        {
            this.occupancyGrid = occupancyGrid;
						this.timestamp = timestamp;
        }
    }
}
