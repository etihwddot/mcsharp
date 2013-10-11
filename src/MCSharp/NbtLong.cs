using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtLong : NbtValueBase<long>
	{
		public NbtLong(string name, long value)
			: base(NbtKind.Long, name, value)
		{
		}
	}
}
