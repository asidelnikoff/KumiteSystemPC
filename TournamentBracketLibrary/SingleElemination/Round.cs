using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentTree
{
    public class Round : TournamentsBracketsBase.IRound
    {
        public List<TournamentsBracketsBase.IMatch> Matches { get; set; }
        public int ID { get; set; }
        int CurMatchInd = 0;

        public Round()
        {
            Matches = new List<TournamentsBracketsBase.IMatch>();
        }
        public void FinishMatch(int matchNr = -1) //Finishing Match
        {
            if (matchNr == -1) { Matches[CurMatchInd].CheckWinner(); }
            else { Matches[matchNr].CheckWinner(); }
        }

        public bool IsFinished()
        {
            foreach (var m in Matches)
            {
                if (m.Winner == null) return false;
            }
            return true;
        }

        public override string ToString()
        {
            return $"{Matches.Count()}";
        }
    }
}
