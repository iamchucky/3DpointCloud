using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common
{
	/// <summary>
	/// NOTE THIS IS ALWAYS ASSUMED TO BE DEGREES!!!!!!!
	/// </summary>
	public struct LLACoord
	{
        public LLACoord(double lat, double lon, double alt)
        { this.lat = lat; this.lon = lon; this.alt = alt; }

		/// <summary>
		/// degrees!
		/// </summary>
		public double lat;
		/// <summary>
		/// degrees!
		/// </summary>
		public double lon;
		/// <summary>
		/// meters!
		/// </summary>
		public double alt;

		public override string ToString()
		{
			return lat.ToString("F16") + "," + lon.ToString("F16") + "," + alt.ToString("F16");
		}
	}
}
