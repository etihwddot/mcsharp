using System;
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
			WorldSave save = await WorldSave.LoadAsync(m_selectedSave);

			Stopwatch stopwatch = Stopwatch.StartNew();
			TransformBlock<RegionFile, Tuple<WorldSave, RegionFile, byte[]>> regionToBytesTransform = new TransformBlock<RegionFile, Tuple<WorldSave, RegionFile, byte[]>>(x =>
			{
				byte[] bytes = GetRegionBytes(x);
				return Tuple.Create(save, x, bytes);
			}, new ExecutionDataflowBlockOptions { CancellationToken = m_source.Token, MaxDegreeOfParallelism = 4});
			
			ActionBlock<Tuple<WorldSave, RegionFile, byte[]>> renderRegionAction = new ActionBlock<Tuple<WorldSave, RegionFile, byte[]>>(x => RenderRegion(x.Item1, x.Item2, x.Item3),
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

			Elapsed = stopwatch.Elapsed;
		}

		private void RenderRegion(WorldSave save, RegionFile region, byte[] bytes)
		{
			int regionXOffset = region.Bounds.X * Constants.RegionBlockWidth - LengthUtility.RegionsToBlocks(save.Bounds.X);
			int regionZOffset = region.Bounds.Z * Constants.RegionBlockWidth - LengthUtility.RegionsToBlocks(save.Bounds.Z);

			Int32Rect regionRect = new Int32Rect(regionXOffset, regionZOffset, Constants.RegionBlockWidth, Constants.RegionBlockWidth);
			m_image.WritePixels(regionRect, bytes, Constants.RegionBlockWidth * s_bytesPerPixel, 0);
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

						Color32 color = BiomeKindUtility.GetColor(biome);

						if (lastHeight.HasValue && height > lastHeight)
							color = Color32.Blend(color, Color32.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
						if (lastHeight.HasValue && height < lastHeight)
							color = Color32.Blend(color, Color32.FromArgb(0x50, 0x00, 0x00, 0x00));

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

		static readonly PixelFormat s_pixelFormat = PixelFormats.Bgra32;
		static readonly int s_bytesPerPixel = s_pixelFormat.BitsPerPixel / 8;

		const int c_imageDpi = 96;

		CancellationTokenSource m_source;
		GameSaveInfo m_selectedSave;
		ReadOnlyCollection<GameSaveInfo> m_availableSaves;
		WriteableBitmap m_image;
		TimeSpan m_elapsed;
		Task m_imageTask;
	}
}
