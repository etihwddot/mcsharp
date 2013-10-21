namespace MCSharp.Utility
{
	public static class LengthUtility
	{
		public static int RegionsToBlocks(int regionSize)
		{
			return regionSize * Constants.RegionBlockWidth;
		}

		public static int ChunksToBlocks(int chunkSize)
		{
			return chunkSize * Constants.ChunkBlockWidth;
		}
	}
}
