using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtIntArray : Nbt
	{
		public NbtIntArray(string name, int[] values)
			: base(NbtKind.IntArray, name)
		{
			m_values = values;
		}

		// TODO: implement IReadOnlyList (this objed *is* the list) OR
		//  expose the array as a ReadOnlyList property (this object *has* the list)?

		private int[] m_values;
	}
}
