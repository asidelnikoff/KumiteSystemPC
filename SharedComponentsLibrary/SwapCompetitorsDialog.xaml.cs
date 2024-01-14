using ModernWpf.Controls;
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

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для SwapCompetitorsDialog.xaml
    /// </summary>
    public partial class SwapCompetitorsDialog : ContentDialog
    {
        public CompetitorDTO Competitor1 { get; private set; }
        public CompetitorDTO Competitor2 { get; private set; }
        public SwapCompetitorsDialog(IEnumerable<CompetitorDTO> competitorsInCategory)
        {
            InitializeComponent();

            DataContext = new SwapCompetitorsDialogViewModel(competitorsInCategory);
            (DataContext as SwapCompetitorsDialogViewModel).CompetitorsChanged += ChangeCompetitors;
        }

        private void ChangeCompetitors(CompetitorDTO competitor1, CompetitorDTO competitor2)
        {
            Competitor1 = competitor1;
            Competitor2 = competitor2;
        }
    }
}
