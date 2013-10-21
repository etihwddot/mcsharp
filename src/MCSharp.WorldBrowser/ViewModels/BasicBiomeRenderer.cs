using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class BasicBiomeRenderer
	{
		public BasicBiomeRenderer(WorldSave save)
		{
			m_save = save;
		}

		internal async Task RenderAsync(IRenderTarget target, CancellationToken token)
		{
			TransformBlock<RegionInfo, Tuple<IRenderTarget, RegionInfo, ColorBgra32[]>> regionToBytesTransform = new TransformBlock<RegionInfo, Tuple<IRenderTarget, RegionInfo, ColorBgra32[]>>(x =>
			{
				ColorBgra32[] pixels = GetRegionPixels(x);
				return Tuple.Create(target, x, pixels);
			}, new ExecutionDataflowBlockOptions { CancellationToken = token, MaxDegreeOfParallelism = 4 });

			ActionBlock<Tuple<IRenderTarget, RegionInfo, ColorBgra32[]>> renderRegionAction = new ActionBlock<Tuple<IRenderTarget, RegionInfo, ColorBgra32[]>>(x => RenderRegion(x.Item1, x.Item2, x.Item3),
				new ExecutionDataflowBlockOptions { CancellationToken = token, TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext() });

			regionToBytesTransform.LinkTo(renderRegionAction, new DataflowLinkOptions { PropagateCompletion = true });
			
			foreach (var region in m_save.Regions)
				regionToBytesTransform.Post(region);
			regionToBytesTransform.Complete();

			try
			{
				await renderRegionAction.Completion;
			}
			catch (OperationCanceledException)
			{
			}
		}

		private void RenderRegion(IRenderTarget target, RegionInfo region, ColorBgra32[] pixels)
		{
			int regionXBlockOffset = LengthUtility.RegionsToBlocks(region.Bounds.X - m_save.Bounds.X);
			int regionZBlockOffset = LengthUtility.RegionsToBlocks(region.Bounds.Z - m_save.Bounds.Z);

			int blockWidth = LengthUtility.RegionsToBlocks(region.Bounds.Width);

			target.WritePixels(regionXBlockOffset, regionZBlockOffset, LengthUtility.RegionsToBlocks(region.Bounds.Width), LengthUtility.RegionsToBlocks(region.Bounds.Height), pixels);
		}

		private static ColorBgra32[] GetRegionPixels(RegionInfo region)
		{
			IEnumerable<Chunk> regionChunks = ChunkLoader.LoadChunksInRegion(region);

			int regionBlockWidth = LengthUtility.RegionsToBlocks(1);

			ColorBgra32[] bytes = new ColorBgra32[regionBlockWidth * regionBlockWidth];

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

						ColorBgra32 color = BiomeKindUtility.GetColor(biome);

						if (lastHeight.HasValue && height > lastHeight)
							color = ColorBgra32.Blend(color, ColorBgra32.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
						if (lastHeight.HasValue && height < lastHeight)
							color = ColorBgra32.Blend(color, ColorBgra32.FromArgb(0x50, 0x00, 0x00, 0x00));

						int pixelStart = (x + xOffset + ((z + zOffset) * regionBlockWidth));
						bytes[pixelStart] = color;

						lastHeight = height;
					}
				}
			}

			return bytes;
		}

		public int PixelWidth
		{
			get { return LengthUtility.RegionsToBlocks(m_save.Bounds.Width); }
		}

		public int PixelHeight
		{
			get { return LengthUtility.RegionsToBlocks(m_save.Bounds.Height); }
		}

		readonly WorldSave m_save;
	}
}
