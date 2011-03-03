using System;
using System.Collections.Generic;
using System.Text;
using Magic.Common;

namespace Magic.Rendering
{
	/// <summary>
	/// Proivdes a common interface for objects to be rendered using the Magic.Renderer
	/// </summary>
	public interface IRender
	{
		string GetName();
		void Draw(Renderer cam);
		void ClearBuffer();
		/// <summary>
		/// Indicates if this item should be drawn relative to a vehicle. 
		/// </summary>
		/// <returns></returns>
		bool VehicleRelative { get; }

		/// <summary>
		/// Indicates which vehicle the renderable should be drawn relative to. Null if not vehicle relative.
		/// </summary>
		int? VehicleRelativeID { get; }


	}
}
