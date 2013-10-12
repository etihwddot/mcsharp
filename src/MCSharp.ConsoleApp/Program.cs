using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MCSharp.ConsoleApp
{
	public class Program
	{
		static void Main(string[] args)
		{
			string regionDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\saves\Mapping\region");
			//string regionDirectory = @"C:\Users\todd\Desktop\region";
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
					int xOffset = (chunk.XPosition * blocksPerChunk) - (minX * regionSize);
					int zOffset = (chunk.ZPosition * blocksPerChunk) - (minZ * regionSize);
					
					for (int x = 0; x < blocksPerChunk; x++)
					{
						for (int z = 0; z < blocksPerChunk; z++)
						{
							BiomeKind biome = chunk.GetBiome(x, z);
							Color color = GetColorForBiomeKind(biome);
							bitmap.SetPixel(x + xOffset, z + zOffset, color);
						}
					}
				}
			}

			bitmap.Save(outputLocation, ImageFormat.Png);
		}

		private static Color GetColorForBiomeKind(BiomeKind biome)
		{
			switch (biome)
			{ 
				case BiomeKind.Uncalculated: return Color.Black;
				case BiomeKind.DeepOcean: return Color.DarkBlue;
				case BiomeKind.Ocean: return Color.Blue;
				case BiomeKind.River: return Color.LightBlue;
				case BiomeKind.Beach: return Color.LightYellow;
				case BiomeKind.SunflowerPlains: return Color.Green;
				case BiomeKind.Plains: return Color.Green;
				case BiomeKind.Forest: return Color.DarkGreen;
				case BiomeKind.ForestHills: return Color.DarkGreen;
				case BiomeKind.ExtremeHills: return Color.Brown;
				case BiomeKind.ExtremeHillsEdge: return Color.SaddleBrown;
				case BiomeKind.Swampland: return Color.DarkOliveGreen;
				case BiomeKind.Jungle: return Color.GreenYellow;
				case BiomeKind.JungleHills: return Color.GreenYellow;
				case BiomeKind.Desert: return Color.Yellow;
				case BiomeKind.DesertHills: return Color.Yellow;
				case BiomeKind.Taiga: return Color.White;
				case BiomeKind.TaigaHills: return Color.WhiteSmoke;
				case BiomeKind.IcePlains: return Color.White;
				case BiomeKind.IceMountains: return Color.White;
				case BiomeKind.FrozenRiver: return Color.AntiqueWhite;
				case BiomeKind.FrozenOcean: return Color.CornflowerBlue;
				default: return Color.Red;
			}
		}
	}
}
