using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Sensors
{
	public interface IInertialNavigationSystem : ISensor
	{
		event EventHandler<TimestampedEventArgs <InertialNavigationData>> MeasurementReceived;
	}

	public interface InertialNavigationData
	{
		LLACoord position {get;}
		RobotPose ENUPosition{get;}
	}

}
