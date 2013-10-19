namespace MCSharp
{
	public struct ChunkInfo
	{
		public ChunkInfo(int ordinalPosition, int offset, int size)
		{
			m_ordinalPosition = ordinalPosition;
			m_offset = offset;
			m_size = size;
		}

		public int OrdinalPosition { get { return m_ordinalPosition; } }

		public int ChunkX { get { return OrdinalPosition % 32; } }
		public int ChunkZ { get { return OrdinalPosition / 32; } }

		public int X { get { return ChunkX * Constants.ChunkBlockWidth; } }
		public int Z { get { return ChunkZ * Constants.ChunkBlockWidth; } }

		public int Offset { get { return m_offset; } }
		public int Size { get { return m_size; } }

		public int AbsoluteOffset { get { return Offset * Constants.RegionFileSectorSize; } }
		public int AbsoluteSize { get { return Size * Constants.RegionFileSectorSize; } }

		public bool ChunkExists { get { return Offset >= 2; } }

		readonly int m_ordinalPosition;
		readonly int m_offset;
		readonly int m_size;
	}

}
