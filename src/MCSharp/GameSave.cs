using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MCSharp
{
	public sealed class GameSave
	{
		public static GameSave Load(GameSaveInfo info)
		{
			string regionFolderPath = Path.Combine(info.Location, c_regionFolderName);
			return new GameSave(regionFolderPath);
		}

		public IEnumerable<RegionFile> Regions
		{
			get
			{
				return Directory.EnumerateFiles(m_folderPath, c_regionExtension)
					.Select(x => new RegionFile(x));
			}
		}

		public GameSaveBounds Bounds
		{
			get { return m_bounds.Value; }
		}

		private GameSave(string folderPath)
		{
			m_folderPath = folderPath;

			m_bounds = new Lazy<GameSaveBounds>(() =>
			{
				// TODO: this will not work when we are doing live mapping
				GameSaveBounds bounds = new GameSaveBounds(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue);

				bounds = Regions.Aggregate(bounds,
					(acc, r) => new GameSaveBounds(
						Math.Min(acc.MinZ, r.RegionZ),
						Math.Max(acc.MaxX, r.RegionX),
						Math.Max(acc.MaxZ, r.RegionZ),
						Math.Min(acc.MinX, r.RegionX)));
				
				return bounds;
			});
		}

		const string c_regionFolderName = "region";
		const string c_regionExtension = "*.mca";

		Lazy<GameSaveBounds> m_bounds;
		string m_folderPath;
	}
}
