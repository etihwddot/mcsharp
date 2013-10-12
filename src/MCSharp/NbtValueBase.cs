namespace MCSharp
{
	public abstract class NbtValueBase<T> : Nbt
	{
		protected NbtValueBase(NbtKind kind, string name, T value)
			: base(kind, name)
		{
			m_value = value;
		}

		public T Value
		{
			get { return m_value; }
		}

		readonly T m_value;
	}
}
