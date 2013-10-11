using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public abstract class Nbt
	{
		public NbtKind Kind
		{
			get { return m_kind; }
		}

		public string Name
		{
			get { return m_name; }
		}

		protected Nbt(NbtKind kind, string name)
		{
			m_kind = kind;
			m_name = name;
		}

		NbtKind m_kind;
		string m_name;
	}
}
