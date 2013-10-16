using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MCSharp.NamedBinaryTag;

namespace MCSharp
{
	public sealed class GameSaveInfo
	{
		public static IEnumerable<GameSaveInfo> GetAvailableSaves()
		{
			string savesPath = Path.Combine(Constants.MinecraftPath, "saves");

			foreach (string saveFolder in Directory.GetDirectories(savesPath))
			{
				// see http://minecraft.gamepedia.com/Level_format#level.dat_format
				string levelDataPath = Path.Combine(saveFolder, "level.dat");

				using (FileStream stream = File.OpenRead(levelDataPath))
				using (NbtReader nbtReader = new NbtReader(stream))
				{
					NbtCompound root = (NbtCompound) nbtReader.ReadTag();
					NbtCompound data = (NbtCompound) root.Tags.Single();
					string name = data.Tags.OfType<NbtString>().First(x => x.Name == "LevelName").Value;
					yield return new GameSaveInfo(name, saveFolder);
				}
			}
		}

		public string Name
		{
			get { return m_name; }
		}

		public string FolderName
		{
			get { return Path.GetFileName(m_location); }
		}

		public string Location
		{
			get { return m_location; }
		}

		private GameSaveInfo(string name, string location)
		{
			m_name = name;
			m_location = location;
		}

		string m_name;
		string m_location;
	}
}
