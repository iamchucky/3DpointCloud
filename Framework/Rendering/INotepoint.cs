using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Shapes;
using System.Drawing;

namespace Magic.Rendering.Renderables
{
	public interface INotepoint : IKeypoint
	{
		/// <summary>
		/// string (optional) associated with notepoint
		/// </summary>
		string Comments { get; set; } 
	}
}
