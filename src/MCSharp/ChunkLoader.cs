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

		public static IEnumerable<ChunkData> LoadChunksInRegion(RegionFile regionFile)
		{
			using (FileStream stream = File.OpenRead(regionFile.FileName))
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
						yield return new ChunkData(chunk, null);
						continue;
					}

					// seek to the start of the data for the chunk
					stream.Seek(chunk.AbsoluteOffset, SeekOrigin.Begin);

					// get the size and type of the compressed chunk data
					int compressedSize = reader.ReadBigEndianInt32();
					ChunkCompressionType compressionType = (ChunkCompressionType) reader.ReadByte();
					byte[] compressedData = reader.ReadBytes(compressedSize - 1);

					NbtCompound root;
					using (MemoryStream memoryStream = new MemoryStream(compressedData))
					using (Stream chunkDataStream = GetDecompressionStream(compressionType, memoryStream))
					using (BinaryReader chunkDataReader = new BinaryReader(chunkDataStream))
					{
						NbtReader nbtReader = new NbtReader(chunkDataReader);

						root = (NbtCompound) nbtReader.ReadTag();

						// verify we read all of the compressed data
						if (memoryStream.Position != memoryStream.Length)
							throw new InvalidOperationException();
					}

					yield return new ChunkData(chunk, root);
				}
			}
		}

		private static Stream GetDecompressionStream(ChunkCompressionType type, Stream stream)
		{
			if (type == ChunkCompressionType.GZip)
				return new GZipStream(stream, CompressionMode.Decompress, true);

			// skip 2 bytes see http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
			byte[] dataFormat = stream.ReadExactly(2);
			if ((dataFormat[0] & 0xF) != 8)
				throw new InvalidOperationException();

			return new DeflateStream(stream, CompressionMode.Decompress, true);
		}

		string m_regionsPath;
	}
}
