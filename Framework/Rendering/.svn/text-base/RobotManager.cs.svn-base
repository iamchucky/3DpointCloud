using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Rendering.Renderables;
using Magic.Common.Path;
using System.Drawing;
using Magic.Common;
using Magic.Common.Sensors;
using Magic.Common.Shapes;


namespace Magic.Rendering
{
	//[Obsolete]
	public class RobotManager
	{
		private Renderer defaultRenderer = null;
		private Dictionary<int, Robot> robots = new Dictionary<int, Robot>();
		//private static Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Magenta };
		private Color[] colors = { Color.Magenta, Color.SlateBlue, Color.Maroon, Color.OrangeRed };
		private Color[] colorsBloated = { Color.DarkMagenta, Color.DarkSlateBlue, Color.DarkRed, Color.DarkOrange };

		public void ClearAllRobots()
		{
			robots = new Dictionary<int, Robot>();
		}

        public Color GetRobotColors(int robotID)
        {
            try
            {
                return colors[robotID - 1];
            }
            catch { return Color.Black; }
        }

		public void AddNewRobot(int robotID, Renderer r)
		{
			Color color;
			try
			{
				color = colors[robotID - 1];
			}
			catch (IndexOutOfRangeException e)
			{
				Random rand = new Random();
				color = Color.FromArgb((int)(rand.NextDouble() * 255), (int)(rand.NextDouble() * 255), (int)(rand.NextDouble() * 255));
			}
			Color colorBloated;
			try
			{
				colorBloated = colorsBloated[robotID - 1];
			}
			catch (IndexOutOfRangeException e)
			{
				Random rand = new Random();
				colorBloated = Color.FromArgb((int)(rand.NextDouble() * 255), (int)(rand.NextDouble() * 255), (int)(rand.NextDouble() * 255));
			}
			if (robotID != 5)
			{
				RobotRenderer rr = new RobotRenderer("Robot " + robotID, color);
				//robots[robotID] = new Robot(rr, color, robotID);
				robots[robotID] = new Robot(rr, color, colorBloated, robotID);
				r.AddRenderable(rr);
				r.AddRenderable(robots[robotID].PathRenderer);
				r.AddRenderable(robots[robotID].LidarRendererFront);
				r.AddRenderable(robots[robotID].LidarRendererRear);
				r.AddRenderable(robots[robotID].PolygonRenderer);
				r.AddRenderable(robots[robotID].BloatedPolygonRenderer);

				r.UpdateRobotPose(robotID, new Magic.Common.RobotPose());
			}
		}

		public void Handle2DLidarData(int robotID, ILidarScan<ILidar2DPoint> scan)
		{
			if (robots.ContainsKey(robotID) == false) { Console.WriteLine("warning: receiving lidar data for unknown robot : " + robotID); return; }

			if (scan.ScannerID == 1) robots[robotID].LidarRendererFront.SetScan(scan);
			if (scan.ScannerID == 0) robots[robotID].LidarRendererFront.SetScan(scan);
		}

		public void AddNewRobot(int robotID)
		{
			AddNewRobot(robotID, defaultRenderer);
		}

		public Renderer DefaultRenderer
		{
			get { return defaultRenderer; }
			set { defaultRenderer = value; }
		}

		public RobotRenderer GetRenderer(int robotID)
		{
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			return robots[robotID].Renderer;
		}

		public IPath GetCurrentPath(int robotID)
		{
			return robots[robotID].CurrentPath;
		}

		public bool HasCommandedPath(int robotID)
		{
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			return robots[robotID].CommandedPath != null;
		}

		public IPath GetCommandedPath(int robotID)
		{
			return robots[robotID].CommandedPath.Clone();
		}

		public void SetCommandedPath(int robotID, IPath path)
		{
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			robots[robotID].CommandedPath = path;
		}

		public void SetCurrentPath(int robotID, IPath path)
		{
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			robots[robotID].CurrentPath = path;
		}

		public void SetSelectedCommandedPaths(IPath path)
		{
			foreach (int id in robots.Keys)
				if (robots[id].Renderer.IsSelected)
					robots[id].CommandedPath = path;
		}

		public void SetPath(int robotID, IPath path)
		{
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			robots[robotID].SetPath(path);
		}

		public bool HasRobot(int robotID)
		{
			return robots.Keys.Contains(robotID);
		}

		public List<int> GetAllRobotIDs()
		{
			return robots.Keys.ToList();
		}
		public int RobotCount()
		{
			return robots.Keys.Count;
		}

