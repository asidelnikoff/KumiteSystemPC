using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentsBracketsBase;

namespace TournamentTree
{
    public class Competitor : TournamentsBracketsBase.ICompetitor ,System.ComponentModel.INotifyPropertyChanged
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


        //public delegate void CheckWinnerDelegate(bool isTimeUp=false);
        public event TournamentsBracketsBase.CheckWinnerDelegate Check_Winner;
        

        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Club { get; set; }

        public int Score;
        public int ScoreProperty
        { 
            get { return Score; }
            set
            {
                Score = value;
                OnPropertyChanged("ScoreProperty");
            }
        }

        public List<int> AllScores { get; set; }
        public bool Senshu { get; set; }
        public int Fouls_C1 { get; private set; }
        public int Fouls_C2 { get; private set; }
        public int Status { get; private set; }

        public bool IsBye { get; set; }

        #region FOR BINDING
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        public Competitor() { AllScores = new List<int>(); }

        public Competitor(Competitor competitor)
        {
            ID = competitor.ID;
            FirstName = competitor.FirstName;
            LastName = competitor.LastName;
            Score = competitor.Score;
            Fouls_C1 = competitor.Fouls_C1;
            Fouls_C2 = competitor.Fouls_C2;
            Status = competitor.Status;
            IsBye = competitor.IsBye;
            AllScores = competitor.AllScores;
            Club = competitor.Club;
        }

        public Competitor(bool isBye=false,int id=0,string FName="", string LName="", string _Club="",int score=0,int fc1=0,int fc2=0, int status =0)
        {
            IsBye = isBye;
            
            FirstName = FName;
            if (isBye) { FirstName = "BYE"; }
            LastName = LName;
            Score = score;
            Fouls_C1 = fc1;
            Fouls_C2 = fc2;
            Status = status;
            ID = id;
            AllScores = new List<int>();
            Club = _Club;
        }

        public void AddPoints(int points)
        {
            ScoreProperty += points;
            AllScores.Add(points);
           // Check_Winner?.Invoke();
        }

        public void SetScore(int score)
        {
            ScoreProperty = score;
            AllScores.Clear();
            AllScores.Add(score);
        }

        public void SetStatus(int status) { Status = status; Check_Winner?.Invoke(); }

        public void SetFoulsC1(int fouls) { Fouls_C1 = fouls; Check_Winner?.Invoke(); }
        public void SetFoulsC2(int fouls) { Fouls_C2 = fouls; Check_Winner?.Invoke(); }

        public void ResetCompetitor()
        {
            AllScores = new List<int>();
            Fouls_C2 = 0;
            Fouls_C1 = 0;
            Score = 0;
            Senshu = false;
        }
        public string GetName()
        {
            return $"{FirstName} {LastName}";
        }
        public string GetFoulsC1()
        {
            switch(Fouls_C1)
            {
                case 0:
                    return "";
                case 1:
                    return "C";
                case 2:
                    return "K";
                case 3:
                    return "HC";
                case 4:
                    return "H";
                default:
                    return "";
            }
        }
        public string GetFoulsC2()
        {
            switch (Fouls_C2)
            {
                case 0:
                    return "";
                case 1:
                    return "C";
                case 2:
                    return "K";
                case 3:
                    return "HC";
                case 4:
                    return "H";
                default:
                    return "";
            }
        }
        public override string ToString()
        {
            if (!IsBye) return $"{FirstName} {LastName}";
            else return "BYE";
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType().ToString() != "MS.Internal.NamedObject")
            {
                
                Competitor comp = (Competitor)obj;
                Console.WriteLine($"{comp}");
                    return (FirstName == comp.FirstName) &&
                            (LastName == comp.LastName) &&
                            (ID == comp.ID);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return ($"{ID}{FirstName}{LastName}").GetHashCode();
        }

        public void Swap(ICompetitor competitor)
        {
            Competitor temp = new Competitor(this);

            ID = competitor.ID;
            FirstName = competitor.FirstName;
            LastName = competitor.LastName;
            Score = (competitor as Competitor).Score;
            Fouls_C1 = competitor.Fouls_C1;
            Fouls_C2 = competitor.Fouls_C2;
            Status = competitor.Status;
            IsBye = competitor.IsBye;
            AllScores = new List<int>(competitor.AllScores);
            Club = competitor.Club;


            (competitor as Competitor).ID = temp.ID;
            (competitor as Competitor).FirstName = temp.FirstName;
            (competitor as Competitor).LastName = temp.LastName;
            (competitor as Competitor).Score = temp.Score;
            (competitor as Competitor).Fouls_C1 = temp.Fouls_C1;
            (competitor as Competitor).Fouls_C2 = temp.Fouls_C2;
            (competitor as Competitor).Status = temp.Status;
            (competitor as Competitor).IsBye = temp.IsBye;
            (competitor as Competitor).AllScores = new List<int>(temp.AllScores);
            (competitor as Competitor).Club = temp.Club;
        }
    }
}
