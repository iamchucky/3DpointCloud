using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Magic.Common.DataTypes;
using System.Threading;
using Magic.Common;
using Magic.Common.Shapes;
using System.Diagnostics;

namespace Magic.Sensors.OcGrid2Poly
{

	public class OcGrid2Poly
	{
		IOccupancyGrid2D occupancyGrid;
		Polygon onePolygon = new Polygon(); // polygon has new Vector2(indexX, indexY)
		List<Polygon> polygonToReturn = new List<Polygon>();
		Dictionary<Vector2, Polygon> indexInPolygonDictionary;


		// threshold for the value of each cell to be counted as obstacle
		double polygonThreshold;

		// number of polygons found
		int numPolygon = 0;
		List<Polygon> polygonList; // start with capacity of one
		Dictionary<Vector2, int> allPointsInPolygons;
		int count = 0;


		/// <summary>
		/// Constructor of OccupancyGrid2Polygons class
		/// </summary>
		/// <param name="ocGridReceived"></param>
		/// <param name="polygonThreshold"></param>
		public OcGrid2Poly(IOccupancyGrid2D ocGridReceived, double polygonThreshold)
		{
			this.occupancyGrid = ocGridReceived;
			this.polygonThreshold = polygonThreshold;
			indexInPolygonDictionary = new Dictionary<Vector2, Polygon>(ocGridReceived.NumCellX * ocGridReceived.NumCellY);
			polygonList = new List<Polygon>(100);
			polygonList.Add(new Polygon());
			allPointsInPolygons = new Dictionary<Vector2, int>(occupancyGrid.NumCellX * occupancyGrid.NumCellY);
		}


		private bool FindPolygon(Vector2 initialIndex, bool turnedUpOnce)
		{
			int indexX = (int)initialIndex.X;
			int indexY = (int)initialIndex.Y;
			// if the cell is invalid, return
			if (occupancyGrid.GetCellByIdx(indexX, indexY) < polygonThreshold) return false;

			// if cell is valid, add to the polygon list - if not, quit
			if (!allPointsInPolygons.ContainsKey(initialIndex))
			{
				double xReal, yReal;
				occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
				polygonList[numPolygon].Add(new Vector2(xReal, yReal));
				allPointsInPolygons.Add(initialIndex, numPolygon);
			}
			else
				return false;

			// check right cell
			if (!turnedUpOnce && occupancyGrid.CheckValidIdx(indexX + 1, indexY))
				FindPolygon(new Vector2(indexX + 1, indexY), false);
			//// check up cell
			if (occupancyGrid.CheckValidIdx(indexX, indexY + 1))
				FindPolygon(new Vector2(indexX, indexY + 1), true);

			return false;
		}


