using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;
using System.Drawing;

namespace Magic.Rendering
{
	public interface ISelectablePolygon : ISelectable
	{
		/// <summary>
		/// The polygon
		/// </summary>
		Polygon Polygon { get; set; }

		/// <summary>
		/// The color of the polygon
		/// </summary>
		Color Color { get; }
	}
}
