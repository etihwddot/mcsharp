using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
			m_queue = new ConcurrentQueue<PixelData>();

			// create background rendering thread
			m_backgroundThread = new Thread(RenderBagWork);
			m_backgroundThread.Start();
		}

		public void SetPixel(int x, int y, ColorBgra32 color)
		{
			m_queue.Enqueue(new PixelData { x = x, y = y, Color = color });
		}
		
		private void RenderBagWork()
		{
			IntPtr buffer = IntPtr.Zero;
			SpinWait waiter = new SpinWait();
			int pixelWrites = 0;
			Rect rect = Rect.Empty;

			while (true)
			{
				// try to get the pixel data
				PixelData data;
				if (!m_queue.TryDequeue(out data))
				{
					if (m_completed)
					{
						m_image.Dispatcher.Invoke(() =>
						{
							m_image.AddDirtyRect(new Int32Rect((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height));
							m_image.Unlock();
							buffer = IntPtr.Zero;
							rect = Rect.Empty;
						});
						return;
					}

					waiter.SpinOnce();
					continue;
				}

				// lock the buffer if it isn't
				if (buffer == IntPtr.Zero)
				{
					m_image.Dispatcher.Invoke(() =>
					{
						m_image.Lock();
						buffer = m_image.BackBuffer;
					});
				}

				if (rect == Rect.Empty)
					rect = new Rect(data.x, data.y, 1, 1);
				else
					rect.Union(new Rect(data.x, data.y, 1, 1));

				// write the pixel
				IntPtr currentPixel = new IntPtr(buffer.ToInt64() + data.y * (m_width * m_bytesPerPixel) + (data.x * m_bytesPerPixel));
				Marshal.Copy(new byte[] { data.Color.Blue, data.Color.Green, data.Color.Red, data.Color.Alpha }, 0, currentPixel, m_bytesPerPixel);
				pixelWrites++;

				if (pixelWrites > 200000)
				{
					m_image.Dispatcher.Invoke(() =>
					{
						m_image.AddDirtyRect(new Int32Rect((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height));
						m_image.Unlock();
						buffer = IntPtr.Zero;
						rect = Rect.Empty;
					});

					pixelWrites = 0;
				}
			}
		}

		public void Dispose()
		{
			m_completed = true;
		}

		private struct PixelData
		{
			public int x;
			public int y;
			public ColorBgra32 Color;
		}

		readonly WriteableBitmap m_image;
		readonly int m_width;
		readonly int m_bytesPerPixel;

		ConcurrentQueue<PixelData> m_queue;
		Thread m_backgroundThread;
		bool m_completed;
	}
}
