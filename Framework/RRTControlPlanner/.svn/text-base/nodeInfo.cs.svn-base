using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;

namespace Magic.PathPlanning
{
	[Serializable]
	public abstract class nodeInfo
	{
		public nodeInfo()
		{
			this.Zero();
		}

		public nodeInfo(nodeInfo p)
		{
			x = p.x;
			y = p.y;
			theta = p.theta;
			v = p.v;
			w = p.w;
			timestamp = p.timestamp;
		}
		/// <summary>
		/// Represents the position and orientation of a robot
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="theta"></param>
		/// <param name="v"></param>
		/// <param name="w"></param>
		/// 
		public nodeInfo(double x, double y, double theta, double v, double w)
		{
			this.x = x; this.y = y; this.theta = theta;
			this.v = v; this.w = w;
		}

		public nodeInfo(double x, double y, double theta, double v, double w, double timestamp)
		{
			this.x = x; this.y = y; this.theta = theta;
			this.v = v; this.w = w;
			this.timestamp = timestamp;
		}

		/// <summary>
		/// x-axis
		/// </summary>
		public double x;

		/// <summary>
		/// y-axis
		/// </summary>
		public double y;

		/// <summary>
		/// theta
		/// </summary>
		public double theta;

		/// <summary>
		/// velocity
		/// </summary>
		public double v;

		/// <summary>
		/// Rotational position
		/// </summary>
		public double w;

		/// <summary>
		/// Rotational position
		/// </summary>
		public double timestamp;


		/// <summary>
		/// Resets all parameters to 0
		/// </summary>
		public void Zero()
		{
			x = y = theta = w = v = timestamp = 0;
		}

		public override string ToString()
		{
			return "nodeInfo: X:" + x + " Y:" + y + " theta:" + theta
							+ " w:" + w + " v:" + v + " @:" + timestamp;
		}

		public Vector2 ToVector2()
		{
			return new Vector2(x, y);
		}
	}
}
