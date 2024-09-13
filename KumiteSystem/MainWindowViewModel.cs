using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TournamentsBracketsBase;
using SharedComponentsLibrary;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Microsoft.Win32;
using Microsoft.VisualBasic;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using LanguageLibrary;

namespace KumiteSystem
{
    public partial class MainWindowViewModel : InternalBoardViewModel
    {

        [ObservableProperty]
        Timer timer;

        [ObservableProperty]
        bool isTimerRunning;

        [ObservableProperty]
        bool isAtoshiBaraku;

        [ObservableProperty]
        Visibility millisecondsVisibility;

        [ObservableProperty]
        int timerSecondsInput;

        [ObservableProperty]
        int timerMinutesInput;

        Window externalBoard;

        ExternalBoardState externalBoardState;

        
        System.Media.SoundPlayer warningSound;

        public MainWindowViewModel(DBService dbService, UserSettings setings) : base(dbService, setings)
        {
            if (userSettings.WarningSound != "")
                warningSound = new System.Media.SoundPlayer(userSettings.WarningSound);

            MillisecondsVisibility = Visibility.Collapsed;

            Timer = new Timer(0, 0);
            Timer.OnTimeUpdated += (a) => OnPropertyChanged(nameof(Timer));
            Timer.OnAtoshiBaraku += Timer_OnAtoshiBaraku;
            Timer.OnTimerFinished += Timer_OnTimeFinished;

            PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        public MainWindowViewModel() : base()
        {
            SetupDbService();

            if (userSettings.WarningSound != "")
                warningSound = new System.Media.SoundPlayer(userSettings.WarningSound);

            MillisecondsVisibility = Visibility.Collapsed;

            Timer = new Timer(0, 0);
            Timer.OnTimeUpdated += (a) => OnPropertyChanged(nameof(Timer));
            Timer.OnAtoshiBaraku += Timer_OnAtoshiBaraku;
            Timer.OnTimerFinished += Timer_OnTimeFinished;

            PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(CurrentMatch))
                if (externalBoardState != null)
                {
                    externalBoardState.CurrentMatchAo = CurrentMatch.AO?.ToString();
                    externalBoardState.CurrentMatchAka = CurrentMatch.AKA?.ToString();
                    externalBoardState.ScoreAka = CurrentMatch.AKA?.Score;
                    externalBoardState.ScoreAo = CurrentMatch.AO?.Score;
                    externalBoardState.AkaSenshu = CurrentMatch.AKA?.Senshu;
                    externalBoardState.AoSenshu = CurrentMatch.AO?.Senshu;
                    externalBoardState.FoulsC1Aka = CurrentMatch.AKA?.Fouls_C1;
                    externalBoardState.FoulsC1Ao = CurrentMatch.AO?.Fouls_C1;
                }

            if (e.PropertyName == nameof(Timer))
                if (externalBoardState != null)
                {
                    externalBoardState.RemainTime = Timer.RemainTime;
                    externalBoardState.IsAtoshiBaraku = IsAtoshiBaraku;
                }

            if (e.PropertyName == nameof(IsTimerRunning))
            {
                if (IsTimerRunning)
                    AddInfoToLog($"{Resources.StartTmier}. {Resources.Timeleft}: {String.Format("{0:mm}:{0:ss}", Timer.RemainTime)}");
                else
                    AddInfoToLog($"{Resources.StopTmier}. {Resources.Timeleft}: {String.Format("{0:mm}:{0:ss}", Timer.RemainTime)}");
            }

            
        }

