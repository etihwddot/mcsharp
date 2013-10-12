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
			foreach (ChunkData chunk in regionChunks)
			{
				if (!chunk.IsEmpty)
					System.Console.WriteLine(chunk.Root);

				int xOffset = chunk.Info.X;
				int zOffset = chunk.Info.Z;

				for (int x = 0; x < blocksPerChunk; x++)
				{
					for (int z = 0; z < blocksPerChunk; z++)
					{
						BiomeKind biome = chunk.GetBiome(x, z);

						if (biome == BiomeKind.Uncalculated)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.Black);
						else if (biome == BiomeKind.DeepOcean)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.DarkBlue);
						else if (biome == BiomeKind.Ocean)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.Blue);
						else if (biome == BiomeKind.River)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.LightBlue);
						else if (biome == BiomeKind.Beach)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.LightYellow);
						else if (biome == BiomeKind.SunflowerPlains)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.Green);
						else if (biome == BiomeKind.Plains)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.Green);
						else if (biome == BiomeKind.Forest)
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.DarkGreen);
						else
							bitmap.SetPixel(x + xOffset, z + zOffset, Color.Red);
					}
				}
			}

			bitmap.Save(outputLocation, ImageFormat.Png);
		}
	}
}
