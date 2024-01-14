using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentsBracketsBase;

namespace KumiteSystem
{
    public partial class ExternalBoardState : ObservableObject
    {
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
    }
}
