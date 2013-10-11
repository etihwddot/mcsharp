﻿using System;
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
					int byteCount = BinaryUtility.ConvertBigEndianToInt32(m_reader.ReadBytes(4), 0, 4);
					byte[] bytes = m_reader.ReadBytes(byteCount);
					return new NbtByteArray(name, bytes);
				case NbtKind.String:
					return null;
				case NbtKind.List:
					NbtKind itemKind = (NbtKind) m_reader.ReadByte();
					int itemCount = BinaryUtility.ConvertBigEndianToInt32(m_reader.ReadBytes(4), 0, 4);
					List<Nbt> items = new List<Nbt>();
					// TODO: read payload data
					return new NbtList(name, items);
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
