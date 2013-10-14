using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MCSharp
{
	public sealed class GameSaveData
	{
		public static GameSaveData Load(string folder)
		{
			// load regions
			string regionFolderPath = Path.Combine(folder, c_regionFolderName);
			ReadOnlyCollection<RegionFile> regions = Directory.GetFiles(regionFolderPath, "*.mca")
				.Select(x => new RegionFile(x))
				.ToList().AsReadOnly();

			// calculate region
			WorldBounds bounds = new WorldBounds(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue);

			bounds = regions.Aggregate(bounds,
				(acc, r) => new WorldBounds(
					Math.Min(acc.Top, r.RegionZ),
					Math.Max(acc.Right, r.RegionX),
					Math.Max(acc.Bottom, r.RegionZ),
					Math.Min(acc.Left, r.RegionX)));
						
			return new GameSaveData(regions, bounds);
		}

		public ReadOnlyCollection<RegionFile> Regions
		{
			get { return m_regions; }
		}

		public WorldBounds Bounds
		{
			get { return m_bounds; }
		}

		private GameSaveData(ReadOnlyCollection<RegionFile> regions, WorldBounds bounds)
		{
			m_bounds = bounds;
			m_regions = regions;
		}

		const string c_regionFolderName = "region";
		const string c_regionExtension = "mca";

		WorldBounds m_bounds;
		ReadOnlyCollection<RegionFile> m_regions;
	}
}