        protected override void CategoryViewer_GotMatch(RoundDTO round, IMatch match)
        {
            ResetTimer();
            base.CategoryViewer_GotMatch(round, match);
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
                    IsAtoshiBaraku = IsAtoshiBaraku,
                    AkaSenshu = CurrentMatch?.AKA?.Senshu,
                    AoSenshu = CurrentMatch?.AO?.Senshu,
                    FoulsC1Aka = CurrentMatch?.AKA?.Fouls_C1,
                    FoulsC1Ao = CurrentMatch?.AO?.Fouls_C1,
                    RemainTime = Timer.RemainTime,
                    NextMatchAka = NextMatch?.AKA?.ToString(),
                    NextMatchAo = NextMatch?.AO?.ToString(),
                    Settings = userSettings
                };
                if(CurrentMatch.Winner != null)
                {
                    externalBoardState.IsAkaWinner = CurrentMatch?.AKA?.Equals(CurrentMatch.Winner) == true;
                    externalBoardState.IsAoWinner = CurrentMatch?.AO?.Equals(CurrentMatch.Winner) == true;
                }
                else
                {
                    externalBoardState.IsAkaWinner = false;
                    externalBoardState.IsAoWinner = false;
                }
                externalBoard = SelectExternalBoardVersion(externalBoardState);
                externalBoard.Loaded += (sender, e) => IsExternalBoardOpened = true;
                externalBoard.Closed += (sender, e) => IsExternalBoardOpened = false;
                externalBoard.Show();
            }
        }

        private Window SelectExternalBoardVersion(ExternalBoardState state)
        {
            if (userSettings.ExternaBoardDesign == 0)
                return new ExternalBoard(state);
            if (userSettings.ExternaBoardDesign == 1)
                return new ExternalBoard2(state);

            return new ExternalBoard2(state);
        }

        TimerBoard knockoutTimerBoard;
        [RelayCommand]
        private void StartKnockoutTime()
        {
            if (knockoutTimerBoard?.IsInitialized == true)
                knockoutTimerBoard?.Close();
            knockoutTimerBoard = new TimerBoard(true);
            knockoutTimerBoard.Show();
        }

        [RelayCommand]
        private new void ResetMatch()
        {
            if (IsTimerRunning)
                return;

            base.ResetMatch();
            ResetTimer();
        }

        protected override void FinishMatch()
        {
            base.FinishMatch();
            SaveLogFile();
        }

        private void Timer_OnAtoshiBaraku()
        {
            warningSound?.Play();
            MillisecondsVisibility = Visibility.Visible;
            IsAtoshiBaraku = true;
        }

        private void Timer_OnTimeFinished()
        {
            MillisecondsVisibility = Visibility.Collapsed;
            IsTimerRunning = false;
            IsAtoshiBaraku = false;
            OnPropertyChanged(nameof(Timer));
            
            CurrentMatch.CheckWinner(true);
        }

        [RelayCommand]
        private void StartStopTimer()
        {
            if (Timer.IsRunning())
            {
                Timer.StopTimer();
                IsTimerRunning = false;
            }
            else
            {
                Timer.StartTimer();
                if(Timer.IsRunning())
                    IsTimerRunning = true;
            }
        }

        [RelayCommand]
        private void SetTimer()
        {
            Timer.SetTimer(TimerMinutesInput, TimerSecondsInput);
            MillisecondsVisibility = Timer.RemainTime <= TimeSpan.FromSeconds(15) ? Visibility.Visible : Visibility.Collapsed;
            IsAtoshiBaraku = Timer.RemainTime <= TimeSpan.FromSeconds(15);
            OnPropertyChanged(nameof(Timer));
        }

        [RelayCommand]
        private void ResetTimer()
        {
            Timer.ResetTimer();
            MillisecondsVisibility = Timer.RemainTime <= TimeSpan.FromSeconds(15) 
                && Timer.RemainTime > TimeSpan.FromSeconds(0)
                ? Visibility.Visible : Visibility.Collapsed;
            IsAtoshiBaraku = Timer.RemainTime <= TimeSpan.FromSeconds(15);
        }

        protected override void ShowWinnerOnExternalBoard(ICompetitor winner)
        {
            if (externalBoardState != null)
            {
                if (CurrentMatch?.AKA?.Equals(winner) == true)
                {
                    externalBoardState.IsAkaWinner = true;
                    externalBoardState.IsAoWinner = false;
                }
                else if (CurrentMatch?.AO?.Equals(winner) == true)
                {
                    externalBoardState.IsAoWinner = true;
                    externalBoardState.IsAkaWinner = false;
                }

            }
        }

        protected override void ShowCategoryNameOnExternalBoard(string name) 
        {
            if (externalBoardState != null)
                externalBoardState.CategoryName = name;
        }

        protected override void ResetExternalBoardState()
        {
            ShowWinnerOnExternalBoard(CurrentMatch.Winner);
        }

        [RelayCommand]
        private async Task AddPointsToCompetitor(object[] parameters)
        {
            try
            {
                ICompetitor comp = parameters[0] as ICompetitor;
                int points = (int)parameters[1];

                int prevScore = comp.Score;
                comp.AddPoints(points);
                if (prevScore == comp.Score)
                    return;

                string action = points > 0 ? Resources.add : Resources.remove;
                string color = "";
                if (comp.Equals(CurrentMatch.AKA))
                    color = "AKA";
                else if (comp.Equals(CurrentMatch.AO))
                    color = "AO";
                AddInfoToLog($"{color} {action} {Resources.point} {points}. {Resources.Points}: {comp.Score}");

                CurrentMatch.CheckWinner(Timer.IsTimeUp);
                OnPropertyChanged(nameof(CurrentMatch));
            }
            catch (Exception ex)
            {
                await Helpers.DisplayMessageDialog($"{Resources.SmthWentWrong}\n{ex.Message}", Resources.Error);
            }
        }


        [RelayCommand]
        private void SetSanctionToCompetitor(object[] parameters)
        {
            ICompetitor comp = parameters[0] as ICompetitor;
            int sanctions = (int)parameters[1];
            if (comp.Fouls_C1 >= sanctions)
                return;
            comp.SetFoulsC1(sanctions);

            OnPropertyChanged(nameof(CurrentMatch));

            if (comp?.Equals(CurrentMatch.AKA) == true)
                AddInfoToLog($"AKA {Resources._set} {Resources.sanction}: {comp.GetFoulsC1()}");
            else if (comp?.Equals(CurrentMatch.AO) == true)
                AddInfoToLog($"AO {Resources._set} {Resources.sanction}: {comp.GetFoulsC1()}");
        }

        [RelayCommand]
        private void RemoveSanctionFromCompetitor(object[] parameters)
        {
            ICompetitor comp = parameters[0] as ICompetitor;
            int sanctions = (int)parameters[1];
            if (comp.Fouls_C1 > sanctions)
            {
                OnPropertyChanged(nameof(CurrentMatch));
                return;
            }

            if (comp?.Equals(CurrentMatch.AKA) == true)
                AddInfoToLog($"AKA {Resources.remove} {Resources.sanction}: {comp.GetFoulsC1()}");
            else if (comp?.Equals(CurrentMatch.AO) == true)
                AddInfoToLog($"AO {Resources.remove} {Resources.sanction}: {comp.GetFoulsC1()}");

            comp.SetFoulsC1(sanctions - 1);

            OnPropertyChanged(nameof(CurrentMatch));
        }

        [RelayCommand]
        private void SetSenshuToCompetitor(ICompetitor comp)
        {
            if (comp == null)
                return;
            comp.Senshu = true;
            if (comp.Equals(CurrentMatch.AKA))
            {
                if (CurrentMatch.AO != null)
                    CurrentMatch.AO.Senshu = false;
            }
            else if (comp.Equals(CurrentMatch.AO))
            {
                if (CurrentMatch.AKA != null)
                    CurrentMatch.AKA.Senshu = false;
            }

            OnPropertyChanged(nameof(CurrentMatch));

            if (comp.Equals(CurrentMatch.AKA))
                AddInfoToLog($"AKA {Resources._set} {Resources.Senshu}");
            else if (comp.Equals(CurrentMatch.AO))
                AddInfoToLog($"AO {Resources._set} {Resources.Senshu}");
        }

        [RelayCommand]
        private void RemoveSenshuFromCompetitor(ICompetitor comp)
        {
            if (comp == null)
                return;
            comp.Senshu = false;
            OnPropertyChanged(nameof(CurrentMatch));

            if (comp.Equals(CurrentMatch.AKA))
                AddInfoToLog($"AKA {Resources.remove} {Resources.Senshu}");
            else if (comp.Equals(CurrentMatch.AO))
                AddInfoToLog($"AO {Resources.remove} {Resources.Senshu}");
        }
    }
}
