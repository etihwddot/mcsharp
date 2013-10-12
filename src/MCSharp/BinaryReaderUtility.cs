using System;
using System.IO;

namespace MCSharp
{
	public static class BinaryReaderUtility
	{
		public static int ReadBigEndianInt32(this BinaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			
			byte[] data = reader.ReadBytes(4);
			return data[0] << 8 * 3 | data[1] << 8 * 2 | data[2] << 8 | data[3];
		}

		public static int ReadBigEndianInt32(this BinaryReader reader, int size)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			if (size < 1 || size > 4)
				throw new ArgumentOutOfRangeException("size");

			byte[] data = reader.ReadBytes(size);
			switch (size)
			{
				case 1: return data[0];
				case 2: return data[0] << 8 | data[1];
				case 3: return data[0] << 8 * 2 | data[1] << 8 | data[2];
				case 4: return data[0] << 8 * 3 | data[1] << 8 * 2 | data[2] << 8 | data[3];
				default:
					throw new InvalidOperationException();
			}
		}

		public static long ReadBigEndianInt64(this BinaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			byte[] data = reader.ReadBytes(8);
			return
				(long) data[0] << 8 * 7 | (long) data[1] << 8 * 6 | (long) data[2] << 8 * 5 | (long) data[3] << 8 * 4 |
				(long) data[4] << 8 * 3 | (long) data[5] << 8 * 2 | (long) data[6] << 8 | data[7];
		}

		public static float ReadBigEndianSingle(this BinaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			byte[] data = reader.ReadBytes(4);
			Array.Reverse(data);
			return BitConverter.ToSingle(data, 0);
		}

		public static double ReadBigEndianDouble(this BinaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			byte[] data = reader.ReadBytes(8);
			Array.Reverse(data);
			return BitConverter.ToDouble(data, 0);
		}
	}
}
