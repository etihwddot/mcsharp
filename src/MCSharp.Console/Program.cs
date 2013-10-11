using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCSharp.Console
{
	public class Program
	{
		static void Main(string[] args)
		{
			string regionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\saves\Mapping\region\r.0.0.mca");

			IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(regionFilePath);
			ChunkData chunk = regionChunks.First();
			foreach (Nbt tag in chunk.Tags)
			{
				System.Console.WriteLine(tag);
			}
		}
	}
}
