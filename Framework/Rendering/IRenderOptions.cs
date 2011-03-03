using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Rendering
{
	/// <summary>
	/// Renderables that have options displayed in a generic way in the UI
	/// </summary>
	public interface IRenderOptions : IRender
	{
		BaseRenderOptions GetOptions();
		void SetOptions(BaseRenderOptions options);
	}
}
