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
    public partial class TimerBoard : Window, ITimerBoard
    {
        public TimerBoard()
        {
            DataContext = new TimerBoardViewModel();
            (DataContext as TimerBoardViewModel).Closed += () => Closed?.Invoke();
            InitializeComponent();
        }

        public TimerBoard(bool isKnockout)
        {
            DataContext = new TimerBoardViewModel(isKnockout);
            (DataContext as TimerBoardViewModel).Closed += () => Closed?.Invoke();
            InitializeComponent();
        }

        public Action Closed { get; set; }
    }
}
