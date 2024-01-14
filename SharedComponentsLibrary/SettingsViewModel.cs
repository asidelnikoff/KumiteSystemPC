using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using WpfScreenHelper;
using LanguageLibrary;
using System.Globalization;
using System.Windows.Markup;

namespace SharedComponentsLibrary
{
    public class ScreenItem
    {
        public int Id { get; set; }
        public string ScreenName { get; set; }

        public override string ToString()
        {
            return $"{Id}: {ScreenName}";
        }
    }

    public class Language
    {
        public string Name { get; set; }
        public string CultureInfo { get; set; }

        public override int GetHashCode()
        {
            return $"{CultureInfo}".GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj?.GetHashCode() == $"{CultureInfo}".GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<ScreenItem> externalScreens;

        [ObservableProperty]
        UserSettings settings;

        [ObservableProperty]
        ObservableCollection<Language> languages = new ObservableCollection<Language>()
        {
            new Language() { Name = "Русский", CultureInfo = "ru-RU"},
            new Language() { Name = "English", CultureInfo = "en-GB"}
        };

        public Action OnSaveSettings;
        public Action OnClose;

        public SettingsViewModel(UserSettings settings)
        {
            Settings = new UserSettings()
            {
                DataPath = Properties.Settings.Default.DataPath,
                DatabasePath = Properties.Settings.Default.DatabasePath,
                EndOfMatchSound = Properties.Settings.Default.EndOfMatchSound,
                WarningSound = Properties.Settings.Default.WarningSound,
                ExternalMonitorIndex = Properties.Settings.Default.ExternalScreenIndex,
                Tatami = Properties.Settings.Default.Tatami,
                IsAutoLoadNextMatchEnabled = Properties.Settings.Default.IsAutoLoadNextMatchEnabled,
                IsNextMatchShownOnExternalBoard = Properties.Settings.Default.IsNextMatchShownOnExternalBoard
            };

            foreach(var lang in Languages)
                if(lang.CultureInfo == Properties.Settings.Default.Language)
                {
                    Settings.Language = lang;
                    break;
                }

            if(settings != null)
            {
                Settings.DataPath = settings.DataPath;
                Settings.DatabasePath = settings.DatabasePath;
                Settings.EndOfMatchSound = settings.EndOfMatchSound;
                Settings.WarningSound = settings.WarningSound;
                Settings.ExternalMonitorIndex = settings.ExternalMonitorIndex;
                Settings.Tatami = settings.Tatami;
                Settings.IsAutoLoadNextMatchEnabled = settings.IsAutoLoadNextMatchEnabled;
                Settings.IsNextMatchShownOnExternalBoard = settings.IsNextMatchShownOnExternalBoard;
                Settings.Language = settings.Language;
            }

            ExternalScreens = new ObservableCollection<ScreenItem>();
            int id = 0;
            foreach(var screen in Screen.AllScreens)
            {
                ExternalScreens.Add(new ScreenItem()
                {
                    Id = id++,
                    ScreenName = $"{Resources.Name}: {screen.DeviceName}\n{Resources.Primary}: {screen.Primary}"
                });
            }
        }

        [RelayCommand]
        private async Task SaveSettings()
        {
            Properties.Settings.Default.DataPath = Settings.DataPath;
            Properties.Settings.Default.DatabasePath = Settings.DatabasePath;
            Properties.Settings.Default.EndOfMatchSound = Settings.EndOfMatchSound;
            Properties.Settings.Default.WarningSound = Settings.WarningSound;
            Properties.Settings.Default.ExternalScreenIndex = Settings.ExternalMonitorIndex;
            Properties.Settings.Default.Tatami = Settings.Tatami;
            Properties.Settings.Default.IsAutoLoadNextMatchEnabled = Settings.IsAutoLoadNextMatchEnabled;
            Properties.Settings.Default.IsNextMatchShownOnExternalBoard = Settings.IsNextMatchShownOnExternalBoard;
            

            Properties.Settings.Default.Language = Settings.Language.CultureInfo;

            Properties.Settings.Default.Save();
            OnSaveSettings?.Invoke();

            await Helpers.DisplayMessageDialog(Resources.SettingsSaved, Resources.Info);
            OnClose?.Invoke();
        }

        [RelayCommand]
        private void ChooseDataPath()
        {
            using (System.Windows.Forms.FolderBrowserDialog folderData = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderData.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Settings.DataPath = folderData.SelectedPath;
            }
        }

        [RelayCommand]
        private void ChooseDatabase()
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "SQLite Databases(*.sqlite)|*.sqlite";
            opf.Title = "Select default database";
            if (opf.ShowDialog() == true)
                Settings.DatabasePath = opf.FileName;
        }

        [RelayCommand]
        private void ChooseEndOfMatchSound()
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "WAV|*.wav";
            opf.FileName = "";
            if (opf.ShowDialog() == true)
            {
                SoundPlayer sound = new SoundPlayer(opf.FileName);
                Settings.EndOfMatchSound = opf.FileName;
                sound.Play();
                sound.Dispose();
            }
        }

        [RelayCommand]
        private void ChooseWarningSound()
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "WAV|*.wav";
            opf.FileName = "";
            if (opf.ShowDialog() == true)
            {
                SoundPlayer sound = new SoundPlayer(opf.FileName);
                Settings.WarningSound = opf.FileName;
                sound.Play();
                sound.Dispose();
            }
        }
    }
}
