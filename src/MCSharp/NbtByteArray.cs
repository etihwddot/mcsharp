using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtByteArray : Nbt
	{
		public NbtByteArray(string name, byte[] bytes)
			: base(NbtKind.ByteArray, name)
		{
			m_bytes = bytes;
		}

		// TODO: should not expose an array directly in the API
		public byte[] Bytes
		{
			get { return m_bytes; }
		}

		byte[] m_bytes;
	}
}
