using CommunityToolkit.Mvvm.ComponentModel;
using LanguageLibrary;
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

        public ExternalBoardViewModel(ExternalBoardState state)
        {
            State = state;

            State.TatamiNumber = Properties.Settings.Default.Tatami;
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
                    State.CurrentMatchAka = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }
            try
            {
                var splitted = State.CurrentMatchAo?.Split(' ', 2);
                if (splitted != null)
                    State.CurrentMatchAo = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }

            IsNextMatchVisible = Properties.Settings.Default.IsNextMatchShownOnExternalBoard;

            PropertyChanged += ExternalBoardViewModel_PropertyChanged;
        }

        private void ExternalBoardViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                var splitted = State.CurrentMatchAka?.Split(' ', 2);
                if (splitted != null)
                    State.CurrentMatchAka = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }
            try
            {
                var splitted = State.CurrentMatchAo?.Split(' ', 2);
                if (splitted != null)
                    State.CurrentMatchAo = $"{splitted[0]}\n{splitted[1]}";
            }
            catch { }
        }
    }
}
