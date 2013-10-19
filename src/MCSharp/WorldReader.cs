using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logos.Utility;
using MCSharp.NamedBinaryTag;

namespace MCSharp
{
	public sealed class WorldReader
	{
		public WorldReader(string saveFolder)
		{
			m_saveFolder = saveFolder;
		}

		public Task<GameSaveBounds> GetCurrentWorldBoundsAsync()
		{
			return Task.Run(() =>
			{
				var files = Directory.GetFiles(Path.Combine(m_saveFolder, c_regionFolderName), c_regionExtension);
				return files.Aggregate(GameSaveBounds.CreateDefault(), (bounds, file) =>
				{
					string regionFileName = Path.GetFileName(file);
					string[] regionFileNameParts = regionFileName.Split('.');
					int x = int.Parse(regionFileNameParts[1]);
					int z = int.Parse(regionFileNameParts[2]);
					return bounds.Union(x, z);
				});
			});
		}

		public Task<ChunkData> GetChunkForChunkCoordinateAsync(int chunkX, int chunkZ)
		{
			return Task.Run(() =>
			{
				using (var regionReader = GetRegionContainingChunk(chunkX, chunkZ))
					return regionReader.ReadChunkData(chunkX, chunkZ);
			});
		}

		private RegionReader GetRegionContainingChunk(int chunkX, int chunkZ)
		{
			int regionX = chunkX / Constants.RegionChunkWidth - (chunkX < 0 ? 1 : 0);
			int regionZ = chunkZ / Constants.RegionChunkWidth - (chunkZ < 0 ? 1 : 0);

			return new RegionReader(m_saveFolder, regionX, regionZ);
		}


		sealed class RegionReader : IDisposable
		{
			public RegionReader(string saveFolder, int regionX, int regionZ)
			{
				m_regionX = regionX;
				m_regionZ = regionZ;
				
				string regionFile = Path.Combine(saveFolder, c_regionFolderName, c_regionFileNameFormat.FormatInvariant(regionX, regionZ));
				if (File.Exists(regionFile))
				{
					Stream stream = File.OpenRead(regionFile);
					m_reader = new BinaryReader(stream);
				}
			}

			public ChunkData ReadChunkData(int chunkX, int chunkZ)
			{
				var regionRelativeChunkX = chunkX - m_regionX * Constants.RegionChunkWidth;
				var regionRelativeChunkZ = chunkZ - m_regionZ * Constants.RegionChunkWidth;

				if (m_reader == null)
					return ChunkData.Create(new ChunkInfo(regionRelativeChunkZ * Constants.RegionChunkWidth + regionRelativeChunkX, 0, 0), null);

				if (m_chunkDataLocations == null)
					ReadHeader();

				var chunkInfo = m_chunkDataLocations[regionRelativeChunkZ * Constants.RegionChunkWidth + regionRelativeChunkX];
				if (!chunkInfo.ChunkExists)
					return ChunkData.Create(chunkInfo, null);

				m_reader.BaseStream.Seek(chunkInfo.AbsoluteOffset, SeekOrigin.Begin);

				// get the size and type of the compressed chunk data
				int compressedSize = m_reader.ReadBigEndianInt32();
				NbtCompressionType compressionType = (NbtCompressionType) m_reader.ReadByte();
				byte[] compressedData = m_reader.ReadBytes(compressedSize - 1);

				NbtCompound root;
				using (MemoryStream memoryStream = new MemoryStream(compressedData))
				using (NbtReader nbtReader = new NbtReader(memoryStream, compressionType))
				{
					root = (NbtCompound) nbtReader.ReadTag();

					// verify we read all of the compressed data
					if (memoryStream.Position != memoryStream.Length)
						throw new InvalidOperationException();
				}

				return ChunkData.Create(chunkInfo, root);
			}

			public void Dispose()
			{
				if (m_reader != null)
					m_reader.Dispose();
			}

			private void ReadHeader()
			{
				m_chunkDataLocations = new ChunkInfo[Constants.ChunksPerRegion * Constants.ChunksPerRegion];
				for (int chunkIndex = 0; chunkIndex < Constants.ChunksPerRegion; chunkIndex++)
					m_chunkDataLocations[chunkIndex] = new ChunkInfo(chunkIndex, m_reader.ReadBigEndianInt32(3), m_reader.ReadByte());
			}

			int m_regionX;
			int m_regionZ;

			BinaryReader m_reader;
			ChunkInfo[] m_chunkDataLocations;
		}

		const string c_regionFolderName = "region";
		const string c_regionExtension = "*.mca";
		const string c_regionFileNameFormat = "r.{0}.{1}.mca";


		readonly string m_saveFolder;
	}
}
