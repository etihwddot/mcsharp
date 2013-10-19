using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public Task<ChunkData> GetChunkForChunkCoordinate(int chunkX, int chunkZ)
		{
			return Task.Run(() => ChunkData.Create(new ChunkInfo(0, 0, 0), null));
		}

		public Task<ChunkData> GetChunkForWorldCoordiate(int worldX, int worldZ)
		{
			return Task.Run(() => ChunkData.Create(new ChunkInfo(0, 0, 0), null));
		}

		const string c_regionFolderName = "region";
		const string c_regionExtension = "*.mca";

		readonly string m_saveFolder;
	}
}
