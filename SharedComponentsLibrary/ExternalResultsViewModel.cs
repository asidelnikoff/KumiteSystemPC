using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TournamentsBracketsBase;

namespace SharedComponentsLibrary
{
    public partial class ExternalResultsViewModel : ObservableObject
    {
        [ObservableProperty]
        string categoryName;

        [ObservableProperty]
        string firstPlaceName;

        [ObservableProperty]
        Visibility firstPlaceVisibility;

        [ObservableProperty]
        string secondPlaceName;

        [ObservableProperty]
        Visibility secondPlaceVisibility;

        [ObservableProperty]
        string thirdPlaceName1;

        [ObservableProperty]
        Visibility thirdPlaceVisibility1;

        [ObservableProperty]
        string thirdPlaceName2;

        [ObservableProperty]
        Visibility thirdPlaceVisibility2;

        public ExternalResultsViewModel(string categoryName, IList<ICompetitor> winners)
        {
            CategoryName = categoryName;
            FirstPlaceVisibility = Visibility.Collapsed;
            SecondPlaceVisibility = Visibility.Collapsed;
            ThirdPlaceVisibility1 = Visibility.Collapsed;
            ThirdPlaceVisibility2 = Visibility.Collapsed;

            if (winners?.Count > 0)
            {
                FirstPlaceName = winners[0].ToString();
                FirstPlaceVisibility = Visibility.Visible;
            }
            
            if(winners?.Count > 1)
            {
                SecondPlaceName = winners[1].ToString();
                SecondPlaceVisibility = Visibility.Visible;
            }

            if (winners?.Count > 2)
            {
                ThirdPlaceName1 = winners[2].ToString();
                ThirdPlaceVisibility1 = Visibility.Visible;
            }

            if (winners?.Count > 3)
            {
                ThirdPlaceName2 = winners[3].ToString();
                ThirdPlaceVisibility2 = Visibility.Visible;
            }
        }
    }
}
