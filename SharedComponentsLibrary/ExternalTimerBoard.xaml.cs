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

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для ExternalTimerBoard.xaml
    /// </summary>
    public partial class ExternalTimerBoard : Window
    {
        public ExternalTimerBoard(ExternalTimerBoardState state)
        {
            InitializeComponent();
            List<Screen> sc = Screen.AllScreens.ToList();
            int screenIndex = Properties.Settings.Default.ExternalScreenIndex;
            if (screenIndex >= sc.Count)
                screenIndex = 0;
            Width /= sc[screenIndex].ScaleFactor;
            Height /= sc[screenIndex].ScaleFactor;
            Left = (sc[screenIndex].WpfBounds.Left + sc[screenIndex].WpfBounds.Right)/2 - Width / 2;
            Top = (sc[screenIndex].WpfBounds.Bottom/2 - Height/2);

            DataContext = new ExternalTimerBoardViewModel(state);
        }
    }
}
