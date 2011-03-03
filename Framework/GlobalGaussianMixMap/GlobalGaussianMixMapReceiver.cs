using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.OccupancyGrid;
using Magic.Network;
using Magic.Common.Messages;
using Magic.Sensor.GaussianMixtureMapping;

namespace Magic.Sensor.GlobalGaussianMixMap
{
    public class GlobalGaussianMixMapReceiver
    {
        bool isWorking = false;

        public bool IsWorking
        {
            get { return isWorking; }
        }

        // field members
        OccupancyGrid2D globalHeightMap;
        public OccupancyGrid2D GlobalHeightMap
        {
            get { return globalHeightMap; }
        }

		OccupancyGrid2D thresholdGlobalHeightMap;
		public OccupancyGrid2D ThresholdHeightMap
		{
			get { return thresholdGlobalHeightMap; }
		}

        OccupancyGrid2D globalSigMap;
        public OccupancyGrid2D GlobalSigMap
        {
            get { return globalSigMap; }
        }

        OccupancyGrid2D globalPijMap;
        public OccupancyGrid2D GlobalPijSumMap
        {
            get { return globalPijMap; }
        }

        OccupancyGrid2D laserHitMap;
        public OccupancyGrid2D LaserHitMap
        { get { return laserHitMap; } }

        // declaration
        INetworkAddressProvider addressProvider;
        GenericMulticastClient<GlobalMapCell> gaussMixMapClient;

        public GlobalGaussianMixMapReceiver()
        {
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            addressProvider = new HardcodedAddressProvider();
            gaussMixMapClient = new GenericMulticastClient<GlobalMapCell>(addressProvider.GetAddressByName("GlobalGaussianMixMap"), new CSharpMulticastSerializer<GlobalMapCell>(true));
            gaussMixMapClient.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
            gaussMixMapClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<GlobalMapCell>>(gaussMixMapClient_MsgReceived);
        }

        void gaussMixMapClient_MsgReceived(object sender, MsgReceivedEventArgs<GlobalMapCell> e)
        {
            isWorking = true;

            if (globalHeightMap == null)
            {
                double globalMapResolution = e.message.DataList[0].largeU; // first index has grid information
                double globalMapExtent = e.message.DataList[0].largeSig;
                globalHeightMap = new OccupancyGrid2D(globalMapResolution, globalMapResolution, globalMapExtent, globalMapExtent);
				globalSigMap = new OccupancyGrid2D(globalHeightMap);
                globalPijMap = new OccupancyGrid2D(globalHeightMap);
                laserHitMap = new OccupancyGrid2D(globalHeightMap);
				thresholdGlobalHeightMap = new OccupancyGrid2D(globalHeightMap);
            }
            // if message is received, update the 3 gridmap
            GlobalGaussianMixMap.ConvertToOcGrid(ref globalHeightMap, ref globalSigMap, ref globalPijMap, ref laserHitMap,
                e.message.DataList);
			for (int i = 0; i < globalHeightMap.NumCellX; i++)
			{
				for (int j = 0; j < globalHeightMap.NumCellY; j++)
				{
					if (globalPijMap.GetCellByIdx(i, j) > 1)
					{
						thresholdGlobalHeightMap.SetCellByIdx(i, j, globalHeightMap.GetCellByIdx(i, j));
					}
					else
					{
						thresholdGlobalHeightMap.SetCellByIdx(i, j, 0);
					}
				}
			}
        }
    }
}
