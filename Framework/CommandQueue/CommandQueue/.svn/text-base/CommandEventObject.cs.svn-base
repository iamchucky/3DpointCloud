using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Messages;

namespace Magic.CommandQueue
{
	class CommandEventObject
	{
		private Func<RobotCommandMessage, bool> predicate;
		private RobotCommandMessage commandMessage;

		public CommandEventObject(Func<RobotCommandMessage, bool> predicate, RobotCommandMessage commandMessage)
		{
			this.predicate = predicate;
			this.commandMessage = commandMessage;
		}
	}
}
