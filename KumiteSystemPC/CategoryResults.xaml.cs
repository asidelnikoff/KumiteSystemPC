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
            SetTextBlockCompetitiorAndVisibility(ref firstComp, ref FirstGrid, comp);
        }
        public void SetSecond(TournamentsBracketsBase.ICompetitor comp)
        {
            SetTextBlockCompetitiorAndVisibility(ref secondComp, ref SecondGrid,comp);
        }
        public void SetThird(TournamentsBracketsBase.ICompetitor comp)
        {
            SetTextBlockCompetitiorAndVisibility(ref thirdComp, ref ThirdGrid,comp);
        }
        public void SetThird1(TournamentsBracketsBase.ICompetitor comp)
        {
            SetTextBlockCompetitiorAndVisibility(ref fourthComp, ref FourthGrid,comp);
        }

        private void SetTextBlockCompetitiorAndVisibility(ref TextBlock textBlock, ref Grid grid,TournamentsBracketsBase.ICompetitor comp)
        {
            textBlock.Text = $"{comp}";
            if (!String.IsNullOrEmpty(comp.Club))
                textBlock.Text += $" ({comp.Club})";
            textBlock.Visibility = Visibility.Visible;
            grid.Visibility = Visibility.Visible;
        }
    }
}
