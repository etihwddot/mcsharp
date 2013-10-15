using System;
using System.IO;
namespace MCSharp
{
	public static class Constants
	{
		/// <summary>
		/// Total number of chunks in a region
		/// </summary>
		public static readonly int ChunksPerRegion = 1024;

		/// <summary>
		/// Number of blocks wide a chunk is
		/// </summary>
		public static readonly int ChunkBlockWidth = 16;

		/// <summary>
		/// Number of chunks wide a region is
		/// </summary>
		public static readonly int RegionChunkWidth = 32;

		/// <summary>
		/// Number of chunks wide a region is
		/// </summary>
		public static readonly int RegionBlockWidth = RegionChunkWidth * ChunkBlockWidth;
		
		/// <summary>
		/// Number of bytes in a region file sector
		/// </summary>
		public static readonly int RegionFileSectorSize = 4096;

		public static readonly string MinecraftPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft");
	}
}
