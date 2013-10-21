using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCSharp.Utility;

namespace MCSharp
{
	public sealed class WorldSave
	{
		public static Task<WorldSave> LoadAsync(GameSaveInfo info)
		{
			return Task.Run(() =>
			{
				// load regions
				string regionFolderPath = Path.Combine(info.Location, c_regionFolderName);
				ReadOnlyCollection<RegionFile> regions = Directory.GetFiles(regionFolderPath, c_regionExtension)
					.Select(x => new RegionFile(x))
					.ToList().AsReadOnly();

				// calculate bounds
				Bounds bounds = null;
				foreach (RegionFile region in regions)
				{
					if (bounds == null)
					{
						bounds = region.Bounds;
						continue;
					}

					bounds = bounds.Union(region.Bounds);
				}
						
				return new WorldSave(regions, bounds);
			});
		}

		public ReadOnlyCollection<RegionFile> Regions
		{
			get { return m_regions; }
		}

		public Bounds Bounds
		{
			get { return m_bounds; }
		}

		private WorldSave(ReadOnlyCollection<RegionFile> regions, Bounds bounds)
		{
			m_bounds = bounds;
			m_regions = regions;
		}

		const string c_regionFolderName = "region";
		const string c_regionExtension = "*.mca";

		Bounds m_bounds;
		ReadOnlyCollection<RegionFile> m_regions;
	}
}
