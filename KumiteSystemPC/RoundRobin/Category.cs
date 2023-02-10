using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundRobin
{
    public class Category : TournamentsBracketsBase.ICategory
    {
        public List<TournamentsBracketsBase.ICompetitor> Competitors { get; set; }
        public List<TournamentsBracketsBase.IRound> Rounds { get; set; }
        public List<TournamentsBracketsBase.ICompetitor> Winners { get; set; }

        public event TournamentsBracketsBase.NxtMatchHandler HaveNxtMatch;
        public event TournamentsBracketsBase.CategoryResultsHandler HaveCategoryResults;

        int curRound = -1, curMatch = -1;
        List<int> nxtMatch;

        public Category(List<Competitor> competitors)
        {
            Competitors = new List<TournamentsBracketsBase.ICompetitor>(competitors);
            Rounds = new List<TournamentsBracketsBase.IRound>();
        }
        public Category()
        {
            Competitors = new List<TournamentsBracketsBase.ICompetitor>();
            Rounds = new List<TournamentsBracketsBase.IRound>();
        }

        public bool isCurMFinished()
        {
            if (curRound >= 0 && curMatch >= 0 && curRound < Rounds.Count()) return Rounds[curRound].Matches[curMatch].isFinished;
            else return true;
        }

        public bool isCategoryFinished()
        {
            foreach (var r in Rounds)
            {
                foreach (var m in r.Matches)
                {
                    if (!m.isFinished) return false;
                }
            }

            return true;
        }

            public void GenerateBrackets()
        {
            int count = Competitors.Count + Competitors.Count % 2;
            List<Competitor> pool_1 = new List<Competitor>();
            List<Competitor> pool_2 = new List<Competitor>();
            for(int i = 0; i < count/2; i++)
                pool_1.Add(Competitors[i] as Competitor);
            for(int i = count/2; i < Competitors.Count; i++)
                pool_2.Add(Competitors[i] as Competitor);
            if (count > Competitors.Count) pool_2.Add(new Competitor(true));

            Rounds.Add(new Round());
            var _round = Rounds[Rounds.Count - 1];
            _round.ID = 0;
            for (int i = 0; i < pool_1.Count; i++)
            {
                _round.Matches.Add(new Match(new Competitor(pool_1[i]), new Competitor(pool_2[i]), i + 1));
            }

            for(int i = 1; i < count - 1; i++)
            {
                Rounds.Add(new Round());
                var round = Rounds[Rounds.Count - 1];
                round.ID = i;
                var first_in_second = pool_2[0];
                var last_in_first = pool_1[pool_1.Count - 1];
                for (int j = 1; j < pool_1.Count - 1; j++)
                    pool_1[j + 1] = pool_1[j];
                pool_1[1] = first_in_second;

                for (int k = 0; k < pool_2.Count - 1;  k++)
                    pool_2[k] = pool_2[k + 1];
                pool_2[pool_2.Count - 1] = last_in_first;

                for(int j = 0; j < pool_1.Count; j++)
                    round.Matches.Add(new Match(new Competitor(pool_1[j]), new Competitor(pool_2[j]), j + 1));
            }

            UpdateAllRounds();
        }

        public void UpdateAllRounds()
        {
            for (int round = 1; round < Rounds.Count; round++)
            {
                List<Competitor> currentRoundWinners = new List<Competitor>();
                foreach (var m in Rounds[round - 1].Matches)
                    if (m.Winner != null) currentRoundWinners.Add(m.Winner as Competitor);
                for (int i = round; i < Rounds.Count; i++)
                {
                    foreach (var m in Rounds[i].Matches)
                    {
                        if (currentRoundWinners.Contains(m.AKA))
                            (m.AKA as Competitor).TotalScore += 3;
                        if (currentRoundWinners.Contains(m.AO))
                            (m.AO as Competitor).TotalScore += 3;
                    }
                }
            }
        }

        public TournamentsBracketsBase.IMatch GetMatch(int mID, int rID)
        {
            if (rID < Rounds.Count())
            {
                curRound = rID;
                curMatch = mID;
                GetNext();
                return Rounds[curRound].Matches[curMatch];
            }
            else return null;
        }

        public void FinishCurrentMatch()
        {
            if (curRound < Rounds.Count())
            {
                if (Rounds[curRound].Matches[curMatch].Winner == null) Rounds[curRound].Matches[curMatch].CheckWinner();
                if (Rounds[curRound].Matches[curMatch].Winner != null) { Rounds[curRound].Matches[curMatch].isFinished = true; }
                UpdateRound(curRound);
            }

            bool isAll = true;
            foreach (var r in Rounds)
            {
                if (!r.IsFinished()) { isAll = false; break; }
            }

            if (isAll)
            {
                FormResults();
                ShowResults();
            }
        }

        void FormResults()
        {
            Winners = new List<TournamentsBracketsBase.ICompetitor>();
            var tempList = new List<Competitor>();
            foreach (var m in Rounds[Rounds.Count - 1].Matches)
            {
                if(!m.AKA.IsBye) tempList.Add(m.AKA as Competitor);
                if (!m.AO.IsBye) tempList.Add(m.AO as Competitor);
            }
            tempList = tempList.OrderByDescending(a => a.TotalScore).ToList();
            Winners.Add(tempList[0]);
            Winners.Add(tempList[1]);
            if(tempList.Count > 2) Winners.Add(tempList[2]);
            if(tempList.Count > 3) Winners.Add(tempList[3]);
            /* Winners.Add(Rounds[Rounds.Count() - 1].Matches[Rounds[Rounds.Count() - 1].Matches.Count() - 1].Winner);
             Winners.Add(Rounds[Rounds.Count() - 1].Matches[Rounds[Rounds.Count() - 1].Matches.Count() - 1].Looser);
             if (RepechageAKA != null) Winners.Add(RepechageAKA.Winner);
             if (RepechageAO != null) Winners.Add(RepechageAO.Winner);
             if (BronzeMatch != null) Winners.Add(BronzeMatch.Winner);*/
        }
        void ShowResults()
        {
            Console.WriteLine("-----------------------\nCATEGORY RESULTS\n-----------------------");
            Console.WriteLine($"1: {Winners[0]}");
            Console.WriteLine($"2: {Winners[1]}");
            if (Winners.Count() > 2 && Winners[2] != null) Console.WriteLine($"3: {Winners[2]}");
            if (Winners.Count() > 3 && Winners[3] != null) Console.WriteLine($"3: {Winners[3]}");

            HaveCategoryResults?.Invoke(Winners);
        }

        public void GetNext()
        {
            nxtMatch = GetNxtDefaultFull();

            Match nxt = new Match(new Competitor(), new Competitor(), 1);
            if (nxtMatch[0] != -1 && nxtMatch[1] != -1)
            {
                Console.WriteLine($"{nxtMatch[0]} {nxtMatch[1]}");
                Console.WriteLine($"{Rounds[nxtMatch[0]].Matches[nxtMatch[1]]}");
                nxt = Rounds[nxtMatch[0]].Matches[nxtMatch[1]] as Match;

            }
            HaveNxtMatch?.Invoke(nxtMatch[0], nxtMatch[1], nxt);
        }

        List<int> GetNxtDefaultFull()
        {
            if (curRound < Rounds.Count - 1)
            {
                List<int> res = new List<int>() { -1, -1 };

                int iM = curMatch + 1, iR = curRound;
                if (iM >= Rounds[iR].Matches.Count) { iR++; iM = 0; }
                if (Rounds[iR].Matches[iM].isFinished) { iM++; if (iM >= Rounds[iR].Matches.Count) { iR++; iM = 0; } }

                Match match = Rounds[iR].Matches[iM] as Match;
                while (!match.isAllCompetitors())
                {
                    iM++;
                    if (iM >= Rounds[iR].Matches.Count) { iR++; iM = 0; }
                    if (iR >= Rounds.Count) { iR = curRound; iM = 0; }

                    match = Rounds[iR].Matches[iM] as Match;
                }
                res[0] = iR; res[1] = iM;
                if (Rounds[curRound].Matches[curMatch] == Rounds[iR].Matches[iM] || Rounds[iR].Matches[iM].isFinished) 
                { return new List<int>() { -1, -1 }; }

                return res;
            }
            else return new List<int>() { -1, -1 };
        }

        public void UpdateRound(int round)
        {
            var comp = Rounds[round].Matches[curMatch].Winner as Competitor;

            for (int i = round; i < Rounds.Count; i++)
            {
                foreach (var m in Rounds[i].Matches)
                {
                    if (comp.Equals(m.AKA as Competitor))
                    {
                        Console.WriteLine("Adding score AKA");
                        (m.AKA as Competitor).TotalScore += 3;
                    }
                    else if (comp.Equals(m.AO as Competitor))
                    {
                        Console.WriteLine("Adding score AO");
                        (m.AO as Competitor).TotalScore += 3;
                    }
                }
            }
        }

        public int GetCurMatchID()
        {
            return curMatch;
        }
    }
}
