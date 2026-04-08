using PassManaAlpha.Core;
using PassManaAlpha.Core.Scurity;
using PassManaAlpha.MVVM.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace PassManaAlpha.MVVM.ViewModel
{
    public class PasswordViewModel : ForkObject
    {
        private readonly SettingsViewModel _settingsVM;

        internal string MasterKey => _settingsVM.MasterKey;

        public string? InputTitle { get; set; }
        public string? InputUsername { get; set; }
        public string? InputPassword { get; set; }

        private string? _consoleLog;
        public string? ConsoleLog
        {
            get => _consoleLog;
            set { _consoleLog = value; OnPropertyChanged(nameof(ConsoleLog)); }
        }

        public void Log(string message) =>
            ConsoleLog += $"[{DateTime.Now:HH:mm:ss}] {message}\n";

        private bool _isLoadEnabled = true;
        public bool IsLoadEnabled
        {
            get => _isLoadEnabled;
            set { _isLoadEnabled = value; OnPropertyChanged(nameof(IsLoadEnabled)); }
        }

        public ObservableCollection<PasswordEntry> Entries { get; set; }

        public PasswordViewModel(SettingsViewModel settingsVM)
        {
            _settingsVM = settingsVM;
            Entries = new ObservableCollection<PasswordEntry>();
        }

        public ICommand ReloadCommand => new RelayCommand(o =>
        {
            Entries.Clear();
            IsLoadEnabled = true;
            Log("Vault entries cleared.");
        });

        public ICommand ClearConsoleCommand => new RelayCommand(o =>
            ConsoleLog = string.Empty);

        public ICommand SaveCommand => new RelayCommand(o =>
        {
            if (string.IsNullOrWhiteSpace(MasterKey))
            {
                Log("No master key set. Go to Settings and enter your master key first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(InputTitle) ||
                string.IsNullOrWhiteSpace(InputUsername) ||
                string.IsNullOrWhiteSpace(InputPassword))
            {
                Log("Please fill in all fields before saving.");
                return;
            }

            var entry = new PasswordEntry
            {
                Title = InputTitle,
                Username = InputUsername,
                Password = InputPassword
            };

            string json = JsonSerializer.Serialize(entry);
            string encrypted;



            try { encrypted = HakoHelper.Encrypt(json, MasterKey); }
            catch (Exception ex) { Log($"Encryption failed: {ex.Message}"); return; }

            BackupVault();
            File.AppendAllText(VaultPath, encrypted + Environment.NewLine);

            try
            {
                File.AppendAllText("vault.dat", encrypted + Environment.NewLine);
                Entries.Add(entry);
                InputTitle = InputUsername = InputPassword = string.Empty;
                OnPropertyChanged(nameof(InputTitle));
                OnPropertyChanged(nameof(InputUsername));
                OnPropertyChanged(nameof(InputPassword));
                Log("Entry saved successfully.");
            }
            catch (Exception ex) { Log($"Failed to save entry: {ex.Message}"); }
        });

        public ICommand LoadCommand => new RelayCommand(o =>
        {
            if (string.IsNullOrWhiteSpace(MasterKey))
            {
                Log("No master key set. Go to Settings and enter your master key first.");
                return;
            }
            if (!File.Exists("vault.dat"))
            {
                Log("Vault file not found.");
                return;
            }

            var lines = File.ReadAllLines("vault.dat");
            if (lines.Length == 0) { Log("Vault is empty."); return; }

            Entries.Clear();
            int loaded = 0;
            int skipped = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    string? decrypted = HakoHelper.Decrypt(line, MasterKey);

                    if (decrypted == null) 
                    {
                        skipped++;
                        continue;
                    }

                    var entry = JsonSerializer.Deserialize<PasswordEntry>(decrypted)
                        ?? throw new Exception("Deserialization returned null");

                    Entries.Add(entry);
                    loaded++;
                }
                catch (Exception ex)
                {
                    Log($"Corrupt entry skipped: {ex.Message}");
                }
            }

            Log($"Loaded {loaded} entr{(loaded == 1 ? "y" : "ies")}." +
                (skipped > 0 ? $" {skipped} entr{(skipped == 1 ? "y belongs" : "ies belong")} to a different key." : ""));
        });

        private static readonly string VaultPath = "vault.dat";
        private static readonly string BackupFolder = "backups";

        internal void BackupVault()
        {
            if (!File.Exists(VaultPath)) return;

            Directory.CreateDirectory(BackupFolder);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupPath = Path.Combine(BackupFolder, $"vault_{timestamp}.dat");

            File.Copy(VaultPath, backupPath);

            var backups = Directory.GetFiles(BackupFolder, "vault_*.dat")
                .OrderBy(f => f)
                .ToList();

            while (backups.Count > 10)
            {
                File.Delete(backups[0]);
                backups.RemoveAt(0);
            }
        }
    }
}