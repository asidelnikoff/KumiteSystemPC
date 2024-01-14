using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary
{
    public partial class UserSettings : ObservableObject
    {
        [ObservableProperty]
        string dataPath;

        [ObservableProperty]
        string databasePath;

        [ObservableProperty]
        string endOfMatchSound;

        [ObservableProperty]
        string warningSound;

        [ObservableProperty]
        int externalMonitorIndex;

        [ObservableProperty]
        int tatami;

        [ObservableProperty]
        bool isAutoLoadNextMatchEnabled;

        [ObservableProperty]
        bool isNextMatchShownOnExternalBoard;

        [ObservableProperty]
        Language language;

    }
}
