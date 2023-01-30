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
        public void SetFirst(TournamentsBracketsBase.ICompetitor comp)
        {
            if (comp.Club != "") firstComp.Text = $"{comp} ({comp.Club})";
            else firstComp.Text = $"{comp}";
            //firstScore.Text = comp.FinalScore.ToString();
            FirstGrid.Visibility = Visibility.Visible;
        }
        public void SetSecond(TournamentsBracketsBase.ICompetitor comp)
        {
            if (comp.Club != "") secondComp.Text = $"{comp} ({comp.Club})";
            else secondComp.Text = $"{comp}";
            //secondScore.Text = comp.FinalScore.ToString();
            SecondGrid.Visibility = Visibility.Visible;
        }
        public void SetThird(TournamentsBracketsBase.ICompetitor comp)
        {
            if (comp.Club != "") thirdComp.Text = $"{comp} ({comp.Club})";
            else thirdComp.Text = $"{comp}";
            //thirdScore.Text = comp.FinalScore.ToString();
            ThirdGrid.Visibility = Visibility.Visible;
        }
        public void SetThird1(TournamentsBracketsBase.ICompetitor comp)
        {
            if (comp.Club != "") fourthComp.Text = $"{comp} ({comp.Club})";
            else fourthComp.Text = $"{comp}";
            //fourthScore.Text = comp.FinalScore.ToString();
            FourthGrid.Visibility = Visibility.Visible;
        }
    }
}
