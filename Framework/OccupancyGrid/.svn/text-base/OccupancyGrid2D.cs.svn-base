using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Magic.Common.DataTypes;
using Magic.Common.Sensors;
using Magic.Common.Robots;
using Magic.Common;
using Magic.Common.Mapack;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace Magic.OccupancyGrid
{
	/// <summary>
	/// This class provides an abstraction of an Occupancy Grid to be used for path planning and other obstacle avoidance components.
	/// Note that it only does the data management, filling cells is the responsibility of other components.
	/// 	
	/// </summary>

	[Serializable]
	public class OccupancyGrid2D : IEnumerable, IOccupancyGrid2D
	{

		public event EventHandler GridUpdated;
		public event EventHandler<NewOccupancyGrid2DAvailableEventArgs> FeaturesUpdated;

		double resolutionX;
		double resolutionY;
		double extentX;
		double extentY;
		protected double[,] occupancyMap;
		int nCellX, nCellY;

		Dictionary<Vector2, Boolean> decayingCells; bool decaying = false; object dataLock = new object();
		Thread decayingThread; double decayingFactor = 1e-3; double minCellValue;

		public double ResolutionX { get { return resolutionX; } }

		public double ResolutionY { get { return resolutionY; } }

		public double ExtentX { get { return extentX; } }

		public double ExtentY { get { return extentY; } }

		public int Height { get { return nCellY; } }

		public int Width { get { return nCellX; } }

		public int NumCellX { get { return nCellX; } }

		public int NumCellY { get { return nCellY; } }

		public double GetValue(int x, int y)
		{
			return GetCellByIdx(x, y);
		}

		public bool IsDecaying { get { return decaying; } }

		/// <summary>
		/// Creates a new Occupancy Grid. Note that everything is in meters.
		/// The extent indicates the size of the grid. It will go from -extentX,-extentY to +extentX,+extentY
		/// </summary>
		/// <param name="resolutionX"></param>
		/// <param name="resolutionY"></param>
		/// <param name="maxX"></param>
		/// <param name="maxY"></param>
		/// <param name="minX"></param>
		/// <param name="minY"></param>
		public OccupancyGrid2D(double resolutionX, double resolutionY, double extentX, double extentY)
		{
			this.resolutionX = resolutionX;
			this.resolutionY = resolutionY;
			this.extentX = extentX;
			this.extentY = extentY;
			this.nCellX = (int)Math.Ceiling(extentX * 2 / resolutionX);
			this.nCellY = (int)Math.Ceiling(extentY * 2 / resolutionY);

			occupancyMap = new double[nCellX, nCellY];
			decayingCells = new Dictionary<Vector2, bool>();
			decayingThread = new Thread(DecayingThread);
		}

		/// <summary>
		/// this is a copy constructor. You can use deepcopy, but this is better.
		/// </summary>
		/// <param name="gridToCopy"></param>
		public OccupancyGrid2D(OccupancyGrid2D gridToCopy)
			: this(gridToCopy.resolutionX, gridToCopy.resolutionY, gridToCopy.extentX, gridToCopy.extentY)
		{
			//Copy the occupancyMap
			Array.Copy(gridToCopy.occupancyMap, this.occupancyMap, gridToCopy.occupancyMap.Length);
		}

		public bool CheckValidIdx(int x, int y)
		{
			return !(x < 0 || y < 0 || x >= nCellX || y >= nCellY);
		}

		public double GetCell(double x, double y)
		{
			int xIndex, yIndex;
			GetIndicies(x, y, out xIndex, out yIndex);
			if (CheckValidIdx(xIndex, yIndex))
				return occupancyMap[xIndex, yIndex];
			return default(double);
		}

		public double GetCellByIdx(int xIdx, int yIdx)
		{
			if (CheckValidIdx(xIdx, yIdx))
				return occupancyMap[xIdx, yIdx];
			return default(double);
		}

		public double GetCellByIdxUnsafe(int xIdx, int yIdx)
		{
			return occupancyMap[xIdx, yIdx];
		}

		public double GetCellReal(double x, double y)
		{
			int xIndex, yIndex;
			GetIndicies(x, y, out xIndex, out yIndex);
			if (CheckValidIdx(xIndex, yIndex))
				return 1.0 - 1.0 / (Math.Exp(occupancyMap[xIndex, yIndex]));
			return default(double);
		}

		public void SetCell(double x, double y, double value)
		{
			//int xIndex = (int)((x / resolutionX) + (extentX / resolutionX / 2));
			//int yIndex = (int)((y / resolutionY) + (extentY / resolutionY / 2));
			int xIndex, yIndex;
			GetIndicies(x, y, out xIndex, out yIndex);
			if (CheckValidIdx(xIndex, yIndex))
			{
				occupancyMap[xIndex, yIndex] = value;
				if (GridUpdated != null) GridUpdated(this, null);
			}
		}

		public void SetCellByIdx(int xIdx, int yIdx, double value)
		{
			if (CheckValidIdx(xIdx, yIdx))
			{
				occupancyMap[xIdx, yIdx] = value;
				if (GridUpdated != null) GridUpdated(this, null);
			}
		}

		public void SetCells(Bitmap bmp)
		{
			//bmp.Save("test.bmp");
			if (bmp.Height == Height && bmp.Width == Width)
				for (int i = 0; i < Height; i++)
					for (int j = 0; j < Width; j++)
						occupancyMap[j, i] = bmp.GetPixel(j, i).B;
		}

		/// <summary>
		/// Setting bunch of points corresponding to the input points with constant value - written for height feature extraction
		/// </summary>
		/// <param name="points"></param>
		/// <param name="value"></param>
		public void UpdatePoints(List<Vector2> points, double value)
		{
			int idxX, idxY;
			foreach (Vector2 pt in points)
			{
				GetIndicies(pt.X, pt.Y, out idxX, out idxY);
				SetCellByIdx(idxX, idxY, value);
				lock (dataLock)
					decayingCells[new Vector2(idxX, idxY)] = true;
			}
			if (FeaturesUpdated != null)
				FeaturesUpdated(this, new NewOccupancyGrid2DAvailableEventArgs(this, 0));
		}

		public void StartDecaying(double decayingFactor, double minCellValue)
		{
			this.decayingFactor = decayingFactor;
			decaying = true;
			this.minCellValue = minCellValue;
			decayingThread.Start();
		}
		public void EndDecaying()
		{
			decaying = false;
		}

		// Decaying effect for feature extraction
		private void DecayingThread()
		{
			Dictionary<Vector2, Boolean> decayingCellsCopy;
			while (decaying)
			{
				lock (dataLock)
				{
					decayingCellsCopy = new Dictionary<Vector2, bool>(decayingCells);
				}
				foreach (KeyValuePair<Vector2, Boolean> pair in decayingCellsCopy)
				{
					int idxX = (int)pair.Key.X, idxY = (int)pair.Key.Y;
					if (pair.Value == true)
					{
						this.SetCellByIdx(idxX, idxY, Math.Max(GetCellByIdx(idxX, idxY) + Math.Log(decayingFactor, Math.E), minCellValue));
					}
					if (GetCellByIdx(idxX, idxY) < 0)
						decayingCells[pair.Key] = false;
				}
				Thread.Sleep(10);
			}
		}

		public unsafe void SetCellsFast(Bitmap bmp, RobotPose pose, IOccupancyGrid2D og, int setDiameter)
		{
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			byte* imgPtr = (byte*)(data.Scan0);
			byte red, green, blue;

			int poseIdxX, poseIdxY;
			og.GetIndicies(pose.x, pose.y, out poseIdxX, out poseIdxY);

			imgPtr += ((poseIdxY - setDiameter / 2) * data.Stride) + (poseIdxX - setDiameter / 2) * 3;

			for (int i = 0; i < setDiameter; i++)
			{
				for (int j = 0; j < setDiameter; j++)
				{
					blue = imgPtr[0];
					green = imgPtr[1];
					red = imgPtr[2];
					occupancyMap[j, i] = (double)blue;
					imgPtr += 3;
				}

				imgPtr += (data.Width - setDiameter) * 3;
			}

			bmp.UnlockBits(data);
		}

		public unsafe void SetCellsFast(Bitmap bmp)
		{
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			byte* imgPtr = (byte*)(data.Scan0);
			byte red, green, blue;


			for (int i = 0; i < data.Height; i++)
			{
				for (int j = 0; j < data.Width; j++)
				{
					blue = imgPtr[0];
					green = imgPtr[1];
					red = imgPtr[2];
					occupancyMap[j, i] = (double)blue;
					imgPtr += 3;
				}
			}

			bmp.UnlockBits(data);
		}

		public string ToLog()
		{
			string toReturn = "";
			OccupancyGrid2D copy = new OccupancyGrid2D(this);
			for (int i = 0; i < NumCellY; i++)
			{
				for (int j = 0; j < NumCellX; j++)
				{
					toReturn += this.GetCellByIdx(j, i).ToString() + " ";
				}
				toReturn += "\n";
			}
			return toReturn;
		}

		public void GetIndicies(double x, double y, out int xIdx, out int yIdx)
		{
			xIdx = (int)((x / resolutionX) + (extentX / resolutionX));
			yIdx = (int)((y / resolutionY) + (extentY / resolutionY));
		}

		public void GetReals(int xIdx, int yIdx, out double x, out double y)
		{
			x = xIdx * resolutionX - extentX;
			y = yIdx * resolutionY - extentY;
		}

		public object DeepCopy()
		{
			OccupancyGrid2D copy = new OccupancyGrid2D(this.resolutionX, this.resolutionY, this.extentX, this.extentY);
			//Copy the occupancyMap
			Array.Copy(this.occupancyMap, copy.occupancyMap, this.occupancyMap.Length);

			return copy;
		}

		public static OccupancyGrid2D operator +(OccupancyGrid2D a, OccupancyGrid2D b)
		{
			OccupancyGrid2D toReturn = new OccupancyGrid2D(a.ResolutionX, a.resolutionY, a.extentX, a.ExtentY);
			if (!(a.ExtentX == b.ExtentX && a.ExtentY == b.ExtentY && a.ResolutionX == b.ResolutionX && a.ResolutionY == b.ResolutionY))
			{
				throw new Exception("OccupancyGrid size does not match");
			}
			else
			{
				for (int i = 0; i < a.Width; i++)
				{
					for (int j = 0; j < a.Height; j++)
					{
						toReturn.SetCellByIdx(i, j, a.GetCellByIdx(i, j) + b.GetCellByIdx(i, j));
					}
				}
			}
			return toReturn;
		}

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return occupancyMap.GetEnumerator();
		}

		#endregion
	}




}
