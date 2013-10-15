using System;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class AppModel : IDisposable
	{
		public MainWindowModel MainWindowModel
		{ 
			get { return m_mainWindowModel; }
		}

		public event EventHandler MainWindowModelCreated;

		public void Startup()
		{
			m_mainWindowModel = new MainWindowModel();
			EventHandler handler = MainWindowModelCreated;
			if (handler != null)
				handler(this, new EventArgs());
		}

		public void Dispose()
		{
		}

		MainWindowModel m_mainWindowModel;
	}
}
