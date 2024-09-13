using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentsBracketsBase;

namespace TournamentTree
{
    public class Match : TournamentsBracketsBase.IMatch
    {
        public int ID { get; set; }
        public TournamentsBracketsBase.ICompetitor AKA { get; set; }
        public TournamentsBracketsBase.ICompetitor AO { get; set; }
        public bool isFinished { get; set; }
        public event Action<ICompetitor> HaveWinner;

        public TournamentsBracketsBase.ICompetitor Winner { get; set; }
        public TournamentsBracketsBase.ICompetitor Looser { get; set; }

        public Match(Competitor _AKA, Competitor _AO, int id)
        {
            AKA = _AKA;
            AO = _AO;
            CheckWinner();

            if (AKA != null) AKA.Check_Winner += CheckWinner;
            if (AO != null) AO.Check_Winner += CheckWinner;
            ID = id;
        }
        public bool IsAllCompetitors()
        {
            //TODO: All Conditions to check fullness of match
            if (AKA == null || AO == null || (AKA.FirstName == null && AKA.LastName == null && AKA.ID == 0) ||
                (AO.FirstName == null && AO.LastName == null && AO.ID == 0) || AKA.IsBye || AO.IsBye) return false;
            else return true;
        }
        public void SetWinner(int winner, bool setLooser = true)
        {
            Competitor aka = AKA as Competitor;
            Competitor ao = AO as Competitor;
            switch (winner)
            {
                case 1:
                    Winner = new Competitor(aka);
                    if (setLooser) Looser = new Competitor(ao);
                    isFinished = true;
                    HaveWinner?.Invoke(aka);
                    break;
                case 2:
                    Winner = new Competitor(ao);
                    if (setLooser) Looser = new Competitor(aka);
                    isFinished = true;
                    HaveWinner?.Invoke(ao);
                    break;
                default:
                    HaveWinner?.Invoke(null);
                    break;
            }
        }

        public void CheckWinner(bool isTimeUP = false)
        {
            Competitor Aka = AKA as Competitor;
            Competitor Ao = AO as Competitor;

            if (Aka == null || Ao == null)
                return;

            if (Aka.Status == (int)Competitor.Statuses.Kiken || Aka.Status == (int)Competitor.Statuses.Shikaku)
                SetWinner(2, false);
            else if (Ao.Status == (int)Competitor.Statuses.Kiken || Ao.Status == (int)Competitor.Statuses.Shikaku) 
                SetWinner(1, false); 
            //
            else if (AKA != null && AKA.IsBye) 
                SetWinner(2);
            else if (AO != null && AO.IsBye)
                SetWinner(1);
            //
            else if (Aka.Fouls_C1 >= (int)Competitor.Fouls.Hansoku) 
                SetWinner(2);
            else if (Ao.Fouls_C1 >= (int)Competitor.Fouls.Hansoku) 
                SetWinner(1);
            //
            else if (Aka.Score - 8 >= Ao.Score && Aka.Fouls_C1 < (int)Competitor.Fouls.Hansoku) 
                SetWinner(1);
            else if (Ao.Score - 8 >= Aka.Score && Ao.Fouls_C1 < (int)Competitor.Fouls.Hansoku) 
                SetWinner(2);
            //
            else if (isTimeUP && Aka.Score > Ao.Score && Aka.Fouls_C1 < (int)Competitor.Fouls.Hansoku) 
                SetWinner(1);
            else if (isTimeUP && Ao.Score > Aka.Score && Ao.Fouls_C1 < (int)Competitor.Fouls.Hansoku) 
                SetWinner(2);
            //
            else if (isTimeUP && Aka.Score == Ao.Score && Aka.Senshu) 
                SetWinner(1);
            else if (isTimeUP && Aka.Score == Ao.Score && Ao.Senshu) 
                SetWinner(2);
            //
            else if(isTimeUP)
            {
                int countIpAka = 0, countWazAka = 0;
                foreach(var sc in Aka.AllScores)
                {
                    if (sc == 3) countIpAka++;
                    else if (sc == 2) countWazAka++;
                }
                foreach(var sc in Ao.AllScores)
                {
                    if (sc == 3) countIpAka--;
                    else if (sc == 2) countWazAka--;
                }

                if (countIpAka > 0) 
                    SetWinner(1);
                else if (countIpAka < 0) 
                    SetWinner(2);
                else
                {
                    if (countWazAka > 0) 
                        SetWinner(1);
                    else if (countWazAka < 0) 
                        SetWinner(2);
                }

                if (!isFinished)
                    SetWinner(0);
            }
            //
            //TODO: All conditions to Set winner



        }
        public void Reset()
        {
            AKA.ResetCompetitor();
            AO.ResetCompetitor();
            isFinished = false;
            Winner = null;
            Looser = null;
            CheckWinner();

        }
        public override string ToString()
        {
            if (AKA != null && AO != null && (AKA.IsBye || AO.IsBye)) { return $"/ - /"; }
            else return $"{AKA} - {AO}";
        }
    }
}
