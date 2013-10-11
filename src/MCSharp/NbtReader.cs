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

			int nameLength = BinaryUtility.ConvertBigEndianToInt32(m_reader.ReadBytes(2));
			string name = Encoding.UTF8.GetString(m_reader.ReadBytes(nameLength));

			switch (kind)
			{
				case NbtKind.Byte:
					return ReadNbtByte(name);
				case NbtKind.Short:
					throw new NotImplementedException();
				case NbtKind.Int:
					return ReadNbtInt(name);
				case NbtKind.Long:
					return ReadNbtLong(name);
				case NbtKind.Float:
					throw new NotImplementedException();
				case NbtKind.Double:
					throw new NotImplementedException();
				case NbtKind.ByteArray:
					int byteCount = ReadInt();
					byte[] bytes = m_reader.ReadBytes(byteCount);
					return new NbtByteArray(name, bytes);
				case NbtKind.String:
					return null;
				case NbtKind.List:
					NbtKind itemKind = (NbtKind) m_reader.ReadByte();
					int itemCount = ReadInt();
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
					return ReadNbtIntArray(name);
				default:
					throw new InvalidOperationException();
			}
		}

		private int ReadInt()
		{
			byte[] bytes = m_reader.ReadBytes(4);
			return BinaryUtility.ConvertBigEndianToInt32(bytes);
		}

		private NbtByte ReadNbtByte(string name)
		{
			byte value = m_reader.ReadByte();
			return new NbtByte(name, value);
		}

		private NbtInt ReadNbtInt(string name)
		{
			int value = ReadInt();
			return new NbtInt(name, value);
		}

		private NbtLong ReadNbtLong(string name)
		{
			byte[] bytes = m_reader.ReadBytes(8);
			long value = BinaryUtility.ConvertBigEndianToInt64(bytes);
			return new NbtLong(name, value);
		}

		private NbtIntArray ReadNbtIntArray(string name)
		{

			int size = ReadInt();
			int[] values = new int[size];
			for (int index = 0; index < size; index++)
				values[index] = ReadInt();
			return new NbtIntArray(name, values);
		}

		BinaryReader m_reader;
	}
}
