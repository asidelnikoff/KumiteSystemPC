using Microsoft.Win32;
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
                    Properties.Settings.Default.Save();
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
                endOfMTXT.Text = opf.FileName;
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
    }
}
