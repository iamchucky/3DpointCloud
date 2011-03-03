using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common;

namespace Magic.Rendering.Renderables
{
	public class OriginRenderer : IRender
	{
		#region IRender Members

		public string GetName()
		{
			return "";
		}

		public void Draw(Renderer cam)
		{
			GLUtility.DrawCircle(new GLPen(Color.Red, 1.0f),new Vector2(0,0),.5f);
		}

		public void ClearBuffer()
		{
			
		}

		public bool VehicleRelative
		{
			get { return false; }
		}

		public int? VehicleRelativeID
		{
			get { return null; }
		}

		#endregion
	}
}
