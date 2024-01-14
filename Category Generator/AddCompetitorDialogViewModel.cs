using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Category_Generator
{
    public partial class AddCompetitorDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        CompetitorDTO competitor;

        [ObservableProperty]
        string firstName;

        [ObservableProperty]
        string lastName;

        [ObservableProperty]
        string club;

        [ObservableProperty]
        bool isPrimaryButtonEnabled;

        public AddCompetitorDialogViewModel(CompetitorDTO competitor = null)
        {
            if(competitor == null)
                Competitor = new CompetitorDTO();
            else
            {
                Competitor = new CompetitorDTO();
                Competitor.FirstName = competitor.FirstName;
                Competitor.LastName = competitor.LastName;
                Competitor.Club = competitor.Club;
            }

            FirstName = Competitor.FirstName;
            LastName = Competitor.LastName;
            Club = Competitor.LastName;

            PropertyChanged += AddCompetitorDialogViewModel_PropertyChanged;
        }

        private void AddCompetitorDialogViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Competitor.FirstName = FirstName == null ? "" : FirstName.Trim();
            Competitor.LastName = LastName == null ? "" : LastName.Trim();
            Competitor.Club = Club == null ? "" : Club.Trim();

            IsPrimaryButtonEnabled = Competitor.FirstName.Length > 0
                && Competitor.LastName.Length > 0
                && Competitor.Club.Length > 0;
        }
    }
}
