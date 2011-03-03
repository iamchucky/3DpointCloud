using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Magic.Common.Mapack;

namespace Magic.Common.Sensors
{
	public interface ISensor
	{
		void Start();
		void Start(IPAddress localBind);
		void Stop();

		SensorPose ToBody { get; }
	}
}
