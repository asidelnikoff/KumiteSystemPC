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
        public ExternalBoard()
        {
            InitializeComponent();
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

        DoubleAnimation opactityAnim = new DoubleAnimation();
        public void ShowSanction(Border santion, int to)
        {
            opactityAnim.From = santion.Opacity;
            opactityAnim.To = to;
            opactityAnim.Duration = TimeSpan.FromSeconds(0.6);
            santion.BeginAnimation(Border.OpacityProperty, opactityAnim);
        }


        public void ShowWinner(Label winner)
        {
            AkaScoreL.BeginAnimation(OpacityProperty, null);
            AoScoreL.BeginAnimation(OpacityProperty, null);
            opactityAnim.From = 1;
            opactityAnim.To = 0;
            opactityAnim.AutoReverse = true;
            opactityAnim.Duration = TimeSpan.FromSeconds(1);
            opactityAnim.RepeatBehavior = new RepeatBehavior(5);
            opactityAnim.FillBehavior = FillBehavior.Stop;
            winner.BeginAnimation(Label.OpacityProperty, opactityAnim);
        }

        //TODO: Colors for round winner
        public void addRound(int roundNr, int scoreAka, int scoreAo, int winner)
        {
            switch (winner)
            {

                case 0:
                    {
                        Viewbox dynamicViewbox = new Viewbox();
                        dynamicViewbox.StretchDirection = StretchDirection.Both;
                        dynamicViewbox.Stretch = Stretch.Fill;

                        Label label = new Label();
                        label.Foreground = Brushes.White;
                        label.Background = Brushes.Gray;
                        label.Content = $"R{roundNr} {scoreAka}:{scoreAo}";

                        dynamicViewbox.Child = label;
                        roundsExt.Children.Add(dynamicViewbox);
                        break;
                    }

                case 1:
                    {
                        Viewbox dynamicViewbox = new Viewbox();
                        dynamicViewbox.StretchDirection = StretchDirection.Both;
                        dynamicViewbox.Stretch = Stretch.Fill;

                        Label label = new Label();
                        label.Foreground = Brushes.White;
                        label.Background = Brushes.DarkRed;
                        label.Content = $"R{roundNr} {scoreAka}:{scoreAo}";

                        dynamicViewbox.Child = label;
                        roundsExt.Children.Add(dynamicViewbox);
                        break;
                    }
                case 2:
                    {
                        Viewbox dynamicViewbox = new Viewbox();
                        dynamicViewbox.StretchDirection = StretchDirection.Both;
                        dynamicViewbox.Stretch = Stretch.Fill;

                        Label label = new Label();
                        label.Foreground = Brushes.White;
                        label.Background = Brushes.DarkBlue;
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
    }
}
