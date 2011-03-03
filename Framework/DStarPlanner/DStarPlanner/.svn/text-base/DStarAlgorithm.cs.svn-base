using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Path;
using Magic.Common;
using Magic.OccupancyGrid;
using Magic.Common.Shapes;
//using VCSKicksCollection;

namespace Magic.PathPlanning
{
	public class DStarAlgorithm
	{
		private class Node : IComparable
		{
			public Vector2 xy;
			public double g;
			public double h;
			public Node dad;
						
			public Node(double x, double y, double g, double h, Node dad)
			{
				this.xy = new Vector2(x, y);
				this.g = g;
				this.h = h;
				this.dad = dad;
			}

			public Node(int x, int y, double g, double h, Node dad)
			{
				this.xy = new Vector2((double) x, (double) y);
				this.g = g;
				this.h = h;
				this.dad = dad;
			}

			public Node(Vector2 xy, double g, double h, Node dad)
			{
				this.xy = new Vector2(xy.X, xy.Y);
				this.g = g;
				this.h = h;
				this.dad = dad;
			}

			public static bool operator <(Node a, Node b)
			{
				if (a.g + a.h == b.g + b.h)
				{
					if (a.h < b.h) return true;
					else return false;
				}
				else if (a.g + a.h < b.g + b.h) return true;
				else return false;
			}

			public static bool operator >(Node a, Node b)
			{
				if (a.g + a.h == b.g + b.h)
				{
					if (a.h > b.h) return true;
					else return false;
				}
				else if (a.g + a.h > b.g + b.h) return true;
				else return false;
			}

			#region IComparable Members

			public int CompareTo(object obj)
			{
				if (obj is Node)
				{
					Node n = (Node)obj;

					if (this.g + this.h == n.g + n.h)
					{
						if (this.h > n.h) return 1;
						else return -1;
					}
					else if (this.g + this.h > n.g + n.h) return 1;
					else return -1;
				}

				throw new ArgumentException();
			}

			#endregion
		}

		private class PriorityQueue
		{
			public List<Node> q;

			public PriorityQueue()
			{
				q = new List<Node>();
			}

			public void Push(Node node)
			{
				if (q.Count == 0)
				{
					q.Add(node);
					return;
				}

				for (int i = 0; i < q.Count; i++)
					if (q.ElementAt(i) > node)
					{
						q.Insert(i, node);
						return;
					}

				q.Add(node);
				return;
			}

			public Node Pop()
			{
				Node top = q.ElementAt(0);
				q.RemoveAt(0);
				return top;
			}
		}

        private double blurWeight;
		private double resX, resY, extentX, extentY;
		private OccupancyGrid2D closed, opened;
		const int MAX_OPEN = 1500;

		public DStarAlgorithm(double blurWeight, double resX, double resY, double extentX, double extentY)
		{
            this.blurWeight = blurWeight;
			this.resX = resX;
			this.resY = resY;
			this.extentX = extentX;
			this.extentY = extentY;
		}

