using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageLibrary;

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

            State.TatamiNumber = Properties.Settings.Default.Tatami;
            IsNextMatchVisible = Properties.Settings.Default.IsNextMatchShownOnExternalBoard;

            TatamiText = $"{Resources.Tatami} {State.TatamiNumber}";
        }
    }
}
