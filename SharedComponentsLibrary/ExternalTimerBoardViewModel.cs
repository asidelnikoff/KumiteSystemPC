using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary
{
    public partial class ExternalTimerBoardViewModel : ObservableObject
    {
        [ObservableProperty]
        ExternalTimerBoardState state;

        public ExternalTimerBoardViewModel(ExternalTimerBoardState state)
        {
            State = state;
        }
    }
}
