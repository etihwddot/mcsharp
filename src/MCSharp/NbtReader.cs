using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MCSharp.NamedBinaryTag;

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
					return ReadNbtByteArray(name);
				case NbtKind.String:
					return ReadNbtString(name);
				case NbtKind.List:
					return ReadNbtList(name);
				case NbtKind.Compound:
					return ReadNbtCompound(name);
				case NbtKind.IntArray:
					return ReadNbtIntArray(name);
				default:
					throw new InvalidOperationException();
			}
		}

		string ReadString()
		{
			int length = m_reader.ReadBigEndianInt32(2);
			return Encoding.UTF8.GetString(m_reader.ReadBytes(length));
		}

		private NbtByte ReadNbtByte(string name)
		{
			return new NbtByte(name, m_reader.ReadSByte());
		}

		private NbtShort ReadNbtShort(string name)
		{
			return new NbtShort(name, (short) m_reader.ReadBigEndianInt32(2));
		}

		private NbtInt ReadNbtInt(string name)
		{
			return new NbtInt(name, m_reader.ReadBigEndianInt32());
		}

		private NbtLong ReadNbtLong(string name)
		{
			return new NbtLong(name, m_reader.ReadBigEndianInt64());
		}

		private NbtFloat ReadNbtFloat(string name)
		{
			return new NbtFloat(name, m_reader.ReadBigEndianSingle());
		}

		private NbtDouble ReadNbtDouble(string name)
		{
			return new NbtDouble(name, m_reader.ReadBigEndianDouble());
		}

		private NbtByteArray ReadNbtByteArray(string name)
		{
			int byteCount = m_reader.ReadBigEndianInt32();
			
			// copy the bytes from the stream to a signed byte array
			sbyte[] bytes = new sbyte[byteCount];
			Buffer.BlockCopy(m_reader.ReadBytes(byteCount), 0, bytes, 0, byteCount);

			return new NbtByteArray(name, bytes);
		}

		private NbtString ReadNbtString(string name)
		{
			return new NbtString(name, ReadString());
		}

		private NbtList ReadNbtList(string name)
		{
			NbtKind itemKind = (NbtKind) m_reader.ReadByte();
			int itemCount = m_reader.ReadBigEndianInt32();
			
			List<Nbt> items = new List<Nbt>(itemCount);
			for (int index = 0; index < itemCount; index++)
				items.Add(ReadTagForKind(itemKind, ""));

			return new NbtList(name, itemKind, items);
		}

		private NbtCompound ReadNbtCompound(string name)
		{
			List<Nbt> tags = new List<Nbt>();
			Nbt currentTag;
			while ((currentTag = ReadTag()).Kind != NbtKind.End)
				tags.Add(currentTag);

			return new NbtCompound(name, tags);
		}

		private NbtIntArray ReadNbtIntArray(string name)
		{
			int size = m_reader.ReadBigEndianInt32();

			int[] values = new int[size];
			for (int index = 0; index < size; index++)
				values[index] = m_reader.ReadBigEndianInt32();

			return new NbtIntArray(name, values);
		}

		BinaryReader m_reader;
	}
}
