﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MCSharp.NamedBinaryTag;

namespace MCSharp
{
	public sealed class ChunkData
	{
		public static ChunkData Create(ChunkInfo info, NbtCompound root)
		{
			var chunk = new ChunkData(info, root);
			chunk.Initialize();
			return chunk;
		}

		private ChunkData(ChunkInfo info, NbtCompound root)
		{
			m_info = info;
			m_root = root;
		}

		public NbtCompound Root
		{
			get { return m_root; }
		}

		public ChunkInfo Info
		{
			get { return m_info; }
		}

		public int? XPosition
		{
			get { return m_xPosition; }
		}

		public int? ZPosition
		{
			get { return m_zPosition; }
		}

		public BiomeKind GetBiome(int chunkX, int chunkZ)
		{
			int index = chunkZ * Constants.ChunkSize + chunkX;
			if (m_biomes == null)
				return BiomeKind.Uncalculated;

			return (BiomeKind) m_biomes.Bytes[index];
		}

		private void Initialize()
		{
			if (m_root == null)
				return;

			m_level = (NbtCompound) m_root.Tags.Single();

			foreach (Nbt tag in m_level.Tags)
			{
				switch (tag.Name)
				{
				case "Biomes":
					m_biomes = (NbtByteArray) tag;
					break;
				case "xPos":
					m_xPosition = ((NbtInt) tag).Value;
					break;
				case "zPos":
					m_zPosition = ((NbtInt) tag).Value;
					break;
				}
			}
		}

		public bool IsEmpty
		{
			get { return m_root == null; }
		}

		NbtCompound m_root;
		NbtCompound m_level;
		ChunkInfo m_info;
		NbtByteArray m_biomes;
		int? m_xPosition;
		int? m_zPosition;
	}
}
