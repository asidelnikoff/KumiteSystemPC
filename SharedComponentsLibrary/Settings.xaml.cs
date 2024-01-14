using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public UserSettings UserSettings { get; private set; }
        public Action<UserSettings> SaveSettings;
        public Settings(UserSettings settings)
        {
            DataContext = new SettingsViewModel(settings);
            var info = new CultureInfo(Properties.Settings.Default.Language);
            Thread.CurrentThread.CurrentUICulture = info;
            Thread.CurrentThread.CurrentCulture = info;

            InitializeComponent();

            (DataContext as SettingsViewModel).OnSaveSettings += OnSaveSettings;
            (DataContext as SettingsViewModel).OnClose += () => Close();
        }

        private void OnSaveSettings()
        {
            UserSettings = (DataContext as SettingsViewModel).Settings;
            SaveSettings?.Invoke(UserSettings);
        }

        public UserSettings LoadSettings()
        {
            return (DataContext as SettingsViewModel).Settings;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings = (DataContext as SettingsViewModel).Settings;
        }
    }
}
