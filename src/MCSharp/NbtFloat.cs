namespace MCSharp
{
	public sealed class NbtFloat : NbtValueBase<float>
	{
		public NbtFloat(string name, float value)
			: base(NbtKind.Float, name, value)
		{
		}
	}
}
