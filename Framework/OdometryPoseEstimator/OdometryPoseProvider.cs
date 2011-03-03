using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common;
//using Magic.Sensors;

namespace OdometryPoseEstimator
{
	/// <summary>
	/// This class provides a pose provider based comepltely on blind faith in the robots odometry. 
	/// The guts of this class are actually handled in the OdometryProcesor...
	/// </summary>

	public class OdometryPoseProvider : IPoseProvider
	{
		IOdometryProcessor odometryProcessor;
		double curTS = 0;
		public OdometryPoseProvider(IOdometryProcessor odometryProcessor)
		{
			this.odometryProcessor = odometryProcessor;
			odometryProcessor.OdometryUpdate += new EventHandler<Magic.Common.TimestampedEventArgs<Magic.Common.RobotPose>>(odometryProcessor_OdometryUpdate);
		}

		void odometryProcessor_OdometryUpdate(object sender, Magic.Common.TimestampedEventArgs<Magic.Common.RobotPose> e)
		{
			curTS = e.TimeStamp;
			if (NewPoseAvailable != null)
				NewPoseAvailable(this, new NewPoseAvailableEventArgs(odometryProcessor.CurrentState));
			if (NewOdomAvailable != null)
				NewOdomAvailable(this, new Magic.Common.TimestampedEventArgs<RobotPose>(curTS, odometryProcessor.CurrentState));
		}

		#region IPoseProvider Members

		public event EventHandler<NewPoseAvailableEventArgs> NewPoseAvailable;
		public event EventHandler<Magic.Common.TimestampedEventArgs<RobotPose>> NewOdomAvailable;

		public Magic.Common.RobotPose Pose
		{
			get { return odometryProcessor.CurrentState; }
		}

		#endregion

		#region ISensor Members

		public void Start()
		{

		}

		public void Start(System.Net.IPAddress localBind)
		{

		}

		public void Stop()
		{

		}

		#endregion

		public void Dispose()
		{

		}

		#region ITimingProvider Members

		public double GetCurrentTime()
		{
			return curTS;
		}

		#endregion
	}
}
