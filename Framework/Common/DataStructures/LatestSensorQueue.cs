using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataInterfaces;

namespace Magic.Common.DataStructures
{
	public class LatestSensorMeasurementQueue<T> where T : ITimeComparable
	{

		int queueLength = 0;
		Queue<T> mQueue;
		public int QueueCount
		{
			get { return mQueue.Count; }
		}

		public LatestSensorMeasurementQueue(int queueLength)
		{
			lock (this)
			{
				this.queueLength = queueLength;
				mQueue = new Queue<T>(queueLength);
			}
		}

		public void Enqueue(T m)
		{
			lock (this)
			{
				while (mQueue.Count >= queueLength)
					mQueue.Dequeue();
				mQueue.Enqueue(m);
			}
		}

		public T Oldest()
		{
			lock (this)
			{
				if (mQueue.Count == 0) return default(T);
				return mQueue.ElementAt(0);
			}
		}

		public T Newest()
		{
			lock (this)
			{
				if (mQueue.Count == 0) return default(T);
				return mQueue.ElementAt(mQueue.Count - 1);

			}
		}
		public T Nearest(double timestamp)
		{
			lock (this)
			{
				for (int i = mQueue.Count - 1; i >= 0; i--)
				{
					if (mQueue.ElementAt(i).TimeStamp < timestamp && i < mQueue.Count - 1)
					{
						return (timestamp - mQueue.ElementAt(i).TimeStamp) < (mQueue.ElementAt(i + 1).TimeStamp - timestamp) ? mQueue.ElementAt(i) : mQueue.ElementAt(i + 1);
						//return mQueue.ElementAt(i);
					}
				}
				return mQueue.ElementAt(mQueue.Count - 1);
			}
		}
	}
}
