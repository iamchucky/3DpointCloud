using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Algorithm;
using System.Drawing;

namespace Magic.Common.DataTypes
{
	public interface IOccupancyGrid2D : IDeepCopyable, IReadAsDoubleGrid
	{
		bool CheckValidIdx(int x, int y);
		double ExtentX { get; }
		double ExtentY { get; }
		double GetCell(double x, double y);
		double GetCellByIdx(int xIdx, int yIdx);
		double GetCellReal(double x, double y);
		void GetIndicies(double x, double y, out int xIdx, out int yIdx);
		void GetReals(int xIdx, int yIdx, out double x, out double y);
		event EventHandler GridUpdated;
		double ResolutionX { get; }
		double ResolutionY { get; }
		int NumCellX { get; }
		int NumCellY { get; }

		void SetCell(double x, double y, double value);
		void SetCells(Bitmap bmp);
		unsafe void SetCellsFast(Bitmap bmp);
		void SetCellByIdx(int xIdx, int yIdx, double value);
	}

}
