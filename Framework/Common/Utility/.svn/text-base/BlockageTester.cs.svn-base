using System;
using System.Collections.Generic;
using System.Text;
using Magic.Common.Shapes;
using Magic.Common.Path;

namespace Magic.Common.Utility
{
	public static class BlockageTester
	{
		private class BlockageData
		{
			public Polygon convolvedPolygon;
			public bool searchMark;

			public BlockageData(Polygon convolvedPolygon)
			{
				this.convolvedPolygon = convolvedPolygon;
				searchMark = false;
			}
		}


		public static bool TestBlockage(IList<Polygon> obstaclePolygons, LinePath leftBound, LinePath rightBound, double expandDist, double trackWidth)
		{
			try
			{
				Circle robotCircle = new Circle(expandDist, Vector2.Zero);
				Polygon robotPoly = robotCircle.ToPolygon(24);

				List<BlockageData> obstacles = new List<BlockageData>(obstaclePolygons.Count);

				foreach (Polygon poly in obstaclePolygons)
				{
					Polygon convolvedPoly = null;
					try
					{
						convolvedPoly = Polygon.ConvexMinkowskiConvolution(robotPoly, poly);
					}
					catch (Exception)
					{
						// minkowski convolution failed, just expand that shit with the gaheezy inflate method
						convolvedPoly = poly.Inflate(expandDist);
					}

					// add the entry to the obstacle collection
					BlockageData data = new BlockageData(convolvedPoly);
					obstacles.Add(data);
				}

				// shrink in the lanes by a half robot width
				leftBound = leftBound.ShiftLateral(-trackWidth / 2.0);
				rightBound = rightBound.ShiftLateral(-trackWidth / 2.0);

				Queue<BlockageData> testQueue = new Queue<BlockageData>();

				foreach (BlockageData obs in obstacles)
				{
					if (obs.convolvedPolygon.DoesIntersect(leftBound))
					{
						// check if this hits the right bound
						if (obs.convolvedPolygon.DoesIntersect(rightBound))
						{
							// this extends across the entire lane, the segment is blocked
							return true;
						}
						else
						{
							testQueue.Enqueue(obs);
							obs.searchMark = true;
						}
					}
				}

				while (testQueue.Count > 0)
				{
					BlockageData obs = testQueue.Dequeue();

					foreach (BlockageData neighbor in obstacles)
					{
						if (!neighbor.searchMark && Polygon.TestConvexIntersection(obs.convolvedPolygon, neighbor.convolvedPolygon))
						{
							if (neighbor.convolvedPolygon.DoesIntersect(rightBound))
								return true;

							testQueue.Enqueue(neighbor);
							neighbor.searchMark = true;
						}
					}
				}

				return false;
			}
			catch (Exception) { }

			return false;
		}
	}
		 
}
