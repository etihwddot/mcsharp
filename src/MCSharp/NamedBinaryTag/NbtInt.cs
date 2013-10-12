namespace MCSharp.NamedBinaryTag
{
	public sealed class NbtInt : NbtValueBase<int>
	{
		public NbtInt(string name, int value)
			: base(NbtKind.Int, name, value)
		{
		}
	}
}
