using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentsBracketsBase
{
    public delegate void HaveWinnerHandler();
    public interface IMatch
    {
        int ID { get; set; }
        ICompetitor AKA { get; set; }
        ICompetitor AO { get; set; }
        ICompetitor Winner { get; }
        ICompetitor Looser { get; }

        bool isFinished { get; set; }
        event Action<ICompetitor> HaveWinner;

        bool IsAllCompetitors();
        void SetWinner(int winner, bool setLooser = true);
        void CheckWinner(bool isTimeUP = false);
        void Reset();
    }
}
