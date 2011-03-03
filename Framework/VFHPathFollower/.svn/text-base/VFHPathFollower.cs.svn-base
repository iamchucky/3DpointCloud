using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Robots;
using System.Threading;
using Magic.Common.Path;
using Magic.SickInterface;
using Magic.Network;
using Magic.Common.Sensors;

namespace Magic.PathPlanning
{
	public class VFHPathFollower : IDisposable
	{
		ILidarScan<ILidar2DPoint> scan;
		SickLMS sick;

		IPath path;
        RobotPose pose;

		object pathLock = new object();

		double maxVelocity, maxTurn;
		double[] histogram;

		public VFHPathFollower(double maxVelocity, double maxTurn)
		{
			INetworkAddressProvider na = new HardcodedAddressProvider();
			sick = new SickLMS(na.GetAddressByName("Sick"), new SensorPose());
			sick.ScanReceived += new EventHandler<ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>>(sick_ScanReceived);
            sick.Start();

			histogram = new double[361];

			this.maxVelocity = maxVelocity;
			this.maxTurn = maxTurn;
		}

        public RobotTwoWheelCommand GetCommand()
        {
            double velocity, turn;

            double error = pose.yaw - 

            return new RobotTwoWheelCommand(velocity, turn);
        }

		public void UpdatePath(IPath path)
		{
			if (path == null) return;
			if (PathUtils.CheckPathsEqual(this.path, path)) return;
			lock (pathLock)
			{
				this.path = path;
			}
		}

        public void UpdatePose(RobotPose pose)
        {
            if (pose == null) return;

            this.pose = pose;
        }

		private void BuildHistogram()
		{
			if (scan.Points.Count != 361)
			{
				Console.WriteLine("Scan count incorrect. It is " + scan.Points.Count);
				return;
			}

			for (int i = 0; i < 361; i++)
				histogram[i] = 0;

			for (int i = 0; i < 361; i++)
			{
				ILidar2DPoint p = scan.Points.ElementAt(i);

				if (i - 2 >= 0) histogram[i - 2] += 0.25 * 80 / p.RThetaPoint.R;
				if (i - 1 >= 0) histogram[i - 1] += 0.5 * 80 / p.RThetaPoint.R;
				histogram[i] += 80 / p.RThetaPoint.R;
				if (i + 1 < 361) histogram[i + 1] += 0.5 * 80 / p.RThetaPoint.R;
				if (i + 2 < 361) histogram[i + 2] += 0.5 * 80 / p.RThetaPoint.R;
			}

			Console.Clear();

			for (int i = 0; i < 361; i += 4)
				Console.Write((int) histogram[i] + " ");

			Console.WriteLine("");
		}

		void sick_ScanReceived(object sender, ILidarScanEventArgs<ILidarScan<ILidar2DPoint>> e)
		{
			scan = e.Scan;
			BuildHistogram();
		}

		#region IDisposable Members

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
