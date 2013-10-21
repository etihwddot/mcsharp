namespace MCSharp.WorldBrowser.ViewModels
{
	public struct PixelSize
	{
		public PixelSize(int width, int height)
		{
			m_width = width;
			m_height = height;
		}

		public int Width
		{
			get { return m_width; }
		}

		public int Height
		{
			get { return m_height; }
		}

		int m_width;
		int m_height;
	}
}
