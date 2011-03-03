using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common.DataTypes;

namespace Magic.Common.Algorithm
{
	public class MSTSegmenter
	{
		//typedef double(*edge_cost_functor)(IplImage*, int , int ,int , int);
		public delegate double EdgeCostFunction(IReadAsDoubleGrid grid, int x1, int y1, int x2, int y2);
		EdgeCostFunction edgeCostFunction;

		//super simple edge structure
		private class Edge : IComparable 
		{

			public Edge()
			{
				this.weight = 0; this.aNode = 0; this.bNode = 0;
			}
			public Edge(int aNode, int bNode, double weight)
			{
				this.weight = weight; this.aNode = aNode; this.bNode = bNode;
			}


			public int aNode, bNode; //pixel locations
			public double weight; //some weight of this edge

			#region IComparable Members

			public int CompareTo(object obj)
			{
				return weight.CompareTo((double)(obj));
			}

			#endregion
		}



		//measures the difference between two pixels
		double PixDiff(Bitmap img, int x1, int y1, int x2, int y2)
		{
			return 0;
			//return GetPixelVal (x1,y1,img) - GetPixelVal (x2,y2,img);
		}

		double ThresholdFunction(double param, int clusterSize)
		{
			//here we are just penalizing based on cluster size such that small clusters are harder to create.
			return param / clusterSize;
			
		}


		DisjointSets ds;
		List<Edge> graph;
		List<double> thresholds;
		double nCell;
		List<int> setmap;

		List<List<int>> clusters;


		MSTSegmenter(int width, int height)
		{
			// pick random colors for each component

			double nCell = (double)(width * height);

			//make our disjoint set - note each element is equally weighted and represents a single pixel at firstB
			ds = new DisjointSets(width * height);
			//the graph
			graph = new List<Edge>(width * height * 4);
			//adaptive thresholds for each element
			thresholds = new List<double>(width * height);
			setmap = new List<int>(width * height);

		}


		//Performs MST based segmentation as described by Pedro and Hutt
		//k - theshold. larger values make fewer components
		//min-size - smallest size of a component
		int Segment_Image(double k, int minsize, IReadAsDoubleGrid src,
																						 IReadAsDoubleGrid dst, 
																							EdgeCostFunction f)
		{
			int width = src.Width;
			int height = src.Height;
			this.edgeCostFunction = f;
			ds.Clear();
			clusters.Clear();
			for (int i = 0; i < width; i++)
				setmap[i] = -1;

			//build the graph we are going to segment over...
			//this is an 8 connected graph (carindal and subcardinal 1 neighbors)

			int numEdges = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (x < width - 1) //right neighbor			
						graph[numEdges++] = new Edge(y * width + x, y * width + (x + 1), edgeCostFunction(src, x, y, x + 1, y));
					if (y > 0) //up neighbor
						graph[numEdges++] = new Edge(y * width + x, (y - 1) * width + x, edgeCostFunction(src, x, y, x, y - 1));
					if ((x < width - 1) && (y > 0)) //up and right neibor
						graph[numEdges++] = new Edge(y * width + x, (y - 1) * width + (x + 1), edgeCostFunction(src, x, y, x + 1, y - 1));
					if ((x < width - 1) && (y < height - 1)) //down and right neighbor			
						graph[numEdges++] = new Edge(y * width + x, (y + 1) * width + (x + 1), edgeCostFunction(src, x, y, x + 1, y + 1));
				}
			}

			//now sort the edges
			graph.Sort();
			

			//ok graph is built - do some segmentation!	


			//initalize the tresholds to their initial values (see discussion in loop)	
			for (int i = 0; i < width * height; i++)
				thresholds[i] = ThresholdFunction(k, 1);

			//traverse the graph in increasing edge weight order...
			//the goal here is to try to assemble cohesive sets using the
			//union. 
			for (int i = 0; i < numEdges; i++)
			{
				//the a node is the current pixel, the b node is the neighbor
				int aID = ds.FindSetRoot(graph[i].aNode);
				int bID = ds.FindSetRoot(graph[i].bNode);

				//if these are two distinct clusters, can we merge them?
				if (aID != bID)
				{
					//merge if both the aID and bID thesholds are bigger than this current elements weight
					if ((graph[i].weight <= thresholds[aID]) && (graph[i].weight <= thresholds[bID]))
					{
						//join the clusters
						ds.Union(aID, bID);

						//update the theshold of the newly formed cluster
						aID = ds.FindSetRoot(aID); //i think this can be optimized...

						//normalize the added theshold weight by the size of this new bigger cluster
						//from the paper: this threshold function is a function of the size of the component. 
						// this ends up making small components require stronger evidence for a boundary
						thresholds[aID] = graph[i].weight + ThresholdFunction(k, ds.NumElements(aID));
					}
				}

			}


			// post process small components
			for (int i = 0; i < numEdges; i++)
			{
				int aID = ds.FindSetRoot(graph[i].aNode);
				int bID = ds.FindSetRoot(graph[i].bNode);
				if ((aID != bID) && ((ds.NumElements(aID) < minsize) || (ds.NumElements(bID) < minsize)))
					ds.Union(aID, bID);
			}



			int sets = ds.NumSets();
			clusters = new List<List<int>>(sets);

			int mapIndex = 0;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int point = y * width + x;
					int comp = ds.FindSetRoot(point);

					//try to get the setmap...
					//first check is this a root?
					if (setmap[comp] == -1)
					{
						setmap[comp] = mapIndex;

						clusters.Add(new List<int>(ds.NumElements(comp)));						
						clusters[mapIndex].Add(point);
						mapIndex++;
					}
					else
					{
						clusters[setmap[comp]].Add(point);
					}
				}
			}


			return sets;
		}
	}
}
