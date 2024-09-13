using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageLibrary;
using SharedComponentsLibrary;

namespace KataSystem
{
    public partial class ExternalBoardViewModel : ObservableObject
    {
        [ObservableProperty]
        ExternalBoardState state;

        [ObservableProperty]
        bool isNextMatchVisible;

        [ObservableProperty]
        string tatamiText;

        public ExternalBoardViewModel(ExternalBoardState state)
        {
            State = state;

            var settings = UserSettings.GetUserSettings();
            State.TatamiNumber = settings.Tatami;
            IsNextMatchVisible = settings.IsNextMatchShownOnExternalBoard;

            TatamiText = $"{Resources.Tatami} {State.TatamiNumber}";
        }
    }
}
