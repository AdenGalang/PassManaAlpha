using PassManaAlpha.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PassManaAlpha.MVVM.View
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Sync PasswordBox when VM loads an existing key from config
            if (DataContext is SettingsViewModel vm)
                MasterKeyBox.Password = vm.MasterKey;
        }

        private void MasterKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
                vm.MasterKey = MasterKeyBox.Password;
        }

        private bool _revealed = false;
        private void RevealKey_Click(object sender, RoutedEventArgs e)
        {
            _revealed = !_revealed;
            if (_revealed)
            {
                MasterKeyBox.Visibility = Visibility.Collapsed;
                MasterKeyPlain.Visibility = Visibility.Visible;
                RevealIcon.Text = "👁";
            }
            else
            {
                MasterKeyPlain.Visibility = Visibility.Collapsed;
                MasterKeyBox.Visibility = Visibility.Visible;
                MasterKeyBox.Password = MasterKeyPlain.Text;
                RevealIcon.Text = "🔒";
            }
        }

        private void SetDefault_SyncBox(object sender, RoutedEventArgs e)
        {
            // After the command clears MasterKey, sync the PasswordBox too
            Dispatcher.InvokeAsync(() =>
            {
                MasterKeyBox.Password = string.Empty;
                MasterKeyPlain.Text = string.Empty;
                MasterKeyBox.Visibility = Visibility.Visible;
                MasterKeyPlain.Visibility = Visibility.Collapsed;
                RevealIcon.Text = "🔒";
                _revealed = false;
            });
        }
    }
}