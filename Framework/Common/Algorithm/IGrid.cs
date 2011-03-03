using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Algorithm
{
	public interface IReadAsDoubleGrid
	{
		double GetValue(int x, int y);
		int Width { get; }
		int Height { get; }
	}
}