		private bool FindPoly2(Vector2 initialIndex)
		{
			// this function assumes that a point is bloated to occupy 3x3 square cells

			// give the initial index, which is the bottom left corner of the blob
			int indexX = (int)initialIndex.X;
			int indexY = (int)initialIndex.Y;
			// if the cell is invalid, return
			if (occupancyGrid.GetCellByIdx(indexX, indexY) < polygonThreshold || allPointsInPolygons.ContainsKey(initialIndex)) return false;

			// check right cell
			if (occupancyGrid.CheckValidIdx(indexX + 1, indexY) && occupancyGrid.GetCellByIdx(indexX + 1, indexY) > polygonThreshold)
			{
				double xReal, yReal;
				occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
				polygonList[numPolygon].Add(new Vector2(xReal, yReal));
				allPointsInPolygons.Add(initialIndex, numPolygon);
				if (!allPointsInPolygons.ContainsKey(new Vector2(indexX, indexY + 1)))
					allPointsInPolygons.Add(new Vector2(indexX, indexY + 1), numPolygon);
				FindPoly2(new Vector2(indexX + 1, indexY));
			}
			// check top cell
			else if (occupancyGrid.CheckValidIdx(indexX, indexY + 1) && occupancyGrid.GetCellByIdx(indexX, indexY + 1) > polygonThreshold)
			{
				double xReal, yReal;
				occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
				polygonList[numPolygon].Add(new Vector2(xReal, yReal));
				allPointsInPolygons.Add(initialIndex, numPolygon);
				if (!allPointsInPolygons.ContainsKey(new Vector2(indexX - 1, indexY)))
					allPointsInPolygons.Add(new Vector2(indexX - 1, indexY), numPolygon);
				FindPoly2(new Vector2(indexX, indexY + 1));
			}
			// check left cell
			else if (occupancyGrid.CheckValidIdx(indexX - 1, indexY) && occupancyGrid.GetCellByIdx(indexX - 1, indexY) > polygonThreshold)
			{
				double xReal, yReal;
				occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
				polygonList[numPolygon].Add(new Vector2(xReal, yReal));
				allPointsInPolygons.Add(initialIndex, numPolygon);
				if (!allPointsInPolygons.ContainsKey(new Vector2(indexX, indexY - 1)))
					allPointsInPolygons.Add(new Vector2(indexX, indexY - 1), numPolygon);
				FindPoly2(new Vector2(indexX - 1, indexY));
			}
			// check bottom cell
			else if (occupancyGrid.CheckValidIdx(indexX, indexY - 1) && occupancyGrid.GetCellByIdx(indexX, indexY - 1) > polygonThreshold)
			{
				double xReal, yReal;
				occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
				polygonList[numPolygon].Add(new Vector2(xReal, yReal));
				allPointsInPolygons.Add(initialIndex, numPolygon);
				if (!allPointsInPolygons.ContainsKey(new Vector2(indexX + 1, indexY)))
					allPointsInPolygons.Add(new Vector2(indexX + 1, indexY), numPolygon);
				FindPoly2(new Vector2(indexX, indexY - 1));
			}

			return false;


		}


		// 1 = bottom side
		// 2 = right side
		// 3 = top side
		// 4 = left side
		// 5 = bottom left corner
		// 6 = bottom right corner
		// 7 = top right corner
		// 8 = top left corner
		const int BSIDE = 1; const int RSIDE = 2; const int TSIDE = 3; const int LSIDE = 4;
		const int BLCORNER = 5; const int BRCORNER = 6; const int TRCORNER = 7; const int TLCORNER = 8;
		const int BLCORNER_INV = 9; const int BRCORNER_INV = 10; const int TRCORNER_INV = 11; const int TLCORNER_INV = 12;
		const int CENTER = 13; const int SINGLE = 0;

		private bool FindPoly3(Vector2 initialIndex)
		{
			int indexX = (int)initialIndex.X;
			int indexY = (int)initialIndex.Y;

			if (occupancyGrid.GetCellByIdx(indexX, indexY) < polygonThreshold || allPointsInPolygons.ContainsKey(initialIndex)) return false;

			// figure out where I am (which side, or which corner)
			int where = FindWhereIam(indexX, indexY);

			double xReal, yReal;
			switch (where)
			{
				//case SINGLE:
				//  occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
				//  polygonList[numPolygon].Add(new Vector2(xReal, yReal));
				//  allPointsInPolygons.Add(initialIndex, numPolygon);
				//  break;

				case CENTER:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					allPointsInPolygons.Add(initialIndex, numPolygon);
					break;

				case BSIDE:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX + 1, indexY));
					break;

