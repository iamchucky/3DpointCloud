using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Shapes;

namespace Magic.Common.Messages
{
	[Serializable]
	public class GaussianMixMapMessage : IRobotMessage
	{
		float[] arrayToSend;

		public float[] DataArray
		{
			get { return arrayToSend; }
		}
		double timestamp;

		public double Timestamp
		{
			get { return timestamp; }
		}

		public GaussianMixMapMessage(double globalMapResolution, double globalMapExtent, float[] largeUDiff, float[] largeSigDiff, float[] pijDiff, int[] colIdx, int[] rowIdx, int messageCount, double timestamp )
		{
			this.timestamp = timestamp;
			int lengthOfEach = largeUDiff.Length;
			this.arrayToSend = new float[lengthOfEach * 3 + 4];
			arrayToSend[0] = (float)globalMapResolution;
			arrayToSend[1] = (float)globalMapExtent;
			arrayToSend[2] = messageCount;
			arrayToSend[3] = lengthOfEach;
			for (int i = 2; i < lengthOfEach + 2; i++)
			{
				arrayToSend[(i * 5) + 0] = largeUDiff[i - 2];
				arrayToSend[(i * 5) + 1] = largeSigDiff[i - 2];
				arrayToSend[(i * 5) + 2] = pijDiff[i - 2];
				arrayToSend[(i * 5) + 3] = colIdx[i - 2];
				arrayToSend[(i * 5) + 4] = rowIdx[i - 2];
			}
		}

		#region IRobotMessage Members

		public int RobotID
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
