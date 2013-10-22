using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class WriteableBitmapRenderTarget : IRenderTarget, IDisposable
	{
		public WriteableBitmapRenderTarget(WriteableBitmap image)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			m_image = image;
			m_width = m_image.PixelWidth;
			m_bytesPerPixel = m_image.Format.BitsPerPixel / 8;
			m_lock = new object();

			m_image.Lock();
			m_buffer = m_image.BackBuffer;
		}

		public void SetPixel(int x, int y, ColorBgra32 color)
		{
			lock (m_lock)
			{
				if (m_pixelWrites > 4096)
				{
					m_image.Dispatcher.Invoke(() =>
					{
						m_image.AddDirtyRect(new Int32Rect(0, 0, m_image.PixelWidth, m_image.PixelHeight));
						m_image.Unlock();
					});

					m_image.Dispatcher.Invoke(() =>
					{
						m_image.Lock();
						m_buffer = m_image.BackBuffer;
						m_pixelWrites = 0;
					});
				}
			}

			// write the pixel
			IntPtr currentPixel = new IntPtr(m_buffer.ToInt64() + y * (m_width * m_bytesPerPixel) + (x * m_bytesPerPixel));
			Marshal.Copy(new byte[] { color.Blue, color.Green, color.Red, color.Alpha }, 0, currentPixel, m_bytesPerPixel);
			Interlocked.Increment(ref m_pixelWrites);
		}

		public void Dispose()
		{
			m_image.AddDirtyRect(new Int32Rect(0, 0, m_image.PixelWidth, m_image.PixelHeight));
			m_image.Unlock();
		}

		readonly WriteableBitmap m_image;
		readonly int m_width;
		readonly int m_bytesPerPixel;

		IntPtr m_buffer;
		object m_lock;
		int m_pixelWrites;
	}
}
