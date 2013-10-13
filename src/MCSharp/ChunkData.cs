using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MCSharp.NamedBinaryTag;

namespace MCSharp
{
	public sealed class ChunkData
	{
		public ChunkData(ChunkInfo info, NbtCompound root)
		{
			m_info = info;
			m_root = root;
			if (m_root != null)
			{
				m_level = (NbtCompound) m_root.Tags.Single();
				m_biomes = m_level.Tags.OfType<NbtByteArray>().FirstOrDefault(x => x.Name == "Biomes");
				m_xPosition = m_level.Tags.OfType<NbtInt>().First(x => x.Name == "xPos").Value;
				m_zPosition = m_level.Tags.OfType<NbtInt>().First(x => x.Name == "zPos").Value;
			}
		}

		public NbtCompound Root
		{
			get { return m_root; }
		}

		public ChunkInfo Info
		{
			get { return m_info; }
		}

		public int? XPosition
		{
			get { return m_xPosition; }
		}

		public int? ZPosition
		{
			get { return m_zPosition; }
		}

		public BiomeKind GetBiome(int chunkX, int chunkZ)
		{
			int index = chunkZ * Constants.ChunkSize + chunkX;
			if (m_biomes == null)
				return BiomeKind.Uncalculated;

			return (BiomeKind) m_biomes.Bytes[index];
		}

		public bool IsEmpty
		{
			get { return m_root == null; }
		}

		NbtCompound m_root;
		NbtCompound m_level;
		ChunkInfo m_info;
		NbtByteArray m_biomes;
		int? m_xPosition;
		int? m_zPosition;
	}
}
