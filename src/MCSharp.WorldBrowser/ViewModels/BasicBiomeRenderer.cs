using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class BasicBiomeRenderer : IRenderer
	{
		public string Title
		{
			get { return "Basic Biome"; }
		}

		public string Name
		{
			get { return "biome"; }
		}

		public async Task RenderAsync(WorldSave save, IRenderTarget target, CancellationToken token)
		{
			IEnumerable<Task> tasks = save.Regions
				.Select(x => RenderRegionAsync(x, save, target, token));

			try
			{
				await Task.WhenAll(tasks);
			}
			catch (OperationCanceledException)
			{
			}
		}

		private static Task RenderRegionAsync(RegionInfo region, WorldSave save, IRenderTarget target, CancellationToken token)
		{
			return Task.Run(() =>
			{
				IEnumerable<Chunk> regionChunks = ChunkLoader.LoadChunksInRegion(region);

				foreach (Chunk chunk in regionChunks.Where(x => !x.IsEmpty))
				{
					int xOffset = LengthUtility.ChunksToBlocks(chunk.XPosition.Value) - LengthUtility.RegionsToBlocks(save.Bounds.X);
					int zOffset = LengthUtility.ChunksToBlocks(chunk.ZPosition.Value) - LengthUtility.RegionsToBlocks(save.Bounds.Z);

					// Stop if canceled in middle of chunk processing
					token.ThrowIfCancellationRequested();

					int chunkBlockWidth = LengthUtility.ChunksToBlocks(1);

					for (int z = 0; z < chunkBlockWidth; z++)
					{
						int? lastHeight = null;

						for (int x = 0; x < chunkBlockWidth; x++)
						{
							int imageX = x + xOffset;
							int imageY = z + zOffset;

							BiomeKind biome = chunk.GetBiome(x, z);

							int height = chunk.GetHeight(x, z);

							ColorBgra32 color = BiomeKindUtility.GetColor(biome);

							if (lastHeight.HasValue && height > lastHeight)
								color = ColorBgra32.Blend(color, ColorBgra32.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
							if (lastHeight.HasValue && height < lastHeight)
								color = ColorBgra32.Blend(color, ColorBgra32.FromArgb(0x50, 0x00, 0x00, 0x00));

							target.SetPixel(imageX, imageY, color);

							lastHeight = height;
						}
					}
				}
			});
		}

		public Task<PixelSize> GetRenderSizeAsync(WorldSave save, CancellationToken token)
		{
			return Task.Run(() => new PixelSize(LengthUtility.RegionsToBlocks(save.Bounds.Width), LengthUtility.RegionsToBlocks(save.Bounds.Height)), token);
		}
	}
}
