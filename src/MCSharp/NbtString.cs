namespace MCSharp
{
	public sealed class NbtString : NbtValueBase<string>
	{
		public NbtString(string name, string value)
			: base(NbtKind.String, name, value)
		{
		}
	}
}
