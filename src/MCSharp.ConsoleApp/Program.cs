using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

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
			
			List<RegionFile> regions = Directory.GetFiles(regionDirectory, "*.mca")
				.Select(x => new RegionFile(x))
				.ToList();

			// sorry Todd; couldn't resist. :-)
			var bounds = new { minX = int.MaxValue, maxX = int.MinValue, minZ = int.MaxValue, maxZ = int.MinValue };
			bounds = regions.Aggregate(bounds,
				(acc, r) => new { minX = Math.Min(acc.minX, r.RegionX), maxX = Math.Max(acc.maxX,
					r.RegionX), minZ = Math.Min(acc.minZ, r.RegionZ), maxZ = Math.Max(acc.maxZ, r.RegionZ) });

			int xRegionCount = bounds.maxX - bounds.minX + 1;
			int zRegionCount = bounds.maxZ - bounds.minZ + 1;
			Bitmap bitmap = new Bitmap(regionSize * xRegionCount, regionSize * zRegionCount);
			Graphics g = Graphics.FromImage(bitmap);			

			foreach (RegionFile region in regions)
			{
				string regionFileName = Path.GetFileName(region.FileName);

				IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(region.FileName);
				foreach (ChunkData chunk in regionChunks)
				{
					int xOffset = (chunk.XPosition * blocksPerChunk) - (bounds.minX * regionSize);
					int zOffset = (chunk.ZPosition * blocksPerChunk) - (bounds.minZ * regionSize);
			
					for (int x = 0; x < blocksPerChunk; x++)
					{
						for (int z = 0; z < blocksPerChunk; z++)
						{
							int imageX = x + xOffset;
							int imageY = z + zOffset;

							BiomeKind biome = chunk.GetBiome(x, z);
							Color color = GetColorForBiomeKind(biome);
							bitmap.SetPixel(imageX, imageY, color);
						}
					}

					if (chunk.XPosition == 0 && chunk.ZPosition == 0)
					{
						const int markerSize = 6;
						g.DrawLine(new Pen(Color.Yellow, 2.5f), xOffset - markerSize, zOffset - markerSize, xOffset + markerSize, zOffset + markerSize);
						g.DrawLine(new Pen(Color.Yellow, 2.5f), xOffset + markerSize, zOffset - markerSize, xOffset - markerSize, zOffset + markerSize);
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
