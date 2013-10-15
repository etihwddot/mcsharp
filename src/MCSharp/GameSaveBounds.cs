namespace MCSharp
{
	public class GameSaveBounds
	{
		public GameSaveBounds(int minZ, int maxX, int maxZ, int minX)
		{
			m_minZ = minZ;
			m_maxX = maxX;
			m_maxZ = maxZ;
			m_minX = minX;
		}

		public int MinZ
		{
			get { return m_minZ; }
		}

		public int MinZBlock
		{
			get { return m_minZ * Constants.RegionBlockWidth; }
		}

		public int MaxX
		{
			get { return m_maxX; }
		}

		public int MaxXBlock
		{
			get { return m_maxX * Constants.RegionBlockWidth; }
		}

		public int MaxZ
		{
			get { return m_maxZ; }
		}

		public int MaxZBlock
		{
			get { return m_maxZ * Constants.RegionBlockWidth; }
		}

		public int MinX
		{
			get { return m_minX; }
		}

		public int MinXBlock
		{
			get { return m_minX * Constants.RegionBlockWidth; }
		}

		public int RegionWidth
		{
			get { return m_maxX - m_minX + 1; }
		}

		public int BlockWidth
		{
			get { return RegionWidth * Constants.RegionBlockWidth; }
		}

		public int RegionHeight
		{
			get { return m_maxZ - m_minZ + 1; }
		}

		public int BlockHeight
		{
			get { return RegionHeight * Constants.RegionBlockWidth; }
		}

		int m_minZ;
		int m_maxX;
		int m_maxZ;
		int m_minX;
	}
}
