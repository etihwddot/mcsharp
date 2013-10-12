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

			string name = ReadString();
			return ReadTagForKind(kind, name);
		}

		private Nbt ReadTagForKind(NbtKind kind, string name)
		{
			switch (kind)
			{
				case NbtKind.Byte:
					return ReadNbtByte(name);
				case NbtKind.Short:
					return ReadNbtShort(name);
				case NbtKind.Int:
					return ReadNbtInt(name);
				case NbtKind.Long:
					return ReadNbtLong(name);
				case NbtKind.Float:
					return ReadNbtFloat(name);
				case NbtKind.Double:
					return ReadNbtDouble(name);
				case NbtKind.ByteArray:
					int byteCount = ReadInt();
					byte[] bytes = m_reader.ReadBytes(byteCount);
					return new NbtByteArray(name, bytes);
				case NbtKind.String:
					return ReadNbtString(name);
				case NbtKind.List:
					NbtKind itemKind = (NbtKind) m_reader.ReadByte();
					int itemCount = ReadInt();
					List<Nbt> items = new List<Nbt>(itemCount);
					for (int index = 0; index < itemCount; index++)
						items.Add(ReadTagForKind(itemKind, ""));

					return new NbtList(name, itemKind, items);
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

		private short ReadShort()
		{
			byte[] bytes = m_reader.ReadBytes(2);
			return (short) BinaryUtility.ConvertBigEndianToInt32(bytes);
		}

		private int ReadInt()
		{
			byte[] bytes = m_reader.ReadBytes(4);
			return BinaryUtility.ConvertBigEndianToInt32(bytes);
		}

		string ReadString()
		{
			int length = ReadShort();
			return Encoding.UTF8.GetString(m_reader.ReadBytes(length));
		}

		private NbtByte ReadNbtByte(string name)
		{
			byte value = m_reader.ReadByte();
			return new NbtByte(name, value);
		}

		private NbtShort ReadNbtShort(string name)
		{
			short value = ReadShort();
			return new NbtShort(name, value);
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

		private NbtFloat ReadNbtFloat(string name)
		{
			byte[] bytes = m_reader.ReadBytes(4);
			float value = BinaryUtility.ConvertBigEndianToSingle(bytes);
			return new NbtFloat(name, value);
		}

		private NbtDouble ReadNbtDouble(string name)
		{
			byte[] bytes = m_reader.ReadBytes(8);
			double value = BinaryUtility.ConvertBigEndianToDouble(bytes);
			return new NbtDouble(name, value);
		}

		private NbtString ReadNbtString(string name)
		{
			string value = ReadString();
			return new NbtString(name, value);
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
