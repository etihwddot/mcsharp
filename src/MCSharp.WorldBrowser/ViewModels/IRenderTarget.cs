using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public interface IRenderTarget
	{
		void WritePixels(int x, int y, int width, int height, ColorBgra32[] pixels);
	}
}
