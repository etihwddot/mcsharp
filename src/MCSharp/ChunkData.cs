using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCSharp
{
	public sealed class ChunkData
	{
		public ChunkData(NbtCompound root)
		{
			if (root == null)
				throw new ArgumentNullException("root");

			m_root = root;
		}

		public static readonly ChunkData Empty = new ChunkData();

		public NbtCompound Root
		{
			get { return m_root; }
		}

		public bool IsEmpty
		{
			get { return m_root == null; }
		}

		private ChunkData()
		{
			m_root = null;
		}

		NbtCompound m_root;
	}
}
