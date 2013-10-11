using System;

namespace MCSharp
{
	public static class BinaryUtility
	{
		public static int ConvertBigEndianToInt32(byte[] bytes, int start, int count)
		{
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
				case 3: return bytes[start] << 16 | bytes[start + 1] << 8 | bytes[start + 2];
				case 4: return bytes[start] << 24 | bytes[start + 1] << 16 | bytes[start + 2] << 8 | bytes[start + 3];
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
