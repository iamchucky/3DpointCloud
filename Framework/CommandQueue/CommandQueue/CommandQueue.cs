using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.CommandQueue
{
	public class CommandQueue
	{
		private Queue<CommandEventObject> commandQueue;

		public CommandQueue()
		{
			commandQueue = new Queue<CommandEventObject>();
		}


	}
}
