namespace MCSharp.NamedBinaryTag
{
	public sealed class NbtByte : NbtValueBase<sbyte>
	{
		public NbtByte(string name, sbyte value)
			: base(NbtKind.Byte, name, value)
		{
		}
	}
}
