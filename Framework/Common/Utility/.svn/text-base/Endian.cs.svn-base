using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common
{
	public static class Endian
	{
		#region bigtolittle
		public static UInt32 BigToLittle(UInt32 gay)
		{
			Byte[] gayness = BitConverter.GetBytes(gay);
			Array.Reverse(gayness);
			return BitConverter.ToUInt32(gayness, 0);
		}
		public static UInt16 BigToLittle(UInt16 gay)
		{
			Byte[] gayness = BitConverter.GetBytes(gay);
			Array.Reverse(gayness);
			return BitConverter.ToUInt16(gayness, 0);
		}
		public static Int16 BigToLittle(Int16 gay)
		{
			Byte[] gayness = BitConverter.GetBytes(gay);
			Array.Reverse(gayness);
			return BitConverter.ToInt16(gayness, 0);
		}
		
		#endregion

	}
}
