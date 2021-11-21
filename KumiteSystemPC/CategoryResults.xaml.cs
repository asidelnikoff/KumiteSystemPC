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
using TournamentTree;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для CategoryResults.xaml
    /// </summary>
    public partial class CategoryResults : Window
    {
        public CategoryResults()
        {
            InitializeComponent();
        }

        public void SetCategory(string name)
        {
            CategoryNameEXT.Content = name;
        }
        public void SetFirst(Competitor comp)
        {
            firstComp.Text = comp.ToString();
            //firstScore.Text = comp.FinalScore.ToString();
            FirstGrid.Visibility = Visibility.Visible;
        }
        public void SetSecond(Competitor comp)
        {
            secondComp.Text = comp.ToString();
            //secondScore.Text = comp.FinalScore.ToString();
            SecondGrid.Visibility = Visibility.Visible;
        }
        public void SetThird(Competitor comp)
        {
            thirdComp.Text = comp.ToString();
            //thirdScore.Text = comp.FinalScore.ToString();
            ThirdGrid.Visibility = Visibility.Visible;
        }
        public void SetThird1(Competitor comp)
        {
            fourthComp.Text = comp.ToString();
            //fourthScore.Text = comp.FinalScore.ToString();
            FourthGrid.Visibility = Visibility.Visible;
        }
    }
}
