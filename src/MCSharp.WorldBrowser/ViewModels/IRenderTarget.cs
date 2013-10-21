using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public interface IRenderTarget
	{
		void WritePixels(int x, int y, int width, int height, ColorBgra32[] pixels);
	}
}
