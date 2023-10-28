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
            SetTextBlockCompetitiorAndVisibility(secondComp, comp);
        }
        public void SetThird(TournamentsBracketsBase.ICompetitor comp)
        {
            SetTextBlockCompetitiorAndVisibility(thirdComp, comp);
        }
        public void SetThird1(TournamentsBracketsBase.ICompetitor comp)
        {
            SetTextBlockCompetitiorAndVisibility(fourthComp, comp);
        }

        private void SetTextBlockCompetitiorAndVisibility(TextBlock textBlock, TournamentsBracketsBase.ICompetitor comp)
        {
            textBlock.Text = $"{comp}";
            if (!String.IsNullOrEmpty(comp.Club))
                textBlock.Text += $" ({comp.Club})";
            textBlock.Visibility = Visibility.Visible;
        }
    }
}
