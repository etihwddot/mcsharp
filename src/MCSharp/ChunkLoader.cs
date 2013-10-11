using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Logos.Utility.IO;

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

		public static IEnumerable<ChunkData> LoadChunksInRegion(string regionFilePath)
		{
			using (FileStream stream = File.OpenRead(regionFilePath))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				// read chunk information from region header
				List<ChunkInfo> infos = new List<ChunkInfo>(Constants.ChunksPerRegion);
				for (int chunkIndex = 0; chunkIndex < Constants.ChunksPerRegion; chunkIndex++)
				{
					byte[] chunkInfo = reader.ReadBytes(4);
					infos.Add(new ChunkInfo(chunkIndex, BinaryUtility.ConvertBigEndianToInt32(chunkInfo, 0, 3), chunkInfo[3]));
				}

				// read chunk data
				foreach (ChunkInfo chunk in infos)
				{
					// return empty chunks
					if (!chunk.ChunkExists)
					{
						yield return ChunkData.Empty;
						continue;
					}

					// seek to the start of the data for the chunk
					stream.Seek(chunk.AbsoluteOffset, SeekOrigin.Begin);

					// get the size and type of the compressed chunk data
					int compressedSize = BinaryUtility.ConvertBigEndianToInt32(reader.ReadBytes(4), 0, 4);
					ChunkCompressionType compressionType = (ChunkCompressionType)reader.ReadByte();

					List<Nbt> tags = new List<Nbt>();
					using (Stream chunkDataStream = GetDecompressionStream(compressionType, stream))
					using (BinaryReader chunkDataReader = new BinaryReader(chunkDataStream))
					{
						NbtReader nbtReader = new NbtReader(chunkDataReader);

						Nbt tag;
						while (stream.Position < compressedSize)
							tags.Add(nbtReader.ReadTag());
					}

					yield return new ChunkData(tags);
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
