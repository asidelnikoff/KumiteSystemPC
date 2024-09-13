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
using WpfScreenHelper;

namespace KumiteSystem
{
    /// <summary>
    /// Логика взаимодействия для ExternalBoard2.xaml
    /// </summary>
    public partial class ExternalBoard2 : Window
    {
        public ExternalBoard2(ExternalBoardState state)
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            List<Screen> sc = Screen.AllScreens.ToList();
            int screenIndex = state.Settings.ExternalMonitorIndex;
            if (screenIndex >= sc.Count || screenIndex < 0)
                screenIndex = 0;
            Left = sc[screenIndex].Bounds.Left;
            Top = sc[screenIndex].Bounds.Top;
            //
            DataContext = new ExternalBoardViewModel(state);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }
    }
}
