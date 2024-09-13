using SharedComponentsLibrary.DTO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для TimerBoardControl.xaml
    /// </summary>
    public partial class TimerBoardControl : UserControl, ITimerBoard
    {
        public TimerBoardControl()
        {
            InitializeComponent();
        }

        public TimerBoardControl(TimerBoardViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        public Action Closed { get; set; }

        public void Close()
        {
            (DataContext as TimerBoardViewModel)?.Close();
            Closed?.Invoke();
        }
    }
}
