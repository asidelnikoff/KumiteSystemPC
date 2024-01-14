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
using TournamentsBracketsBase;
using WpfScreenHelper;

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для ExternalResults.xaml
    /// </summary>
    public partial class ExternalResults : Window
    {
        public ExternalResults(string categoryName, IList<ICompetitor> winners)
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            List<Screen> sc = Screen.AllScreens.ToList();
            int screenIndex = Properties.Settings.Default.ExternalScreenIndex;
            if (screenIndex >= sc.Count)
                screenIndex = 0;
            Left = sc[screenIndex].Bounds.Left;
            Top = sc[screenIndex].Bounds.Top;
            ShowActivated = false;

            DataContext = new ExternalResultsViewModel(categoryName, winners);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }
    }
}
