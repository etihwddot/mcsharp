using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace MCSharp.Console
{
	public class Program
	{
		static void Main(string[] args)
		{
			string regionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\saves\Mapping\region\r.0.0.mca");
			string outputLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"map.png");


			const int pixelsPerBlock = 1;
			const int chunksPerRegion = 32;
			const int blocksPerChunk = 16;
			int size = pixelsPerBlock * chunksPerRegion * blocksPerChunk;

			Bitmap bitmap = new Bitmap(size, size);

			IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(regionFilePath);
			foreach (ChunkData chunk in regionChunks.Where(x => !x.IsEmpty))
			{
				System.Console.WriteLine(chunk.Root);
				

			}

			bitmap.Save(outputLocation, ImageFormat.Png);
		}
	}
}
