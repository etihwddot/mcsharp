using System.Runtime.InteropServices;

namespace MCSharp.Utility
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ColorBgra32
	{
		public static ColorBgra32 FromArgb(byte alpha, byte red, byte green, byte blue)
		{
			return new ColorBgra32(alpha, red, green, blue);
		}

		public static ColorBgra32 FromRgb(byte red, byte green, byte blue)
		{
			return new ColorBgra32(0xFF, red, green, blue);
		}

		public static ColorBgra32 Blend(ColorBgra32 background, ColorBgra32 overlay)
		{
			// from http://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
			double alpha = overlay.Alpha / 256.0;

			byte blendedR = (byte) (overlay.Red * alpha + background.Red * (1 - alpha));
			byte blendedG = (byte) (overlay.Green * alpha + background.Green * (1 - alpha));
			byte blendedB = (byte) (overlay.Blue * alpha + background.Blue * (1 - alpha));
			return ColorBgra32.FromRgb(blendedR, blendedG, blendedB);
		}

		private ColorBgra32(byte alpha, byte red, byte green, byte blue)
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

		[FieldOffset(0)]
		byte m_blue;

		[FieldOffset(1)]
		byte m_green;

		[FieldOffset(2)]
		byte m_red;

		[FieldOffset(3)]
		byte m_alpha;
	}
}
