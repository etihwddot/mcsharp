using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCSharp
{
	public sealed class ChunkData
	{
		public ChunkData(IEnumerable<NamedBinaryTag> tags)
		{
			if (tags == null)
				throw new ArgumentNullException("tags");

			m_tags = tags.ToList().AsReadOnly();
		}

		public static readonly ChunkData Empty = new ChunkData();

		public ReadOnlyCollection<NamedBinaryTag> Tags
		{
			get { return m_tags; }
		}

		public bool IsEmpty
		{
			get { return m_tags.Count == 0; }
		}

		private ChunkData()
		{
			m_tags = new List<NamedBinaryTag>().AsReadOnly();
		}

		ReadOnlyCollection<NamedBinaryTag> m_tags;
	}
}
