using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentTree
{
    public class Match
    {
        public int ID { get; set; }
        public Competitor AKA { get; set; }
        public Competitor AO { get; set; }
        //List<Competitor> Competitors { get; set; }
        public bool isFinished = false;
        public delegate void HaveWinnerHandler();
        public event HaveWinnerHandler HaveWinner;
        
        public Competitor Winner { get; set; }
        public Competitor Looser { get; set; }

        public Match(Competitor _AKA,Competitor _AO, int id)
        {
            //Competitors = new List<Competitor>();
            //Competitors.Add(_AKA);
            //Competitors.Add(_AO);
            AKA = _AKA;
            AO = _AO;
            /*AKA = _AKA;
            AO = _AO;*/
            if (AKA!= null && AKA.IsBye) { Winner = new Competitor(AO); Looser = new Competitor(AKA); }
            else if (AO != null && AO.IsBye) { Winner = new Competitor(AKA); Looser = new Competitor(AO); }
            //else { Winner = new Competitor(); Looser = new Competitor(); }
            if(AKA!=null) AKA.Check_Winner += CheckWinner;
            if (AO != null) AO.Check_Winner += CheckWinner;
            ID = id;
        }
        public bool isAllCompetitors()
        {
            //TODO: All Conditions to check fullness of match
            if ((AKA.FirstName == null && AKA.LastName == null && AKA.ID == 0) ||
                (AO.FirstName == null && AO.LastName == null && AO.ID == 0) || AKA.IsBye || AO.IsBye) return false;
            else return true;
        }
        public void SetWinner(int winner, bool setLooser=true)
        {
            Competitor aka = AKA;
            Competitor ao = AO;
            switch(winner)
            {
                case 1:
                    Winner = new Competitor(aka);
                    if(setLooser) Looser = new Competitor(ao);
                    //isFinished = true;
                    HaveWinner?.Invoke();
                    break;
                case 2:
                    Winner = new Competitor(ao);
                    if(setLooser) Looser = new Competitor(aka);
                    //isFinished = true;
                    HaveWinner?.Invoke();
                    break;
            }
        }

        /*public void SetAKA(Competitor competitor)
        {
            AKA = competitor;
            AKA = competitor;
        }
        public void SetAO(Competitor competitor)
        {
            AO = competitor;
            AO = competitor;
        }*/

        public void CheckWinner(bool isTimeUP=false)
        {
            Competitor Aka = AKA;
            Competitor Ao = AO;

            if (Aka.Status == 2 || Aka.Status == 3) { SetWinner(2,false); }
            else if (Ao.Status == 2 || Ao.Status == 3) { SetWinner(1,false); }
            //
            else if (Aka.Fouls_C1 >= 4 || Aka.Fouls_C2 >= 4) { SetWinner(2); }
            else if (Ao.Fouls_C1 >= 4 || Ao.Fouls_C2 >= 4) { SetWinner(1); }
            //
            else if (Aka.Score - 8 >= Ao.Score && Aka.Fouls_C1 < 4 && Aka.Fouls_C2 < 4) { SetWinner(1); }
            else if (Ao.Score - 8 >= Aka.Score && Ao.Fouls_C1 < 4 && Ao.Fouls_C2 < 4) { SetWinner(2); }
            //
            else if (isTimeUP && Aka.Score > Ao.Score && Aka.Fouls_C1<4 && Aka.Fouls_C2<4) { SetWinner(1);}
            else if (isTimeUP && Ao.Score > Aka.Score && Ao.Fouls_C1 < 4 && Ao.Fouls_C2 < 4) { SetWinner(2);}
            //
            else if (isTimeUP && Aka.Score == Ao.Score && Aka.Senshu) { SetWinner(1); }
            else if (isTimeUP && Aka.Score == Ao.Score && Ao.Senshu) { SetWinner(2); }
            //
            
            //
            //TODO: All conditions to Set winner
            
        }
        public void Reset()
        {
            AKA.ResetCompetitor();
            AO.ResetCompetitor();
        }
        public override string ToString()
        {
            if ((AKA != null && AO != null) &&(AKA.IsBye || AO.IsBye)) { return $"/ - /"; }
            else return $"{AKA} - {AO}";
        }
    }
}
