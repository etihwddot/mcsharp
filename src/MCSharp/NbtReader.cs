using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp
{
	public sealed class NbtReader
	{
		public NbtReader(BinaryReader reader)
		{
			m_reader = reader;
		}

		public Nbt ReadTag()
		{
			NbtKind kind = (NbtKind) m_reader.ReadByte();
			if (kind == NbtKind.End)
				return new NbtEnd();

			int nameLength = BinaryUtility.ConvertBigEndianToInt32(m_reader.ReadBytes(2), 0, 2);
			string name = Encoding.UTF8.GetString(m_reader.ReadBytes(nameLength));

			switch (kind)
			{
				case NbtKind.Byte:
					return null;
				case NbtKind.Short:
					return null;
				case NbtKind.Int:
					return null;
				case NbtKind.Long:
					return null;
				case NbtKind.Float:
					return null;
				case NbtKind.Double:
					return null;
				case NbtKind.ByteArray:
					return null;
				case NbtKind.String:
					return null;
				case NbtKind.List:
					return null;
				case NbtKind.Compound:
					List<Nbt> tags = new List<Nbt>();
					Nbt currentTag;
					while ((currentTag = ReadTag()).Kind != NbtKind.End)
						tags.Add(currentTag);

					return new NbtCompound(name, tags);
				case NbtKind.IntArray:
					return null;
				default:
					throw new InvalidOperationException();
			}
		}

		BinaryReader m_reader;
	}
}
