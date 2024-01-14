using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundRobin
{
    public class Competitor : TournamentTree.Competitor
    {
        public int TotalScore { get; set; }
        public Competitor() : base() { }
        public Competitor(Competitor competitor) : base(competitor) 
        { TotalScore = competitor.TotalScore; }
        public Competitor(TournamentTree.Competitor competitor) : base(competitor) 
        { TotalScore = 0; }

        public Competitor(bool isBye = false, int id = -1, string FName = "", string LName = "", string _Club = "", int totalScore = 0,int score = 0, int fc1 = 0, int fc2 = 0, int status = 0) :
            base(isBye,id,FName,LName,_Club,score,fc1,fc2,status)
        {
            TotalScore = totalScore;
        }

        public override int GetHashCode()
        {
            return ($"{ID}{FirstName}{LastName}").GetHashCode();
        }

    }
}
