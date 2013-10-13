using System.IO;

namespace MCSharp
{
	public sealed class RegionFile
	{
		public RegionFile(string fileName)
		{
			m_fileName = fileName;

			string regionFileName = Path.GetFileName(fileName);
			string[] regionFileNameParts = regionFileName.Split('.');
			m_x = int.Parse(regionFileNameParts[1]);
			m_z = int.Parse(regionFileNameParts[2]);
		}

		public string FileName
		{
			get { return m_fileName; }
		}

		public int RegionX
		{
			get { return m_x; }
		}

		public int RegionZ
		{
			get { return m_z; }
		}

		readonly string m_fileName;
		readonly int m_x;
		readonly int m_z;
	}
}
