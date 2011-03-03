using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Shapes;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public interface IWaypoint : IKeypoint
	{
		/// <summary>
		/// integer corresponding to the path ID it is a part of
		/// </summary>
		int PathID { get; set; }

		/// <summary>
		/// integer corresponding to the waypoint's order in the path
		/// </summary>
		int SequenceID { get; set; }
	}
}
