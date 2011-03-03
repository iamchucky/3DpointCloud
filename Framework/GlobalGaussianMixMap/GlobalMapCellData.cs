using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Sensor.GaussianMixtureMapping;

namespace Magic.Sensor.GlobalGaussianMixMap
{
    #region working code but slow

    [Serializable]
    public class GlobalMapCell
    {
        [Serializable]
        public struct GlobalMapCellData
        {
            public float largeU;
            public float largeSig;
            public float pij;
            public int laserHit;
            public int rowIdx;
            public int colIdx;

            public GlobalMapCellData(float largeU, float largeSig, float pij, float laserHit, int rowIdx, int colIdx)
            {
                this.largeU = largeU;
                this.largeSig = largeSig;
                this.pij = pij;
                this.rowIdx = rowIdx;
                this.colIdx = colIdx;
                this.laserHit = (int)laserHit;
            }
        }

        List<GlobalMapCellData> dataList;

        public List<GlobalMapCellData> DataList
        {
            get { return dataList; }
        }

        public GlobalMapCell(double globalMapResolution, double globalMapExtent, List<Index> indexList, List<float> largeU, List<float> largeSig, List<float> pij, List<float> laserHit)
        {
            dataList = new List<GlobalMapCellData>(largeSig.Count + 1);
            // save grid info to the first data cell
            dataList.Add(new GlobalMapCellData((float)globalMapResolution, (float)globalMapExtent, 0, 0, 0, 0));
            for (int i = 1; i < largeU.Count + 1; i++)
            {
                dataList.Add(new GlobalMapCellData(largeU[i - 1], largeSig[i - 1], pij[i - 1], laserHit[i - 1], indexList[i - 1].Row, indexList[i - 1].Col));
            }
        }
    }

    #endregion

    /*
	[Serializable]
	public struct GlobalMapCell
	{

		List<float> largeU;

		public List<float> LargeU
		{
			get { return largeU; }
		}
		List<float> largeSig;

		public List<float> LargeSig
		{
			get { return largeSig; }
		}
		List<float> pijSum;

		public List<float> PijSum
		{
			get { return pijSum; }
		}

		List<Index> indices;

		public List<Index> Indices
		{
			get { return indices; }
		}

		double globalMapResolution;

		public double GlobalMapResolution
		{
			get { return globalMapResolution; }
		}
		double globalMapExtent;

		public double GlobalMapExtent
		{
			get { return globalMapExtent; }
		}

		public GlobalMapCell(double globalMapResolution, double globalMapExtent, List<Index> indexList, List<float> heightList, List<float> covList, List<float> pijSumList)
		{
			this.globalMapExtent = globalMapExtent;
			this.globalMapResolution = globalMapResolution;
			this.largeU = heightList;
			this.largeSig = covList;
			this.pijSum = pijSumList;
			this.indices = indexList;
		}
	}
	 * */
}
