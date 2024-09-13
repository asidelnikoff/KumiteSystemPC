using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using TournamentsBracketsBase;
using static System.Windows.Forms.AxHost;

namespace KumiteSystem
{
    public partial class ExternalBoardState : ObservableObject
    {
        public UserSettings Settings { get; set; }

        [ObservableProperty]
        string? categoryName;

        [ObservableProperty]
        int? tatamiNumber;

        [ObservableProperty]
        string? currentMatchAka;

        [ObservableProperty]
        string? currentMatchAo;

        [ObservableProperty]
        int? scoreAka;

        [ObservableProperty]
        int? scoreAo;

        [ObservableProperty]
        int? foulsC1Aka;

        [ObservableProperty]
        int? foulsC1Ao;

        [ObservableProperty]
        bool? akaSenshu;

        [ObservableProperty]
        bool? aoSenshu;

        [ObservableProperty]
        TimeSpan remainTime;

        [ObservableProperty]
        bool isAtoshiBaraku;

        [ObservableProperty]
        string? nextMatchAka;

        [ObservableProperty]
        string? nextMatchAo;

        [ObservableProperty]
        bool isAkaWinner;

        [ObservableProperty]
        bool isAoWinner;

        public Action<object?, System.ComponentModel.PropertyChangedEventArgs> StatePropertyChanged;

        public ExternalBoardState()
        {
            PropertyChanged += (sender, e) => StatePropertyChanged?.Invoke(sender, e);
        }
    }
}
