using System;
using System.IO;
using MCSharp.Utility;

namespace MCSharp
{
	public sealed class RegionInfo
	{
		public RegionInfo(string location)
		{
			if (location == null)
				throw new ArgumentNullException("location");

			m_location = location;

			string regionFileName = Path.GetFileName(location);
			string[] regionFileNameParts = regionFileName.Split('.');
			int x = int.Parse(regionFileNameParts[1]);
			int z = int.Parse(regionFileNameParts[2]);
			m_bounds = new Bounds(x, z, 1, 1);
		}

		/// <summary>
		/// Path to the region file
		/// </summary>
		public string Location
		{
			get { return m_location; }
		}

		/// <summary>
		/// The bounds of the region in region units
		/// </summary>
		public Bounds Bounds
		{
			get { return m_bounds; }
		}

		readonly string m_location;
		readonly Bounds m_bounds;
	}
}
