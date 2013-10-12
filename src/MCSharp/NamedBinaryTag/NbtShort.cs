namespace MCSharp.NamedBinaryTag
{
	public sealed class NbtShort : NbtValueBase<short>
	{
		public NbtShort(string name, short value)
			: base(NbtKind.Short, name, value)
		{
		}
	}
}
