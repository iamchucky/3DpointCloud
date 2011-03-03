using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Magic.Common.Hardware
{
	public interface IHostComputer
	{
		string Name { get; }
		IPAddress IP { get; }
	}
}
