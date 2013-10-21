using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class HillshadeRenderer : IRenderer
	{
		public string Title
		{
			get { return "Hillshade"; }
		}

		public string Name
		{
			get { return "hillshade"; }
		}

		public Task<PixelSize> GetRenderSizeAsync(WorldSave save, System.Threading.CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public Task RenderAsync(WorldSave save, IRenderTarget target, System.Threading.CancellationToken token)
		{
			throw new NotImplementedException();
		}
	}
}
