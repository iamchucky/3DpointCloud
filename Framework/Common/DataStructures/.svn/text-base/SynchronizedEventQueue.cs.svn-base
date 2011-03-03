using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataInterfaces;
using System.Collections;

namespace Magic.Common.DataStructures
{
	/// <summary>
	/// Non-Thread Safe Event Queue for Timestamped Items
	/// </summary>
	public class SynchronizedEventQueue
	{
		double maxAgeDifference = 2.000; //seconds
		List<ITimestampedEventQueueItem> events;
		double curTS = 0;
		int capacity = 100;
		bool emptyWithoutError = false;
		public SynchronizedEventQueue(int capacity, bool emptyWithoutError)
		{
			events = new List<ITimestampedEventQueueItem>(capacity);
			this.capacity = capacity;
			this.emptyWithoutError = emptyWithoutError;
		}

		public SynchronizedEventQueue(int capacity)
		{
			events = new List<ITimestampedEventQueueItem>(capacity);
			this.capacity = capacity;
		}

		public void Clear()
		{
			lock (this)
				events.Clear();
		}

		public void AddEvent(ITimestampedEventQueueItem item)
		{
			//find the index by doing a simple linear search

			lock (this)
			{
				//the earliest items are at the top of this queue and the latest items on the bottom. 
				//the goal is to start from the bottom and insert right after the first item that is 
				//before us. 
				if (events.Count >= capacity)
				{
					//if (!emptyWithoutError)
						//Console.WriteLine("EVENT QUEUE IS FULL. OLDEST MEASUREMENT PURGED.");
					Pop();
				}

				//Chuck measurements that are too old
				if ((curTS - item.TimeStamp) > maxAgeDifference && curTS != 0)
				{
					Console.WriteLine("WARNING: ERRONEOUS MEASUREMENT TIMESTAMP!!! ");
					Console.WriteLine("Last Measurement TS: " + curTS.ToString("F6") + " Type: " + item.DataType);
					Console.WriteLine("Recieved         TS: " + item.TimeStamp.ToString("F6") + " Type: " + item.DataType);
					Console.WriteLine("IGNORING OLD MEASUREMENT");
					events.Clear();
					curTS = 0;
					return;
				}

				//If a new measurement comes in and is too new, reset the queue and add it
				if ((item.TimeStamp - curTS) > maxAgeDifference && curTS != 0)
				{
					Console.WriteLine("WARNING: ERRONEOUS MEASUREMENT TIMESTAMP!!! ");
					Console.WriteLine("Last Measurement TS: " + curTS.ToString("F6") + " Type: " + item.DataType);
					Console.WriteLine("Recieved         TS: " + item.TimeStamp.ToString("F6") + " Type: " + item.DataType);
					Console.WriteLine("CLEARING QUEUE");
					events.Clear();
					events.Add(item);
					curTS = item.TimeStamp;
					return;
				}
				
				if (events.Count == 0)
				{
					events.Add(item);
					curTS = item.TimeStamp;
				}
				else
				{
					int index = 0;
					//Start at end of queue
					for (int i = events.Count - 1; i >= 0; i--)
					{
						if (item.TimeStamp > events[i].TimeStamp)
						{
							index = i + 1;
							break;
						}
					}
					if (index == events.Count)
						curTS = item.TimeStamp;
					events.Insert(index, item);
				}
			}
		}

		public int NumItems()
		{
			lock (this)
			{
				return events.Count;
			}
		}

		public ITimestampedEventQueueItem Peek()
		{
			return (Peek(0));
		}

		public ITimestampedEventQueueItem Peek(double curTS)
		{
			lock (this)
			{
				ITimestampedEventQueueItem ret;
				if (events.Count == 0)
				{
					Console.WriteLine("Circular queue is empty.");
					ret = null;
				}
				else
				{
					ret = events[0];
				}
				return ret;
			}
		}

		public ITimestampedEventQueueItem Pop()
		{
			return Pop(0);
		}
		public ITimestampedEventQueueItem Pop(double curTS)
		{
			lock (this)
			{
				ITimestampedEventQueueItem ret;
				if (events.Count == 0)
				{
					Console.WriteLine("Circular queue is empty.");
					ret = null;
				}
				else
				{
					ret = events[0];
					events.RemoveAt(0);
				}
				return ret;
			}

		}

        public bool HasMeasurements(double dqLen, double curTime)
        {
            lock (this)
            {
                if (NumItems() == 0) return false;
                return ((Peek(0).TimeStamp + dqLen <= curTime));		
            }
        }
    }
}
