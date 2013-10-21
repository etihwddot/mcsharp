﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using MCSharp.Utility;

namespace MCSharp.ConsoleApp
{
	public class Program
	{
		static void Main(string[] args)
		{
			string outputLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"map.png");

			Stopwatch stopwatchTotal = Stopwatch.StartNew();

			GameSaveInfo saveInfo = GameSaveInfo.GetAvailableSaves().FirstOrDefault(x => x.Name == "Mapping" || Path.GetFileName(x.Location) == "Mapping");
			//GameSaveInfo saveInfo = GameSaveInfo.GetAvailableSaves().FirstOrDefault(x => x.Name == "world" || Path.GetFileName(x.Location) == "world");
			if (saveInfo == null)
			{
				Console.WriteLine("Unable to load save.");
				return;
			}

			GameSave save = GameSave.Load(saveInfo);

			int? originXOffset = null;
			int? originZOffset = null;

			int[,] heightMap = new int[save.Bounds.BlockHeight, save.Bounds.BlockWidth];

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

								byte hillshadeByte = (byte) hillshade;

								Color32 color = BiomeKindUtility.GetColor(biome);
								Color32 overlay = Color32.FromArgb(150, hillshadeByte, hillshadeByte, hillshadeByte);

								if (hillshade == 180)
									overlay = Color32.FromArgb(100, hillshadeByte, hillshadeByte, hillshadeByte);

								Color32 blendedColor = Color32.Blend(color, overlay);

								bitmapWriter.SetPixel(imageX, imageY, Color.FromArgb(blendedColor.Alpha, blendedColor.Red, blendedColor.Green, blendedColor.Blue));
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
