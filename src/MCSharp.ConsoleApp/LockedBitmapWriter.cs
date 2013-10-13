using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MCSharp.ConsoleApp
{
	public sealed class LockedBitmapWriter : IDisposable
	{
		public LockedBitmapWriter(Bitmap bitmap)
		{
			m_bitmap = bitmap;

			//Lock Image
			m_lockedBitmap = m_bitmap.LockBits(new Rectangle(new Point(), m_bitmap.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			m_data = m_lockedBitmap.Scan0;
		}

		public void Dispose()
		{
			if (m_lockedBitmap != null)
			{
				m_bitmap.UnlockBits(m_lockedBitmap);
				m_lockedBitmap = null;
				m_data = IntPtr.Zero;
			}
		}

		public void SetPixel(int x, int y, Color color)
		{
			IntPtr currentPixel = new IntPtr(m_data.ToInt64() + y * (m_bitmap.Width * c_bytesPerPixel) + (x * c_bytesPerPixel));
			Marshal.Copy(new byte[] { color.B, color.G, color.R, color.A }, 0, currentPixel, c_bytesPerPixel);
		}
		
		const int c_bytesPerPixel = 4;

		Bitmap m_bitmap;
		BitmapData m_lockedBitmap;
		IntPtr m_data;
	}
}
