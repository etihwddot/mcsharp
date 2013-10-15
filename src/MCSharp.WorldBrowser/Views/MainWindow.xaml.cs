using System.Windows;
using MCSharp.WorldBrowser.ViewModels;

namespace MCSharp.WorldBrowser.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow(MainWindowModel model)
		{
			m_model = model;
			DataContext = m_model;

			InitializeComponent();
		}

		MainWindowModel m_model;
	}
}