		public void ShowCameraFOV(int robotID, bool onoff)
		{
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			robots[robotID].Renderer.SetCameraOnOff = onoff;
			//else { robots[1].Renderer.SetCameraOnOff = true; }
		}

		public void UpdatePolygons(int robotID, List<Polygon> obstacles)
		{
			List<Polygon> convolutedObstacles = new List<Polygon>();
			Polygon vehiclePolygon = Polygon.VehiclePolygonWithRadius(0.5);
			foreach (Polygon p in obstacles)
			{
				Polygon convPoly = p;// Polygon.ConvexMinkowskiConvolution(p, vehiclePolygon);
				convolutedObstacles.Add(convPoly);
			}
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			////robots[robotID].PolygonRenderer.UpdatePolygons(obstacles);
			robots[robotID].PolygonRenderer.UpdatePolygons(convolutedObstacles);
		}
		public void UpdateBloatedPolygons(int robotID, List<Polygon> obstacles)
		{
			List<Polygon> convolutedObstacles = new List<Polygon>();
			Polygon vehiclePolygon = Polygon.VehiclePolygonWithRadius(0.5);
			foreach (Polygon p in obstacles)
			{
				Polygon convPoly = p;// Polygon.ConvexMinkowskiConvolution(p, vehiclePolygon);
				convolutedObstacles.Add(convPoly);
			}
			if (!robots.ContainsKey(robotID)) AddNewRobot(robotID);
			//robots[robotID].PolygonRenderer.UpdatePolygons(obstacles);
			robots[robotID].BloatedPolygonRenderer.UpdatePolygons(convolutedObstacles);
		}
		public void ClearPolygons(int robotID)
		{
			robots[robotID].PolygonRenderer.ClearPolygons();
			robots[robotID].BloatedPolygonRenderer.ClearPolygons();
		}

		public void DrawPaths(bool d)
		{
			foreach (int ii in robots.Keys)
			{
				robots[ii].PathRenderer.DrawBool = d;
			}
		}
	}

	class Robot
	{
		RobotRenderer renderer;
		IPath commandedPath;
		IPath currentPath;
		PathFromRobotRenderer pathRenderer;
		LidarRenderer lidarRendererFront;
		PolygonRenderer polyRenderer;
		PolygonRenderer bloatedPolygonRenderer;

		public LidarRenderer LidarRendererFront
		{
			get { return lidarRendererFront; }

		}
		LidarRenderer lidarRendererRear;

		public LidarRenderer LidarRendererRear
		{
			get { return lidarRendererRear; }

		}

		public Robot(RobotRenderer r, Color color, int id)
		{
			renderer = r;
			pathRenderer = new PathFromRobotRenderer(r, color, .1f);
			lidarRendererRear = new LidarRenderer("rear", 0, id, new SensorPose(-.36, 0, 0, Math.PI, 0, 0, 0));
			lidarRendererFront = new LidarRenderer("front", 1, id, new SensorPose(0, 0, .20, 0, 0, 0, 0));
			polyRenderer = new PolygonRenderer(color);
			bloatedPolygonRenderer = new PolygonRenderer(color);
		}
		public Robot(RobotRenderer r, Color color, Color colorBloated, int id)
		{
			renderer = r;
			pathRenderer = new PathFromRobotRenderer(r, color, .1f);
			lidarRendererRear = new LidarRenderer("rear", 0, id, new SensorPose(-.36, 0, 0, Math.PI, 0, 0, 0));
			lidarRendererFront = new LidarRenderer("front", 1, id, new SensorPose(0, 0, .20, 0, 0, 0, 0));
			polyRenderer = new PolygonRenderer(color);
			bloatedPolygonRenderer = new PolygonRenderer(colorBloated);
		}

		public RobotRenderer Renderer
		{
			get { return renderer; }
		}

		public IPath CommandedPath
		{
			get { return commandedPath; }
			set { commandedPath = value; }
		}

		public IPath CurrentPath
		{
			get { return currentPath; }
			set { currentPath = value; }
		}

		public void SetPath(IPath p)
		{
			pathRenderer.SetPath(p);
		}

		public PathFromRobotRenderer PathRenderer
		{
			get { return pathRenderer; }
		}

		public PolygonRenderer PolygonRenderer
		{
			get { return polyRenderer; }
		}
		public PolygonRenderer BloatedPolygonRenderer
		{
			get { return bloatedPolygonRenderer; }
		}
	}
}
