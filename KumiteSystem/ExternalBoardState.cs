using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentsBracketsBase;
using static System.Windows.Forms.AxHost;

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

        public ExternalBoardState()
        {
            PropertyChanged += ExternalBoardState_PropertyChanged;
        }

        private void ExternalBoardState_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                var splitted = CurrentMatchAka?.Split(' ', 2);
                if (splitted != null)
                    CurrentMatchAka = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }
            try
            {
                var splitted = CurrentMatchAo?.Split(' ', 2);
                if (splitted != null)
                    CurrentMatchAo = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }
        }
    }
}
