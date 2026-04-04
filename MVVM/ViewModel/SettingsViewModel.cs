using PassManaAlpha.Core;
using System.Windows;
using System.Windows.Input;

namespace PassManaAlpha.MVVM.ViewModel
{
    public class SettingsViewModel : ForkObject
    {
        private readonly AppConfig _config;

        private string _masterKey = string.Empty;
        public string MasterKey
        {
            get => _masterKey;
            set
            {
                _masterKey = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel()
        {
            _config = AppConfig.Load();
            MasterKey = _config.MasterKey; // may be empty on first run
        }

        public ICommand SaveKeyCommand => new RelayCommand(o =>
        {
            if (string.IsNullOrWhiteSpace(MasterKey))
            {
                MessageBox.Show(
                    "Please enter a master key before saving.",
                    "No Master Key",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _config.MasterKey = MasterKey;
            _config.Save();
            MessageBox.Show("Master key saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        });

        public ICommand SetDefaultCommand => new RelayCommand(o =>
        {
            var result = MessageBox.Show(
                "This will clear the saved master key. Continue?",
                "Reset Master Key",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MasterKey = string.Empty;
                _config.MasterKey = string.Empty;
                _config.Save();
            }
        });
    }
}