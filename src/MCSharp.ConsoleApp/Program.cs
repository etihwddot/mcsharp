﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MCSharp.ConsoleApp
{
	public class Program
	{
		static void Main(string[] args)
		{
			string regionDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\saves\Mapping\region");
			string outputLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"map.png");

			Stopwatch stopwatchTotal = Stopwatch.StartNew();
			
			List<RegionFile> regions = Directory.GetFiles(regionDirectory, "*.mca")
				.Select(x => new RegionFile(x))
				.ToList();

			// sorry Todd; couldn't resist. :-)
			var bounds = new
			{
				minX = int.MaxValue,
				maxX = int.MinValue,
				minZ = int.MaxValue,
				maxZ = int.MinValue
			};
			bounds = regions.Aggregate(bounds,
				(acc, r) => new
				{
					minX = Math.Min(acc.minX, r.RegionX),
					maxX = Math.Max(acc.maxX, r.RegionX),
					minZ = Math.Min(acc.minZ, r.RegionZ),
					maxZ = Math.Max(acc.maxZ, r.RegionZ)
				});

			int xRegionCount = bounds.maxX - bounds.minX + 1;
			int zRegionCount = bounds.maxZ - bounds.minZ + 1;

			int? originXOffset = null;
			int? originZOffset = null;

			Bitmap bitmap = new Bitmap(Constants.RegionBlockWidth * xRegionCount, Constants.RegionBlockWidth * zRegionCount);
			using (LockedBitmapWriter bitmapWriter = new LockedBitmapWriter(bitmap))
			{
				regions.AsParallel().ForAll(region =>
				{
					string regionFileName = Path.GetFileName(region.FileName);

					IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(region);

					foreach (ChunkData chunk in regionChunks.Where(x => !x.IsEmpty))
					{
						int xOffset = (chunk.XPosition.Value * Constants.ChunkBlockWidth) - (bounds.minX * Constants.RegionBlockWidth);
						int zOffset = (chunk.ZPosition.Value * Constants.ChunkBlockWidth) - (bounds.minZ * Constants.RegionBlockWidth);


						for (int x = 0; x < Constants.ChunkBlockWidth; x++)
						{
							int? lastHeight = null;
							for (int z = 0; z < Constants.ChunkBlockWidth; z++)
							{
								int imageX = x + xOffset;
								int imageY = z + zOffset;

								BiomeKind biome = chunk.GetBiome(x, z);

								int height = chunk.GetHeight(x, z);

								Color color = GetColorForBiomeKind(biome);

								if (lastHeight.HasValue && height > lastHeight)
									color = ControlPaint.Light(color);
								if (lastHeight.HasValue && height < lastHeight)
									color = ControlPaint.Dark(color, 0.1f);

								bitmapWriter.SetPixel(imageX, imageY, color);
								lastHeight = height;
							}
						}

						if (chunk.XPosition == 0 && chunk.ZPosition == 0)
						{
							originXOffset = xOffset;
							originZOffset = zOffset;
						}
					}
				});
			}

			// draw marker at 0, 0
			if (originXOffset.HasValue && originZOffset.HasValue)
			{
				Graphics g = Graphics.FromImage(bitmap);
				const int markerSize = 6;
				g.DrawLine(new Pen(Color.Black, 3.5f), originXOffset.Value - markerSize, originZOffset.Value - markerSize, originXOffset.Value + markerSize, originZOffset.Value + markerSize);
				g.DrawLine(new Pen(Color.Yellow, 2.5f), originXOffset.Value - markerSize, originZOffset.Value - markerSize, originXOffset.Value + markerSize, originZOffset.Value + markerSize);

				g.DrawLine(new Pen(Color.Black, 3.5f), originXOffset.Value + markerSize, originZOffset.Value - markerSize, originXOffset.Value - markerSize, originZOffset.Value + markerSize);
				g.DrawLine(new Pen(Color.Yellow, 2.5f), originXOffset.Value + markerSize, originZOffset.Value - markerSize, originXOffset.Value - markerSize, originZOffset.Value + markerSize);
			}
			
			bitmap.Save(outputLocation, ImageFormat.Png);
			
			Console.WriteLine("Total time: {0}", stopwatchTotal.ElapsedMilliseconds);
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
				
				case BiomeKind.SunflowerPlains:
					return rng.Next(31) < 30 ? Color.Green : Color.Yellow;
				
				case BiomeKind.Plains: return Color.Green;
				case BiomeKind.Forest: return Color.DarkGreen;
				case BiomeKind.ForestHills: return Color.DarkGreen;
				case BiomeKind.ExtremeHills: return Color.DarkGray;
				case BiomeKind.ExtremeHillsEdge: return Color.LightGray;

				// ExtremeHills+ has trees; randomly add some green pixels
				case BiomeKind.ExtremeHillsPlus:
					return rng.Next(11) < 10 ? Color.DarkGray : Color.ForestGreen;

				case BiomeKind.Swampland: return Color.DarkOliveGreen;
				case BiomeKind.Jungle: return Color.FromArgb(0x0D, 0x35, 0x01);
				case BiomeKind.JungleHills: return Color.FromArgb(0x0D, 0x35, 0x01);
				case BiomeKind.Desert: return Color.FromArgb(0xDB, 0xD3, 0xA0);
				case BiomeKind.DesertHills: return Color.FromArgb(0xDB, 0xD3, 0xA0);
				case BiomeKind.ColdTaiga: return Color.White;
				case BiomeKind.ColdTaigaHills: return Color.WhiteSmoke;
				case BiomeKind.Taiga: return Color.ForestGreen;
				case BiomeKind.TaigaHills: return Color.ForestGreen;
				case BiomeKind.IcePlains: return Color.White;
				case BiomeKind.IceMountains: return Color.White;
				case BiomeKind.FrozenRiver: return Color.AntiqueWhite;
				case BiomeKind.FrozenOcean: return Color.CornflowerBlue;
				case BiomeKind.ColdBeach: return Color.Beige;
				case BiomeKind.StoneBeach: return Color.DarkGray;
				
				// RoofedForest has mushrooms; randomly add some red and tan pixels
				case BiomeKind.RoofedForest:
					int value = rng.Next(100);
					return value < 98 ? Color.ForestGreen : value < 99 ? Color.Tan : Color.Red;
				default: return Color.Red;
			}
		}

		private static Random rng = new Random();
	}
}
