using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KataSystem
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
            if (e.PropertyName == nameof(CategoryName))
                try
                {
                    var splitted = CategoryName?.Split(new char[] { ' ' }, 2);
                    if (splitted != null)
                        CategoryName = $"{splitted[0]}\n{splitted[1]}";
                }
                catch { }
        }
    }
}
