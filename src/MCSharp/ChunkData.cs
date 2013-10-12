using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCSharp
{
	public sealed class ChunkData
	{
		public ChunkData(ChunkInfo info, NbtCompound root)
		{
			m_info = info;
			m_root = root;
			if (m_root != null)
				m_level = (NbtCompound) m_root.Tags.Single();
		}

		public NbtCompound Root
		{
			get { return m_root; }
		}

		public ChunkInfo Info
		{
			get { return m_info; }
		}

		public bool IsEmpty
		{
			get { return m_root == null; }
		}

		NbtCompound m_root;
		NbtCompound m_level;
		ChunkInfo m_info;
	}
}
