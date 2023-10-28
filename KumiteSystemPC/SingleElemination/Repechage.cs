using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentTree
{
    public class Repechage
    {
        public List<Competitor> Competitors { get; set; }
        List<Round> Rounds { get; set; } 
        public List<TournamentsBracketsBase.IMatch> Matches { get; set; }
        public Competitor Winner { get; set; }
        int curRound=0;

        public Repechage(List<Competitor> competitors)
        {
            Competitors = new List<Competitor>(competitors);
        }
        public Repechage()
        {
            Competitors = new List<Competitor>();
            Matches = new List<TournamentsBracketsBase.IMatch>();
        }
        public void Generate()
        {
            Rounds = new List<Round>();
            Matches = new List<TournamentsBracketsBase.IMatch>();
            if (Competitors.Count() > 0)
            {
                Round round1 = new Round();
                Competitor aka1 = Competitors[0];
                Competitor ao1 = new Competitor(true);
                Matches.Add(new Match(aka1, ao1, Matches.Count()+1));
                round1.Matches.Add(new Match(aka1, ao1, 0));
                
                Rounds.Add(round1);
                int num = Competitors.Count();
                while (Rounds.Count() < num)
                {
                    int count = Rounds.Count();
                    Competitor ao = Competitors[count];
                    Competitor aka = Matches[count-1].Winner as Competitor;
                    Round res = new Round();
                    Matches.Add(new Match(aka, ao, Matches.Count()+1));
                    res.Matches.Add(new Match(aka, ao, Matches.Count()+1));
                    res.ID = count;
                    Rounds.Add(res);
                }
            }
            if (Matches.Count() == 1) { Winner = Matches[0].Winner as Competitor; }
        }
        public bool IsFinished()
        {
            if (Winner == null) { return false; }
            return true;
        }

        public void GetMatch()
        {
            try
            {
                while (curRound < Rounds.Count())
                {
                    if (Matches[curRound].Winner == null)
                    {
                        Console.WriteLine($"Current match: {Matches[curRound]}\nPlease set winner: ");
                        int w = Convert.ToInt32(Console.ReadLine());
                        Matches[curRound].SetWinner(w);
                        if (curRound + 1 < Matches.Count()) UpdateRound(curRound + 1);
                    }
                    curRound++;
                }
                Winner = Matches[Matches.Count() - 1].Winner as Competitor;
            }
            catch { }
        }
        public void UpdateRound(int index)
        {
            if (index < Matches.Count())
            {
                Matches[index].AKA = (Matches[index - 1].Winner);
                if (index + 1 == Matches.Count()) { Winner = Matches[index].AKA as Competitor; }
            }
            else if (index == Matches.Count()) { Winner = Matches[index - 1].Winner as Competitor; }
        }

        public void ShowRepechage()
        {
            try
            {
                    foreach (var m in Matches)
                    {
                        Console.WriteLine($"{m}");
                    }
            }
            catch { }



        }

    }
}
