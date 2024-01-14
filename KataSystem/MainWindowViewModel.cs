using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedComponentsLibrary;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentsBracketsBase;

namespace KataSystem
{
    public partial class MainWindowViewModel : InternalBoardViewModel
    {
        ExternalBoard externalBoard;
        ExternalBoardState externalBoardState;

        [ObservableProperty]
        ObservableCollection<int> judjesCollection;

        [ObservableProperty]
        ObservableCollection<int> judjesList;

        [ObservableProperty]
        int? selectedJudjesAka;

        [ObservableProperty]
        int? selectedJudjesAo;

        [ObservableProperty]
        int judjesNumberInput;

        public MainWindowViewModel() : base()
        {
            LoadSettings();
            SetupDbService();

            JudjesCollection = new ObservableCollection<int>() { 3, 5, 7 };

            PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentMatch))
                if (externalBoardState != null)
                {
                    externalBoardState.CurrentMatchAo = CurrentMatch.AO?.ToString();
                    externalBoardState.CurrentMatchAka = CurrentMatch.AKA?.ToString();
                    externalBoardState.ScoreAka = CurrentMatch.AKA?.Score;
                    externalBoardState.ScoreAo = CurrentMatch.AO?.Score;
                }
            if (e.PropertyName == nameof(JudjesNumberInput))
            {
                JudjesList = new ObservableCollection<int>(Enumerable.Range(0, JudjesNumberInput + 1));
                SelectedJudjesAka = null;
                SelectedJudjesAo = null;
            }

            if(e.PropertyName == nameof(SelectedJudjesAka))
            {
                if (SelectedJudjesAka == null)
                    return;
                CurrentMatch.AKA.SetScore(SelectedJudjesAka.Value);
                SelectedJudjesAo = JudjesNumberInput - SelectedJudjesAka;
                OnPropertyChanged(nameof(CurrentMatch));
                CurrentMatch.CheckWinner(true);
            }

            if(e.PropertyName == nameof(SelectedJudjesAo))
            {
                if (SelectedJudjesAo == null)
                    return;
                CurrentMatch.AO.SetScore(SelectedJudjesAo.Value);
                OnPropertyChanged(nameof(CurrentMatch));
                SelectedJudjesAka = JudjesNumberInput - SelectedJudjesAo;
            }

        }

        [RelayCommand]
        private new void Close()
        {
            base.Close();
            externalBoard?.Close();
        }

        [RelayCommand]
        private void OpenExternalBoard()
        {
            if (IsExternalBoardOpened)
                externalBoard?.Close();
            else
            {
                externalBoardState = new ExternalBoardState()
                {
                    CategoryName = currentCategory?.Name,
                    ScoreAka = CurrentMatch?.AKA?.Score,
                    ScoreAo = CurrentMatch?.AO?.Score,
                    CurrentMatchAka = CurrentMatch?.AKA?.ToString(),
                    CurrentMatchAo = CurrentMatch?.AO?.ToString(),
                    NextMatchAka = NextMatch?.AKA?.ToString(),
                    NextMatchAo = NextMatch?.AO?.ToString(),
                };
                if (CurrentMatch.Winner != null)
                {
                    externalBoardState.IsAkaWinner = CurrentMatch?.AKA?.Equals(CurrentMatch.Winner) == true;
                    externalBoardState.IsAoWinner = CurrentMatch?.AO?.Equals(CurrentMatch.Winner) == true;
                }
                else
                {
                    externalBoardState.IsAkaWinner = false;
                    externalBoardState.IsAoWinner = false;
                }
                externalBoard = new ExternalBoard(externalBoardState);
                externalBoard.Loaded += (sender, e) => IsExternalBoardOpened = true;
                externalBoard.Closed += (sender, e) => IsExternalBoardOpened = false;
                externalBoard.Show();
            }
        }

        protected override void SetupMatch(IMatch match)
        {
            match.HaveWinner += Match_HaveWinner;
        }

        private new async void Match_HaveWinner(ICompetitor winner)
        {
            if (externalBoardState != null)
            {
                externalBoardState.IsAkaWinner = CurrentMatch?.AKA?.Equals(winner) == true;
                externalBoardState.IsAoWinner = CurrentMatch?.AO?.Equals(winner) == true;
            }
            base.Match_HaveWinner(winner);
        }


        [RelayCommand]
        private new void ResetMatch()
        {
            base.ResetMatch();
            SelectedJudjesAo = null;
            SelectedJudjesAka = null;
        }

        protected override void CategoryViewer_GotMatch(RoundDTO round, IMatch match)
        {
            SelectedJudjesAo = null;
            SelectedJudjesAka = null;
            base.CategoryViewer_GotMatch(round, match);
        }

        protected override void LoadSettings()
        {
            var setup = new Settings(null);
            var settings = setup.LoadSettings();
            Settings_SaveSettings(settings);
            setup.Close();
        }
        protected override void Settings_SaveSettings(UserSettings settings)
        {
            Properties.Settings.Default.DataPath = settings.DataPath;
            Properties.Settings.Default.DatabasePath = settings.DatabasePath;
            Properties.Settings.Default.EndOfMatchSound = settings.EndOfMatchSound;
            Properties.Settings.Default.WarningSound = settings.WarningSound;
            Properties.Settings.Default.ExternalScreenIndex = settings.ExternalMonitorIndex;
            Properties.Settings.Default.Tatami = settings.Tatami;
            Properties.Settings.Default.IsAutoLoadNextMatchEnabled = settings.IsAutoLoadNextMatchEnabled;
            Properties.Settings.Default.IsNextMatchShownOnExternalBoard = settings.IsNextMatchShownOnExternalBoard;
            Properties.Settings.Default.Language = settings.Language.CultureInfo;

            Properties.Settings.Default.Save();
        }
    }
}
