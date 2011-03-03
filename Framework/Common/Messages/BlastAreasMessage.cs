using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;

namespace Magic.Common.Messages
{
	[Serializable]
	public class BlastAreasMessage
	{
		public BlastAreasMessage(List<int> targetIds, List<Polygon> blastAreas)
		{
			TargetIds = targetIds;
			BlastAreas = blastAreas;
		}

		public List<Polygon> BlastAreas
		{
			get;
			set;
		}

		public List<int> TargetIds
		{
			get;
			set;
		}
	}
}
