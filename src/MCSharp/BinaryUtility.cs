using System;

namespace MCSharp
{
    public static class BinaryUtility
    {
        public int ConvertBigEndianToInt32(byte[] bytes, int start, int count)
        {
	        if (count > 4)
		        throw new ArgumentOutOfRangeException("count", "Count must be less than 4 bytes.");
	        if (count <= 0)
		        throw new ArgumentOutOfRangeException("count", "Count must be greater than 0.");

	        int value = 0;	
	        for (int byteIndex = 0; byteIndex < count; byteIndex++)
		        value |= bytes[start + byteIndex] << (count - 1 - byteIndex) * 8;			
	        return value;
        }
    }
}
