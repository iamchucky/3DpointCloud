using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Magic.Network
{

	public interface IMulticastSerializer<T>
	{
		byte[] Serialize(T obj);
		T Deserialize(MemoryStream data);
		string GetName();
		bool SupportsReliable { get; }
	}

}
