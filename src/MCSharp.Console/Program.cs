using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using Logos.Utility;
using System.Text.RegularExpressions;

namespace MCSharp.Console
{
	public class Program
	{
		static void Main(string[] args)
		{
			// string regionDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\saves\Mapping\region");
			string regionDirectory = @"C:\Users\todd\Desktop\region";
			string outputLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"map.png");
			
			const int pixelsPerBlock = 1;
			const int chunksPerRegion = 32;
			const int blocksPerChunk = 16;
			int regionSize = pixelsPerBlock * chunksPerRegion * blocksPerChunk;
			string[] regionFiles = Directory.GetFiles(regionDirectory, "*.mca");

			int minX = int.MaxValue;
			int maxX = int.MinValue;
			int minZ = int.MaxValue;
			int maxZ = int.MinValue;
			foreach (string regionFile in regionFiles)
			{
				string regionFileName = Path.GetFileName(regionFile);
				string[] regionFileNameParts = regionFileName.Split('.');
				int x = int.Parse(regionFileNameParts[1]);
				int z = int.Parse(regionFileNameParts[2]);

				minX = Math.Min(minX, x);
				maxX = Math.Max(maxX, x);
				minZ = Math.Min(minZ, z);
				maxZ = Math.Max(maxZ, z);
			}

			int xRegionCount = maxX - minX + 1;
			int zRegionCount = maxZ - minZ + 1;
			Bitmap bitmap = new Bitmap(regionSize * xRegionCount, regionSize * zRegionCount);
			
			foreach (string regionFile in regionFiles)
			{
				string regionFileName = Path.GetFileName(regionFile);

				IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(regionFile);
				foreach (ChunkData chunk in regionChunks)
				{
					//if (!chunk.IsEmpty)
					//	System.Console.WriteLine(chunk.Root);

					int xOffset = (chunk.XPosition * blocksPerChunk) - (minX * regionSize);
					int zOffset = (chunk.ZPosition * blocksPerChunk) - (minZ * regionSize);
					
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
							else if (biome == BiomeKind.ForestHills)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.DarkGreen);
							else if (biome == BiomeKind.ExtremeHills)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.Brown);
							else if (biome == BiomeKind.ExtremeHillsEdge)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.SaddleBrown);
							else if (biome == BiomeKind.Swampland)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.DarkOliveGreen);
							else if (biome == BiomeKind.Jungle)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.GreenYellow);
							else if (biome == BiomeKind.JungleHills)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.GreenYellow);
							else if (biome == BiomeKind.Desert)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.Yellow);
							else if (biome == BiomeKind.DesertHills)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.Yellow);
							else if (biome == BiomeKind.Taiga)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.White);
							else if (biome == BiomeKind.TaigaHills)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.WhiteSmoke);
							else if (biome == BiomeKind.IcePlains)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.White);
							else if (biome == BiomeKind.IceMountains)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.White);
							else if (biome == BiomeKind.FrozenRiver)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.AntiqueWhite);
							else if (biome == BiomeKind.FrozenOcean)
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.CornflowerBlue);
							else
								bitmap.SetPixel(x + xOffset, z + zOffset, Color.Red);
						}
					}
				}
			}

			bitmap.Save(outputLocation, ImageFormat.Png);
		}
	}
}
