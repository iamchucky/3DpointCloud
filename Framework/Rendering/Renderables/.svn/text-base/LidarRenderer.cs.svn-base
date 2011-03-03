using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common;
using Magic.Common.Sensors;


namespace Magic.Rendering.Renderables
{

	public class LidarRenderOptions : BaseRenderOptions
	{
		private Color renderColor = Color.Green;
		private Color boundaryColor = Color.Red;
		public Color RenderColor { get { return renderColor; } set { renderColor = value; } }
		public Color BoundaryColor { get { return boundaryColor; } set { boundaryColor = value; } }
		bool showOrigin = false; public bool ShowOrigin { get { return showOrigin; } set { showOrigin = value; } }
		bool showTS = false; public bool ShowTS { get { return showTS; } set { showTS = value; } }
		bool isUpsideDown = false; public bool IsUpsideDown { get { return isUpsideDown; } set { isUpsideDown = value; } }
		bool showIndex = false; public bool ShowIndex { get { return showIndex; } set { showIndex = value; } }
		bool showBoundary = false; public bool ShowBoundary { get { return showBoundary; } set { showBoundary = value; } }
		float boundaryWidth = 1.0f; public float BoundaryWidth { get { return boundaryWidth; } set { boundaryWidth = value; } }
	}


	public class LidarRenderer : IRenderOptions
	{
		ILidarScan<ILidar2DPoint> scan;
		LidarRenderOptions options = new LidarRenderOptions();
		private object drawLock = new object();
		private string name;
		private double time = 0;
		private int packetNum = 0;
		private int numPoints = 0;
		private int startIdx = 0;
		private int endIdx = 0;
		public int sensorID;
		public int robotID;
		RobotPose robotPose = new RobotPose(0, 0, 0, 0, 0, 0, 0);

		//SensorCoverage coverage;
		SensorPose laserToBody;

		public LidarRenderer(string name, int sensorID, int robotID, SensorPose laserToBody)
		{
			this.name = name;
			this.sensorID = sensorID;
			this.robotID = robotID;
			this.laserToBody = laserToBody;
		}
		#region IRender Members

		public void SetScan(ILidarScan<ILidar2DPoint> scan)
		{
			lock (this.drawLock)
			{
				this.scan = scan;
				this.time = scan.Timestamp;
				this.numPoints = scan.Points.Count;
			}
		}

		public void SetScan(ILidarScan<ILidar2DPoint> scan, RobotPose robotPose, int startIdx, int endIdx)
		{
			lock (this.drawLock)
			{
				this.scan = scan;
				this.time = scan.Timestamp;
				this.numPoints = endIdx - startIdx + 1;
				this.robotPose = robotPose;
				this.startIdx = startIdx;
				this.endIdx = endIdx;
			}
		}

		public void SetScan(ILidarScan<ILidar2DPoint> scan, RobotPose robotPose)
		{
			lock (this.drawLock)
			{
				this.scan = scan;
				this.time = scan.Timestamp;
				this.numPoints = scan.Points.Count;
				this.robotPose = robotPose;
				this.startIdx = 0;
				this.endIdx = scan.Points.Count - 1;
			}
		}

		public double TimeStamp
		{ get { return time; } }
		public int PacketNumber
		{ get { return packetNum; } }
		public int NumPoints
		{ get { return numPoints; } }
		public string GetName()
		{
			return name;
		}
		public void ClearBuffer()
		{
			scan = null;
		}
		
		public void Draw(Renderer r)
		{
			if (options.Show == false) return;
			if (scan == null) return; //nothing to do
			GLUtility.DisableNiceLines();
			GLUtility.GoToSensorPose(laserToBody);

			lock (this.drawLock)
			{
				//foreach (ILidar2DPoint sp in scan.Points)
				for (int i = startIdx; i <= endIdx; i++)
				{
					PointF p = PointF.Empty;
					ILidar2DPoint sp = scan.Points[i];
					if (options.IsUpsideDown)
						p = new PointF((float)(sp.RThetaPoint.R * Math.Cos(-sp.RThetaPoint.theta + robotPose.yaw) + robotPose.x), (float)(sp.RThetaPoint.R * Math.Sin(-sp.RThetaPoint.theta + robotPose.yaw) + robotPose.y));
					else
						p = new PointF((float)(sp.RThetaPoint.R * Math.Cos(sp.RThetaPoint.theta + robotPose.yaw) + robotPose.x), (float)(sp.RThetaPoint.R * Math.Sin(sp.RThetaPoint.theta + robotPose.yaw) + robotPose.y));
					GLUtility.DrawCross(new GLPen(options.RenderColor, 1), Vector2.FromPointF(p), .1f);
					if (options.ShowIndex)
						GLUtility.DrawString("Index: " + i, Color.Black, p);
				}
			}
			if (options.ShowTS) GLUtility.DrawString("Time: " + scan.Timestamp.ToString(), Color.Black, new PointF(0, 0));
			if (options.ShowOrigin) GLUtility.DrawCircle(new GLPen(Color.Red, 1), new PointF(0, 0), .075f);
			if (options.ShowBoundary)
			{
				PointF p = PointF.Empty, p2 = PointF.Empty;
				PointF pose = PointF.Empty;
				ILidar2DPoint spInitial = scan.Points[0];
				ILidar2DPoint spEnd = scan.Points[scan.Points.Count - 1];
				if (options.IsUpsideDown)
				{
					p = new PointF((float)(spInitial.RThetaPoint.R * Math.Cos(-spInitial.RThetaPoint.theta + robotPose.yaw) + robotPose.x), (float)(spInitial.RThetaPoint.R * Math.Sin(-spInitial.RThetaPoint.theta + robotPose.yaw) + robotPose.y));
					p2 = new PointF((float)(spEnd.RThetaPoint.R * Math.Cos(-spEnd.RThetaPoint.theta + robotPose.yaw) + robotPose.x), (float)(spEnd.RThetaPoint.R * Math.Sin(-spEnd.RThetaPoint.theta + robotPose.yaw) + robotPose.y));
				}
				else
				{
					p = new PointF((float)(spInitial.RThetaPoint.R * Math.Cos(spInitial.RThetaPoint.theta + robotPose.yaw) + robotPose.x), (float)(spInitial.RThetaPoint.R * Math.Sin(spInitial.RThetaPoint.theta + robotPose.yaw) + robotPose.y));
					p2 = new PointF((float)(spEnd.RThetaPoint.R * Math.Cos(spEnd.RThetaPoint.theta + robotPose.yaw) + robotPose.x), (float)(spEnd.RThetaPoint.R * Math.Sin(spEnd.RThetaPoint.theta + robotPose.yaw) + robotPose.y));
				}
				pose.X = (float)robotPose.x; pose.Y = (float)robotPose.y;
				GLUtility.DrawLine(new GLPen(options.BoundaryColor, options.BoundaryWidth), p, pose);
				GLUtility.DrawLine(new GLPen(options.BoundaryColor, options.BoundaryWidth), p2, pose);
			}
			GLUtility.ComeBackFromSensorPose();
			GLUtility.EnableNiceLines();
		}

		public bool VehicleRelative
		{
			get { return true; }
		}

		public int? VehicleRelativeID
		{
			get { return robotID; }
		}

		public BaseRenderOptions GetOptions()
		{
			return options;
		}

		public void SetOptions(BaseRenderOptions options)
		{
			this.options = (LidarRenderOptions)options;
		}

		#endregion
		/*
		#region ISensorCoverage Members

		public SensorCoverage GetSensorCoverage()
		{
			return coverage;
		}

		#endregion

        */
	}
}
