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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для Kata_ExternalBoard.xaml
    /// </summary>
    public partial class Kata_ExternalBoard : Window
    {
        public Kata_ExternalBoard()
        {
            InitializeComponent();
            if (!Properties.Settings.Default.ShowNextMatchEXT) { nextMGrid.Visibility = Visibility.Collapsed; }
            TatamiEXT.Content = $"Tatami {Properties.Settings.Default.TatamiNr}";
        }

        public void GridOpacityAnim(Grid grid, double to)
        {
            AKA_Grid.BeginAnimation(OpacityProperty, null);
            AO_Grid.BeginAnimation(OpacityProperty, null);

            DoubleAnimation opactityAnim = new DoubleAnimation();
            opactityAnim.From = grid.Opacity;
            opactityAnim.To = to;
            opactityAnim.Duration = TimeSpan.FromSeconds(0.4);
            grid.BeginAnimation(OpacityProperty, opactityAnim);
        }

        public void ShowWinner(Label winner, Grid looserGrid)
        {
            DoubleAnimation opactityAnim = new DoubleAnimation();
            AkaScoreL.BeginAnimation(OpacityProperty, null);
            AoScoreL.BeginAnimation(OpacityProperty, null);
            GridOpacityAnim(looserGrid, 0.5);
            opactityAnim.From = 1;
            opactityAnim.To = 0;
            opactityAnim.AutoReverse = true;
            opactityAnim.Duration = TimeSpan.FromSeconds(1);
            opactityAnim.RepeatBehavior = new RepeatBehavior(3);
            opactityAnim.FillBehavior = FillBehavior.Stop;
            winner.BeginAnimation(Label.OpacityProperty, opactityAnim);
        }
    }
}
