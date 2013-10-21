using System.Threading;
using System.Threading.Tasks;

namespace MCSharp.WorldBrowser.ViewModels
{
	public interface IRenderer
	{
		string Title { get; }

		string Name { get; }

		Task<PixelSize> GetRenderSizeAsync(WorldSave save, CancellationToken token);

		Task RenderAsync(WorldSave save, IRenderTarget target, CancellationToken token);
	}
}
