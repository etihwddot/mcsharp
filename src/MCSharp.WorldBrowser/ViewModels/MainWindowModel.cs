using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCSharp.WorldBrowser.ViewModels
{
	public sealed class MainWindowModel : ViewModel
	{
		public MainWindowModel()
		{
			m_availableSaves = GameSaveInfo.GetAvailableSaves().ToList().AsReadOnly();
			m_selectedSave = m_availableSaves.FirstOrDefault();
		}

		public ReadOnlyCollection<GameSaveInfo> AvailableSaves
		{
			get { return m_availableSaves; }
		}

		public static readonly string SelectedSaveProperty = "SelectedSave";
		public GameSaveInfo SelectedSave
		{
			get
			{
				return m_selectedSave;
			}
			set
			{
				if (value != m_selectedSave)
				{
					m_selectedSave = value;
					RaisePropertyChanged(SelectedSaveProperty);
				}
			}
		}

		GameSaveInfo m_selectedSave;
		ReadOnlyCollection<GameSaveInfo> m_availableSaves;
	}
}
