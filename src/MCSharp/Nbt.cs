using Logos.Utility;

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

		public override string ToString()
		{
			return "{0}({1})".FormatInvariant(Kind, Name);
		}

		NbtKind m_kind;
		string m_name;
	}
}
