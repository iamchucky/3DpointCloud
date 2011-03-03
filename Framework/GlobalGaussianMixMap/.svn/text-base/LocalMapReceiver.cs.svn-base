using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.OccupancyGrid;
using Magic.Network;

namespace Magic.Sensor.GlobalGaussianMixMap
{
	/// <summary>
	/// Class for robot to receive local map update from central sensor processor
	/// </summary>
	public class LocalMapReceiver
	{
		INetworkAddressProvider addressProvider;
		public GenericMulticastClient<UpdateMapDataMessage> localMapUpdateClient;

		OccupancyGrid2D heightMap;
		OccupancyGrid2D covMap;
		OccupancyGrid2D pijMap;
		object locker = new object();

		//public LocalMapReceiver(ref OccupancyGrid2D heightMap, ref OccupancyGrid2D covMap, ref OccupancyGrid2D pijMap)
		//public LocalMapReceiver(ref GaussianMixtureMapping.GaussianMixMappingQ gaussMapper)
		public LocalMapReceiver()
		{
			//this.heightMap = gaussMapper.UhatGM;
			//this.covMap = gaussMapper.SigSqrCov;
			//this.pijMap = gaussMapper.Pij_sum;

			addressProvider = new HardcodedAddressProvider();
			localMapUpdateClient = new GenericMulticastClient<UpdateMapDataMessage>(addressProvider.GetAddressByName("LocalMapResponse"), new CSharpMulticastSerializer<UpdateMapDataMessage>(true));
			localMapUpdateClient.Start();
			//localMapUpdateClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<UpdateMapDataMessage>>(localMapUpdateClient_MsgReceived);
		}

		//void localMapUpdateClient_MsgReceived(object sender, MsgReceivedEventArgs<UpdateMapDataMessage> e)
		//{
		//    lock (locker)
		//    {
		//        //UpdateCurrentLocalMap(ref heightMap, ref covMap, ref pijMap, e.message);
		//        UpdateCurrentLocalMap(ref heightMap, e.message);
		//    }
		//}

		void UpdateCurrentLocalMap(ref OccupancyGrid2D currentLocalHeightMap, ref OccupancyGrid2D currentLocalCovMap,
															 ref OccupancyGrid2D currentLocalPijMap, UpdateMapDataMessage updateMessage)
		{
			try
			{
				//check if the size matches
				foreach (UpdateMapDataMessage.UpdateMapDataCell cell in updateMessage.CellData)
				{
					if (cell.X == cell.Y && cell.Y == 0) continue; // the first packet
					double x = cell.X; double y = cell.Y;
					//currentLocalHeightMap.SetCell(x, y, cell.Height);
					//currentLocalCovMap.SetCell(x, y, cell.Cov);
					//currentLocalPijMap.SetCell(x, y, cell.Pij);
				}
			}
			catch
			{
				throw new IndexOutOfRangeException("The update message and current map are not in the same size");
			}

		}

		public void UpdateCurrentLocalMap(ref GaussianMixtureMapping.GaussianMixMappingQ gaussMapper, UpdateMapDataMessage updateMessage)
		{
			try
			{
				//check if the size matches
				foreach (UpdateMapDataMessage.UpdateMapDataCell cell in updateMessage.CellData)
				{
					double x = cell.X; double y = cell.Y;
					gaussMapper.SetPijSum(x, y, cell.PijSum);
					gaussMapper.SetPuHat(x, y, cell.PuHat);
					gaussMapper.SetPuHatSquare(x, y, cell.PuHatSquare);
					gaussMapper.SetPsigUhatSquare(x, y, cell.PSigUhatSquare);
					gaussMapper.UpdateHeight(x, y);
				}
			}
			catch
			{
				throw new IndexOutOfRangeException("The update message and current map are not in the same size");
			}

		}
	}
}
