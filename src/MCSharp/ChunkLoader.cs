using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logos.Utility;

namespace MCSharp
{
	public sealed class ChunkLoader
	{
		public static ChunkLoader Create(string regionsPath)
		{
			if (regionsPath == null)
				throw new ArgumentNullException("regionsPath");

			return new ChunkLoader(regionsPath);
		}

		private ChunkLoader(string regionsPath)
		{
			m_regionsPath = regionsPath;
		}

		string m_regionsPath;
	}
}
