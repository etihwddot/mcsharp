using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp.Utility
{
	public static class BiomeKindUtility
	{
		public static Color32 GetColor(BiomeKind biome)
		{
			switch (biome)
			{
			case BiomeKind.Uncalculated: return NamedColors.Black;
			case BiomeKind.DeepOcean: return NamedColors.DarkBlue;
			case BiomeKind.Ocean: return NamedColors.Blue;
			case BiomeKind.River: return NamedColors.LightBlue;
			case BiomeKind.Beach: return NamedColors.LightYellow;

			case BiomeKind.SunflowerPlains:
				return s_random.Next(31) < 30 ? NamedColors.Green : NamedColors.Yellow;

			case BiomeKind.Plains: return NamedColors.Green;
			case BiomeKind.Forest: return NamedColors.DarkGreen;
			case BiomeKind.ForestHills: return NamedColors.DarkGreen;
			case BiomeKind.ExtremeHills: return NamedColors.DarkGray;
			case BiomeKind.ExtremeHillsEdge: return NamedColors.LightGray;

			// ExtremeHills+ has trees; randomly add some green pixels
			case BiomeKind.ExtremeHillsPlus:
				return s_random.Next(11) < 10 ? NamedColors.DarkGray : NamedColors.ForestGreen;

			case BiomeKind.Swampland: return NamedColors.DarkOliveGreen;
			case BiomeKind.Jungle: return Color32.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.JungleHills: return Color32.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.Desert: return Color32.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.DesertHills: return Color32.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.ColdTaiga: return NamedColors.White;
			case BiomeKind.ColdTaigaHills: return NamedColors.WhiteSmoke;
			case BiomeKind.Taiga: return NamedColors.ForestGreen;
			case BiomeKind.TaigaHills: return NamedColors.ForestGreen;
			case BiomeKind.IcePlains: return NamedColors.White;
			case BiomeKind.IceMountains: return NamedColors.White;
			case BiomeKind.FrozenRiver: return NamedColors.AntiqueWhite;
			case BiomeKind.FrozenOcean: return NamedColors.CornflowerBlue;
			case BiomeKind.ColdBeach: return NamedColors.Beige;
			case BiomeKind.StoneBeach: return NamedColors.DarkGray;

			// RoofedForest has mushrooms; randomly add some red and tan pixels
			case BiomeKind.RoofedForest:
				int value = s_random.Next(100);
				return value < 98 ? NamedColors.ForestGreen : value < 99 ? NamedColors.Tan : NamedColors.Red;
			default: return NamedColors.Red;
			}
		}

		static readonly Random s_random = new Random();
	}
}
