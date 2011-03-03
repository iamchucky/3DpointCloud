using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;

namespace Magic.Common.Messages
{
	[Serializable]
	public class HeightFeatureMessage
	{
		List<Polygon> features;
		public List<Polygon> Features { get { return features; } }

		public HeightFeatureMessage(List<Polygon> features)
		{
			this.features = features;
		}
	}
}
