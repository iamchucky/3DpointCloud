using System;
using System.Collections.Generic;
using System.Text;

namespace Magic.Common.Shapes {
	[Serializable]
	public class LineList : List<Vector2> {

		public LineList() {
		}

		public LineList(IEnumerable<Vector2> points)
			: base(points) {
		}

		public LineList(int capacity)
			: base(capacity) {
		}

		public LineList Transform(IVector2Transformer transformer) {
			LineList ret = new LineList();
			ret.Capacity = this.Count;

			ret.AddRange(transformer.TransformPoints(this));

			return ret;
		}
	}
}
