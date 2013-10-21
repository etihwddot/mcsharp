using System;

namespace MCSharp.Utility
{
	public class Bounds
	{
		public Bounds(int x, int z, int width, int height)
		{
			m_x = x;
			m_z = z;
			m_width = width;
			m_height = height;
		}

		public int X
		{
			get { return m_x; }
		}

		public int Z
		{
			get { return m_z; }
		}

		public int Width
		{
			get { return m_width; }
		}

		public int Height
		{
			get { return m_height; }
		}

		public Bounds Union(Bounds rect)
		{
			int minX = Math.Min(rect.X, m_x);
			int minZ = Math.Min(rect.Z, m_z);

			int maxX = Math.Max(rect.X + rect.m_width, m_x + m_width);
			int maxZ = Math.Max(rect.Z + rect.m_height, m_z + m_height);

			int width = maxX - minX;
			int height = maxZ - minZ;

			return new Bounds(minX, minZ, width, height);
		}
		
		int m_x;
		int m_z;
		int m_width;
		int m_height;
	}
}
