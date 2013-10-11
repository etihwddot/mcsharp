using System;

namespace MCSharp
{
	public static class BinaryUtility
	{
		public static int ConvertBigEndianToInt32(byte[] bytes, int start = 0, int? count = null)
		{
			count = count ?? bytes.Length;

			if (start < 0 || start >= bytes.Length)
				throw new ArgumentOutOfRangeException("start", "Start must be a valid array index");
			if (count < 1 || count > 4)
				throw new ArgumentOutOfRangeException("count", "Count must be between 1 and 4 bytes.");
			if (start + count > bytes.Length)
				throw new ArgumentOutOfRangeException("count", "Start + Count must be within the array's bounds.");

			switch (count)
			{
				case 1: return bytes[start];
				case 2: return bytes[start] << 8 | bytes[start + 1];
				case 3: return bytes[start] << 8 * 2 | bytes[start + 1] << 8 | bytes[start + 2];
				case 4: return bytes[start] << 8 * 3 | bytes[start + 1] << 8 * 2 | bytes[start + 2] << 8 | bytes[start + 3];
				default:
					throw new InvalidOperationException();
			}
		}

		public static long ConvertBigEndianToInt64(byte[] bytes)
		{
			if (bytes.Length != 8)
				throw new ArgumentException("bytes", "Array must be 8 bytes.");

			return
				(long) bytes[0] << 8 * 7 |
				(long) bytes[1] << 8 * 6 |
				(long) bytes[2] << 8 * 5 |
				(long) bytes[3] << 8 * 4 |
				(long) bytes[4] << 8 * 3 |
				(long) bytes[5] << 8 * 2 |
				(long) bytes[6] << 8 |
				(long) bytes[7];
		}

	}
}
