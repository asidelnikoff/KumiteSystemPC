using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TournamentTree;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для OpenCategoryDialog.xaml
    /// </summary>
    public partial class OpenCategoryDialog : Window
    {
        public OpenCategoryDialog()
        {
            InitializeComponent();
        }
        SQLiteConnection m_dbConn;
        SQLiteCommand m_sqlCmd;
        Dictionary<int, string> Tournaments;
        Dictionary<int, string> Categories;
        public TournamentsBracketsBase.ICategory GlobalCategory;
        public int categoryType { get; private set; }
        public int CategoryID;
        public OpenCategoryDialog(SQLiteConnection con)
        {
            InitializeComponent();

            m_dbConn = con;
            m_sqlCmd = new SQLiteCommand();
            m_sqlCmd.Connection = m_dbConn;

            Tournaments = new Dictionary<int, string>();
            tournamentCB.ItemsSource = Tournaments.Values;

            Categories = new Dictionary<int, string>();
            cateogryCB.ItemsSource = Categories.Values;

            ReadTournaments();

            if (Properties.Settings.Default.LastSelectedTournament > -1)
            { tournamentCB.SelectedIndex = Properties.Settings.Default.LastSelectedTournament; }
        }

        void ReadTournaments()
        {
            if (m_dbConn.State == ConnectionState.Open)
            {
                try
                {
                    m_sqlCmd.CommandText = $"SELECT * FROM Tournament";

                    using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = reader["ID"];
                                var name = reader["Name"];
                                Tournaments.Add(Convert.ToInt32(id), (string)name);
                                tournamentCB.Items.Refresh();
                            }
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    ShowMessageDialog(ex.Message, "Error");
                }
            }
        }

        async void ShowMessageDialog(string message, string header = "Info")
        {
            ModernWpf.Controls.ContentDialog ShowMessage = new ModernWpf.Controls.ContentDialog
            {
                Title = header,
                Content = $"{message}",
                PrimaryButtonText = "Ok",
                CloseButtonText = "Cancel"
            };

            await ContentDialogMaker.CreateContentDialogAsync(ShowMessage, true);
        }

        private void tournamentCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tournamentCB.SelectedIndex > -1)
            {
                ReadCategories(Tournaments.ElementAt(tournamentCB.SelectedIndex).Key);
                cateogryCB.Visibility = Visibility.Visible;
            }
        }

        void ReadCategories(int tournamentID = 0)
        {
            if (m_dbConn.State == ConnectionState.Open)
            {
                try
                {
                    m_sqlCmd.CommandText = $"SELECT * FROM Category WHERE Tournament={tournamentID}";

                    using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                    {
                        Categories.Clear();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = reader["ID"];
                                var name = reader["Name"];
                                var type = reader["Type"];
                                //CategoryNames.Add((string)name);
                                Categories.Add(Convert.ToInt32(id), (string)name);
                                //categoryType.SelectedIndex = Convert.ToInt32(type);
                                categoryType = Convert.ToInt32(type);
                            }
                        }
                        cateogryCB.Items.Refresh();
                    }
                }
                catch (SQLiteException ex)
                {
                    ShowMessageDialog(ex.Message, "Error");
                }
            }
        }

        void Read_SE_Category_DB()
        {
            List<TournamentsBracketsBase.ICompetitor> comps = new List<TournamentsBracketsBase.ICompetitor>();
            Repechage aka_rep = new Repechage();
            Repechage ao_rep = new Repechage();
            Match BronzeMatch = null;
            if (m_dbConn.State == System.Data.ConnectionState.Open)
            {
                #region Read Default Rounds
                m_sqlCmd.CommandText = $"SELECT * FROM Round WHERE Category = {CategoryID} AND Repechage=-1";
                int roundCount = 0;
                List<int> defaultRoundsID = new List<int>();
                using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            defaultRoundsID.Add(Convert.ToInt32(reader["ID"]));
                            roundCount++;
                        }
                    }
                }
                List<Round> Rounds = new List<Round>();
                foreach (var i in defaultRoundsID)
                {
                    Round round = new Round();
                    round.ID = i;
                    for (int j = 1; j <= Math.Pow(2, roundCount - i - 1); j++)
                    {
                        m_sqlCmd.CommandText = $"SELECT Match.ID as MatchID, Match.Round, Match.AKA, " +
                         $"Match.AO, Match.Winner, Match.Looser, Match.AKA_C1, Match.AKA_C2, " +
                         $"Match.AO_C1, Match.AO_C2, Match.AKA_score, Match.AO_score, Match.Senshu, Match.isFinished, Competitor.*" +
                         $"FROM Match " +
                         $"LEFT JOIN Competitor on (Competitor.ID = Match.AKA or Competitor.ID = Match.AO) " +
                         $"WHERE Category = {CategoryID} AND Round = {i} AND MatchID = {j}";

                        //Match m = new Match(null, null, j);
                        using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                                round.Matches.Add(ReadMatch(reader,ref comps,j));
                        }
                       //round.Matches.Add(m);
                    }
                    Rounds.Add(round);
                    #endregion
                    if (categoryType == 0)
                    {
                        aka_rep = ReadRepechage(0, roundCount, defaultRoundsID[defaultRoundsID.Count - 1]);
                        ao_rep = ReadRepechage(1, roundCount, defaultRoundsID[defaultRoundsID.Count - 1]);
                    }
                    else if (categoryType == 1)
                        BronzeMatch = ReadBronzeMatch();
                }
                GlobalCategory = new Category();
                GlobalCategory.Competitors = comps;
                GlobalCategory.Rounds = new List<TournamentsBracketsBase.IRound>(Rounds);

                if (categoryType == 0) 
                { 
                    (GlobalCategory as Category).RepechageAKA = aka_rep; 
                    (GlobalCategory as Category).RepechageAO = ao_rep;
                    (GlobalCategory as Category).is1third = false;
                }
                else if(categoryType == 1)
                {
                    (GlobalCategory as Category).is1third = true;
                    (GlobalCategory as Category).BronzeMatch = BronzeMatch;
                }

                GlobalCategory.Winners = ReadWinners(CategoryID);
                Properties.Settings.Default.LastSelectedTournament = tournamentCB.SelectedIndex;
                Properties.Settings.Default.Save();
            }
        }

        private Match ReadBronzeMatch()
        {
            Match result = null;
            m_sqlCmd.CommandText = $"SELECT * FROM Round WHERE Category = {CategoryID} AND Repechage=2";
            int roundID = 0;
            using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
            {
                if (reader.HasRows)
                    while (reader.Read())
                        roundID = Convert.ToInt32(reader["ID"]);
            }
            m_sqlCmd.CommandText = $"SELECT * FROM Match WHERE Category = {CategoryID} AND Round={roundID}";
            int matchID = 0;
            using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
            {
                if (reader.HasRows)
                    while (reader.Read())
                        matchID = Convert.ToInt32(reader["ID"]);
            }
            m_sqlCmd.CommandText = $"SELECT Match.ID as MatchID, Match.Round, Match.AKA, " +
                         $"Match.AO, Match.Winner, Match.Looser, Match.AKA_C1, Match.AKA_C2, " +
                         $"Match.AO_C1, Match.AO_C2, Match.AKA_score, Match.AO_score, Match.Senshu, Match.isFinished, Competitor.*" +
                         $"FROM Match " +
                         $"LEFT JOIN Competitor on (Competitor.ID = Match.AKA or Competitor.ID = Match.AO) " +
                         $"WHERE Category = {CategoryID} AND Round = {roundID} AND MatchID = {matchID}";

            //Match m = new Match(null, null, j);
            var foo = new List<TournamentsBracketsBase.ICompetitor>();
            using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
            {
                if (reader.HasRows)
                    result = ReadMatch(reader, ref foo, matchID) as Match;
            }
            return result;
        }

        List<TournamentsBracketsBase.ICompetitor> ReadWinners(int categoryID)
        {
            var res = new List<TournamentsBracketsBase.ICompetitor>();

            m_sqlCmd.CommandText = $"SELECT Competitor.*, Place FROM Winners " +
                $"LEFT JOIN Competitor on (Competitor.ID = Competitor) WHERE Category={categoryID}";
            using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        res.Add(ReadWinner(reader));
                    }
                }
            }

            return res;
        }
        TournamentsBracketsBase.ICompetitor ReadWinner(SQLiteDataReader reader)
        {
            return new Competitor(Convert.ToBoolean(reader["isBye"]),
                                            Convert.ToInt32(reader["ID"]), (string)reader["FirstName"],
                                            (string)reader["LastName"], (string)reader["Club"], 0, 0,
                                            0, Convert.ToInt32(reader["Status"]));
        }

        TournamentsBracketsBase.IMatch ReadMatch(SQLiteDataReader reader, ref List<TournamentsBracketsBase.ICompetitor> comps, int mID)
        {
            TournamentsBracketsBase.IMatch m = new Match(null, null, mID);
            if (categoryType == 3) m = new RoundRobin.Match(null, null, mID);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (reader["AKA"] != DBNull.Value && Convert.ToInt32(reader["AKA"]) == Convert.ToInt32(reader["ID"]))
                    {
                        m.AKA = ReadCompetitor(reader, 0);
                        if (categoryType == 3) m.AKA = new RoundRobin.Competitor(m.AKA as Competitor);
                        if (!m.AKA.IsBye && !comps.Contains(m.AKA)) comps.Add(m.AKA);
                    }
                    if (reader["AO"] != DBNull.Value && Convert.ToInt32(reader["AO"]) == Convert.ToInt32(reader["ID"]))
                    {
                        m.AO = ReadCompetitor(reader, 1);
                        if (categoryType == 3) m.AO = new RoundRobin.Competitor(m.AO as Competitor);
                        if (!m.AO.IsBye && !comps.Contains(m.AO)) comps.Add(m.AO);
                    }
                    if (m != null && reader["Winner"] != DBNull.Value && Convert.ToInt32(reader["Winner"]) == Convert.ToInt32(reader["AKA"]))
                    {
                        if (reader["Looser"] != DBNull.Value && Convert.ToInt32(reader["Looser"]) != 0 && 
                            m.AO!=null && m.AKA!=null) m.SetWinner(1);
                        else if(m.AKA!=null)
                        {
                            if (reader["Looser"] != DBNull.Value && Convert.ToInt32(reader["Looser"]) == 0) m.AO = new Competitor(true);
                            m.SetWinner(1, false);
                        }
                        m.isFinished = true;
                    }
                    else if (m != null && reader["Winner"] != DBNull.Value && Convert.ToInt32(reader["Winner"]) == Convert.ToInt32(reader["AO"]))
                    {
                        if (reader["Looser"] != null && Convert.ToInt32(reader["Looser"]) != 0 
                            && m.AKA!=null && m.AO!=null) m.SetWinner(2);
                        else if(m.AO!=null)
                        {
                            if (reader["Looser"] != DBNull.Value && Convert.ToInt32(reader["Looser"]) == 0) m.AKA = new Competitor(true);
                            m.SetWinner(2, false);
                        }
                        m.isFinished = true;
                    }
                    int senshu = Convert.ToInt32(reader["Senshu"]);
                    if (senshu != 0)
                    {
                        if (senshu == 1) { m.AKA.Senshu = true; m.AO.Senshu = false; }
                        else if (senshu == 2) { m.AO.Senshu = true; m.AKA.Senshu = false; }
                    }
                }
            }
            return m;
        }

        Repechage ReadRepechage(int repechageId, int roundCount, int lastRoundId)
        {
            Repechage rep = null;
            m_sqlCmd.CommandText = $"SELECT * FROM Round WHERE Category = {CategoryID} AND Repechage={repechageId}";
            int repID = lastRoundId + 1;
            using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        repID = Convert.ToInt32(reader["ID"]);
                    }
                }
            }
            List<TournamentsBracketsBase.ICompetitor> temp = new List<TournamentsBracketsBase.ICompetitor>();
            for (int j = 1; j <= roundCount; j++)
            {
                m_sqlCmd.CommandText = $"SELECT Match.ID as MatchID, Match.Round, Match.AKA, " +
                 $"Match.AO, Match.Winner, Match.Looser, Match.AKA_C1, Match.AKA_C2, " +
                 $"Match.AO_C1, Match.AO_C2, Match.AKA_score, Match.AO_score, Match.Senshu, Match.isFinished, Competitor.*" +
                 $"FROM Match " +
                 $"LEFT JOIN Competitor on (Competitor.ID = Match.AKA or Competitor.ID = Match.AO) " +
                 $"WHERE Category = {CategoryID} AND Round = {repID} AND MatchID = {j}";

                Match m = new Match(null, null, j);
                //Repechage rerepAka = new Repechage();
                using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if(rep == null)rep = new Repechage();
                        rep.Matches.Add(ReadMatch(reader, ref temp, j) as Match);
                    }
                }  
            }
            if (rep != null)
            {
                rep.Competitors = new List<Competitor>();
                foreach (var competitor in temp)
                {
                    rep.Competitors.Add(new Competitor(competitor as Competitor));
                }
                if (rep.Matches[rep.Matches.Count - 1].Winner != null) rep.Winner = rep.Matches[rep.Matches.Count - 1].Winner as Competitor;
            }
            return rep;
        }

        Competitor ReadCompetitor(SQLiteDataReader reader, int comp)
        {
            Competitor res;
            if (comp == 0) res = new Competitor(Convert.ToBoolean(reader["isBye"]),
                                                Convert.ToInt32(reader["ID"]), (string)reader["FirstName"],
                                                (string)reader["LastName"], (string)reader["Club"], 0, Convert.ToInt32(reader["AKA_C1"]),
                                                Convert.ToInt32(reader["AKA_C2"]), Convert.ToInt32(reader["Status"]));
            else res = new Competitor(Convert.ToBoolean(reader["isBye"]),
                                            Convert.ToInt32(reader["ID"]), (string)reader["FirstName"],
                                            (string)reader["LastName"], (string)reader["Club"], 0, Convert.ToInt32(reader["AO_C1"]),
                                            Convert.ToInt32(reader["AO_C2"]), Convert.ToInt32(reader["Status"]));
            string score = "";
            if (comp == 0) score = (string)reader["AKA_score"];
            else score = (string)reader["AO_score"];
            for (int k = 0; k < score.Length; k++)
            {
                if (Char.IsDigit(score[k])) { res.AddPoints(Convert.ToInt32(score[k]) - 48); }
            }
            return res;
        }

        private void cancelBTN_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void selectBTN_Click(object sender, RoutedEventArgs e)
        {
            if (categoryType == 0 || categoryType == 1)
                Read_SE_Category_DB();
            else if (categoryType == 3)
            {
                Read_RR_Category_DB();
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Read_RR_Category_DB()
        {
            List<TournamentsBracketsBase.ICompetitor> comps = new List<TournamentsBracketsBase.ICompetitor>();
            if (m_dbConn.State == System.Data.ConnectionState.Open)
            {
                #region Read Default Rounds
                m_sqlCmd.CommandText = $"SELECT * FROM Round WHERE Category = {CategoryID} AND Repechage=-1";
                int roundCount = 0;
                List<int> defaultRoundsID = new List<int>();
                using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            defaultRoundsID.Add(Convert.ToInt32(reader["ID"]));
                            roundCount++;
                        }
                    }
                }
                m_sqlCmd.CommandText = $"SELECT * from CompetitorCategory where Category = {CategoryID};";
                int competitorsCount = 0;
                using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                            competitorsCount++;
                }
                competitorsCount += competitorsCount % 2;
                List<RoundRobin.Round> Rounds = new List<RoundRobin.Round>();
                foreach (var i in defaultRoundsID)
                {
                    RoundRobin.Round round = new RoundRobin.Round();
                    round.ID = i;
                    for (int j = 1; j <= competitorsCount / 2; j++)
                    {
                        m_sqlCmd.CommandText = $"SELECT Match.ID as MatchID, Match.Round, Match.AKA, " +
                         $"Match.AO, Match.Winner, Match.Looser, Match.AKA_C1, Match.AKA_C2, " +
                         $"Match.AO_C1, Match.AO_C2, Match.AKA_score, Match.AO_score, Match.Senshu, Match.isFinished, Competitor.*" +
                         $"FROM Match " +
                         $"LEFT JOIN Competitor on (Competitor.ID = Match.AKA or Competitor.ID = Match.AO) " +
                         $"WHERE Category = {CategoryID} AND Round = {i} AND MatchID = {j}";

                        using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                                round.Matches.Add(ReadMatch(reader, ref comps, j));
                        }
                    }
                    Rounds.Add(round);
                }
                #endregion

                GlobalCategory = new RoundRobin.Category();
                GlobalCategory.Competitors = comps;
                GlobalCategory.Rounds = new List<TournamentsBracketsBase.IRound>(Rounds);

                (GlobalCategory as RoundRobin.Category).UpdateAllRounds();

                GlobalCategory.Winners = ReadWinners(CategoryID);
                Properties.Settings.Default.LastSelectedTournament = tournamentCB.SelectedIndex;
                Properties.Settings.Default.Save();
            }
        }

        private void cateogryCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cateogryCB.SelectedIndex > -1) 
            { 
                CategoryID = Categories.ElementAt(cateogryCB.SelectedIndex).Key;
                m_sqlCmd.CommandText = $"SELECT * FROM Category WHERE Tournament={Tournaments.ElementAt(tournamentCB.SelectedIndex).Key}" +
                    $" AND ID={CategoryID}";

                using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var id = reader["ID"];
                            var name = reader["Name"];
                            var type = reader["Type"];
                            categoryType = Convert.ToInt32(type);
                        }
                    }
                    cateogryCB.Items.Refresh();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
