using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Logos.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class MainWindowModel : ViewModel
	{
		public MainWindowModel()
		{
			m_availableSaves = GameSaveInfo.GetAvailableSaves().ToList().AsReadOnly();
			SelectedSave = m_availableSaves.FirstOrDefault();	
		}

		public ReadOnlyCollection<GameSaveInfo> AvailableSaves
		{
			get { return m_availableSaves; }
		}

		public static readonly string SelectedSaveProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.SelectedSave);
		public GameSaveInfo SelectedSave
		{
			get
			{
				VerifyAccess();
				return m_selectedSave;
			}
			set
			{
				VerifyAccess();
				if (value != m_selectedSave)
				{
					m_selectedSave = value;
					RaisePropertyChanged(SelectedSaveProperty);
					GenerateImage();
				}
			}
		}

		public static readonly string ImageProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.Image);
		public ImageSource Image
		{
			get
			{
				VerifyAccess();
				return m_image;
			}
		}

		private void GenerateImage()
		{
			if (m_selectedSave == null)
				return;

			GameSave save = GameSave.Load(m_selectedSave);

			m_image = new WriteableBitmap(save.Bounds.BlockWidth, save.Bounds.BlockHeight, c_imageDpi, c_imageDpi, s_pixelFormat, null);
			RaisePropertyChanged(ImageProperty);
			
			foreach (RegionFile region in save.Regions)
			{
				IEnumerable<ChunkData> regionChunks = ChunkLoader.LoadChunksInRegion(region);

				foreach (ChunkData chunk in regionChunks.Where(x => !x.IsEmpty))
				{
					int xOffset = (chunk.XPosition.Value * Constants.ChunkBlockWidth) - save.Bounds.MinXBlock;
					int zOffset = (chunk.ZPosition.Value * Constants.ChunkBlockWidth) - save.Bounds.MinZBlock;

					for (int x = 0; x < Constants.ChunkBlockWidth; x++)
					{
						int? lastHeight = null;
						for (int z = 0; z < Constants.ChunkBlockWidth; z++)
						{
							int imageX = x + xOffset;
							int imageY = z + zOffset;

							BiomeKind biome = chunk.GetBiome(x, z);

							int height = chunk.GetHeight(x, z);

							Color color = GetColorForBiomeKind(biome);

							if (lastHeight.HasValue && height > lastHeight)
								color = BlendWith(color, Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
							if (lastHeight.HasValue && height < lastHeight)
								color = BlendWith(color, Color.FromArgb(0x50, 0x00, 0x00, 0x00));

							m_image.WritePixels(new Int32Rect(imageX, imageY, 1, 1), new byte[] { color.B, color.G, color.R, color.A }, 4, 0);
							lastHeight = height;
						}
					}
				}
			}
		}

		private static Color BlendWith(Color background, Color overlay)
		{
			// from http://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
			double alpha = overlay.A / 256.0;

			byte blendedR = (byte) (overlay.R * alpha + background.R * (1 - alpha));
			byte blendedG = (byte) (overlay.G * alpha + background.G * (1 - alpha));
			byte blendedB = (byte) (overlay.B * alpha + background.B * (1 - alpha));
			return Color.FromRgb(blendedR, blendedG, blendedB);
		}

		private static Color GetColorForBiomeKind(BiomeKind biome)
		{
			switch (biome)
			{
			case BiomeKind.Uncalculated: return Colors.Black;
			case BiomeKind.DeepOcean: return Colors.DarkBlue;
			case BiomeKind.Ocean: return Colors.Blue;
			case BiomeKind.River: return Colors.LightBlue;
			case BiomeKind.Beach: return Colors.LightYellow;

			case BiomeKind.SunflowerPlains:
				return s_random.Next(31) < 30 ? Colors.Green : Colors.Yellow;

			case BiomeKind.Plains: return Colors.Green;
			case BiomeKind.Forest: return Colors.DarkGreen;
			case BiomeKind.ForestHills: return Colors.DarkGreen;
			case BiomeKind.ExtremeHills: return Colors.DarkGray;
			case BiomeKind.ExtremeHillsEdge: return Colors.LightGray;

			// ExtremeHills+ has trees; randomly add some green pixels
			case BiomeKind.ExtremeHillsPlus:
				return s_random.Next(11) < 10 ? Colors.DarkGray : Colors.ForestGreen;

			case BiomeKind.Swampland: return Colors.DarkOliveGreen;
			case BiomeKind.Jungle: return Color.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.JungleHills: return Color.FromRgb(0x0D, 0x35, 0x01);
			case BiomeKind.Desert: return Color.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.DesertHills: return Color.FromRgb(0xDB, 0xD3, 0xA0);
			case BiomeKind.ColdTaiga: return Colors.White;
			case BiomeKind.ColdTaigaHills: return Colors.WhiteSmoke;
			case BiomeKind.Taiga: return Colors.ForestGreen;
			case BiomeKind.TaigaHills: return Colors.ForestGreen;
			case BiomeKind.IcePlains: return Colors.White;
			case BiomeKind.IceMountains: return Colors.White;
			case BiomeKind.FrozenRiver: return Colors.AntiqueWhite;
			case BiomeKind.FrozenOcean: return Colors.CornflowerBlue;
			case BiomeKind.ColdBeach: return Colors.Beige;
			case BiomeKind.StoneBeach: return Colors.DarkGray;

			// RoofedForest has mushrooms; randomly add some red and tan pixels
			case BiomeKind.RoofedForest:
				int value = s_random.Next(100);
				return value < 98 ? Colors.ForestGreen : value < 99 ? Colors.Tan : Colors.Red;
			default: return Colors.Red;
			}
		}

		static readonly Random s_random = new Random();
		static readonly PixelFormat s_pixelFormat = PixelFormats.Bgra32;
		const int c_imageDpi = 96;

		GameSaveInfo m_selectedSave;
		ReadOnlyCollection<GameSaveInfo> m_availableSaves;
		WriteableBitmap m_image;
	}
}