		public List<Waypoint> FindPath(Waypoint start, Waypoint goal, OccupancyGrid2D og, out bool success)
		{
			List<Waypoint> path = new List<Waypoint>();

			//added by aaron (sort of a hack)
			if (og == null || goal.Coordinate.DistanceTo(start.Coordinate) == 0)
			{
				path.Add(new Waypoint(start.Coordinate, true, 0));
				success = true;
				return path;
			}

			int xIdx, yIdx;
			success = true;
			Vector2[] NESWVector = new Vector2[4];
			Vector2[] diagVector = new Vector2[4];
			bool[] NESW = new bool[4];
			Vector2 startV = start.Coordinate; // Start Vector2
			Vector2 goalV = goal.Coordinate; // Goal Vector2

			PriorityQueue open = new PriorityQueue();
			closed = new OccupancyGrid2D(resX, resY, extentX, extentY);
			opened = new OccupancyGrid2D(resX, resY, extentX, extentY);

			GetIndicies(startV.X, startV.Y, out xIdx, out yIdx);
			startV = new Vector2(xIdx, yIdx);
			GetIndicies(goalV.X, goalV.Y, out xIdx, out yIdx);
			goalV = new Vector2(xIdx, yIdx);

			Node root = new Node(goalV, goalV.DistanceTo(startV), 0, null);

			Node current = root;
			open.Push(current);

			// Do the spreading/discovering stuff until we discover a path.
			while (current.xy != startV)
			{
				if (open.q.Count == 0 || open.q.Count > MAX_OPEN)
				{
                    Console.WriteLine("Failure in DSstar. Open count is: " + open.q.Count);
					success = false;
					break;
				}
				current = open.Pop();

				NESWVector[0] = new Vector2(current.xy.X, current.xy.Y - 1);
				NESWVector[1] = new Vector2(current.xy.X + 1, current.xy.Y);
				NESWVector[2] = new Vector2(current.xy.X, current.xy.Y + 1);
				NESWVector[3] = new Vector2(current.xy.X - 1, current.xy.Y);

				diagVector[0] = new Vector2(current.xy.X + 1, current.xy.Y - 1);
				diagVector[1] = new Vector2(current.xy.X + 1, current.xy.Y + 1);
				diagVector[2] = new Vector2(current.xy.X - 1, current.xy.Y + 1);
				diagVector[3] = new Vector2(current.xy.X - 1, current.xy.Y - 1);

				for (int i = 0; i < 4; i++)
				{
					if ((int)og.GetCellByIdx((int)NESWVector[i].X, (int)NESWVector[i].Y) < 255)
					{
						if (closed.GetCellByIdx((int)NESWVector[i].X, (int)NESWVector[i].Y) == 0)
						{
							NESW[i] = true;
							if (opened.GetCellByIdx((int)NESWVector[i].X, (int)NESWVector[i].Y) == 0)
							{
								open.Push(new Node(NESWVector[i], NESWVector[i].DistanceTo(startV), current.h + 1
									+ og.GetCellByIdx((int)NESWVector[i].X, (int)NESWVector[i].Y) / blurWeight, current));
								opened.SetCellByIdx((int)NESWVector[i].X, (int)NESWVector[i].Y, 1);
							}
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (NESW[i % 4] && NESW[(i + 1) % 4])
					{
						if (og.GetCellByIdx((int)diagVector[i].X, (int)diagVector[i].Y) < 255)
						{
							if (closed.GetCellByIdx((int)diagVector[i].X, (int)diagVector[i].Y) == 0)
							{
								if (opened.GetCellByIdx((int)diagVector[i].X, (int)diagVector[i].Y) == 0)
								{
									open.Push(new Node(diagVector[i], diagVector[i].DistanceTo(startV), current.h + 1.4
										+ og.GetCellByIdx((int)diagVector[i].X, (int)diagVector[i].Y) / blurWeight, current));
									opened.SetCellByIdx((int)diagVector[i].X, (int)diagVector[i].Y, 1);
								}
							}
						}
					}
				}

				for (int i = 0; i < 4; i++) NESW[i] = false;

				closed.SetCellByIdx((int) current.xy.X, (int) current.xy.Y, 1);
			}

			// Build a path using the discovered path.
			double x, y;
			Waypoint waypoint;

			// First waypoint is a user waypoint
			GetReals((int)current.xy.X, (int)current.xy.Y, out x, out y);
            waypoint = new Waypoint(x + resX / 2, y + resY / 2, true, og.GetCellByIdx((int)current.xy.X, (int)current.xy.Y));
			path.Add(waypoint);
			current = current.dad;

			// Middle waypoints are path waypoints
			while (current != root && current != null)
			{
				GetReals((int) current.xy.X, (int) current.xy.Y, out x, out y);
                waypoint = new Waypoint(x + resX / 2, y + resY / 2, false, og.GetCellByIdx((int)current.xy.X, (int)current.xy.Y));
				path.Add(waypoint);
				current = current.dad;
			}

			// Last waypoint is a user waypoint
            if (current != null)
            {
                GetReals((int)current.xy.X, (int)current.xy.Y, out x, out y);
                waypoint = new Waypoint(x + resX / 2, y + resY / 2, true, og.GetCellByIdx((int)current.xy.X, (int)current.xy.Y));
                path.Add(waypoint);
            }

			return path;
		}

		public List<Polygon> GetOpened()
		{
			List<Polygon> pixels = new List<Polygon>();

			for (int i = 0; i < opened.Width; i++)
				for (int j = 0; j < opened.Height; j++)
					if (opened.GetCellByIdx(i, j) != 0)
						pixels.Add(DrawPixel((i - extentX / resX) * resX, (j - extentY / resY) * resY));

			return pixels;
		}

		public List<Polygon> GetClosed()
		{
			List<Polygon> pixels = new List<Polygon>();

			for (int i = 0; i < closed.Width; i++)
				for (int j = 0; j < closed.Height; j++)
					if (closed.GetCellByIdx(i, j) != 0)
						pixels.Add(DrawPixel((i - extentX / resX) * resX, (j - extentY / resY) * resY));

			return pixels;
		}

		private Polygon DrawPixel(double x, double y)
		{
			List<Vector2> points = new List<Vector2>();

			points.Add(new Vector2(x, y));
			points.Add(new Vector2(x + .25, y));
			points.Add(new Vector2(x + .25, y + .25));
			points.Add(new Vector2(x, y + .25));

			return new Polygon(points);
		}

		private void GetIndicies(double x, double y, out int xIdx, out int yIdx)
		{
			xIdx = (int)((x / resX) + (extentX / resX));
			yIdx = (int)((y / resY) + (extentY / resY));
		}

		private void GetReals(int xIdx, int yIdx, out double x, out double y)
		{
			x = xIdx * resX - extentX;
			y = yIdx * resY - extentY;
		}
	}
}
