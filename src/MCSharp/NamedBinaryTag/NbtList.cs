using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Logos.Utility;

namespace MCSharp.NamedBinaryTag
{
	public sealed class NbtList : Nbt
	{
		public NbtList(string name, NbtKind itemKind, IEnumerable<Nbt> items)
			: base(NbtKind.List, name)
		{
			m_itemKind = itemKind;
			m_items = items.ToList().AsReadOnly();
		}

		public NbtKind ItemKind
		{
			get { return m_itemKind; }
		}

		public ReadOnlyCollection<Nbt> Items
		{
			get { return m_items; }
		}

		public override string ToString()
		{
			return base.ToString() + "[{0}] <{1}>".FormatInvariant(Items.Count, ItemKind);
		}

		readonly NbtKind m_itemKind;
		readonly ReadOnlyCollection<Nbt> m_items;
	}
}
