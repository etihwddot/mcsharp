using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public abstract class NbtValueBase<T> : Nbt
	{
		protected NbtValueBase(NbtKind kind, string name, T value)
			: base(kind, name)
		{
		}

		public T Value
		{
			get { return m_value; }
		}

		T m_value;
	}
}
