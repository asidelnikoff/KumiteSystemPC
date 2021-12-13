using System;
using System.Collections.Generic;
using System.Linq;
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

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для TreeTypeDialog.xaml
    /// </summary>
    public partial class TreeTypeDialog : Window
    {

        List<string> TreeTypes = new List<string>() { "Sengle Elemination (2 third places)", "Single Elemintaion (1 third place)" };

        public TreeTypeDialog()
        {
            InitializeComponent();
            treeTypeCB.ItemsSource = TreeTypes;
            treeTypeCB.SelectedIndex = Properties.Settings.Default.DefaultTreeType;
        }

        private void selectBTN_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DefaultTreeType = treeTypeCB.SelectedIndex;
            Properties.Settings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Properties.Settings.Default.DefaultTreeType = treeTypeCB.SelectedIndex;
                Properties.Settings.Default.Save();
                this.DialogResult = true;
                this.Close();
            }
            else if(e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void cancelBTN_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
