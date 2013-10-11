using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtInt : NbtValueBase<int>
	{
		public NbtInt(string name, int value)
			: base(NbtKind.Int, name, value)
		{
		}
	}
}
