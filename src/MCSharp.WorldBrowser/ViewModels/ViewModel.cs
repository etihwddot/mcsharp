using System.ComponentModel;
using System.Windows.Threading;

namespace MCSharp.WorldBrowser.ViewModels
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		public ViewModel()
		{
			m_dispatcher = Dispatcher.CurrentDispatcher;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged(string property)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(property));
		}

		protected void VerifyAccess()
		{
			m_dispatcher.VerifyAccess();
		}

		Dispatcher m_dispatcher;
	}
}
