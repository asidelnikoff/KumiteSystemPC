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
using System.Windows.Shapes;
using TournamentsBracketsBase;

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для CategoryViewer.xaml
    /// </summary>
    public partial class CategoryViewer : Window, ICategoryViewer
    {
        public CategoryViewer(DBService dbService, CategoryDTO category, bool isGenerationNeeded, 
            bool shuffleCompetitors=false, bool isSwapCompetitorsEnabled = false)
        {
            InitializeComponent();
            DataContext = new CategoryViewerViewModel(category, dbService, isGenerationNeeded, shuffleCompetitors, isSwapCompetitorsEnabled);
            (DataContext as CategoryViewerViewModel).Closed += () => Closed?.Invoke();
        }

        public Action<RoundDTO, IMatch> GotMatch
        {
            get => (DataContext as CategoryViewerViewModel).GotMatch;
            set
            {
                (DataContext as CategoryViewerViewModel).GotMatch = value;
            }
        }

        public Action<RoundDTO, IMatch> GotNextMatch
        {
            get => (DataContext as CategoryViewerViewModel).GotNextMatch;
            set
            {
                (DataContext as CategoryViewerViewModel).GotNextMatch = value;
            }
        }

        public Action<IList<ICompetitor>> GotCategoryResults
        {
            get => (DataContext as CategoryViewerViewModel).GotCategoryResults;
            set
            {
                (DataContext as CategoryViewerViewModel).GotCategoryResults = value;
            }
        }

        public Action Closed { get; set; }

        public void WriteMatchResults(RoundDTO round, IMatch match)
        {
            (DataContext as CategoryViewerViewModel).WriteMatchResults(round, match);
        }

        public void LoadMatch(RoundDTO round, IMatch match)
        {
            (DataContext as CategoryViewerViewModel).LoadMatch(round, match);
        }
    }
}
