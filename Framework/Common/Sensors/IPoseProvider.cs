using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Sensors
{
	/// <summary>
	/// This is essentially a component that provides 3D Localization information.
	/// </summary>
	public interface IPoseProvider : IDisposable
	{
		event EventHandler<NewPoseAvailableEventArgs> NewPoseAvailable;

		RobotPose Pose { get; }
	}

	public class NewPoseAvailableEventArgs : EventArgs
	{
		private RobotPose pose;

		public RobotPose Pose
		{
			get { return pose; }
			set { pose = value; }
		}

		public NewPoseAvailableEventArgs(RobotPose pose)
		{
			this.pose = pose;
		}
	}

	public class NewStateAvailableEventArgs : EventArgs
	{
		private PoseFilterState state;

		public PoseFilterState State
		{
			get { return state; }
			set { state = value; }
		}

		public NewStateAvailableEventArgs(PoseFilterState state)
		{
			this.state = state;
		}
	}
}