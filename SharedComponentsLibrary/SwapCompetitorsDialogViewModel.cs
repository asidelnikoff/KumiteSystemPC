using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary
{
    public partial class SwapCompetitorsDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<CompetitorDTO> competitorsInCategory;

        [ObservableProperty]
        CompetitorDTO selectedCompetitor1;

        [ObservableProperty]
        CompetitorDTO selectedCompetitor2;

        [ObservableProperty]
        bool isSwapButtonEnabled;

        public Action<CompetitorDTO, CompetitorDTO> CompetitorsChanged;

        public SwapCompetitorsDialogViewModel(IEnumerable<CompetitorDTO> competitorsInCategory)
        {
            CompetitorsInCategory = new ObservableCollection<CompetitorDTO>(competitorsInCategory);

            PropertyChanged += SwapCompetitorsDialogViewModel_PropertyChanged;
        }

        private void SwapCompetitorsDialogViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsSwapButtonEnabled = SelectedCompetitor1 != null && SelectedCompetitor2 != null
                && SelectedCompetitor1.Id != SelectedCompetitor2.Id;
            if (SelectedCompetitor1 != null && SelectedCompetitor2 != null)
                CompetitorsChanged?.Invoke(SelectedCompetitor1, SelectedCompetitor2);
        }
    }
}
