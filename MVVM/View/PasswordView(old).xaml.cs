using PassManaAlpha.MVVM.Model;
using PassManaAlpha.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.IconPacks;

namespace PassManaAlpha.MVVM.View
{
    public class BoolToEyeConverter : IValueConverter
    {
        public static readonly BoolToEyeConverter Instance = new();
       

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b
            ? PackIconMaterialKind.Eye
            : PackIconMaterialKind.EyeOff;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public partial class PasswordView : UserControl
    {
        private readonly Dictionary<PasswordEntry, DispatcherTimer> _timers = new();
        private const double HideAfterSeconds = 3.0;

        public PasswordView()
        {
            InitializeComponent();
            
        }

        private PasswordViewModel? VM => DataContext as PasswordViewModel;

        private void CopyToClipboard(string text, string label)
        {
            Clipboard.SetText(text);
            VM?.Log($"Copied {label} to clipboard.");
        }


        private void EntryBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2
                && sender is Border border
                && border.Tag is PasswordEntry entry
                && !string.IsNullOrEmpty(entry.Password))
            {
                CopyToClipboard(entry.Password, "password");
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu && menu.PlacementTarget is Border border
                && border.Tag is PasswordEntry entry)
            {
                menu.Tag = entry;
            }
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            if (item.Parent is not ContextMenu menu) return;
            if (menu.Tag is not PasswordEntry entry) return;

            switch (item.Tag?.ToString())
            {
                case "title":
                    CopyToClipboard(entry.Title ?? "", "name");
                    break;
                case "username":
                    CopyToClipboard(entry.Username ?? "", "username");
                    break;
                case "password":
                    CopyToClipboard(entry.Password ?? "", "password");
                    break;
                case "all":
                    CopyToClipboard(
                        $"Name: {entry.Title}\nUsername: {entry.Username}\nPassword: {entry.Password}",
                        "all fields");
                    break;
                case "delete":
                    DeleteEntry(entry);
                    break;
            }
        }

        private void DeleteEntry(PasswordEntry entry)
        {
            var vm = VM;
            if (vm == null) return;

            vm.Entries.Remove(entry);
            StopTimer(entry);

            try
            {
                if (!File.Exists("vault.dat"))
                {
                    vm.Log("Vault file not found — nothing to rewrite.");
                    return;
                }

                var lines = new List<string>();
                foreach (var e in vm.Entries.ToList())
                {
                    string json = System.Text.Json.JsonSerializer.Serialize(e);
                    string encrypted = Core.Scurity.HakoHelper.Encrypt(json, vm.MasterKey);
                    lines.Add(encrypted);
                }
                vm.BackupVault(); 
                File.WriteAllLines("vault.dat", lines);
                vm.Log($"Deleted \"{entry.Title}\" and rewrote vault.");
            }
            catch (Exception ex)
            {
                vm.Log($"Failed to rewrite vault after delete: {ex.Message}");
            }
        }

        private void EyeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PasswordEntry entry)
            {
                entry.IsPasswordVisible = !entry.IsPasswordVisible;
                if (entry.IsPasswordVisible)
                    StartOrResetTimer(entry);
                else
                    StopTimer(entry);
            }
        }

        private void EntryBorder_MouseActivity(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.Tag is PasswordEntry entry && entry.IsPasswordVisible)
                StartOrResetTimer(entry);
        }

        private void EntryBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.Tag is PasswordEntry entry && entry.IsPasswordVisible)
                StartOrResetTimer(entry);
        }

        private void StartOrResetTimer(PasswordEntry entry)
        {
            if (_timers.TryGetValue(entry, out var existing))
            {
                existing.Stop();
                existing.Start();
                return;
            }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(HideAfterSeconds) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                entry.IsPasswordVisible = false;
                _timers.Remove(entry);
            };
            _timers[entry] = timer;
            timer.Start();
        }

        private void StopTimer(PasswordEntry entry)
        {
            if (_timers.TryGetValue(entry, out var timer))
            {
                timer.Stop();
                _timers.Remove(entry);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
