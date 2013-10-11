using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtList : Nbt
	{
		public NbtList(string name, IEnumerable<Nbt> items)
			: base(NbtKind.List, name)
		{
			m_items = items.ToList().AsReadOnly();
		}

		ReadOnlyCollection<Nbt> m_items;
	}
}
