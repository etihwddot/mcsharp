using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Logos.Utility;
using MCSharp.Utility;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class MainWindowModel : ViewModel, IRenderTarget
	{
		public MainWindowModel()
		{
			m_log = new StringBuilder();
			m_availableSaves = GameSaveInfo.GetAvailableSaves().ToList().AsReadOnly();
			SelectedSave = m_availableSaves.FirstOrDefault();
			
			m_availableRenderers = new List<IRenderer>
			{
				new BasicBiomeRenderer(),
				new HillshadeRenderer(),
			}.AsReadOnly();
			SelectedRenderer = m_availableRenderers.FirstOrDefault();
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

		public ReadOnlyCollection<IRenderer> AvailableRenderers
		{
			get { return m_availableRenderers; }
		}

		public static readonly string SelectedRendererProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.SelectedRenderer);
		public IRenderer SelectedRenderer
		{
			get
			{
				VerifyAccess();
				return m_selectedRenderer;
			}
			set
			{
				VerifyAccess();
				if (value != m_selectedRenderer)
				{
					m_selectedRenderer = value;
					RaisePropertyChanged(SelectedRendererProperty);
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

		public static readonly string LogProperty = ExpressionUtility.GetPropertyName((MainWindowModel x) => x.Log);
		public string Log
		{
			get
			{
				VerifyAccess();
				return m_log.ToString();
			}
		}

		private void WriteLine(string text)
		{
			m_log.AppendLine(text);
			RaisePropertyChanged(LogProperty);
		}

		private void WriteLine(string format, params object[] args)
		{
			m_log.AppendFormat(format, args);
			m_log.AppendLine();
			RaisePropertyChanged(LogProperty);
		}

		private void GenerateImage()
		{
			if (m_selectedSave == null || m_selectedRenderer == null)
				return;

			// cancel existing work
			if (m_source != null)
				m_source.Cancel();

			// TODO: determine if something like this is necessary
			if (m_imageTask != null && m_imageTask.Status == TaskStatus.Running)
				m_imageTask.Wait();

			m_source = new CancellationTokenSource();
			m_imageTask = DoGenerateImage(m_source.Token);
		}

		private async Task DoGenerateImage(CancellationToken token)
		{
			// Time the generate process
			Stopwatch stopwatch = Stopwatch.StartNew();

			// load the save
			WriteLine("Loading: {0}", m_selectedSave.Name);
			WorldSave save = await WorldSave.LoadAsync(m_selectedSave);

			PixelSize size = await m_selectedRenderer.GetRenderSizeAsync(save, token);

			m_image = new WriteableBitmap(size.Width, size.Height, c_imageDpi, c_imageDpi, s_pixelFormat, null);
			RaisePropertyChanged(ImageProperty);

			await m_selectedRenderer.RenderAsync(save, this, token);

			WriteLine("Total time: {0}", stopwatch.Elapsed);
		}

		public void WritePixels(int x, int y, int width, int height, ColorBgra32[] pixels)
		{
			if (m_image == null)
				return;

			if (pixels.Length != width * height)
				throw new InvalidOperationException("The number of pixels needs to be the same as the height times width.");

			Int32Rect invalidArea = new Int32Rect(x, y, width, height);
			
			m_image.WritePixels(invalidArea, pixels, width * s_bytesPerPixel, 0);
		}

		static readonly PixelFormat s_pixelFormat = PixelFormats.Bgra32;
		static readonly int s_bytesPerPixel = s_pixelFormat.BitsPerPixel / 8;

		const int c_imageDpi = 96;

		readonly StringBuilder m_log;

		CancellationTokenSource m_source;
		GameSaveInfo m_selectedSave;
		ReadOnlyCollection<GameSaveInfo> m_availableSaves;
		ReadOnlyCollection<IRenderer> m_availableRenderers;
		WriteableBitmap m_image;
		Task m_imageTask;
		private IRenderer m_selectedRenderer;
	}
}
