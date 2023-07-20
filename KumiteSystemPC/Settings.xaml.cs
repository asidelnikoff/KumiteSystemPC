using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfScreenHelper;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            int j = 0;
            foreach (var screen in Screen.AllScreens)
            {
                j++;
                screenC.Items.Add(j.ToString() + ". | Name: " + screen.DeviceName + "\n | Primary: " + screen.Primary);
            }

            screenC.SelectedIndex = Properties.Settings.Default.ScreenNR;

            endOfMTXT.Text = Properties.Settings.Default.EndOfMatch;

            dataPathTXT.Text = Properties.Settings.Default.DataPath;

            defaultDB.Text = Properties.Settings.Default.DefaultDBPath;

            tatamiNr.Text = Properties.Settings.Default.TatamiNr.ToString();

            pointsToWin.Text = Properties.Settings.Default.PointToWin.ToString();

            if (Properties.Settings.Default.AutoNextLoad) { AutoLoadNextCB.IsChecked = true; }
            if (Properties.Settings.Default.ShowNextMatchEXT) { ShoNextEXTCB.IsChecked = true; }
        }

        private void endOfMatch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "WAV|*.wav";
            opf.FileName = "End of match Sound file";
            if (opf.ShowDialog() == true)
            {
                endOfMTXT.Text = opf.FileName;
                SoundPlayer sound = new SoundPlayer(opf.FileName);
                Properties.Settings.Default.EndOfMatch = opf.FileName;
                Properties.Settings.Default.Save();
                sound.Play();
                sound.Dispose();
            }

        }


        private void dataPathTXT_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folderData = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderData.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.DataPath = folderData.SelectedPath;
                    dataPathTXT.Text = Properties.Settings.Default.DataPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void screenC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.ScreenNR = screenC.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void warningMTXT_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "WAV|*.wav";
            opf.FileName = "Warning Sound file";
            if (opf.ShowDialog() == true)
            {
                warningMTXT.Text = opf.FileName;
                SoundPlayer sound = new SoundPlayer(opf.FileName);
                Properties.Settings.Default.WarningSound = opf.FileName;
                Properties.Settings.Default.Save();
                sound.Play();
                sound.Dispose();
            }
        }

        private void AutoLoadNextCB_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoNextLoad = true;
            Properties.Settings.Default.Save();
        }

        private void AutoLoadNextCB_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoNextLoad = false;
            Properties.Settings.Default.Save();
        }

        private void ShoNextEXTCB_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowNextMatchEXT = true;
            Properties.Settings.Default.Save();
        }

        private void ShoNextEXTCB_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowNextMatchEXT = false;
            Properties.Settings.Default.Save();
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "SQLite Databases(*.sqlite)|*.sqlite";
            opf.Title = "Select default database";
            if (opf.ShowDialog() == true)
            {
                defaultDB.Text = opf.FileName;
                Properties.Settings.Default.DefaultDBPath = opf.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void tatamiNr_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                try
                {
                    Properties.Settings.Default.TatamiNr = Convert.ToInt32(tatamiNr.Text);
                    Properties.Settings.Default.Save();
                }
                catch
                {
                    tatamiNr.Text = Properties.Settings.Default.TatamiNr.ToString();
                }
            }
        }

        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
            try {Properties.Settings.Default.TatamiNr = Convert.ToInt32(tatamiNr.Text);}
            catch {tatamiNr.Text = Properties.Settings.Default.TatamiNr.ToString();}
            Properties.Settings.Default.DefaultDBPath = defaultDB.Text;
            Properties.Settings.Default.ShowNextMatchEXT = Convert.ToBoolean(ShoNextEXTCB.IsChecked);
            Properties.Settings.Default.AutoNextLoad = Convert.ToBoolean(AutoLoadNextCB.IsChecked);
            Properties.Settings.Default.WarningSound = warningMTXT.Text;
            Properties.Settings.Default.DataPath = dataPathTXT.Text;
            Properties.Settings.Default.ScreenNR = screenC.SelectedIndex;
            Properties.Settings.Default.EndOfMatch = endOfMTXT.Text;
            Properties.Settings.Default.ShowCompetitorClub = Convert.ToBoolean(ShowCompClub.IsChecked);
            Properties.Settings.Default.Save();

            DisplayMessageDialog("Info", "Settings saved");
        }


        private async void DisplayMessageDialog(string caption, string message)
        {
            try
            {
                ContentDialog CategoryResults = new ContentDialog
                {
                    Title = $"{caption}",
                    PrimaryButtonText = "Ok",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = $"{message}",
                };
                await ContentDialogMaker.CreateContentDialogAsync(CategoryResults, awaitPreviousDialog: true);
            }
            catch { }
        }

        private void ShowCompClub_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowCompetitorClub = true;
            Properties.Settings.Default.Save();
        }

        private void ShowCompClub_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowCompetitorClub = false;
            Properties.Settings.Default.Save();
        }

        private void pointsToWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Properties.Settings.Default.PointToWin = Convert.ToInt32(pointsToWin.Text);
                    Properties.Settings.Default.Save();
                }
                catch
                {
                    pointsToWin.Text = Properties.Settings.Default.PointToWin.ToString();
                }
            }
        }
    }
}
