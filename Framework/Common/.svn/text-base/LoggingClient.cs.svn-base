using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Magic.Common;

namespace Magic.Common
{
	public class LoggingClient <T> : IDisposable where T: ILoggable
	{
		public bool isDisposing = false;
		public class LoggedItem 
		{
			public T data; 
			public double timestamp;
			public LoggedItem(T data, double timestamp)
			{
				this.data = data; this.timestamp = timestamp;
			}
			public string ToLog()
			{
				return timestamp.ToString() + "\t" + data.ToLog() + Environment.NewLine;
			}
		}

		FileStream fs;
		
		string sensorName;
		public LoggingClient(string fileName, string sensorName)
		{
			fs = new FileStream(fileName, FileMode.Create);			
			this.sensorName = sensorName;
		}

		public void AddData (T data, double timestamp)
		{
			if (isDisposing) return;
			LoggedItem li = new LoggingClient<T>.LoggedItem(data, timestamp);
			ASCIIEncoding ascii = new ASCIIEncoding();

			byte[] b = ascii.GetBytes(li.ToLog());
			fs.Write(b,0,b.Length);
		}

	
		#region IDisposable Members

		public void  Dispose()
		{
			isDisposing = true;
			fs.Close();
			Console.WriteLine("Closed logger " + sensorName);
		}

		#endregion
}
}
