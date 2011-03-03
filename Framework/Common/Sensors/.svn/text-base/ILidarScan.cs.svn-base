using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataInterfaces;

namespace Magic.Common.Sensors
{

	public interface ILidarScan<T> : ILoggable, ITimestampedEventQueueItem
	{
		/// <summary>
		/// An internal ID used to identify scans
		/// </summary>
		int ScannerID { get; }

        /// <summary>
        /// The timestamp of the scan in seconds
        /// </summary>
        double Timestamp { get; set; }

        /// <summary>
        /// A List of Points
        /// </summary>
        List<T> Points { get; }
    }

    public class ILidarScanEventArgs<T> : EventArgs
    {
        private T s;

        public ILidarScanEventArgs(T s)
        {
            this.s = s;
        }

        public T Scan
        {
            get { return s; }
        }
    }
}
