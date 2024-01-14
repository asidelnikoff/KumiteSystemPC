using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace SharedComponentsLibrary
{
    public class Timer
    {
        System.Diagnostics.Stopwatch stopWatch;
        TimeSpan timerTime;

        private TimeSpan remainTime;
        private int factor1 = (int)Math.Pow(10, 7);
        private int factor2 = (int)Math.Pow(10, 5);
        public TimeSpan RemainTime
        {
            get
            {
                if (!atoshibaraku)
                    return new TimeSpan((long)Math.Round((1.0 * remainTime.Ticks / factor1)) * factor1);
                else
                    return new TimeSpan((long)Math.Round((1.0 * remainTime.Ticks / factor2)) * factor2);
            }
        }

        bool atoshibaraku;

        public Action OnTimerFinished;
        public Action<TimeSpan> OnTimeUpdated;
        public Action OnAtoshiBaraku;

        public bool IsTimeUp
        {
            get => RemainTime <= TimeSpan.Zero;
        }

        public Timer()
        {
            stopWatch = new System.Diagnostics.Stopwatch();
            remainTime = new TimeSpan();
        }

        public Timer(TimeSpan timerTime)
        {
            stopWatch = new System.Diagnostics.Stopwatch();
            remainTime = new TimeSpan();
            SetTimer(timerTime.Minutes, timerTime.Seconds);
        }

        public Timer(int min, int sec)
        {
            stopWatch = new System.Diagnostics.Stopwatch();
            remainTime = new TimeSpan();
            SetTimer(min, sec);
        }

        public bool IsRunning() => stopWatch.IsRunning;

        public void StartTimer()
        {
            if (RemainTime > TimeSpan.Zero)
            {
                stopWatch.Start();
                ControlTime();
            }
        }

        public void StopTimer()
        {
            stopWatch.Stop();
        }

        public void ResetTimer()
        {
            SetTimer(timerTime.Minutes, timerTime.Seconds);
        }

        public void SetTimer(int min, int sec)
        {
            if (!stopWatch.IsRunning)
            {
                atoshibaraku = false;

                if (sec > 60)
                {
                    min = sec / 60;
                    sec -= min * 60;
                }

                timerTime = new TimeSpan(0, min, sec);
                remainTime = timerTime;

                atoshibaraku = remainTime < TimeSpan.FromSeconds(15);

                OnTimeUpdated?.Invoke(RemainTime);

                stopWatch.Reset();
            }
        }

        public void SetTimer(TimeSpan timerTime) => SetTimer(timerTime.Minutes, timerTime.Seconds);

        private async void ControlTime()
        {
            do
            {
                TimeSpan ts = stopWatch.Elapsed;

                OnTimeUpdated?.Invoke(RemainTime);
                remainTime = timerTime - ts;

                if (RemainTime <= TimeSpan.Zero)
                    TimerFinished();

                if (RemainTime <= TimeSpan.FromSeconds(15) && !atoshibaraku)
                    AtoshiBaraku();

                await Task.Delay(10);

            } while (stopWatch.IsRunning);

            if (RemainTime <= TimeSpan.Zero)
                OnTimeUpdated?.Invoke(TimeSpan.Zero);
        }
        void TimerFinished()
        {
            stopWatch.Stop();
            OnTimerFinished?.Invoke();
        }
        void AtoshiBaraku()
        {
            atoshibaraku = true;
            OnAtoshiBaraku?.Invoke();
        }

        public override string ToString()
        {
            return RemainTime.ToString();
        }
    }
}
