using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCSharp
{
	public sealed class ChunkData
	{
		public ChunkData(IEnumerable<Nbt> tags)
		{
			if (tags == null)
				throw new ArgumentNullException("tags");

			m_tags = tags.ToList().AsReadOnly();
		}

		public static readonly ChunkData Empty = new ChunkData();

		public ReadOnlyCollection<Nbt> Tags
		{
			get { return m_tags; }
		}

		public bool IsEmpty
		{
			get { return m_tags.Count == 0; }
		}

		private ChunkData()
		{
			m_tags = new List<Nbt>().AsReadOnly();
		}

		ReadOnlyCollection<Nbt> m_tags;
	}
}
