﻿namespace MCSharp.Utility
{
	public struct Color32
	{
		public static Color32 FromArgb(byte alpha, byte red, byte green, byte blue)
		{
			return new Color32(alpha, red, green, blue);
		}

		public static Color32 FromRgb(byte red, byte green, byte blue)
		{
			return new Color32(0xFF, red, green, blue);
		}

		public static Color32 Blend(Color32 background, Color32 overlay)
		{
			// from http://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
			double alpha = overlay.Alpha / 256.0;

			byte blendedR = (byte) (overlay.Red * alpha + background.Red * (1 - alpha));
			byte blendedG = (byte) (overlay.Green * alpha + background.Green * (1 - alpha));
			byte blendedB = (byte) (overlay.Blue * alpha + background.Blue * (1 - alpha));
			return Color32.FromRgb(blendedR, blendedG, blendedB);
		}

		private Color32(byte alpha, byte red, byte green, byte blue)
		{
			m_alpha = alpha;
			m_red = red;
			m_green = green;
			m_blue = blue;
		}

		public byte Alpha
		{
			get { return m_alpha; }
		}

		public byte Red
		{
			get { return m_red; }
		}

		public byte Green
		{
			get { return m_green; }
		}

		public byte Blue
		{
			get { return m_blue; }
		}

		byte m_alpha;
		byte m_red;
		byte m_green;
		byte m_blue;
	}
}
