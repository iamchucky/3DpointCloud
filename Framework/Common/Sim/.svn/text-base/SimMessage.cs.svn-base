using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Sim
{
	[Serializable]
	public class SimMessage<T>
	{
		public int RobotID;
		public T Message;
		public double Timestamp;

		public SimMessage(int robot, T message, double timestamp)
		{
			RobotID = robot;
			Message = message;
			Timestamp = timestamp;
		}
	}
}
