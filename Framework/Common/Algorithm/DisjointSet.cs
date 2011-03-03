using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Algorithm
{
	public class DisjointSets
	{

		int count;
		int numSets;
		List<DSElement> elements; //all the elements

		struct DSElement
		{
			public DSElement(int rank, int parent, int size)
			{
				this.rank = rank; this.parent = parent; this.size = size;
			}
			public void SetParent(int p)
			{
				this.parent = p;
			}

			public void IncreaseRank(int amount)
			{ rank += amount; }

			public void IncreaseSize(int amount)
			{
				size += amount;
			}
			public int rank;		//roughly max height of the node in subtree		
			public int parent; //the parent array index of the element
			public int size; //number of elements
		}

		public DisjointSets(int count)
		{
			// insert and initialize the specified number of element nodes
			elements = new List<DSElement>(count);
			this.count = count;
			Clear();

		}

		public void Clear()
		{
			// insert and initialize the specified number of element nodes
			for (int i = 0; i < count; i++)
				elements[i] = new DSElement(0, i, 1);
			// update element and set counts	
			numSets = count;

		}


		//finds the set id that element belongs to
		public int FindSetRoot(int elemID)
		{
			int queryID = elemID;

			// Find the root element that represents the set which elemID belongs to	
			while (elements[queryID].parent != queryID) //an item is a root if its parent matches its ID
				queryID = elements[queryID].parent;

			int rootID = queryID;

			int oldParentID = elemID;
			// Walk to the root, again, but now updating the parents of elemID.
			// this will improve future calls to this function
			while (elements[oldParentID].parent != rootID)
			{
				oldParentID = elements[oldParentID].parent;
				elements[oldParentID].SetParent(rootID);
			}

			return rootID;
		}


		//combines two sets into one logical set
		public void Union(int setID1, int setID2)
		{
			// Determine which node representing a set has a higher rank. 

			//The node with the higher rank is likely to have a bigger 
			// subtree so in order to better balance the tree representing the
			// union, the node with the higher rank is made the parent of the 
			// one with the lower rank and not the other way around.
			if (elements[setID1].rank > elements[setID2].rank) //setID1 is the master
			{
				elements[setID2].SetParent(setID1);
				elements[setID1].IncreaseSize(elements[setID2].size);
			}
			else //setID2 becomes the master
			{
				elements[setID1].SetParent(setID2);
				elements[setID2].IncreaseSize(elements[setID1].size);

				//if the two ranks are equal, increase their ranks
				if (elements[setID1].rank == elements[setID2].rank)
					elements[setID2].IncreaseRank(1);
			}
			numSets--;
		}

		//gets the number of elements
		public int NumElements(int setID)
		{
			return elements[setID].size;
		}

		//gets the number of sets
		public int NumSets()
		{
			return numSets;
		}

	}
}
