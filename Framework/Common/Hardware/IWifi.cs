using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.Hardware
{
	public interface IWifi
	{
		string CurrentSSID { get; }
		double SignalStrength { get; }
		WifiSignalStrengthUnits SignalStrengthUnits { get; }
		WifiConnectionStatus ConnectionStatus { get; }
	}
	public enum WifiSignalStrengthUnits
	{
		Unknown, dBm, Relative
	}
	public enum WifiConnectionStatus
	{
		Unknown, Associated, Adhoc, Disconnected
	}
}
