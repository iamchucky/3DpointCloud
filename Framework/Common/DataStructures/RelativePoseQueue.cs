using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.DataStructures
{
	public class FilterQueue
	{
		int queueLength = 0;
		PoseInterpolator poseInterpolator;
		Queue<PoseFilterState> filterQueue;

		public FilterQueue(int queueLength)
		{
			lock (this)
			{
				this.queueLength = queueLength;
				filterQueue = new Queue<PoseFilterState>(queueLength);
				poseInterpolator = new PoseInterpolator(200);
			}
		}

		public void Enqueue(PoseFilterState state)
		{
			lock (this)
			{
				if (filterQueue.Count >= queueLength)
					filterQueue.Dequeue();
				filterQueue.Enqueue(state);
				poseInterpolator.Add(state);
			}
		}

		public PoseFilterState NearestPose(double timestamp)
		{
			lock (this)
			{
				
				PoseFilterState toReturn;
				if (timestamp > poseInterpolator.OldestTime && timestamp < poseInterpolator.NewestTime)
				{
					toReturn = poseInterpolator.PoseAtTime(timestamp);
				}
				else
				{
					for (int i = filterQueue.Count - 1; i >= 0; i--)
					{
						if (filterQueue.ElementAt(i).timestamp < timestamp)
							return filterQueue.ElementAt(i);
					}
					toReturn = filterQueue.ElementAt(filterQueue.Count - 1);
					Console.WriteLine("Failed to use PoseInterpolator");
				}
				return toReturn;
			}
		}
	}

	public class PoseQueue
	{
		int queueLength = 0;
		Queue<RobotPose> poseQueue;

		public PoseQueue(int queueLength)
		{
			lock (this)
			{
				this.queueLength = queueLength;
				poseQueue = new Queue<RobotPose>(queueLength);
			}
		}

		public void Enqueue(RobotPose pose)
		{
			lock (this)
			{
				if (poseQueue.Count >= queueLength)
					poseQueue.Dequeue();
				poseQueue.Enqueue(pose);
			}
		}

		public RobotPose NearestPose(double timestamp)
		{
			lock (this)
			{
				for (int i = poseQueue.Count - 1; i >= 0; i--)
				{
					if (poseQueue.ElementAt(i).timestamp < timestamp)
						return poseQueue.ElementAt(i);
				}
				return poseQueue.ElementAt(poseQueue.Count - 1);
			}
		}
	}
}
