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

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для TimerBoard.xaml
    /// </summary>
    public partial class TimerBoard : Window
    {


        public TimerBoard()
        {
            InitializeComponent();

            DataContext = new TimerBoardViewModel();
        }

        public TimerBoard(bool isKnockout)
        {
            InitializeComponent();

            DataContext = new TimerBoardViewModel(isKnockout);
        }
    }
}
