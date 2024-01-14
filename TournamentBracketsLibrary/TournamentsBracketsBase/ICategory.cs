using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentsBracketsBase
{
    public delegate void CategoryResultsHandler(List<ICompetitor> winners);
    public delegate void NxtMatchHandler(int round, int match, IMatch nxtMatch);
    public interface ICategory
    {
        List<ICompetitor> Competitors { get; set; }
         List<IRound> Rounds { get; set; }
         List<ICompetitor> Winners { get; set; }
        event CategoryResultsHandler HaveCategoryResults;
        event NxtMatchHandler HaveNxtMatch;
        bool isCurMFinished();
        IMatch GetMatch(int mID, int rID);
        void FinishCurrentMatch();
        void GetNext();
        void GenerateBrackets();
        void UpdateRound(int round);
    }
}
