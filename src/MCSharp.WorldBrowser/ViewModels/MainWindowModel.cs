﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Logos.Utility;
using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class MainWindowModel : ViewModel
	{
		public MainWindowModel()
		{
			m_availableSaves = GameSaveInfo.GetAvailableSaves().ToList().AsReadOnly();
			SelectedSave = m_availableSaves.FirstOrDefault();
			m_elapsed = TimeSpan.FromMilliseconds(0);
		}

		public ReadOnlyCollection<GameSaveInfo> AvailableSaves
		{
			get { return m_availableSaves; }
		}

		public static readonly string SelectedSaveProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.SelectedSave);
		public GameSaveInfo SelectedSave
		{
			get
			{
				VerifyAccess();
				return m_selectedSave;
			}
			set
			{
				VerifyAccess();
				if (value != m_selectedSave)
				{
					m_selectedSave = value;
					RaisePropertyChanged(SelectedSaveProperty);
					GenerateImage();
				}
			}
		}

		public static readonly string ImageProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.Image);
		public ImageSource Image
		{
			get
			{
				VerifyAccess();
				return m_image;
			}
		}

		public static readonly string ElapsedProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.Elapsed);
		public TimeSpan Elapsed
		{
			get
			{
				VerifyAccess();
				return m_elapsed;
			}
			set
			{
				VerifyAccess();
				if (value != m_elapsed)
				{
					m_elapsed = value;
					RaisePropertyChanged(ElapsedProperty);
				}
			}
		}

		private async void GenerateImage()
		{
			if (m_selectedSave == null)
				return;

			// cancel existing work
			if (m_source != null)
				m_source.Cancel();

			Stopwatch stopwatch = Stopwatch.StartNew();
			
			m_source = new CancellationTokenSource();

			GameSave save = GameSave.Load(m_selectedSave);

			TransformBlock<RegionFile, Tuple<GameSave, RegionFile, byte[]>> regionToBytesTransform = new TransformBlock<RegionFile, Tuple<GameSave, RegionFile, byte[]>>(x =>
			{
				byte[] bytes = GetRegionBytes(x);
				return Tuple.Create(save, x, bytes);
			}, new ExecutionDataflowBlockOptions { CancellationToken = m_source.Token, MaxDegreeOfParallelism = 4});
			
			ActionBlock<Tuple<GameSave, RegionFile, byte[]>> renderRegionAction = new ActionBlock<Tuple<GameSave, RegionFile, byte[]>>(x => RenderRegion(x.Item1, x.Item2, x.Item3),
				new ExecutionDataflowBlockOptions { CancellationToken = m_source.Token, TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext() });

			regionToBytesTransform.LinkTo(renderRegionAction, new DataflowLinkOptions { PropagateCompletion = true });

			m_image = new WriteableBitmap(save.Bounds.BlockWidth, save.Bounds.BlockHeight, c_imageDpi, c_imageDpi, s_pixelFormat, null);
			RaisePropertyChanged(ImageProperty);

			foreach (var region in save.Regions)
				regionToBytesTransform.Post(region);
			regionToBytesTransform.Complete();

			try
			{
				await renderRegionAction.Completion;
			}
			catch (OperationCanceledException)
			{
			}

			// update status to say we're done
			Elapsed = stopwatch.Elapsed;
		}

		private void RenderRegion(GameSave save, RegionFile region, byte[] bytes)
		{
			int regionXOffset = region.RegionX*Constants.RegionBlockWidth - save.Bounds.MinXBlock;
			int regionZOffset = region.RegionZ*Constants.RegionBlockWidth - save.Bounds.MinZBlock;

			Int32Rect regionRect = new Int32Rect(regionXOffset, regionZOffset, Constants.RegionBlockWidth,
			                                     Constants.RegionBlockWidth);
			m_image.WritePixels(regionRect, bytes, Constants.RegionBlockWidth*s_bytesPerPixel, 0);
		}

		private static byte[] GetRegionBytes(RegionFile region)
		{
			IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(region);

			Byte[] bytes = new Byte[Constants.RegionBlockWidth * Constants.RegionBlockWidth * s_bytesPerPixel];

			foreach (ChunkData chunk in regionChunks.Where(x => !x.IsEmpty))
			{
				int xOffset = chunk.Info.ChunkX * Constants.ChunkBlockWidth;
				int zOffset = chunk.Info.ChunkZ * Constants.ChunkBlockWidth;
				
				// Stop if canceled in middle of chunk processing
				//token.ThrowIfCancellationRequested();

				for (int z = 0; z < Constants.ChunkBlockWidth; z++)
				{
					int? lastHeight = null;
						
					for (int x = 0; x < Constants.ChunkBlockWidth; x++)
					{
						BiomeKind biome = chunk.GetBiome(x, z);

						int height = chunk.GetHeight(x, z);

						Color32 color = GetColorForBiomeKind(biome);

						if (lastHeight.HasValue && height > lastHeight)
							color = BlendWith(color, Color32.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
						if (lastHeight.HasValue && height < lastHeight)
							color = BlendWith(color, Color32.FromArgb(0x50, 0x00, 0x00, 0x00));

						int pixelStart = (x + xOffset + ((z + zOffset) * Constants.RegionBlockWidth)) * s_bytesPerPixel;
						bytes[pixelStart] = color.Blue;
						bytes[pixelStart + 1] = color.Green;
						bytes[pixelStart + 2] = color.Red;
						bytes[pixelStart + 3] = color.Alpha;

						lastHeight = height;
					}
				}
			}

			return bytes;
		}

		private static Color32 BlendWith(Color32 background, Color32 overlay)
		{
			// from http://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
			double alpha = overlay.Alpha / 256.0;

			byte blendedR = (byte) (overlay.Red * alpha + background.Red * (1 - alpha));
			byte blendedG = (byte) (overlay.Green * alpha + background.Green * (1 - alpha));
			byte blendedB = (byte) (overlay.Blue * alpha + background.Blue * (1 - alpha));
			return Color32.FromRgb(blendedR, blendedG, blendedB);
		}

		private static Color32 GetColorForBiomeKind(BiomeKind biome)
		{
			switch (biome)
			{
			case BiomeKind.Uncalculated: return NamedColors.Black;
			case BiomeKind.DeepOcean: return NamedColors.DarkBlue;
			case BiomeKind.Ocean: return NamedColors.Blue;
			case BiomeKind.River: return NamedColors.LightBlue;
			case BiomeKind.Beach: return NamedColors.LightYellow;

			case BiomeKind.SunflowerPlains:
				return s_random.Next(31) < 30 ? NamedColors.Green : NamedColors.Yellow;

			case BiomeKind.Plains: return NamedColors.Green;
			case BiomeKind.Forest: return NamedColors.DarkGreen;
			case BiomeKind.ForestHills: return NamedColors.DarkGreen;
			case BiomeKind.ExtremeHills: return NamedColors.DarkGray;
			case BiomeKind.ExtremeHillsEdge: return NamedColors.LightGray;

			// ExtremeHills+ has trees; randomly add some green pixels
			case BiomeKind.ExtremeHillsPlus:
				return s_random.Next(11) < 10 ? NamedColors.DarkGray : NamedColors.ForestGreen;

			case BiomeKind.Swampland: return NamedColors.DarkOliveGreen;
			case BiomeKind.Jungle: return Color32.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.JungleHills: return Color32.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.Desert: return Color32.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.DesertHills: return Color32.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.ColdTaiga: return NamedColors.White;
			case BiomeKind.ColdTaigaHills: return NamedColors.WhiteSmoke;
			case BiomeKind.Taiga: return NamedColors.ForestGreen;
			case BiomeKind.TaigaHills: return NamedColors.ForestGreen;
			case BiomeKind.IcePlains: return NamedColors.White;
			case BiomeKind.IceMountains: return NamedColors.White;
			case BiomeKind.FrozenRiver: return NamedColors.AntiqueWhite;
			case BiomeKind.FrozenOcean: return NamedColors.CornflowerBlue;
			case BiomeKind.ColdBeach: return NamedColors.Beige;
			case BiomeKind.StoneBeach: return NamedColors.DarkGray;

			// RoofedForest has mushrooms; randomly add some red and tan pixels
			case BiomeKind.RoofedForest:
				int value = s_random.Next(100);
				return value < 98 ? NamedColors.ForestGreen : value < 99 ? NamedColors.Tan : NamedColors.Red;
			default: return NamedColors.Red;
			}
		}

		static readonly Random s_random = new Random();
		static readonly PixelFormat s_pixelFormat = PixelFormats.Bgra32;
		static readonly int s_bytesPerPixel = s_pixelFormat.BitsPerPixel / 8;

		const int c_imageDpi = 96;

		CancellationTokenSource m_source;
		GameSaveInfo m_selectedSave;
		ReadOnlyCollection<GameSaveInfo> m_availableSaves;
		WriteableBitmap m_image;
		TimeSpan m_elapsed;
	}
}
