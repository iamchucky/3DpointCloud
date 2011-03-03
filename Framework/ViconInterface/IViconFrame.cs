using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.ViconInterface
{
	public class IViconFrameEventArgs : EventArgs
	{
		private List<double> s;

		public IViconFrameEventArgs(List<double> s)
		{
			this.s = s;
		}

		public List<double> Frame
		{
			get { return s; }
		}
	}
}
