using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtByte : NbtValueBase<sbyte>
	{
		public NbtByte(string name, sbyte value)
			: base(NbtKind.Byte, name, value)
		{
		}
	}
}