				case TSIDE:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX - 1, indexY));
					break;

				case RSIDE:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX, indexY + 1));
					break;

				case LSIDE:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX, indexY - 1));
					break;

				case BLCORNER:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX + 1, indexY));
					break;

				case BRCORNER:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX, indexY + 1));
					break;

				case TRCORNER:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX - 1, indexY));
					break;

				case TLCORNER:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX, indexY - 1));
					break;

				case BLCORNER_INV:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX, indexY - 1));
					break;

				case BRCORNER_INV:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX + 1, indexY));
					break;

				case TRCORNER_INV:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX, indexY + 1));
					break;

				case TLCORNER_INV:
					occupancyGrid.GetReals(indexX, indexY, out xReal, out yReal);
					polygonList[numPolygon].Add(new Vector2(xReal, yReal));
					allPointsInPolygons.Add(initialIndex, numPolygon);
					FindPoly3(new Vector2(indexX - 1, indexY));
					break;
			}

			return false;

		}

		private int FindWhereIam(int indexX, int indexY)
		{
			//check 4 cells around the given cell (top, bottom, left, and right)
			bool topOccupied = (occupancyGrid.GetCellByIdx(indexX, indexY + 1) > polygonThreshold) && occupancyGrid.CheckValidIdx(indexX, indexY + 1);
			bool bottomOccupied = occupancyGrid.GetCellByIdx(indexX, indexY - 1) > polygonThreshold && occupancyGrid.CheckValidIdx(indexX, indexY - 1);
			bool leftOccupied = occupancyGrid.GetCellByIdx(indexX - 1, indexY) > polygonThreshold && occupancyGrid.CheckValidIdx(indexX - 1, indexY);
			bool rightOccupied = occupancyGrid.GetCellByIdx(indexX + 1, indexY) > polygonThreshold && occupancyGrid.CheckValidIdx(indexX + 1, indexY);

			// for sides
			if (!bottomOccupied && leftOccupied && rightOccupied)
				return BSIDE;
			else if (!rightOccupied && leftOccupied && topOccupied && bottomOccupied)
				return RSIDE;
			else if (!topOccupied && bottomOccupied && leftOccupied && rightOccupied)
				return TSIDE;
			else if (!leftOccupied && rightOccupied && bottomOccupied && topOccupied)
				return LSIDE;
			// for corners
			else if (!leftOccupied && !bottomOccupied && topOccupied && rightOccupied)
				return BLCORNER;
			else if (!rightOccupied && !bottomOccupied && topOccupied && leftOccupied)
				return BRCORNER;
			else if (!rightOccupied && !topOccupied && leftOccupied && bottomOccupied)
				return TRCORNER;
			else if (!leftOccupied && !topOccupied && rightOccupied && bottomOccupied)
				return TLCORNER;

			bool blcornerOccupied = (occupancyGrid.GetCellByIdx(indexX - 1, indexY - 1) > polygonThreshold) && occupancyGrid.CheckValidIdx(indexX - 1, indexY - 1);
			bool brcornerOccupied = (occupancyGrid.GetCellByIdx(indexX + 1, indexY - 1) > polygonThreshold) && occupancyGrid.CheckValidIdx(indexX + 1, indexY - 1);
			bool trcornerOccupied = (occupancyGrid.GetCellByIdx(indexX + 1, indexY + 1) > polygonThreshold) && occupancyGrid.CheckValidIdx(indexX + 1, indexY + 1);
			bool tlcornerOccupied = (occupancyGrid.GetCellByIdx(indexX - 1, indexY + 1) > polygonThreshold) && occupancyGrid.CheckValidIdx(indexX - 1, indexY + 1);

			if (leftOccupied && rightOccupied && topOccupied && bottomOccupied && blcornerOccupied && brcornerOccupied && tlcornerOccupied && trcornerOccupied)
				return CENTER;
			else if (bottomOccupied && leftOccupied && !blcornerOccupied)
				return BLCORNER_INV;
			else if (bottomOccupied && rightOccupied && !brcornerOccupied)
				return BRCORNER_INV;
			else if (topOccupied && rightOccupied && !trcornerOccupied)
				return TRCORNER_INV;
			else if (topOccupied && leftOccupied && !tlcornerOccupied)
				return TLCORNER_INV;
			// if fails
			return SINGLE;
		}


		public List<Polygon> FindPolygons()
		{
			return FindPolygons(new RobotPose(), occupancyGrid.ExtentX, occupancyGrid.ExtentY);
		}

		/// <summary>
		/// Find and extracts polygons from occupancy grid
		/// </summary>
		/// <returns>List of Polygon</returns>
		public List<Polygon> FindPolygons(RobotPose currentPose, double extentX, double extentY)
		{
			//// find the cells to compute
			int xIdx, yIdx, numCellXHlf = 0, numCellYHlf = 0;
			if (extentX % 2 == 1)
				numCellXHlf = (int)(extentX / occupancyGrid.ResolutionX) + 1;
			else
				numCellXHlf = (int)(extentX / occupancyGrid.ResolutionX);
			if (extentY % 2 == 1)
				numCellYHlf = (int)(extentY / occupancyGrid.ResolutionY) + 1;
			else
				numCellYHlf = (int)(extentY / occupancyGrid.ResolutionY);
			occupancyGrid.GetIndicies(currentPose.x, currentPose.y, out xIdx, out yIdx);

			// extract polygons
			polygonList.Clear();
			polygonList.Add(new Polygon());
			numPolygon = 0;
			allPointsInPolygons.Clear();
			
			//for (int y = 0; y < occupancyGrid.NumCellY; y++)
			//{
			//    for (int x = 0; x < occupancyGrid.NumCellX; x++)
			//    {
			//        FindPolygon(new Vector2(x, y), false);
			//        //FindPoly3(new Vector2(x, y));
			//        if (polygonList[numPolygon].Count > 0)
			//        {
			//            numPolygon++;
			//            polygonList.Add(new Polygon()); // place holder
			//        }
			//    }
			//}

			for (int y = yIdx - numCellYHlf; y < yIdx + numCellXHlf; y++)
			{
				for (int x = xIdx - numCellXHlf; x < xIdx + numCellXHlf; x++)
				{
					FindPolygon(new Vector2(x, y), false);
					//FindPoly3(new Vector2(x, y));
					if (polygonList[numPolygon].Count > 0)
					{
						numPolygon++;
						polygonList.Add(new Polygon()); // place holder
					}
				}
			}
			if (polygonList.Count == 1)
				return new List<Polygon>(); //empty list
			else
			{
//				Console.WriteLine("found " + (polygonList.Count - 1) + " polygons before decomposition");
				// the very last element has no information - so remove it if we found >= 1 polygon
				polygonList.RemoveAt(polygonList.Count - 1);
				//Polygon vehiclePolygon = Polygon.VehiclePolygonWithRadius(0.5); // vehicle model of polygon. Note that radius is parameter

				// Inside foreach, I cannot reassign eachpolygon. So, make a List<Polygon>, assign into this, and return this
				List<Polygon> convolutionPolygonList = new List<Polygon>(polygonList.Capacity);
				foreach (Polygon eachPolygon in polygonList)
				{
					// Do the MinKowskiConvolution and GrahamScan to remove interior points
					//Polygon convolutionPolygon = Polygon.ConvexMinkowskiConvolution(eachPolygon, vehiclePolygon);
					Polygon convexHull;
					if (eachPolygon.Count > 1)
					{
						convexHull = eachPolygon;
						//convexHull = FindOuterBoundary2(eachPolygon.points);
						//convexHull = Polygon.GrahamScan(eachPolygon, .001);
					}
					else
					{
						convexHull = eachPolygon;
					}
					//convexHull.Add(convexHull[0]);
					//convexHull = Polygon.ConvexMinkowskiConvolution(convexHull, vehiclePolygon);
					//convolutionPolygonList.Add(bloated);
					convolutionPolygonList.Add(convexHull);
				}
				//convolutionPolygonList = DecomposePolygonList(convolutionPolygonList);
				//Console.WriteLine("done " + sw.ElapsedMilliseconds + " and made " + convolutionPolygonList.Count + " triangles");
				return convolutionPolygonList;
			}
		}

		private Polygon FindOuterBoundary(List<Vector2> points)
		{
			// Sort the points
			points.Sort();
			// start finding the boundary polygons
			Polygon boundaryPoligon = new Polygon();
			List<Vector2> minList = new List<Vector2>();
			List<Vector2> maxList = new List<Vector2>();
			double currentX = points[0].X;
			double lastX = points[0].X;
			double minY = Double.MaxValue;
			double maxY = Double.MinValue;
			foreach (Vector2 eachPoint in points)
			{
				currentX = eachPoint.X;
				// find the min and max y value
				if (eachPoint.Y > maxY)
					maxY = eachPoint.Y;
				if (eachPoint.Y < minY)
					minY = eachPoint.Y;

				// if x changes, save the min and max y points in dictionary
				if ((currentX != lastX) && !eachPoint.Equals(points[points.Count - 1]))
				{
					minList.Add(new Vector2(lastX, minY));
					maxList.Add(new Vector2(lastX, maxY));
					minY = Double.MaxValue;
					maxY = Double.MinValue;
				}
				else if (eachPoint.Equals(points[points.Count - 1]))
				{
					minList.Add(new Vector2(currentX, minY));
					maxList.Add(new Vector2(currentX, maxY));
				}
				lastX = currentX;
			}

			for (int i = 0; i < minList.Count; i++)
			{
				boundaryPoligon.Add(minList[i]);
			}
			maxList.Reverse();
			for (int i = 0; i < minList.Count; i++)
			{
				boundaryPoligon.Add(maxList[i]);
			}
			return boundaryPoligon;
		}

		private Polygon FindOuterBoundary2(List<Vector2> points)
		{
			Polygon boundaryPolygon = new Polygon(); // polygon to return
			List<Vector2> transposedPoints = new List<Vector2>(points.Count);
			// reverse the order of the points
			for (int i = 0; i < points.Count; i++)
			{
				transposedPoints.Add(points[i].Transposed);
				// note that now the point have [y, x] instead of [x, y]
			}
			// sort the points - respect to the y coordinate (low to high)
			transposedPoints.Sort();

			// for each y-coordinate, find the left and right x-coordinates
			double minX = double.MaxValue;
			double maxX = double.MinValue;
			double currentY = points[0].X; // this is in fact Y
			double lastY = points[0].X; // same
			List<Vector2> leftSide = new List<Vector2>();
			List<Vector2> rightSide = new List<Vector2>();
			foreach (Vector2 eachPoint in transposedPoints)
			{
				currentY = eachPoint.X;
				if (eachPoint.Y > maxX)
					maxX = eachPoint.Y;
				if (eachPoint.Y < minX)
					minX = eachPoint.Y;

				if ((currentY != lastY) && !eachPoint.Equals(transposedPoints[transposedPoints.Count - 1]))
				{
					leftSide.Add(new Vector2(minX, currentY));
					rightSide.Add(new Vector2(maxX, currentY));
					minX = double.MaxValue;
					maxX = double.MinValue;

				}
				else if (eachPoint.Equals(transposedPoints[transposedPoints.Count - 1]))
				{
					leftSide.Add(new Vector2(minX, currentY));
					rightSide.Add(new Vector2(maxX, currentY));
				}
				lastY = currentY;
			}

			// end of the sorting, merge left side and right side points
			// first, reverse the order of the right side
			leftSide.Reverse();
			for (int i = 0; i < leftSide.Count; i++)
			{
				boundaryPolygon.Add(leftSide[i]);
			}
			for (int i = 0; i < rightSide.Count; i++)
			{
				boundaryPolygon.Add(rightSide[i]);
			}
			return boundaryPolygon;
		}

		/// <summary>
		/// Decompose polygons into list of triangles
		/// </summary>
		/// <param name="polygonList"></param>
		/// <returns></returns>
		public List<Polygon> DecomposePolygonList(List<Polygon> polygonList)
		{
			List<Polygon> triangles = new List<Polygon>();
			foreach (Polygon eachPolygon in polygonList)
			{
				bool foundAll = false;
				while (!foundAll)
				{
					for (int i = 1; i < eachPolygon.Count - 1; i++)
					{
						if (eachPolygon.Count == 3)
						{
							triangles.Add(eachPolygon);
							foundAll = true;
						}
						// make a triangle
						Polygon thisTriangle = new Polygon();
						for (int j = -1; j <= 1; j++)
							thisTriangle.Add(eachPolygon[i - j]);
						// check if the triangle is an ear. If so, add to the list of triangles
						if (IsEar(thisTriangle, eachPolygon))
						{
							triangles.Add(thisTriangle);
							eachPolygon.RemoveAt(i);
							continue;
						}
					}
				}
			}
			return triangles;
		}
		/// <summary>
		/// Decompose polygon into triangles
		/// </summary>
		/// <param name="polygon"></param>
		/// <returns></returns>
		public List<Polygon> DecomposePolygon(Polygon polygon)
		{
			List<Polygon> triangles = new List<Polygon>();
			bool foundAll = false;
			while (!foundAll)
			{
				for (int i = 1; i < polygon.Count - 1; i++)
				{
					if (polygon.Count == 3)
					{
						triangles.Add(polygon);
						foundAll = true;
					}
					// make a triangle
					Polygon thisTriangle = new Polygon();
					for (int j = -1; j <= 1; j++)
						thisTriangle.Add(polygon[i - j]);
					// check if the triangle is an ear. If so, add to the list of triangles
					if (IsEar(thisTriangle, polygon))
					{
						triangles.Add(thisTriangle);
						polygon.RemoveAt(i);
						continue;
					}
				}
			}
			return triangles;
		}

		/// <summary>
		/// Check if the triangle is an ear of the polygon
		/// </summary>
		/// <param name="triangle">Polygon consists of 3 points</param>
		/// <param name="entirePolygon">Large polygon that triangle belongs to</param>
		/// <returns></returns>
		private bool IsEar(Polygon triangle, Polygon entirePolygon)
		{
			foreach (Vector2 v in entirePolygon)
			{
				// do not check on the points of triangle
				if (triangle.Contains(v))
					continue;
				// check if this is an ear
				if (triangle.IsInside(v))
					return false;
			}
			return true;
		}


		/// <summary>
		/// Update OccupancyGrid
		/// </summary>
		/// <param name="occupancyGrid">OccupancyGrid to be assigned</param>
		public void UpdateOccupancyGrid(IOccupancyGrid2D occupancyGrid)
		{
			this.occupancyGrid = (IOccupancyGrid2D)occupancyGrid.DeepCopy();
		}


		/*
		 * 
		 * PUT THIS IN YOUR TESTBENCH - amn32
			#region Static functions
			/// <summary>
			/// Generate a occupancy grid with random rectangles (not generated randomly).
			/// </summary>
			/// <returns>Returns OccupancyGrid of 20m x 20m with 0.1m resolution</returns>
			public static IOccupancyGrid2D TestingOccupancyGrid()
			{
					double resolutionX = 0.1;
					double resolutionY = 0.1;
					double extentX = 20;
					double extentY = 20;
					IOccupancyGrid2D ocGrid = new OccupancyGrid2D(resolutionX, resolutionY, extentX, extentY);
					for (double i = 0; i < 10 + resolutionX; i += resolutionX)
					{
							for (double j = 0; j < 10 + resolutionY; j += resolutionY)
							{
									ocGrid.SetCell(i + 5.0, j + 5.0, 10);
									ocGrid.SetCell(i + 3.0, j - 3.0, 10);
									ocGrid.SetCell(i - 1.0, j + 2.0, 10);
							}
					}

					for (double i = 0; i < 3 + resolutionX; i += resolutionX)
					{
							for (double j = 0; j < 3 + resolutionY; j += resolutionY)
							{
									ocGrid.SetCell(i - 14.0, j, 10);
									ocGrid.SetCell(i - 15.0, j + 3.0, 10);
									ocGrid.SetCell(i - 11.5, j + 8.1, 10);
									ocGrid.SetCell(i - 10.0, j - 10.0, 10);
									ocGrid.SetCell(i - 10.0, j + 15.0, 10);
									ocGrid.SetCell(i - 14.0, j - 13.0, 10);
							}
					}

					for (double i = 0; i < 3; i += resolutionX)
					{
							ocGrid.SetCell(10, i - 10, 10);
							ocGrid.SetCell(10 + resolutionX, i - 10, 10);
							//ocGrid.SetCell(13, i - 10, 10);
							//ocGrid.SetCell(13 + resolutionX, i - 10, 10);
					}
					for (double i = 0; i < 3; i += resolutionY)
							ocGrid.SetCell(i + 10, -10, 10);

					//for (double i = 0; i < 1; i += resolutionX)
					//{
					//    for (double j = 0; j < 1; j += resolutionY)
					//        ocGrid.SetCell(i, j, 10);
					//}
					return ocGrid;
			}

			*/

		//#endregion
	}
}
