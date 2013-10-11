using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtCompound : Nbt
	{
		public NbtCompound(string name, IEnumerable<Nbt> tags) 
			: base(NbtKind.Compound, name)
		{
			m_tags = tags.ToList().AsReadOnly();
		}

		public ReadOnlyCollection<Nbt> Tags
		{ 
			get { return m_tags; }
		}

		ReadOnlyCollection<Nbt> m_tags;
	}
}
