using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public interface IRenderTarget
	{
		void SetPixel(int x, int y, ColorBgra32 color);
	}
}
