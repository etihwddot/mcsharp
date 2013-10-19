using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MCSharp.ConsoleApp
{
	public class Program
	{
		static void Main(string[] args)
		{
			string outputLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"map.png");

			GameSaveInfo saveInfo = GameSaveInfo.GetAvailableSaves().FirstOrDefault(x => x.Name == "Mapping" || Path.GetFileName(x.Location) == "Mapping");
			//GameSaveInfo saveInfo = GameSaveInfo.GetAvailableSaves().FirstOrDefault(x => x.Name == "world" || Path.GetFileName(x.Location) == "world");
			if (saveInfo == null)
			{
				Console.WriteLine("Unable to load save.");
				return;
			}

			NewCreateHillshadeBitmap(outputLocation, saveInfo).Wait();
			// CreateHillshadeBitmap(outputLocation, saveInfo);
		}

		private static async Task NewCreateHillshadeBitmap(string outputLocation, GameSaveInfo saveInfo)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			WorldReader reader = new WorldReader(saveInfo.Location);

			GameSaveBounds bounds = await reader.GetCurrentWorldBoundsAsync();
			
			Bitmap bitmap = new Bitmap(bounds.BlockWidth, bounds.BlockHeight);
			using (LockedBitmapWriter bitmapWriter = new LockedBitmapWriter(bitmap))
			{
				// cache height for each block
				Dictionary<int, double> heightCache = new Dictionary<int, double>();

				for (int zChunk = bounds.MinZChunk; zChunk <= bounds.MaxZChunk; zChunk++)
				{
					for (int xChunk = bounds.MinXChunk; xChunk <= bounds.MaxXChunk; xChunk++)
					{
						ChunkData chunk = await reader.GetChunkForChunkCoordinateAsync(xChunk, zChunk);
						if (chunk == null || chunk.IsEmpty)
							continue;

						int chunkLeft = chunk.XPosition.Value * Constants.ChunkBlockWidth;
						int chunkTop = chunk.ZPosition.Value * Constants.ChunkBlockWidth;

						int xOffset = chunkLeft - bounds.MinXBlock;
						int zOffset = chunkTop - bounds.MinZBlock;

						for (int z = 0; z < Constants.ChunkBlockWidth; z++)
						{
							for (int x = 0; x < Constants.ChunkBlockWidth; x++)
							{
								int imageX = x + xOffset;
								int imageY = z + zOffset;

								BiomeKind biome = chunk.GetBiome(x, z);

								// calculate hillshade value
								double hillshade = 0;
								if (imageX > 0 && imageX < bounds.BlockWidth - 1 && imageY > 0 && imageY < bounds.BlockHeight - 1)
								{
									double a = await GetBlockHeight(chunk, z - 1, x - 1, reader);
									double b = await GetBlockHeight(chunk, z - 1, x, reader);
									double c = await GetBlockHeight(chunk, z - 1, x + 1, reader);
									double d = await GetBlockHeight(chunk, z, x - 1, reader);
									double e = await GetBlockHeight(chunk, z, x, reader);
									double f = await GetBlockHeight(chunk, z, x + 1, reader);
									double g = await GetBlockHeight(chunk, z + 1, x - 1, reader);
									double h = await GetBlockHeight(chunk, z + 1, x, reader);
									double i = await GetBlockHeight(chunk, z + 1, x + 1, reader);

									// compute hillshade
									const int cellsize = 4; // no idea what this is...
									const double zFactor = 0.25; // no idea what this is...

									double dzOverDx = ((c + 2 * f + i) - (a + 2 * d + g)) / 8 * cellsize;
									double dzOverDy = ((g + 2 * h + i) - (a + 2 * b + c)) / 8 * cellsize;

									double slopeRad = Math.Atan(zFactor * Math.Sqrt(Math.Pow(dzOverDx, 2) + Math.Pow(dzOverDy, 2)));
									double aspectRad = 0;
									if (dzOverDx != 0)
									{
										aspectRad = Math.Atan2(dzOverDy, -dzOverDx);
										if (aspectRad < 0)
											aspectRad = 2 * Math.PI + aspectRad;
									}
									else if (dzOverDx == 0)
									{
										if (dzOverDy > 0)
											aspectRad = Math.PI / 2.0;
										else if (dzOverDy < 0)
											aspectRad = 2 * Math.PI - Math.PI / 2;
										else
											aspectRad = aspectRad;
									}

									hillshade = 255.0 * ((Math.Cos(c_zenithRadians) * Math.Cos(slopeRad)) + (Math.Sin(c_zenithRadians) * Math.Sin(slopeRad) * Math.Cos(c_azimuthRadians - aspectRad)));
									if (hillshade < 0)
										hillshade = 0;
								}

								hillshade = (int) hillshade;

								Color color = GetColorForBiomeKind(biome);
								Color overlay = Color.FromArgb(150, (int) hillshade, (int) hillshade, (int) hillshade);

								if (hillshade == 180)
									overlay = Color.FromArgb(100, (int) hillshade, (int) hillshade, (int) hillshade);

								bitmapWriter.SetPixel(imageX, imageY, BlendWith(color, overlay));
							}
						}
					}
				}
			}

			bitmap.Save(outputLocation, ImageFormat.Png);

			Console.WriteLine("Total time: {0}", stopwatch.ElapsedMilliseconds);
		}

		private static async Task<double> GetBlockHeight(ChunkData chunk, int blockX, int blockZ, WorldReader reader)
		{
			// load correct chunk
			if (blockZ < 0)
			{
				if (blockX > 0)
					chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value - 1, chunk.ZPosition.Value - 1);
				else if (blockX >= Constants.ChunkBlockWidth)
					chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value + 1, chunk.ZPosition.Value - 1);
				else
					chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value, chunk.ZPosition.Value - 1);
			}
			else if (blockZ >= Constants.ChunkBlockWidth)
			{
				if (blockX < 0)
					chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value - 1, chunk.ZPosition.Value + 1);
				else if (blockX >= Constants.ChunkBlockWidth)
					chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value + 1, chunk.ZPosition.Value + 1);
				else
					chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value, chunk.ZPosition.Value + 1);
			}
			else if (blockX < 0)
			{
				chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value - 1, chunk.ZPosition.Value);
			}
			else if (blockX >= Constants.ChunkBlockWidth)
			{
				chunk = await reader.GetChunkForChunkCoordinateAsync(chunk.XPosition.Value + 1, chunk.ZPosition.Value);
			}

			// adjust block
			if (blockX < 0)
				blockX = Constants.ChunkBlockWidth - 1;
			else if (blockX >= Constants.ChunkBlockWidth)
				blockX = 0;

			if (blockZ < 0)
				blockZ = Constants.ChunkBlockWidth - 1;
			else if (blockZ >= Constants.ChunkBlockWidth)
				blockZ = 0;

			return chunk.GetHeight(blockX, blockZ);
		}

		private static void CreateHillshadeBitmap(string outputLocation, GameSaveInfo saveInfo)
		{
			Stopwatch stopwatchTotal = Stopwatch.StartNew();

			GameSave save = GameSave.Load(saveInfo);

			int? originXOffset = null;
			int? originZOffset = null;

			int[,] heightMap = new int[save.Bounds.BlockHeight, save.Bounds.BlockWidth];

			Stopwatch time = Stopwatch.StartNew();

			save.Regions.AsParallel().ForAll(region =>
			{
				foreach (ChunkData chunk in ChunkLoader.LoadChunksInRegion(region).Where(x => !x.IsEmpty))
				{
					int xOffset = (chunk.XPosition.Value * Constants.ChunkBlockWidth) - save.Bounds.MinXBlock;
					int zOffset = (chunk.ZPosition.Value * Constants.ChunkBlockWidth) - save.Bounds.MinZBlock;

					for (int z = 0; z < Constants.ChunkBlockWidth; z++)
						for (int x = 0; x < Constants.ChunkBlockWidth; x++)
							heightMap[z + zOffset, x + xOffset] = chunk.GetHeight(x, z);
				}
			});

			Console.WriteLine(time.Elapsed);

			Bitmap bitmap = new Bitmap(save.Bounds.BlockWidth, save.Bounds.BlockHeight);
			using (LockedBitmapWriter bitmapWriter = new LockedBitmapWriter(bitmap))
			{
				save.Regions.AsParallel().ForAll(region =>
				{
					string regionFileName = Path.GetFileName(region.FileName);

					IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(region);


					foreach (ChunkData chunk in regionChunks.Where(x => !x.IsEmpty))
					{
						int xOffset = (chunk.XPosition.Value * Constants.ChunkBlockWidth) - save.Bounds.MinXBlock;
						int zOffset = (chunk.ZPosition.Value * Constants.ChunkBlockWidth) - save.Bounds.MinZBlock;


						for (int z = 0; z < Constants.ChunkBlockWidth; z++)
						{
							for (int x = 0; x < Constants.ChunkBlockWidth; x++)
							{
								int imageX = x + xOffset;
								int imageY = z + zOffset;

								BiomeKind biome = chunk.GetBiome(x, z);

								double hillshade = 0;
								if (imageX > 0 && imageX < save.Bounds.BlockWidth - 1 &&
									imageY > 0 && imageY < save.Bounds.BlockHeight - 1)
								{
									double a = heightMap[imageY - 1, imageX - 1];
									double b = heightMap[imageY - 1, imageX];
									double c = heightMap[imageY - 1, imageX + 1];
									double d = heightMap[imageY, imageX - 1];
									double e = heightMap[imageY, imageX];
									double f = heightMap[imageY, imageX + 1];
									double g = heightMap[imageY + 1, imageX - 1];
									double h = heightMap[imageY + 1, imageX];
									double i = heightMap[imageY + 1, imageX + 1];

									// compute hillshade
									const int cellsize = 4; // no idea what this is...
									const double zFactor = 0.25; // no idea what this is...

									double dzOverDx = ((c + 2 * f + i) - (a + 2 * d + g)) / 8 * cellsize;
									double dzOverDy = ((g + 2 * h + i) - (a + 2 * b + c)) / 8 * cellsize;

									double slopeRad = Math.Atan(zFactor * Math.Sqrt(Math.Pow(dzOverDx, 2) + Math.Pow(dzOverDy, 2)));
									double aspectRad = 0;
									if (dzOverDx != 0)
									{
										aspectRad = Math.Atan2(dzOverDy, -dzOverDx);
										if (aspectRad < 0)
											aspectRad = 2 * Math.PI + aspectRad;
									}
									else if (dzOverDx == 0)
									{
										if (dzOverDy > 0)
											aspectRad = Math.PI / 2.0;
										else if (dzOverDy < 0)
											aspectRad = 2 * Math.PI - Math.PI / 2;
										else
											aspectRad = aspectRad;
									}

									hillshade = 255.0 * ((Math.Cos(c_zenithRadians) * Math.Cos(slopeRad)) + (Math.Sin(c_zenithRadians) * Math.Sin(slopeRad) * Math.Cos(c_azimuthRadians - aspectRad)));
									if (hillshade < 0)
										hillshade = 0;
								}

								hillshade = (int) hillshade;


								Color color = GetColorForBiomeKind(biome);
								Color overlay = Color.FromArgb(150, (int) hillshade, (int) hillshade, (int) hillshade);

								if (hillshade == 180)
									overlay = Color.FromArgb(100, (int) hillshade, (int) hillshade, (int) hillshade);

								bitmapWriter.SetPixel(imageX, imageY, BlendWith(color, overlay));
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

		private static Color BlendWith(Color background, Color overlay)
		{
			// from http://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
			var alpha = overlay.A / 256.0;

			var blendedR = (byte)(overlay.R * alpha + background.R * (1 - alpha));
			var blendedG = (byte)(overlay.G * alpha + background.G * (1 - alpha));
			var blendedB = (byte)(overlay.B * alpha + background.B * (1 - alpha));
			return Color.FromArgb(blendedR, blendedG, blendedB);
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
				case BiomeKind.FlowerForest: return Color.Green; // TODO: add flowers
				case BiomeKind.BirchForest: return Color.DarkSeaGreen;
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
				case BiomeKind.MegaTaiga: return Color.ForestGreen;
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

		// hillshading setup

		// illumination angle
		const double c_altitudeDegrees = 45.0;
		const double c_zenithDegrees = 90.0 - c_altitudeDegrees;
		const double c_zenithRadians = c_zenithDegrees * Math.PI / 180.0;

		// illumination direction
		const double c_azimuthDegrees = 315.0;
		const double c_azimuthMathDegrees = 360.0 - c_azimuthDegrees + 90.0; // no idea why it's called azimuth math; see http://edndoc.esri.com/arcobjects/9.2/net/shared/geoprocessing/spatial_analyst_tools/how_hillshade_works.htm
		const double c_azimuthRadians = c_azimuthMathDegrees * Math.PI / 180.0;

		private static Random rng = new Random();
	}
}
