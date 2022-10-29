using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentTree
{
    public class Category
    {
        public List<Competitor> Competitors { get; set; }
        public List<Round> Rounds { get; set; }
        public List<Competitor> Winners { get; set; }

        public delegate void RepechageGeneratedHandler();
        public event RepechageGeneratedHandler RepechageGen;

        public delegate void BronzeGenerateHandler();
        public event BronzeGenerateHandler BronzeGen;

        public delegate void CategoryResultsHandler(List<Competitor> winners);
        public event CategoryResultsHandler HaveCategoryResults;

        public delegate void NxtMatchHandler(int round, int match,Match nxtMatch);
        public event NxtMatchHandler HaveNxtMatch;

        int curRound = -1, curMatch = -1;
        List<int> nxtMatch;

        public Repechage RepechageAKA { get; set; }
        public Repechage RepechageAO { get; set; }

       public Match BronzeMatch { get; set; }
       public bool is1third { get; set; }

        public Category(List<Competitor> competitors,bool _is1third=false)
        {
            Competitors = new List<Competitor>(competitors);
            Rounds = new List<Round>();
            is1third = _is1third;
        }
        public Category(bool _is1third = false)
        {
            Competitors = new List<Competitor>();
            Rounds = new List<Round>();
            is1third = _is1third;
        }

        public bool isCurMFinished()
        {
            if (curRound >= 0 && curMatch >= 0 && curRound < Rounds.Count()) return Rounds[curRound].Matches[curMatch].isFinished;
            else if (curMatch >= 0 && curRound == Rounds.Count() && !is1third) return RepechageAKA.Matches[curMatch].isFinished;
            else if (curMatch >= 0 && curRound == Rounds.Count() && is1third) return BronzeMatch.isFinished;
            else if (curMatch >= 0 && curRound == Rounds.Count() + 1) return RepechageAO.Matches[curMatch].isFinished;
            else return true;
        }

        public bool isCategoryFinished()
        {
            foreach(var r in Rounds)
            {
                foreach(var m in r.Matches)
                {
                    if (!m.isFinished) return false;
                }
            }
            
            if (!is1third) return RepechageAKA.IsFinished() && RepechageAO.IsFinished();
            else return BronzeMatch.isFinished;

        }

        public Match GetCurMatch(int mID, int rID)
        {
            if (rID < Rounds.Count())
            {
                curRound = rID;
                curMatch = mID;
                GetNext();
                return Rounds[curRound].Matches[curMatch];
            }
            else if (rID == Rounds.Count())
            {
                curRound = rID;
                curMatch = mID;
                if (!is1third)
                {
                    GetNext();
                    return RepechageAKA.Matches[curMatch];
                }
                else return BronzeMatch;
            }
            else if (rID == Rounds.Count() + 1 && !is1third)
            {
                curRound = rID;
                curMatch = mID;
                GetNext();
                return RepechageAO.Matches[curMatch];
            }
            else return null;
        }

        List<int> GetNxtRep()
        {
            List<int> res = new List<int>() { -1, -1 };
            int r_count = Rounds.Count();
            int tmp_r = -1;
            if (curRound == r_count) { tmp_r = r_count + 1; }
            else if (curRound == r_count + 1 || curRound+2==r_count) { tmp_r = r_count; }

            int iM = 0;
            Match match;

            if (tmp_r == r_count) match = RepechageAKA.Matches[iM];
            else if (tmp_r == r_count + 1) match = RepechageAO.Matches[iM];
            else match = null;
            iM++;
            if(match!=null)
            {
                try
                {
                    if (tmp_r == r_count + 1)
                    {
                        while (match.Winner != null && iM < RepechageAKA.Matches.Count)
                        {
                            match = RepechageAKA.Matches[iM];
                            res[0] = tmp_r-1; res[1] = iM;
                            iM++;
                        }
                        if(iM == RepechageAKA.Matches.Count)
                        {
                            iM = 0;
                            while (match.Winner != null && iM < RepechageAO.Matches.Count)
                            {
                                match = RepechageAO.Matches[iM];
                                res[0] = tmp_r; res[1] = iM;
                                iM++;
                            }
                        }
                        if(res[0] == curRound)
                        {
                            if (res[0] == r_count && res[1]+ 1 == RepechageAKA.Matches.Count) { res[0] = r_count-1;res[1] = 0; }
                            if (res[0] == r_count + 1 && res[1] + 1 == RepechageAO.Matches.Count) { res[0] = r_count-1; res[1] = 0; }
                        }
                    }
                    else if(tmp_r == r_count)
                    {
                        while (match.Winner != null && iM < RepechageAO.Matches.Count)
                        {
                            match = RepechageAO.Matches[iM];
                            res[0] = tmp_r+1; res[1] = iM;
                            iM++;
                        }
                        if (iM == RepechageAO.Matches.Count)
                        {
                            iM = 0;
                            while (match.Winner != null && iM < RepechageAKA.Matches.Count)
                            {
                                match = RepechageAKA.Matches[iM];
                                res[0] = tmp_r; res[1] = iM;
                                iM++;
                            }
                        }
                        if (res[0] == curRound)
                        {
                            if (res[0] == r_count && res[1]+ 1 == RepechageAKA.Matches.Count) { res[0] = r_count - 1; res[1] = 0; }
                            if (res[0] == r_count + 1 && res[1] + 1 == RepechageAO.Matches.Count) { res[0] = r_count - 1; res[1] = 0; }
                        }
                    }
                }
                catch { res[0] = -1; res[1] = -1; }
            }

            /*int r_count = Rounds.Count();
            int iR = r_count + 1;
            if (iR == r_count) { iR = r_count + 1; }
            else if (iR == r_count + 1) { iR = r_count; }
            int iM = 0;
            Match match;
            if (iR == r_count) { match = RepechageAKA.Matches[iM]; }
            else if (iR == r_count + 1) { match = RepechageAO.Matches[iM]; }
            else match = null;
            if (match != null)
            {
                if (iR == r_count && match == RepechageAKA.Matches[curMatch]) 
                {
                    iM++; 
                    if (iM >= RepechageAKA.Matches.Count()) iR = r_count + 1; 
                }
                if (iR == r_count + 1 && match == RepechageAO.Matches[curMatch]) 
                { 
                    iM++;
                    if (iM >= RepechageAKA.Matches.Count()) iR = r_count;
                }
                try
                {
                    /*while (match.Winner != null && (RepechageAKA.Winner == null || RepechageAO.Winner == null))
                    {
                        iM++;
                        if (iR == r_count) { match = RepechageAKA.Matches[iM]; }
                        else if (iR == r_count + 1) { match = RepechageAO.Matches[iM]; }
                        res[0] = iR; res[1] = iM;
                        if (iR == r_count) { iR = r_count + 1; iM = 0; }
                        else if (iR == r_count + 1) { iR = r_count; iM = 0; }
                    }

                }
                catch { res[0] = -1; res[1] = -1; }
            }*/
            return res;
        }

        public void FinishCurMatch()
        {
            //GetNext();
            if (curRound < Rounds.Count())
            {
                if (Rounds[curRound].Matches[curMatch] == null) Rounds[curRound].Matches[curMatch].CheckWinner();
                if (Rounds[curRound].Matches[curMatch].Winner != null) { Rounds[curRound].Matches[curMatch].isFinished = true; }
                UpdateRound(curRound);
            }
            else if (curRound == Rounds.Count())
            {
                if (!is1third)
                {
                    if (RepechageAKA.Matches[curMatch].Winner == null) RepechageAKA.Matches[curMatch].CheckWinner();
                    if (RepechageAKA.Matches[curMatch].Winner != null) { RepechageAKA.Matches[curMatch].isFinished = true; }
                    RepechageAKA.UpdateRound(curMatch + 1);
                }
                else
                {
                    if (BronzeMatch.Winner == null) BronzeMatch.CheckWinner();
                    if (BronzeMatch.Winner != null) BronzeMatch.isFinished = true;
                }
            }
            else if (curRound == Rounds.Count() + 1 && !is1third)
            {
                if (RepechageAO.Matches[curMatch].Winner == null) RepechageAO.Matches[curMatch].CheckWinner();
                if (RepechageAO.Matches[curMatch].Winner != null) { RepechageAO.Matches[curMatch].isFinished = true; }
                RepechageAO.UpdateRound(curMatch + 1);
            }

            if (curRound + 2 == Rounds.Count() && Rounds[curRound].IsFinished())
            { 
                if ((RepechageAKA == null && RepechageAO == null) || (BronzeMatch == null && is1third)) GenerateBronze();
                GetNext();
            }

            bool isAll = true;
            foreach (var r in Rounds)
            {
                if (!r.IsFinished()) { isAll = false; break; }
            }
            if (RepechageAKA != null && RepechageAKA.Matches.Count > 0 && RepechageAKA.Winner == null) { isAll = false; }
            if (RepechageAO != null &&  RepechageAO.Matches.Count > 0 && RepechageAO.Winner == null) { isAll = false; }

            if (curRound + 1 == Rounds.Count() && isAll)
            {
                FormResults();
                ShowResults();
            }
        }

        public void GetNext()
        {
            nxtMatch = GetNxtDefaultFull();
            Match nxt = new Match(new Competitor(),new Competitor(),1);
            if (nxtMatch[0] != -1 && nxtMatch[1] != -1)
            {
                Console.WriteLine($"{nxtMatch[0]} {nxtMatch[1]}");
                Console.WriteLine($"{Rounds[nxtMatch[0]].Matches[nxtMatch[1]]}");
                nxt = Rounds[nxtMatch[0]].Matches[nxtMatch[1]];

            }
            else if (RepechageAKA != null && RepechageAO != null && !is1third)
            {
                if (!RepechageAKA.IsFinished() || !RepechageAO.IsFinished())
                {
                    nxtMatch = GetNxtRep();
                    if (nxtMatch[0] != -1 && nxtMatch[1] != -1)
                    {
                        Console.WriteLine($"{nxtMatch[0]} {nxtMatch[1]}");
                        if (nxtMatch[0] == Rounds.Count()) { nxt = RepechageAKA.Matches[nxtMatch[1]]; }
                        else if (nxtMatch[0] == Rounds.Count() + 1) { nxt = RepechageAO.Matches[nxtMatch[1]]; }
                        //HaveNxtMatch?.Invoke(nxtMatch[0], nxtMatch[1]);
                    }
                }
                else if(curRound != Rounds.Count() - 1 && curMatch!=0)
                {
                    nxtMatch[0] = Rounds.Count() - 1;
                    nxtMatch[1] = 0;
                    Console.WriteLine($"{nxtMatch[0]} {nxtMatch[1]}");
                    nxt = Rounds[nxtMatch[0]].Matches[nxtMatch[1]];
                    //HaveNxtMatch?.Invoke(nxtMatch[0], nxtMatch[1]);
                }
            }
            else if(BronzeMatch!= null && !BronzeMatch.isFinished && is1third)
            {
                nxtMatch[0] = Rounds.Count();
                nxtMatch[1] = Competitors.Count();
                nxt = BronzeMatch;
            }
            HaveNxtMatch?.Invoke(nxtMatch[0], nxtMatch[1],nxt);
        }

        /*public void GetMatch()
        {
            while (curRound < Rounds.Count())
            {
                if (Rounds[curRound].Matches[curMatch].Winner == null)
                {
                    Console.WriteLine($"Current match: {Rounds[curRound].Matches[curMatch]}\nPlease set winner: ");
                    int w = Convert.ToInt32(Console.ReadLine());
                    Rounds[curRound].Matches[curMatch].SetWinner(w);
                    if (curRound + 1 < Rounds.Count()) UpdateRound(curRound + 1);
                }
                curMatch++;
                if (curMatch >= Rounds[curRound].Matches.Count()) { curMatch = 0; curRound++; }
                if (curRound + 1 == Rounds.Count())
                {
                    if (RepechageAKA == null && RepechageAO == null) GenerateBronze();
                    if (RepechageAKA != null && !RepechageAKA.IsFinished()) { RepechageAKA.GetMatch(); }
                    if (RepechageAO != null && !RepechageAO.IsFinished()) { RepechageAO.GetMatch(); }
                }
            }
            if (curRound >= Rounds.Count()) { FormResults(); ShowResults(); }
        }*/

        void FormResults()
        {
            Winners = new List<Competitor>();
            Winners.Add(Rounds[Rounds.Count() - 1].Matches[Rounds[Rounds.Count() - 1].Matches.Count() - 1].Winner);
            Winners.Add(Rounds[Rounds.Count() - 1].Matches[Rounds[Rounds.Count() - 1].Matches.Count() - 1].Looser);
            if (RepechageAKA != null) Winners.Add(RepechageAKA.Winner);
            if (RepechageAO != null) Winners.Add(RepechageAO.Winner);
            if (BronzeMatch != null) Winners.Add(BronzeMatch.Winner);
        }
        void ShowResults()
        {
            Console.WriteLine("-----------------------\nCATEGORY RESULTS\n-----------------------");
            Console.WriteLine($"1: {Winners[0]}");
            Console.WriteLine($"2: {Winners[1]}");
            if (Winners.Count()>2 &&Winners[2]!=null) Console.WriteLine($"3: {Winners[2]}");
            if (Winners.Count()>3 &&Winners[3] != null) Console.WriteLine($"3: {Winners[3]}");

            HaveCategoryResults?.Invoke(Winners);
        }
        public void GenerateBronze()
        {
            if (!is1third)
            {
                List<Competitor> repechagersAka = new List<Competitor>();
                List<Competitor> repechagersAo = new List<Competitor>();
                Competitor FinalistAka = Rounds[curRound + 1].Matches[0].AKA;
                Competitor FinalistAo = Rounds[curRound + 1].Matches[0].AO;
                for (int i = 0; i < Rounds.Count() - 1; i++)
                {
                    foreach (var m in Rounds[i].Matches)
                    {
                        if (m.AKA.IsBye || m.AO.IsBye) continue;
                        m.Looser.ResetCompetitor();
                        if (m.Winner.ID == FinalistAka.ID &&
                            m.Winner.FirstName == FinalistAka.FirstName &&
                            m.Winner.LastName == FinalistAka.LastName) { repechagersAka.Add(m.Looser); }
                        else if (m.Winner.ID == FinalistAo.ID &&
                                m.Winner.FirstName == FinalistAo.FirstName &&
                                m.Winner.LastName == FinalistAo.LastName) { repechagersAo.Add(m.Looser); }
                    }
                }
                Repechage repechage1 = new Repechage(repechagersAka);
                repechage1.Generate();
                repechage1.ShowRepechage();
                RepechageAKA = repechage1;
                Console.WriteLine("-----------");
                Repechage repechage2 = new Repechage(repechagersAo);
                repechage2.Generate();
                repechage2.ShowRepechage();
                RepechageAO = repechage2;
                Console.WriteLine("-----------");
                RepechageGen?.Invoke();
            }
            else
            {
                Competitor aka= new Competitor(true),ao=new Competitor(true);
                if (Rounds[curRound].Matches[0].Looser != null) aka = new Competitor(Rounds[curRound].Matches[0].Looser);
                if (Rounds[curRound].Matches[1].Looser != null) ao = new Competitor(Rounds[curRound].Matches[1].Looser);
                aka.ResetCompetitor(); ao.ResetCompetitor();
                BronzeMatch = new Match(aka, ao, Competitors.Count());
                
                BronzeGen?.Invoke();
            }
            
        }

        int CountCompetitorsNoBye()
        {
            int counter = 0;
            foreach (var c in Competitors)
            {
                if (!c.IsBye) counter++;
            }
            return counter;
        }

        public void GenerateTree()
        {
            GenerateFirstRound();
            GenerateOtherRounds();
            ShowTree();
            Console.WriteLine(".........................");
        }



        public void UpdateRound(int round)
        {
            Competitor comp;
            if (round < Rounds.Count())
            {
                comp = Rounds[round].Matches[curMatch].Winner;
                comp.ResetCompetitor();
                if (round + 1 < Rounds.Count())
                {
                    if (curMatch % 2 == 0)
                    {
                        Rounds[round + 1].Matches[curMatch / 2].AKA = comp;
                        Rounds[round + 1].Matches[curMatch / 2].AKA.Check_Winner += Rounds[round + 1].Matches[curMatch / 2].CheckWinner;
                    }
                    else
                    {
                        Rounds[round + 1].Matches[(curMatch - 1) / 2].AO = comp;
                        Rounds[round + 1].Matches[curMatch / 2].AO.Check_Winner += Rounds[round + 1].Matches[curMatch / 2].CheckWinner;
                    }
                }
            }
            //ShowTree();
        }

        List<int> GetNxtDefaultFull()
        {
            if (curRound < Rounds.Count() - 1 && RepechageAKA == null && RepechageAO == null)
            {
                List<int> res = new List<int>() { -1, -1 };

                int iM = curMatch + 1, iR = curRound;
                if (iM >= Rounds[iR].Matches.Count()) { iR++; iM = 0; }
                if (Rounds[iR].Matches[iM].isFinished) { iM++; if (iM >= Rounds[iR].Matches.Count()) { iR++; iM = 0; } }

                Match match = Rounds[iR].Matches[iM];
                while (!match.isAllCompetitors())
                {
                    iM++;
                    if (iM >= Rounds[iR].Matches.Count()) { iR++; iM = 0; }
                    if (iR >= Rounds.Count()) { iR = curRound; iM = 0; }

                    match = Rounds[iR].Matches[iM];   
                }
                res[0] = iR; res[1] = iM;
                if (Rounds[curRound].Matches[curMatch] == Rounds[iR].Matches[iM] || Rounds[iR].Matches[iM].isFinished) { return new List<int>() { -1, -1 }; }
                return res;
            }
            else return new List<int>() { -1, -1 };
        }

        void GenerateFirstRound()
        {
            Round res = new Round();
            int Byes = GetNumberofByes(Competitors.Count());
            for (int i = 0; i < Byes; i++)
            {
                Competitors.Add(new Competitor(true));
            }
            List<Match> Group1 = new List<Match>();
            List<Match> Group2 = new List<Match>();
            int id = 1;
            for (int i = 0; i < Competitors.Count() / 2; i++)
            {
                //Create Match
                Competitor aka = Competitors[i];
                Competitor ao = Competitors[Competitors.Count() - 1 - i];
                if (i % 2 == 0) { Group1.Add(new Match(aka, ao, id)); }
                else { Group2.Add(new Match(aka, ao, id)); }
                id++;
            }
            res.Matches.AddRange(Group1);
            res.Matches.AddRange(Group2);
            res.Matches.Reverse();
            res.ID = 0;
            Rounds.Add(res);
        }
        void GenerateOtherRounds()
        {
            while (Rounds[Rounds.Count() - 1].Matches.Count() > 1)
            {
                Round res = new Round();
                int matchNum = Rounds[Rounds.Count() - 1].Matches.Count() / 2;
                int counter = 0;
                for (int i = 0; i < matchNum; i++)
                {
                    Competitor aka = Rounds[Rounds.Count() - 1].Matches[counter].Winner;
                    Competitor ao = Rounds[Rounds.Count() - 1].Matches[counter + 1].Winner;
                    counter += 2;
                    res.Matches.Add(new Match(aka, ao, i + 1));
                }
                res.ID = Rounds.Count();
                Rounds.Add(res);
            }
        }

        int GetNumberofByes(int compNum)
        {
            int AllComps = Convert.ToInt32(Math.Pow(2, Math.Ceiling(Math.Log(compNum, 2))));
            return AllComps - compNum;
        }
        public int GetCurMatchID()
        {
            return curMatch;
        }

        public string GetCurStage()
        {
            if (curRound >= 0) return Rounds[curRound].ToString();
            else return "";
        }

        public int GetCurRoundID()
        {
            return curRound;
        }

        public void ShowTree()
        {
            int r_i = 0;
            foreach(var r in Rounds)
            {
                Console.WriteLine($"R - {r.ID}");
                foreach(var m in r.Matches)
                {
                    Console.WriteLine($"{m}");
                }
                r_i++;
            }
        }
    }
}
