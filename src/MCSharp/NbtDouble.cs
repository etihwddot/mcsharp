namespace MCSharp
{
	public sealed class NbtDouble : NbtValueBase<double>
	{
		public NbtDouble(string name, double value)
			: base(NbtKind.Double, name, value)
		{
		}
	}
}