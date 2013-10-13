using System;
using System.Collections;
using System.Collections.Generic;

namespace MCSharp.NamedBinaryTag
{
	public sealed class NbtIntArray : Nbt, IReadOnlyList<int>
	{
		public NbtIntArray(string name, int[] values)
			: base(NbtKind.IntArray, name)
		{
			m_values = values;
		}

		public int this[int index]
		{
			get { return m_values[index]; }
		}

		public int Count
		{
			get { return m_values.Length; }
		}

		public IEnumerator<int> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		int[] m_values;
	}
}
