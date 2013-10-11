﻿namespace MCSharp
{
	public sealed class ChunkInfo
	{
		public ChunkInfo(int ordinalPosition, int offset, int size)
		{
			OrdinalPosition = ordinalPosition;
			Offset = offset;
			Size = size;
		}

		public int OrdinalPosition { get; private set; }

		public int ChunkX { get { return OrdinalPosition % 32; } }
		public int ChunkZ { get { return OrdinalPosition / 32; } }

		public int X { get { return ChunkX * Constants.ChunkSize; } }
		public int Z { get { return ChunkZ * Constants.ChunkSize; } }

		public int Offset { get; private set; }
		public int Size { get; private set; }

		public int AbsoluteOffset { get { return Offset * Constants.SectorSize; } }
		public int AbsoluteSize { get { return Size * Constants.SectorSize; } }

		public bool ChunkExists { get { return Offset >= 2; } }
	}

}
