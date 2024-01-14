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

namespace Category_Generator
{
    /// <summary>
    /// Логика взаимодействия для AddCompetitorDialog.xaml
    /// </summary>
    public partial class AddCompetitorDialog : ContentDialog
    {
        public CompetitorDTO ResultCompetitor { get; private set; }

        public AddCompetitorDialog(CompetitorDTO competitor)
        {
            InitializeComponent();

            this.DataContext = new AddCompetitorDialogViewModel(competitor);
            ResultCompetitor = (DataContext as AddCompetitorDialogViewModel).Competitor;
        }


    }
}
