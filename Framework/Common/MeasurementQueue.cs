using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Utility;
using System.Diagnostics;

namespace Magic.Common
{
	public static class TimeSync
	{
		private static double currentTime = 0;

		public static double CurrentTime
		{
			get { return TimeSync.currentTime; }
			set { TimeSync.currentTime = value; }
		}
	}

	public class MeasurementQueue<T> 
	{				
		private class TimestampedMeasurement
		{
			public TimestampedMeasurement(T measurement, double timestamp)
			{
				this.measurement = measurement; this.timestamp = timestamp;
			}
			public T measurement;
			public double timestamp;
		}

		Queue<TimestampedMeasurement> q = new Queue<TimestampedMeasurement>();
		int maxcap;
		public MeasurementQueue (int maxCapacity)
		{
			maxcap = maxCapacity;
		}

		public void AddMeasurement(T measurement)
		{	
			double ts = TimeSync.CurrentTime;
			lock (this)
			{
				if (q.Count > maxcap) Console.WriteLine("FULL QUEUE!");
				while (q.Count > maxcap) q.Dequeue();
			
				q.Enqueue(new TimestampedMeasurement(measurement, ts));
			}
		}

		public bool HasMeasurementsWithDelay(double delay)
		{
			if (q.Count == 0) return false;
			//i.e. is the oldest measurement greater than delay time old?
			return (TimeSync.CurrentTime > (delay + q.Peek().timestamp));			
		}
		public bool HasMeasurements
		{
			get {return (q.Count != 0); }
		}

		public T GetAndPop(out double ts)
		{
			TimestampedMeasurement obj;
			lock (this)
			{
				 obj = q.Dequeue();
			}
			ts = obj.timestamp;
			return obj.measurement;
		}
	}
}
