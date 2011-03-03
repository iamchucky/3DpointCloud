using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Magic.Network
{
	/// <summary>
	/// Provides a means of serializing objects using Google's ProtoBuff serializer for .net. 
	/// Good for classes that need to go between C# and C++
	/// </summary>
	/// <typeparam name="T">The type of class to serialize</typeparam>
	public class ProtoBuffSerializer<T> : IMulticastSerializer<T>
	{

		#region IMulticastSerializer Members

		public byte[] Serialize(T obj)
		{
			MemoryStream stream = new MemoryStream();
			ProtoBuf.Serializer.Serialize<T>(stream, obj);
			byte[] binary = stream.ToArray();
			if (binary.Length > 65530) throw new InvalidOperationException("The message attempted to be sent is too large. It is : " + binary.Length + " bytes.");
			return binary;
		}

		public T Deserialize(MemoryStream data)
		{
			return ProtoBuf.Serializer.Deserialize<T>((data));
		}

		public string GetName()
		{
			return "ProtoBuff Serializer";
		}

		public bool SupportsReliable { get { return false; } }

		#endregion
	}
}
