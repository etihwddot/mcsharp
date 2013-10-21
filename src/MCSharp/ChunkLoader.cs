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
		public static ChunkLoader Create(string regionsPath)
		{
			if (regionsPath == null)
				throw new ArgumentNullException("regionsPath");

			return new ChunkLoader(regionsPath);
		}

		private ChunkLoader(string regionsPath)
		{
			m_regionsPath = regionsPath;
		}

		public static IEnumerable<Chunk> LoadChunksInRegion(RegionInfo regionFile)
		{
			using (Stream stream = File.OpenRead(regionFile.Location))
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
						yield return Chunk.Create(chunk, null);
						continue;
					}

					// seek to the start of the data for the chunk
					stream.Seek(chunk.AbsoluteOffset, SeekOrigin.Begin);

					// get the size and type of the compressed chunk data
					int compressedSize = reader.ReadBigEndianInt32();
					NbtCompressionType compressionType = (NbtCompressionType) reader.ReadByte();
					byte[] compressedData = reader.ReadBytes(compressedSize - 1);

					NbtCompound root;
					using (MemoryStream memoryStream = new MemoryStream(compressedData))
					using (NbtReader nbtReader = new NbtReader(memoryStream, compressionType))
					{
						root = (NbtCompound) nbtReader.ReadTag();

						// verify we read all of the compressed data
						if (memoryStream.Position != memoryStream.Length)
							throw new InvalidOperationException();
					}

					yield return Chunk.Create(chunk, root);
				}
			}
		}

		string m_regionsPath;
	}
}
