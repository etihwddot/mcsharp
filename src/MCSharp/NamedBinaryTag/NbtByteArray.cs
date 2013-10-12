namespace MCSharp.NamedBinaryTag
{
	public sealed class NbtByteArray : Nbt
	{
		public NbtByteArray(string name, sbyte[] bytes)
			: base(NbtKind.ByteArray, name)
		{
			m_bytes = bytes;
		}

		// TODO: should not expose an array directly in the API
		public sbyte[] Bytes
		{
			get { return m_bytes; }
		}

		sbyte[] m_bytes;
	}
}
