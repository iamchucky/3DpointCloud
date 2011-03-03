using System;

namespace Magic.ViconInterface
{
	public static class ViconRequest
	{
		private static byte[] close = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		private static byte[] info = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		private static byte[] data = { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		private static byte[] streamingOn = { 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		private static byte[] streamingOff = { 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		private static int requestSize = 8;

		public static byte[] Close { get { return close; } }
		public static byte[] Info { get { return info; } }
		public static byte[] Data { get { return data; } }
		public static byte[] StreamingOn { get { return streamingOn; } }
		public static byte[] StreamingOff { get { return streamingOff; } }
		public static int RequestSize { get { return requestSize; } }
	}
}