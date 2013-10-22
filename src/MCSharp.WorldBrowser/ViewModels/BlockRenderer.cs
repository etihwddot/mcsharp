using System;
using System.Threading;
using System.Threading.Tasks;
using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class BlockRenderer : IRenderer
	{
		public string Title
		{
			get { return "Block Renderer"; }
		}

		public string Name
		{
			get { return "block-renderer"; }
		}

		public Task<PixelSize> GetRenderSizeAsync(WorldSave save, CancellationToken token)
		{
			return Task.Run(() => new PixelSize(LengthUtility.RegionsToBlocks(save.Bounds.Width), LengthUtility.RegionsToBlocks(save.Bounds.Height)), token);
		}

		public async Task RenderAsync(WorldSave save, IRenderTarget target, CancellationToken token)
		{
			foreach (RegionInfo region in save.Regions)
			{
			}
		}
	}
}
