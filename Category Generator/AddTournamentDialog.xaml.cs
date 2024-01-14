using ModernWpf.Controls;
using SharedComponentsLibrary.Models;
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
    /// Логика взаимодействия для AddTournamentDialog.xaml
    /// </summary>
    public partial class AddTournamentDialog : ContentDialog
    {
        public Tournament ResultTournament { get; private set; }
        public AddTournamentDialog(Tournament tournmanet)
        {
            InitializeComponent();

            DataContext = new AddTournamentDialogViewModel(tournmanet);
            ResultTournament = (DataContext as AddTournamentDialogViewModel).Tournament;
        }
    }
}
