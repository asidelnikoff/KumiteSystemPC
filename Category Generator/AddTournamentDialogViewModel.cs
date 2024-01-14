using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Category_Generator
{
    public partial class AddTournamentDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        Tournament tournament;

        [ObservableProperty]
        string name;

        [ObservableProperty]
        bool isPrimaryButtonEnabled;

        public AddTournamentDialogViewModel(Tournament tournament)
        {
            Tournament = new Tournament();
            if (tournament != null)
                Tournament.Name = tournament.Name;
            Name = Tournament.Name;

            PropertyChanged += AddTournamentDialogViewModel_PropertyChanged;
        }

        private void AddTournamentDialogViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Tournament.Name = Name == null ? "" : Name.Trim();

            IsPrimaryButtonEnabled = Tournament.Name.Length > 0;
        }
    }
}
