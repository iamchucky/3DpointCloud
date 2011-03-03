using System;
using System.Collections.Generic;
namespace Magic.Common
{
	public interface IPathPlanner
	{
		void Dispose();
		Magic.Common.Path.IPath GetPathToGoal();
		void UpdateObstacles(System.Collections.Generic.List<Magic.Common.Shapes.Polygon> obstacles);
		void UpdatePose(Magic.Common.RobotPose pose);
		bool UpdateWaypoints(List<Vector2> waypoints);
        List<Magic.Common.Shapes.Polygon> GetObstacles();
	}
}
