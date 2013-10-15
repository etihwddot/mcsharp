using System.Windows;
using MCSharp.WorldBrowser.ViewModels;
using System;

namespace MCSharp.WorldBrowser.Views
{
	public partial class App : Application
	{
		public App()
		{
			m_app = new AppModel();
			m_app.MainWindowModelCreated += App_MainWindowModelCreated;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			m_app.Startup();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			if (m_app != null)
			{
				m_app.Dispose();
				m_app = null;
			}

			base.OnExit(e);
		}

		private void App_MainWindowModelCreated(object sender, EventArgs e)
		{
			CreateMainWindow();
		}

		private void CreateMainWindow()
		{
			MainWindow window = new MainWindow(m_app.MainWindowModel);
			window.Show();
			MainWindow = window;
		}

		AppModel m_app;
	}
}
