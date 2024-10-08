﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary
{
    public partial class ExternalTimerBoardState : ObservableObject
    {
        public UserSettings Settings { get; set; }

        [ObservableProperty]
        TimeSpan remainTime;

        [ObservableProperty]
        bool isAtoshiBaraku;
    }
}
