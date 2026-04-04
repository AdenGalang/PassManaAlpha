using PassManaAlpha.Core;

namespace PassManaAlpha.MVVM.ViewModel
{
    class MainViewModel : ForkObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand PasswordViewCommand { get; set; }
        public RelayCommand SettingsViewCommand { get; set; }
        public RelayCommand AboutViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public PasswordViewModel PasswordVM { get; set; }
        public SettingsViewModel SettingsVM { get; set; }
        public AboutViewModel AboutVM { get; set; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            SettingsVM = new SettingsViewModel();
            PasswordVM = new PasswordViewModel(SettingsVM);
            HomeVM = new HomeViewModel();
            AboutVM = new AboutViewModel();
       
            _currentView = HomeVM;

            HomeViewCommand = new RelayCommand(o => CurrentView = HomeVM);
            PasswordViewCommand = new RelayCommand(o => CurrentView = PasswordVM);
            SettingsViewCommand = new RelayCommand(o => CurrentView = SettingsVM);
            AboutViewCommand = new RelayCommand(o => CurrentView = AboutVM);
        }
    }
}