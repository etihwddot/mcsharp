using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MCSharp
{
	public sealed class GameSave
	{
		public static GameSave Load(GameSaveInfo info)
		{
			// load regions
			string regionFolderPath = Path.Combine(info.Location, c_regionFolderName);
			ReadOnlyCollection<RegionFile> regions = Directory.GetFiles(regionFolderPath, c_regionExtension)
				.Select(x => new RegionFile(x))
				.ToList().AsReadOnly();

			// calculate region
			GameSaveBounds bounds = new GameSaveBounds(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue);

			bounds = regions.Aggregate(bounds,
				(acc, r) => new GameSaveBounds(
					Math.Min(acc.MinZ, r.RegionZ),
					Math.Max(acc.MaxX, r.RegionX),
					Math.Max(acc.MaxZ, r.RegionZ),
					Math.Min(acc.MinX, r.RegionX)));
						
			return new GameSave(regions, bounds);
		}

		public ReadOnlyCollection<RegionFile> Regions
		{
			get { return m_regions; }
		}

		public GameSaveBounds Bounds
		{
			get { return m_bounds; }
		}

		private GameSave(ReadOnlyCollection<RegionFile> regions, GameSaveBounds bounds)
		{
			m_bounds = bounds;
			m_regions = regions;
		}

		const string c_regionFolderName = "region";
		const string c_regionExtension = "*.mca";

		GameSaveBounds m_bounds;
		ReadOnlyCollection<RegionFile> m_regions;
	}
}
