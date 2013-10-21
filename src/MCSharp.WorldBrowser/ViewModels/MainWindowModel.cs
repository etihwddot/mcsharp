﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
			m_log = new StringBuilder();
			m_availableSaves = GameSaveInfo.GetAvailableSaves().ToList().AsReadOnly();
			SelectedSave = m_availableSaves.FirstOrDefault();
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

		public static readonly string LogProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.Log);
		public string Log
		{
			get
			{
				VerifyAccess();
				return m_log.ToString();
			}
		}

		private void WriteLine(string text)
		{
			m_log.AppendLine(text);
			RaisePropertyChanged(LogProperty);
		}

		private void WriteLine(string format, params object[] args)
		{
			m_log.AppendFormat(format, args);
			m_log.AppendLine();
			RaisePropertyChanged(LogProperty);
		}

		private void GenerateImage()
		{
			if (m_selectedSave == null)
				return;

			// cancel existing work
			if (m_source != null)
				m_source.Cancel();

			// TODO: determine if something like this is necessary
			if (m_imageTask != null && m_imageTask.Status == TaskStatus.Running)
				m_imageTask.Wait();

			m_source = new CancellationTokenSource();
			m_imageTask = DoGenerateImage(m_source.Token);
		}

		private async Task DoGenerateImage(CancellationToken token)
		{
			// Time the generate process
			Stopwatch stopwatch = Stopwatch.StartNew();

			// load the save
			WriteLine("Loading: {0}", m_selectedSave.Name);
			WorldSave save = await WorldSave.LoadAsync(m_selectedSave);

			TransformBlock<RegionInfo, Tuple<WorldSave, RegionInfo, byte[]>> regionToBytesTransform = new TransformBlock<RegionInfo, Tuple<WorldSave, RegionInfo, byte[]>>(x =>
			{
				byte[] bytes = GetRegionBytes(x);
				return Tuple.Create(save, x, bytes);
			}, new ExecutionDataflowBlockOptions { CancellationToken = m_source.Token, MaxDegreeOfParallelism = 4});
			
			ActionBlock<Tuple<WorldSave, RegionInfo, byte[]>> renderRegionAction = new ActionBlock<Tuple<WorldSave, RegionInfo, byte[]>>(x => RenderRegion(x.Item1, x.Item2, x.Item3),
				new ExecutionDataflowBlockOptions { CancellationToken = m_source.Token, TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext() });

			regionToBytesTransform.LinkTo(renderRegionAction, new DataflowLinkOptions { PropagateCompletion = true });

			m_image = new WriteableBitmap(LengthUtility.RegionsToBlocks(save.Bounds.Width), LengthUtility.RegionsToBlocks(save.Bounds.Height), c_imageDpi, c_imageDpi, s_pixelFormat, null);
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

			WriteLine("Total time: {0}", stopwatch.Elapsed);
		}

		private void RenderRegion(WorldSave save, RegionInfo region, byte[] bytes)
		{
			int regionXBlockOffset = LengthUtility.RegionsToBlocks(region.Bounds.X - save.Bounds.X);
			int regionZBlockOffset = LengthUtility.RegionsToBlocks(region.Bounds.Z - save.Bounds.Z);

			int blockWidth = LengthUtility.RegionsToBlocks(region.Bounds.Width);

			Int32Rect regionRect = new Int32Rect(regionXBlockOffset, regionZBlockOffset, LengthUtility.RegionsToBlocks(region.Bounds.Width), LengthUtility.RegionsToBlocks(region.Bounds.Height));
			m_image.WritePixels(regionRect, bytes, blockWidth * s_bytesPerPixel, 0);
		}

		private static byte[] GetRegionBytes(RegionInfo region)
		{
			IEnumerable<Chunk> regionChunks = ChunkLoader.LoadChunksInRegion(region);

			int regionBlockWidth = LengthUtility.RegionsToBlocks(1);

			Byte[] bytes = new Byte[regionBlockWidth * regionBlockWidth * s_bytesPerPixel];

			foreach (Chunk chunk in regionChunks.Where(x => !x.IsEmpty))
			{
				int xOffset = LengthUtility.ChunksToBlocks(chunk.Info.ChunkX);
				int zOffset = LengthUtility.ChunksToBlocks(chunk.Info.ChunkZ);
				
				// Stop if canceled in middle of chunk processing
				//token.ThrowIfCancellationRequested();

				int chunkBlockWidth = LengthUtility.ChunksToBlocks(1);

				for (int z = 0; z < chunkBlockWidth; z++)
				{
					int? lastHeight = null;

					for (int x = 0; x < chunkBlockWidth; x++)
					{
						BiomeKind biome = chunk.GetBiome(x, z);

						int height = chunk.GetHeight(x, z);

						Color32 color = BiomeKindUtility.GetColor(biome);

						if (lastHeight.HasValue && height > lastHeight)
							color = Color32.Blend(color, Color32.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
						if (lastHeight.HasValue && height < lastHeight)
							color = Color32.Blend(color, Color32.FromArgb(0x50, 0x00, 0x00, 0x00));

						int pixelStart = (x + xOffset + ((z + zOffset) * regionBlockWidth)) * s_bytesPerPixel;
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

		static readonly PixelFormat s_pixelFormat = PixelFormats.Bgra32;
		static readonly int s_bytesPerPixel = s_pixelFormat.BitsPerPixel / 8;

		const int c_imageDpi = 96;

		readonly StringBuilder m_log;

		CancellationTokenSource m_source;
		GameSaveInfo m_selectedSave;
		ReadOnlyCollection<GameSaveInfo> m_availableSaves;
		WriteableBitmap m_image;
		Task m_imageTask;		
	}
}
