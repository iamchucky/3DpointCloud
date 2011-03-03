using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Sensor.GaussianMixtureMapping;

namespace Magic.Sensor.GlobalGaussianMixMap
{
	[Serializable]
	public class UpdateMapDataMessage
	{
		[Serializable]
		public struct UpdateMapDataCell
		{
			// indices
			float x;
			public float X { get { return x; } }
			float y;
			public float Y { get { return y; } }
			// height value
			//float height;
			//public float Height { get { return height; } }
			//float cov;
			//public float Cov { get { return cov; } }
			//float pij;
			//public float Pij { get { return pij; } }

			float pijSum; public float PijSum { get { return pijSum; } }
			float puhat; public float PuHat { get { return puhat; } }
			float puHatSquare; public float PuHatSquare { get { return puHatSquare; } }
			float pSigUhatSquare; public float PSigUhatSquare { get { return pSigUhatSquare; } }

			// constructor
			public UpdateMapDataCell(float x, float y, float pijSum, float puhat, float puHatSquare, float pSigUhatSquare)
			{
				this.x = x; this.y = y; this.pijSum = pijSum; this.puhat = puhat; this.puHatSquare = puHatSquare; this.pSigUhatSquare = pSigUhatSquare;
			}

		}

		List<UpdateMapDataCell> cellData;
		public List<UpdateMapDataCell> CellData { get { return cellData; } }

		//public UpdateMapDataMessage(int robotID, List<Position> indexList, List<float> heightList, List<float> covList, List<float> pijList)
		//{
		//    cellData = new List<UpdateMapDataCell>(indexList.Count + 1);
		//    cellData.Add(new UpdateMapDataCell(robotID, robotID, 0.0f, 0.0f, 0.0f));
		//    for (int i = 1; i < cellData.Capacity; i++)
		//    {
		//        cellData.Add(new UpdateMapDataCell(indexList[i - 1].X, indexList[i - 1].Y, heightList[i - 1], covList[i - 1], pijList[i - 1]));
		//    }
		//}

		public UpdateMapDataMessage(List<UpdateMapDataCell> list)
		{
			this.cellData = new List<UpdateMapDataCell>(list);
		}

		public UpdateMapDataMessage(int robotID, List<Position> indexList, List<float> pijSum, List<float> puhat, List<float> puHatSquare, List<float> pSigUhatSquare)
		{
			cellData = new List<UpdateMapDataCell>(indexList.Count + 1);
			cellData.Add(new UpdateMapDataCell(robotID, robotID, 0.0f, 0.0f, 0.0f, 0.0f));
			for (int i = 1; i < cellData.Capacity; i++)
			{
				cellData.Add(new UpdateMapDataCell(indexList[i - 1].X, indexList[i - 1].Y, pijSum[i - 1], puhat[i - 1], puHatSquare[i - 1], pSigUhatSquare[i - 1]));
			}
		}
	}
}
