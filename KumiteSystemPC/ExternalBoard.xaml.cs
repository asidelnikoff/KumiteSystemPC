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
    /// Логика взаимодействия для ExternalBoard.xaml
    /// </summary>
    public partial class ExternalBoard : Window
    {

        public delegate void IsOpened(bool status);
        public event IsOpened Send_Status;

        public ExternalBoard()
        {
            InitializeComponent();
            if (!Properties.Settings.Default.ShowNextMatchEXT) { nextMGrid.Visibility = Visibility.Hidden; }
            TatamiEXT.Content = $"Tatami {Properties.Settings.Default.TatamiNr}";
        }

        public void TimerText(int sec, int min)
        {
            TimerEXT.Content = String.Format("{0:d2}:{1:d2}", min, sec);
            TimerEXT.Foreground = Brushes.White;
            if (min == 0 && sec <= 15)
            {
                TimerEXT.Foreground = Brushes.DarkRed;
            }
        }

        
        public void ShowSanction(Border santion, int to)
        {
            DoubleAnimation opactityAnim = new DoubleAnimation();
            opactityAnim.From = santion.Opacity;
            opactityAnim.To = to;
            opactityAnim.Duration = TimeSpan.FromSeconds(0.6);
            santion.BeginAnimation(Border.OpacityProperty, opactityAnim);
        }

        public void GridOpacityAnim(Grid grid,double to)
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

        //TODO: Colors for round winner
        public void addRound(int roundNr, int scoreAka, int scoreAo, int winner)
        {
            LinearGradientBrush myLinearGradientBrush;
            switch (winner)
            {
                case 0:
                    {
                        Viewbox dynamicViewbox = new Viewbox();
                        dynamicViewbox.StretchDirection = StretchDirection.Both;
                        dynamicViewbox.Stretch = Stretch.Fill;

                        Label label = new Label();
                        label.Foreground = Brushes.White;
                        label.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF4E5059"); ;
                        label.Content = $"R{roundNr} {scoreAka}:{scoreAo}";

                        dynamicViewbox.Child = label;
                        roundsExt.Children.Add(dynamicViewbox);
                        break;
                    }

                case 1:
                    {
                        myLinearGradientBrush =
               new LinearGradientBrush();
                        myLinearGradientBrush.StartPoint = new Point(0, 0);
                        myLinearGradientBrush.EndPoint = new Point(1, 1);
                        myLinearGradientBrush.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#FF0000"), 0.0));
                        myLinearGradientBrush.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#990000"), 1.0));
                        Viewbox dynamicViewbox = new Viewbox();
                        dynamicViewbox.StretchDirection = StretchDirection.Both;
                        dynamicViewbox.Stretch = Stretch.Fill;

                        Label label = new Label();
                        label.Foreground = Brushes.White;
                        label.Background = myLinearGradientBrush;
                        label.Content = $"R{roundNr} {scoreAka}:{scoreAo}";

                        dynamicViewbox.Child = label;
                        roundsExt.Children.Add(dynamicViewbox);
                        break;
                    }
                case 2:
                    {
                        myLinearGradientBrush =
               new LinearGradientBrush();
                        myLinearGradientBrush.StartPoint = new Point(0, 0);
                        myLinearGradientBrush.EndPoint = new Point(1, 1);
                        myLinearGradientBrush.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#FF009FFD"), 0.0));
                        myLinearGradientBrush.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#FF2A2A72"), 1.0));
                        Viewbox dynamicViewbox = new Viewbox();
                        dynamicViewbox.StretchDirection = StretchDirection.Both;
                        dynamicViewbox.Stretch = Stretch.Fill;

                        Label label = new Label();
                        label.Foreground = Brushes.White;
                        label.Background = myLinearGradientBrush;
                        label.Content = $"R{roundNr} {scoreAka}:{scoreAo}";

                        dynamicViewbox.Child = label;
                        roundsExt.Children.Add(dynamicViewbox);
                        break;

                    }
            }
        }

        public void addText(int roundNr, int rounds)
        {
            Viewbox dynamicViewbox = new Viewbox();
            dynamicViewbox.StretchDirection = StretchDirection.Both;
            dynamicViewbox.Stretch = Stretch.Fill;

            Label label = new Label();
            label.Foreground = Brushes.White;
            label.Content = $"Round {roundNr}/{rounds}";

            dynamicViewbox.Child = label;
            roundsExt.Children.Add(dynamicViewbox);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Send_Status?.Invoke(false);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Send_Status?.Invoke(true);
        }
    }
}
