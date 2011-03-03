using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Network
{
	public interface INetworkAddressProvider
	{
		NetworkAddress GetAddressByName(string name);
		bool NameExists(string name);
		List<NetworkAddress> NetworkAddresses { get; }

		string GetIdentifierString { get; }
	}

	public class NetworkAddressProviderNotFoundException : Exception
	{
		public string name;
		public NetworkAddressProviderNotFoundException(string name) : base ("The name could not be located in the given provider.")
		{
			this.name = name;
		}
	}

}
