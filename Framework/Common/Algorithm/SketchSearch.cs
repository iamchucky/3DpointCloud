using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;


namespace Magic.Common.Algorithm
{
	public class SketchSearch
	{
		List<Vector4> pixelsWorld;
		List<Vector4> pixelsScreen;
		int mClasses;
		int kBest;
		List<int> strokeClasses;
		//List<List<float>> searchTree;
		List<Node> searchTree;


		public SketchSearch(List<Vector4> pw, List<Vector4> ps)
		{
			this.pixelsScreen = ps;
			this.pixelsWorld = pw;
			this.mClasses = 10;
			this.kBest = 1;
			strokeClasses = new List<int>();
			SearchAlgo(pixelsWorld, pixelsScreen, mClasses, kBest);
		}

		public SketchSearch(List<Vector4> pw, List<Vector4> ps, int m)
		{
			this.pixelsScreen = ps;
			this.pixelsWorld = pw;
			this.mClasses = m;
			this.kBest = 1;
			strokeClasses = new List<int>();
			SearchAlgo(pixelsWorld, pixelsScreen, mClasses, kBest);
		}

		public SketchSearch(List<Vector4> pw, List<Vector4> ps, int m, int k)
		{
			this.pixelsScreen = ps;
			this.pixelsWorld = pw;
			this.mClasses = m;
			this.kBest = k;
			strokeClasses = new List<int>();
			SearchAlgo(pixelsWorld, pixelsScreen, mClasses, kBest);
		}

		public List<int> StrokeAssignments
		{
			get { return strokeClasses; }
		}

		private void SearchAlgo(List<Vector4> pw, List<Vector4> ps, int mm, int kk)
		{
			//Console.WriteLine("hello?");




			
		}

		private class Node
		{
			int id;
			int parent;
			List<int> children;
			float c;
			float h;
			float f;

			public int ID
			{
				get { return id; }
				set { id = value; }
			}
			public int Parent
			{
				get { return parent; }
				set { parent = value; }
			}
			public List<int> Children
			{
				get { return children; }
				set { children = value; }
			}
			public float C
			{
				get { return c; }
				set { c = value; }
			}
			public float H
			{
				get { return h; }
				set { h = value; }
			}
			public float F
			{
				get { return f; }
				set { f = value; }
			}
		}
	}
}
