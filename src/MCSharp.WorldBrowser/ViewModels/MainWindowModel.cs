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

			var regionProcessor = new TransformBlock<RegionFile, RegionImageData>(async r =>
			{
				byte[] data = new byte[Constants.RegionBlockWidth * Constants.RegionBlockWidth * s_bytesPerPixel];

				var chunkLoaderTransform = new TransformBlock<RegionChunkLoader, RegionChunk>(x => x.ReadChunk(),
					new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, CancellationToken = m_source.Token });

				var chunkToImageData = new ActionBlock<RegionChunk>(x => WriteChunkImageData(save, x, data),
					new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = m_source.Token });

				chunkLoaderTransform.LinkTo(chunkToImageData, new DataflowLinkOptions { PropagateCompletion = true });

				foreach (var chunk in ChunkLoader.LoadChunksInRegion(r))
					chunkLoaderTransform.Post(new RegionChunkLoader(r, chunk));

				chunkLoaderTransform.Complete();

				await chunkToImageData.Completion;

				int imageX = r.RegionX * Constants.RegionBlockWidth - save.Bounds.MinXBlock;
				int imageY = r.RegionZ * Constants.RegionBlockWidth - save.Bounds.MinZBlock;

				return new RegionImageData(imageX, imageY, data);
			}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2, CancellationToken = m_source.Token });

			var renderRegion = new ActionBlock<RegionImageData>(x => RenderRegion(x),
				new ExecutionDataflowBlockOptions { TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext(), CancellationToken = m_source.Token });

			regionProcessor.LinkTo(renderRegion, new DataflowLinkOptions { PropagateCompletion = true });

			m_image = new WriteableBitmap(save.Bounds.BlockWidth, save.Bounds.BlockHeight, c_imageDpi, c_imageDpi, s_pixelFormat, null);
			RaisePropertyChanged(ImageProperty);

			foreach (var region in save.Regions)
				regionProcessor.Post(region);

			regionProcessor.Complete();

			try
			{
				await renderRegion.Completion;
			}
			catch (OperationCanceledException)
			{
			}

			// update status to say we're done
			Elapsed = stopwatch.Elapsed;
		}

		private void RenderRegion(RegionImageData imageData)
		{
			if (imageData == null)
				return;

			Int32Rect regionRect = new Int32Rect(imageData.XOffset, imageData.ZOffset, Constants.RegionBlockWidth, Constants.RegionBlockWidth);
			m_image.WritePixels(regionRect, imageData.ImageData, Constants.RegionBlockWidth * s_bytesPerPixel, 0);
		}

		private static void WriteChunkImageData(GameSave save, RegionChunk regionChunk, byte[] data)
		{
			var chunk = regionChunk.Chunk;
			if (chunk.IsEmpty)
				return;

			int xOffset = chunk.Info.ChunkX * Constants.ChunkBlockWidth;
			int zOffset = chunk.Info.ChunkZ * Constants.ChunkBlockWidth;

			for (int z = 0; z < Constants.ChunkBlockWidth; z++)
			{
				int? lastHeight = null;

				for (int x = 0; x < Constants.ChunkBlockWidth; x++)
				{
					BiomeKind biome = chunk.GetBiome(x, z);

					int height = chunk.GetHeight(x, z);

					Color color = GetColorForBiomeKind(biome);

					if (lastHeight.HasValue && height > lastHeight)
						color = BlendWith(color, Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
					if (lastHeight.HasValue && height < lastHeight)
						color = BlendWith(color, Color.FromArgb(0x50, 0x00, 0x00, 0x00));

					int pixelStart = ((x + xOffset) + ((z + zOffset) * Constants.RegionBlockWidth)) * s_bytesPerPixel;
					data[pixelStart] = color.B;
					data[pixelStart + 1] = color.G;
					data[pixelStart + 2] = color.R;
					data[pixelStart + 3] = color.A;

					lastHeight = height;
				}
			}
		}

		private static Color BlendWith(Color background, Color overlay)
		{
			// from http://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
			double alpha = overlay.A / 256.0;

			byte blendedR = (byte) (overlay.R * alpha + background.R * (1 - alpha));
			byte blendedG = (byte) (overlay.G * alpha + background.G * (1 - alpha));
			byte blendedB = (byte) (overlay.B * alpha + background.B * (1 - alpha));
			return Color.FromRgb(blendedR, blendedG, blendedB);
		}

		private static Color GetColorForBiomeKind(BiomeKind biome)
		{
			switch (biome)
			{
			case BiomeKind.Uncalculated: return Colors.Black;
			case BiomeKind.DeepOcean: return Colors.DarkBlue;
			case BiomeKind.Ocean: return Colors.Blue;
			case BiomeKind.River: return Colors.LightBlue;
			case BiomeKind.Beach: return Colors.LightYellow;

			case BiomeKind.SunflowerPlains:
				return s_random.Next(31) < 30 ? Colors.Green : Colors.Yellow;

			case BiomeKind.Plains: return Colors.Green;
			case BiomeKind.Forest: return Colors.DarkGreen;
			case BiomeKind.ForestHills: return Colors.DarkGreen;
			case BiomeKind.ExtremeHills: return Colors.DarkGray;
			case BiomeKind.ExtremeHillsEdge: return Colors.LightGray;

			// ExtremeHills+ has trees; randomly add some green pixels
			case BiomeKind.ExtremeHillsPlus:
				return s_random.Next(11) < 10 ? Colors.DarkGray : Colors.ForestGreen;

			case BiomeKind.Swampland: return Colors.DarkOliveGreen;
			case BiomeKind.Jungle: return Color.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.JungleHills: return Color.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.Desert: return Color.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.DesertHills: return Color.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.ColdTaiga: return Colors.White;
			case BiomeKind.ColdTaigaHills: return Colors.WhiteSmoke;
			case BiomeKind.Taiga: return Colors.ForestGreen;
			case BiomeKind.TaigaHills: return Colors.ForestGreen;
			case BiomeKind.IcePlains: return Colors.White;
			case BiomeKind.IceMountains: return Colors.White;
			case BiomeKind.FrozenRiver: return Colors.AntiqueWhite;
			case BiomeKind.FrozenOcean: return Colors.CornflowerBlue;
			case BiomeKind.ColdBeach: return Colors.Beige;
			case BiomeKind.StoneBeach: return Colors.DarkGray;

			// RoofedForest has mushrooms; randomly add some red and tan pixels
			case BiomeKind.RoofedForest:
				int value = s_random.Next(100);
				return value < 98 ? Colors.ForestGreen : value < 99 ? Colors.Tan : Colors.Red;
			default: return Colors.Red;
			}
		}

		sealed class RegionImageData
		{
			public RegionImageData(int xOffset, int zOffset, byte[] imageData)
			{
				m_xOffset = xOffset;
				m_zOffset = zOffset;
				m_imageData = imageData;
			}

			public int XOffset
			{
				get { return m_xOffset; }
			}

			public int ZOffset
			{
				get { return m_zOffset; }
			}

			public byte[] ImageData
			{
				get { return m_imageData; }
			}

			int m_xOffset;
			int m_zOffset;
			byte[] m_imageData;
		}

		sealed class RegionChunk
		{
			public RegionChunk(RegionFile region, ChunkData chunk)
			{
				m_region = region;
				m_chunk = chunk;
			}

			public RegionFile Region
			{
				get { return m_region; }
			}

			public ChunkData Chunk
			{
				get { return m_chunk; }
			}

			readonly RegionFile m_region;
			readonly ChunkData m_chunk;
		}

		sealed class RegionChunkLoader
		{
			public RegionChunkLoader(RegionFile region, ChunkLoader loader)
			{
				m_region = region;
				m_chunkLoader = loader;
			}

			public RegionChunk ReadChunk()
			{
				return new RegionChunk(m_region, m_chunkLoader.ReadData());
			}

			readonly RegionFile m_region;
			readonly ChunkLoader m_chunkLoader;
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
