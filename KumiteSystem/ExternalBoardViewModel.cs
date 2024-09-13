using CommunityToolkit.Mvvm.ComponentModel;
using LanguageLibrary;
using SharedComponentsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KumiteSystem
{
    public partial class ExternalBoardViewModel : ObservableObject
    {
        [ObservableProperty]
        ExternalBoardState state;

        [ObservableProperty]
        bool isNextMatchVisible;

        [ObservableProperty]
        string tatamiText;

        [ObservableProperty]
        string currentAka;

        [ObservableProperty]
        string currentAo;

        public ExternalBoardViewModel(ExternalBoardState state)
        {
            State = state;

            var settings = UserSettings.GetUserSettings();
            State.TatamiNumber = settings.Tatami;
            IsNextMatchVisible = settings.IsNextMatchShownOnExternalBoard;
            TatamiText = $"{Resources.Tatami} {State.TatamiNumber}";

            try
            {
                var splitted = State.CategoryName?.Split(new char[] { ' ' }, 2);
                if (splitted != null)
                    State.CategoryName = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }
            try
            {
                var splitted = State.CurrentMatchAka?.Split(' ', 2);
                if (splitted != null)
                    CurrentAka = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }
            try
            {
                var splitted = State.CurrentMatchAo?.Split(' ', 2);
                if (splitted != null)
                    CurrentAo = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }

            State.StatePropertyChanged += ExternalBoardViewModel_PropertyChanged;
        }

        private void ExternalBoardViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(State.CurrentMatchAka))
                try
                {
                    var splitted = State.CurrentMatchAka?.Split(' ', 2);
                    if (splitted != null)
                        CurrentAka = $"{splitted[0]}\n{splitted[1]}";
                }
                catch { }
            if (e.PropertyName == nameof(State.CurrentMatchAo))
                try
                {
                    var splitted = State.CurrentMatchAo?.Split(' ', 2);
                    if (splitted != null)
                        CurrentAo = $"{splitted[0]}\n{splitted[1]}";
                }
                catch { }

            /*if(e.PropertyName == nameof(State.CurrentMatchAka) || e.PropertyName == nameof(State.CurrentMatchAo))
            {
                if (CurrentAka.Length > CurrentAo.Length)
                    for (int i = 0; i < CurrentAka.Length - CurrentAo.Length; i++)
                        CurrentAo += "|";
                else if (CurrentAka.Length < CurrentAo.Length)
                    for (int i = 0; i < CurrentAo.Length - CurrentAka.Length; i++)
                        CurrentAka += "|";
            }*/
        }
    }
}
