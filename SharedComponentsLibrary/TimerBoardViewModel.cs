using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SharedComponentsLibrary
{
    public partial class TimerBoardViewModel : ObservableObject
    {
        [ObservableProperty]
        Timer timer;

        [ObservableProperty]
        bool isTimerRunning;

        [ObservableProperty]
        bool isAtoshiBaraku;

        [ObservableProperty]
        int timerSecondsInput;

        [ObservableProperty]
        int timerMinutesInput;

        ExternalTimerBoard externalBoard;
        ExternalTimerBoardState externalTimerBoardState;

        [ObservableProperty]
        bool isExternalBoardOpened;

        public TimerBoardViewModel()
        {
            Timer = new Timer(1, 0);
            TimerMinutesInput = 1;
            TimerSecondsInput = 0;
            IsTimerRunning = false;
            IsAtoshiBaraku = false;

            externalTimerBoardState = new ExternalTimerBoardState();

            Timer.OnTimeUpdated += (a) => OnPropertyChanged(nameof(Timer));
            Timer.OnAtoshiBaraku += Timer_OnAtoshiBaraku;
            Timer.OnTimerFinished += Timer_OnTimeFinished;

            PropertyChanged += TimerBoardViewModel_PropertyChanged;
        }

        public TimerBoardViewModel(bool isKnockout)
        {
            Timer = new Timer(0, 11);
            TimerMinutesInput = 0;
            TimerSecondsInput = 10;
            IsTimerRunning = false;
            IsAtoshiBaraku = true;

            externalTimerBoardState = new ExternalTimerBoardState();

            Timer.OnTimeUpdated += (a) => OnPropertyChanged(nameof(Timer));
            Timer.OnAtoshiBaraku += Timer_OnAtoshiBaraku;
            Timer.OnTimerFinished += Timer_OnTimeFinished;

            PropertyChanged += TimerBoardViewModel_PropertyChanged;

            StartStopTimer();
        }

        private void TimerBoardViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            externalTimerBoardState.RemainTime = Timer.RemainTime;

            if(e.PropertyName == nameof(IsAtoshiBaraku))
                externalTimerBoardState.IsAtoshiBaraku = IsAtoshiBaraku;
        }

        private void Timer_OnAtoshiBaraku()
        {
            IsAtoshiBaraku = true;
        }

        private void Timer_OnTimeFinished()
        {
            OnPropertyChanged(nameof(Timer));
            IsTimerRunning = false;
            externalBoard?.Close();
            isExternalBoardOpened = false;
        }

        [RelayCommand]
        private void Close()
        {
            externalBoard?.Close();
        }

        [RelayCommand]
        private void OpenExternalBoard()
        {
            if(IsExternalBoardOpened)
                externalBoard?.Close();
            else
            {
                externalTimerBoardState.RemainTime = Timer.RemainTime;
                externalTimerBoardState.IsAtoshiBaraku = IsAtoshiBaraku;
                externalBoard = new ExternalTimerBoard(externalTimerBoardState);
                externalBoard.Loaded += (sender, e) => IsExternalBoardOpened = true;
                externalBoard.Closed += (sender, e) => IsExternalBoardOpened = false;
                externalBoard.Show();
            }
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
                if (Timer.IsTimeUp)
                    SetTimer();
                if(!IsExternalBoardOpened)
                    OpenExternalBoard();

                Timer.StartTimer();
                if (Timer.IsRunning())
                    IsTimerRunning = true;
            }
        }

        [RelayCommand]
        private void SetTimer()
        {
            Timer.SetTimer(TimerMinutesInput, TimerSecondsInput);
            IsAtoshiBaraku = Timer.RemainTime <= TimeSpan.FromSeconds(15);
            OnPropertyChanged(nameof(Timer));
        }

        [RelayCommand]
        private void ResetTimer()
        {
            Timer.ResetTimer();
            IsAtoshiBaraku = Timer.RemainTime <= TimeSpan.FromSeconds(15);
        }
    }
}
