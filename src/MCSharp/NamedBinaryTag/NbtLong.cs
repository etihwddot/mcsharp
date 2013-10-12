namespace MCSharp.NamedBinaryTag
{
	public sealed class NbtLong : NbtValueBase<long>
	{
		public NbtLong(string name, long value)
			: base(NbtKind.Long, name, value)
		{
		}
	}
}
