namespace MCSharp
{
	public class WorldBounds
	{
		public WorldBounds(int top, int right, int bottom, int left)
		{
			m_top = top;
			m_right = right;
			m_bottom = bottom;
			m_left = left;
		}

		public int Top
		{
			get { return m_top; }
		}

		public int TopBlock
		{
			get { return m_top * Constants.RegionBlockWidth; }
		}

		public int Right
		{
			get { return m_right; }
		}

		public int RightBlock
		{
			get { return m_right * Constants.RegionBlockWidth; }
		}

		public int Bottom
		{
			get { return m_bottom; }
		}

		public int BottomBlock
		{
			get { return m_bottom * Constants.RegionBlockWidth; }
		}

		public int Left
		{
			get { return m_left; }
		}

		public int LeftBlock
		{
			get { return m_left * Constants.RegionBlockWidth; }
		}

		public int HorizontalRegionWidth
		{
			get { return m_right - m_left + 1; }
		}

		public int HorizontalBlockWidth
		{
			get { return HorizontalRegionWidth * Constants.RegionBlockWidth; }
		}

		public int VerticalRegionWidth
		{
			get { return m_bottom - m_top + 1; }
		}

		public int VerticalBlockWidth
		{
			get { return VerticalRegionWidth * Constants.RegionBlockWidth; }
		}

		int m_top;
		int m_right;
		int m_bottom;
		int m_left;
	}
}
