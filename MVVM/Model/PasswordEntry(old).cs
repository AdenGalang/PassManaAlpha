using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PassManaAlpha.MVVM.Model
{
    public class PasswordEntry : INotifyPropertyChanged
    {
        public string? Title { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        private bool _isPasswordVisible = false;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PasswordBlur));
            }
        }

        // Blur radius: 0 when visible, 6 when hidden
        public double PasswordBlur => _isPasswordVisible ? 0 : 6;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
