using System.IO;
using MCSharp.Utility;

namespace MCSharp
{
	public sealed class RegionFile
	{
		public RegionFile(string fileName)
		{
			m_fileName = fileName;

			string regionFileName = Path.GetFileName(fileName);
			string[] regionFileNameParts = regionFileName.Split('.');
			int x = int.Parse(regionFileNameParts[1]);
			int z = int.Parse(regionFileNameParts[2]);
			m_bounds = new Bounds(x, z, 1, 1);
		}

		public string FileName
		{
			get { return m_fileName; }
		}

		public Bounds Bounds
		{
			get { return m_bounds; }
		}

		readonly string m_fileName;
		readonly Bounds m_bounds;
	}
}
