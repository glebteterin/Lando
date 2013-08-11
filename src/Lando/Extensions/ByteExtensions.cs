using System.Collections;

namespace Lando.Extensions
{
	public static class ByteExtensions
	{
		public static byte ToByte(this BitArray array)
		{
			byte[] bytes = new byte[1];
			array.CopyTo(bytes, 0);
			return bytes[0];
		}
	}
}