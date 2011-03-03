using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataInterfaces;

namespace Magic.Common
{
	/// <summary>
	/// Provides a generic message that has a system assigned timestamp in seconds.
	/// In general, it is safe to assume that the timestamps are synchronized.
	/// This class is constructed to be immutable.
	/// It is OK to inherit from this class and create child classes.
	/// </summary>
	/// <typeparam name="T">The type of message you are receiving.</typeparam>
	public class TimestampedEventArgs<T> : EventArgs, ITimeComparable
	{
		private double timestamp;

		private T message;

		public T Message
		{
			get { return message; }
		}
		public TimestampedEventArgs(double timestamp, T message)
		{
			this.timestamp = timestamp;
			this.message = message;
		}

         #region ITimeComparable Members

        public int CompareTo(ITimeComparable obj)
        {
            if (this.timestamp < obj.TimeStamp) return -1;
            if (this.timestamp > obj.TimeStamp) return 1;
            return 0;
        }

        public double TimeStamp
        {
            get {return this.timestamp;}
            set {this.timestamp = value;}
        }

        #endregion

	}
}
