using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Logos.Utility.IO;
using MCSharp.NamedBinaryTag;

namespace MCSharp
{
	public sealed class ChunkLoader
	{
		public static IEnumerable<ChunkLoader> LoadChunksInRegion(RegionFile regionFile)
		{
			using (Stream stream = File.OpenRead(regionFile.FileName))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				// read chunk information from region header
				List<ChunkInfo> infos = new List<ChunkInfo>(Constants.ChunksPerRegion);
				for (int chunkIndex = 0; chunkIndex < Constants.ChunksPerRegion; chunkIndex++)
					infos.Add(new ChunkInfo(chunkIndex, reader.ReadBigEndianInt32(3), reader.ReadByte()));

				// read chunk data
				foreach (ChunkInfo chunk in infos)
				{
					// return empty chunks
					if (!chunk.ChunkExists)
					{
						yield return new ChunkLoader(chunk);
						continue;
					}

					// seek to the start of the data for the chunk
					stream.Seek(chunk.AbsoluteOffset, SeekOrigin.Begin);

					// get the size and type of the compressed chunk data
					int compressedSize = reader.ReadBigEndianInt32();
					NbtCompressionType compressionType = (NbtCompressionType) reader.ReadByte();
					byte[] compressedData = reader.ReadBytes(compressedSize - 1);

					yield return new ChunkLoader(chunk, compressionType, compressedData);
				}
			}
		}

		public ChunkData ReadData()
		{
			if (m_compressedChunkData == null)
				return ChunkData.Create(m_chunkInfo, null);

			NbtCompound root;
			using (MemoryStream memoryStream = new MemoryStream(m_compressedChunkData))
			using (NbtReader nbtReader = new NbtReader(memoryStream, m_compressionType))
			{
				root = (NbtCompound)nbtReader.ReadTag();

				// verify we read all of the compressed data
				if (memoryStream.Position != memoryStream.Length)
					throw new InvalidOperationException();
			}

			return ChunkData.Create(m_chunkInfo, root);
		}

		private ChunkLoader(ChunkInfo info)
			: this(info, default(NbtCompressionType), null)
		{
		}

		private ChunkLoader(ChunkInfo info, NbtCompressionType compressionType, byte[] compressedChunkData)
		{
			m_chunkInfo = info;
			m_compressionType = compressionType;
			m_compressedChunkData = compressedChunkData;
		}

		readonly ChunkInfo m_chunkInfo;
		readonly NbtCompressionType m_compressionType;
		readonly byte[] m_compressedChunkData;
	}
}
