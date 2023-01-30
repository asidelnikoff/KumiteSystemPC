using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentsBracketsBase
{
    public delegate void CheckWinnerDelegate(bool isTimeUp = false);
    public interface ICompetitor
    {
        /// <Fouls>
        /// 1 - C,
        /// 2 - K,
        /// 3 - HC,
        /// 4 - H
        /// </Fouls>


        /// <Status>
        /// 0 - Active
        /// 1 - KIKEN
        /// 2 - SHIKAKU
        /// </Status>

        event CheckWinnerDelegate Check_Winner;


         int ID { get; }
         string FirstName { get; set; }
         string LastName { get; set; }
         string Club { get; }
         int ScoreProperty {get; }

        List<int> AllScores { get; }
         bool Senshu { get; set; }
         int Fouls_C1 { get;  }
         int Fouls_C2 { get; }
         int Status { get; }
         bool IsBye { get;}

        void Swap(ICompetitor competitor);
        void AddPoints(int points);
        void SetStatus(int status);
        void SetFoulsC1(int fouls);
        void SetFoulsC2(int fouls);
        void SetScore(int score);
        void ResetCompetitor();
        string GetName();
        string GetFoulsC1();
        string GetFoulsC2();
    }
}
