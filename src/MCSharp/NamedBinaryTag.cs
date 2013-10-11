using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public abstract class NamedBinaryTag
	{
		public NamedBinaryTagKind Kind
		{
			get { return m_kind; }
		}

		public string Name
		{
			get { return m_name; }
		}

		protected NamedBinaryTag(NamedBinaryTagKind kind, string name)
		{
			m_kind = kind;
			m_name = name;
		}

		NamedBinaryTagKind m_kind;
		string m_name;
	}
}
