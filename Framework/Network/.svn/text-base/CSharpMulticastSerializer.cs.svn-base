using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;

namespace Magic.Network
{
	/// <summary>
	/// Serializer using the BinaryFormatter serializer built into .NET. 
	/// Although this can serialize any class marked ISerializable, it is not particularly fast nor does it support c++ 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CSharpMulticastSerializer<T> : IMulticastSerializer<T>
	{
		BinaryFormatter serializerEngine;
		bool useCompression;

		public CSharpMulticastSerializer()
		{
			serializerEngine = new BinaryFormatter();
			useCompression = false;
		}

		public CSharpMulticastSerializer(bool compression)
			: this()
		{
			useCompression = compression;
		}
		#region IMulticastSerializer<T> Members

		public byte[] Serialize(T obj)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				if (useCompression)
				{
					using (GZipOutputStream zip = new GZipOutputStream(ms))
					{
						zip.SetLevel(9);
						serializerEngine.Serialize(zip, obj);
						zip.Finish();
					}
				}
				else
					serializerEngine.Serialize(ms, obj);
				return ms.ToArray();
			}
		}

		public T Deserialize(MemoryStream data)
		{
			if (!useCompression)
			{
				object o = serializerEngine.Deserialize(data);
				if (o is T) return (T)o;
				else
				{
					Console.WriteLine("Warning received unexpected type in c# deserializer.");
					return default(T);
				}
			}
			else
			{
				using (GZipInputStream zout = new GZipInputStream(data))
				{
					object o = serializerEngine.Deserialize(zout);
					if (o is T) return (T)o;
					else
					{
						Console.WriteLine("Warning received unexpected type in c# deserializer.");
						return default(T);
					}
				}
			}

		}

		public string GetName()
		{
			return "C# Serializer";
		}

		public bool SupportsReliable
		{
			get { return false; }
		}

		#endregion
	}
}
