using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
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
using Excel = Microsoft.Office.Interop.Excel;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для CategoryViewer.xaml
    /// </summary>
    public partial class CategoryViewer : Window
    {

        List<Competitor> CompetitorsList;
        Category GlobalCategory;
        public string CategoryName;

        Excel.Application exApp;
        Excel.Workbook workbook;

        public delegate void GetMatchHandler(int mID, int rID);
        public event GetMatchHandler GetMatchEv;

        SQLiteConnection m_dbConn;
        SQLiteCommand m_sqlCmd;
        List<TournamentsBracketsBase.ICompetitor> Winners;

        public CategoryViewer()
        {
            InitializeComponent();
        }
        
        public CategoryViewer(Category category, string categoryName, SQLiteConnection con, int categoryID)
        {
            InitializeComponent();
            GlobalCategory = category;
            GlobalCategory.RepechageGen += GlobalCategory_RepechageGenDB;
            GlobalCategory.BronzeGen += GlobalCategory_BronzeGenDB;
            //GlobalCategory.HaveNxtMatch += GlobalCategory_HaveNxtMatch;
            GlobalCategory.HaveCategoryResults += CategoryHaveResultsDB;
            CategoryName = categoryName;
            CategoryID = categoryID;
            m_dbConn = con;
            m_sqlCmd = new SQLiteCommand();
            m_sqlCmd.Connection = m_dbConn;

            this.Title = categoryName;
            foreach (var g in GlobalCategory.Rounds)
            {
                groups_List.Items.Add($"1/{g}");
            }
            if (GlobalCategory.BronzeMatch != null) { groups_List.Items.Add("Bronze Match"); }
            if (GlobalCategory.RepechageAKA != null) { groups_List.Items.Add("Repechage 1"); }
            if (GlobalCategory.RepechageAO != null) { groups_List.Items.Add("Repechage 2"); }
            groups_List.SelectedIndex = 0;
            //CompetitorsGrid.IsReadOnly = true;
            //NxtMatch = new List<int>() { -1, -1 };

            categoryNameL.Content = $"Category: {categoryName}";

            if (GlobalCategory.isCategoryFinished())
            {
                categoryComplition.Content = "- Completed";
                categoryComplition.Foreground = Brushes.LightGreen;
                groups_List.Items.Add("Results");
            }
            else
            {
                categoryComplition.Content = "- Not Completed";
                categoryComplition.Foreground = Brushes.Red;
            }


            DrawBrackets(BracketsGrid);
        }

        private void GlobalCategory_BronzeGenDB()
        {
            if (groups_List.Items.Contains("Bronze Match"))
                groups_List.Items.RemoveAt(groups_List.Items.IndexOf("Bronze Match"));
            groups_List.Items.Add("Bronze Match");
            groups_List_ContextMenu.Visibility = Visibility.Visible;
            if (m_dbConn.State == System.Data.ConnectionState.Open)
            {
                m_sqlCmd.CommandText = $"INSERT or REPLACE INTO Round VALUES" +
                    $" ({groups_List.Items.Count - 1},{CategoryID},2)";
                m_sqlCmd.ExecuteNonQuery();

                InsertRepechageMatch(GlobalCategory.BronzeMatch, groups_List.Items.Count - 1);
            }
        }

        private void GlobalCategory_RepechageGenDB()
        {
            groups_List.Items.Add("Repechage 1");
            groups_List.Items.Add("Repechage 2");
            groups_List_ContextMenu.Visibility = Visibility.Visible;
            if (m_dbConn.State == System.Data.ConnectionState.Open)
            {
                m_sqlCmd.CommandText = $"INSERT or REPLACE INTO Round VALUES" +
                    $" ({groups_List.Items.Count - 2},{CategoryID},0)," +
                    $" ({groups_List.Items.Count - 1},{CategoryID},1)";
                m_sqlCmd.ExecuteNonQuery();

                for (int i = 0; i < GlobalCategory.RepechageAKA.Matches.Count; i++)
                {
                    Match m = GlobalCategory.RepechageAKA.Matches[i] as Match;
                    InsertRepechageMatch(m, groups_List.Items.Count - 2);
                }
                for (int i = 0; i < GlobalCategory.RepechageAO.Matches.Count; i++)
                {
                    Match m = GlobalCategory.RepechageAO.Matches[i] as Match;
                    InsertRepechageMatch(m, groups_List.Items.Count - 1);
                }
            }
        }

        void CategoryHaveResultsDB(List<TournamentsBracketsBase.ICompetitor> winners)
        {

            if (m_dbConn.State == System.Data.ConnectionState.Open)
            {
                m_sqlCmd.CommandText = $"INSERT or REPLACE INTO Winners (Category,Competitor,Place) VALUES" +
                    $"({CategoryID},{winners[0].ID},1), ({CategoryID},{winners[1].ID},2)";
                m_sqlCmd.ExecuteNonQuery();
                if (winners.Count() > 2 && winners[2] != null)
                {
                    m_sqlCmd.CommandText = $"INSERT or REPLACE INTO Winners (Category,Competitor,Place) VALUES" +
                   $"({CategoryID},{winners[2].ID},3)";
                    m_sqlCmd.ExecuteNonQuery();
                }
                if (winners.Count() > 3 && winners[3] != null)
                {
                    m_sqlCmd.CommandText = $"INSERT or REPLACE INTO Winners (Category,Competitor,Place) VALUES" +
                   $"({CategoryID},{winners[3].ID},3)";
                    m_sqlCmd.ExecuteNonQuery();
                }
            }

            Winners = new List<TournamentsBracketsBase.ICompetitor>(winners);
            categoryComplition.Content = "- Completed";
            categoryComplition.Foreground = Brushes.Green;

            groups_List.Items.Add("Results");
        }
        
        void InsertRepechageMatch(Match m, int repechageID)
        {
            if (m.Winner != null && m.Looser != null)
                m_sqlCmd.CommandText = $"INSERT OR REPLACE INTO Match(ID,Round,Category,AKA,AO,Winner,Looser,isFinished) VALUES" +
                $"({m.ID},{repechageID},{CategoryID}," +
                $"{m.AKA.ID},{m.AO.ID},{m.Winner.ID},{m.Looser.ID},{m.isFinished})";
            else if (m.AKA != null)
                m_sqlCmd.CommandText = $"INSERT OR REPLACE INTO Match(ID,Round,Category,AKA,AO,isFinished) VALUES" +
                $"({m.ID},{repechageID},{CategoryID}," +
                $"{m.AKA.ID},{m.AO.ID},{m.isFinished})";
            else
                m_sqlCmd.CommandText = $"INSERT OR REPLACE INTO Match(ID,Round,Category,AO,isFinished) VALUES" +
                $"({m.ID},{repechageID},{CategoryID}," +
                $"{m.AO.ID},{m.isFinished})";
            m_sqlCmd.ExecuteNonQuery();
        }

        #region Draw Brackets

        void DrawBrackets(Grid BracketsGrid)
        {
            BracketsGrid.Children.Clear();
            BracketsGrid.RowDefinitions.Clear();
            BracketsGrid.ColumnDefinitions.Clear();
            if (groups_List.SelectedIndex < GlobalCategory.Rounds.Count || groups_List.SelectedItem.ToString()=="Results")
                DrawDefaultBrackets(BracketsGrid, GlobalCategory);
            else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count)
            {
                if (GlobalCategory.RepechageAKA != null) 
                    DrawRepechageBrackets(BracketsGrid, GlobalCategory.RepechageAKA);
                else if (GlobalCategory.BronzeMatch != null) 
                {
                    var repechage = new Repechage();
                    repechage.Competitors.Add(GlobalCategory.BronzeMatch.AKA as Competitor);
                    repechage.Competitors.Add(GlobalCategory.BronzeMatch.AO as Competitor);
                    repechage.Matches.Add(GlobalCategory.BronzeMatch);
                    if (GlobalCategory.BronzeMatch.Winner != null) repechage.Winner = GlobalCategory.BronzeMatch.Winner as Competitor;
                    DrawRepechageBrackets(BracketsGrid, repechage);
                }
            }
            else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count + 1)
                DrawRepechageBrackets(BracketsGrid, GlobalCategory.RepechageAO);
        }
        void DrawDefaultBrackets(Grid BracketsGrid, Category GlobalCategory)
        {
            int prev_row = 0;
            for (int i = 0; i < GlobalCategory.Rounds.Count; i++)
            {
                int row = 0;
                int add = Convert.ToInt32(Math.Pow(2, i + 1));
                if (i > 0) { row = prev_row + add / 4; prev_row = row; }
                BracketsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                for (int j = 0; j < GlobalCategory.Rounds[i].Matches.Count; j++)
                {
                    if (i == 0)
                    {
                        BracketsGrid.RowDefinitions.Add(new RowDefinition());
                        BracketsGrid.RowDefinitions.Add(new RowDefinition());
                    }
                    Grid fool = MakeMatch(new Competitor(), new Competitor(), false, false);
                    if (row > 0) Grid.SetRow(fool, row - 1);
                    else Grid.SetRow(fool, row);
                    Grid.SetColumn(fool, i);
                    BracketsGrid.Children.Add(fool);
                    Competitor aka = new Competitor(), ao = new Competitor();

                    if (GlobalCategory.Rounds[i].Matches[j].AKA != null) aka = GlobalCategory.Rounds[i].Matches[j].AKA as Competitor;
                    if (GlobalCategory.Rounds[i].Matches[j].AO != null) ao = GlobalCategory.Rounds[i].Matches[j].AO as Competitor;
                    Grid match;
                    if (j % 2 == 0)
                    {
                        if (i != 0) match = MakeMatch(aka, ao, true, true);
                        else match = MakeMatch(aka, ao, true, false);
                        // from row+1 to row+add-1 make right line
                        if (i + 1 != GlobalCategory.Rounds.Count)
                        {
                            for (int k = row + 1; k < row + add; k++)
                            {
                                Border myBorder = new Border()
                                {
                                    BorderBrush = Brushes.White,
                                    BorderThickness = new Thickness(0, 0, 1, 0)

                                };
                                Grid.SetColumn(myBorder, i);
                                Grid.SetRow(myBorder, k);
                                BracketsGrid.Children.Add(myBorder);
                            }
                        }
                        else match = MakeFinal(aka, ao);
                    }
                    else
                    {
                        if (i != 0) match = MakeMatch(aka, ao, true, true, 0);
                        else match = MakeMatch(aka, ao, true, false, 0);
                    }
                    Grid.SetRow(match, row);
                    Grid.SetColumn(match, i);
                    BracketsGrid.Children.Add(match);
                    row += add;
                }
            }
            if (GlobalCategory.Rounds[GlobalCategory.Rounds.Count - 1].IsFinished())
            {
                BracketsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                int row = prev_row;
                Label Winner = new Label()
                {
                    Content = GlobalCategory.Rounds[GlobalCategory.Rounds.Count - 1].Matches[0].Winner.ToString(),
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(1, 1, 1, 1);
                rectangle.BorderBrush = Brushes.White;
                Grid.SetColumn(rectangle, GlobalCategory.Rounds.Count);
                Grid.SetRow(rectangle, row);
                Grid.SetColumn(Winner, GlobalCategory.Rounds.Count);
                Grid.SetRow(Winner, row);

                BracketsGrid.Children.Add(Winner);
                BracketsGrid.Children.Add(rectangle);
            }
        }
        void DrawRepechageBrackets(Grid BracketsGrid, Repechage _Repechage)
        {
            int row = 0;
            for (int i = 0; i < _Repechage.Matches.Count; i++)
            {
                BracketsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                BracketsGrid.RowDefinitions.Add(new RowDefinition());
                BracketsGrid.RowDefinitions.Add(new RowDefinition());
                Grid fool = MakeMatch(new Competitor(), new Competitor(), false, false);
                if (row > 0) Grid.SetRow(fool, row - 1);
                else Grid.SetRow(fool, row);
                Grid.SetColumn(fool, i);
                BracketsGrid.Children.Add(fool);
                Competitor aka = new Competitor(), ao = new Competitor();

                if (_Repechage.Matches[i].AKA != null) aka = _Repechage.Matches[i].AKA as Competitor;
                if (_Repechage.Matches[i].AO != null) ao = _Repechage.Matches[i].AO as Competitor;
                Grid match;
                if (i % 2 == 0)
                {
                    if (i != 0) match = MakeMatch(aka, ao, true, true);
                    else match = MakeMatch(aka, ao, true, false);
                    // from row+1 to row+add-1 make right line
                    if (i + 1 != _Repechage.Matches.Count)
                    {
                        for (int k = row + 1; k < row + 1; k++)
                        {
                            Border myBorder = new Border()
                            {
                                BorderBrush = Brushes.White,
                                BorderThickness = new Thickness(0, 0, 1, 0)

                            };
                            Grid.SetColumn(myBorder, i);
                            Grid.SetRow(myBorder, k);
                            BracketsGrid.Children.Add(myBorder);
                        }
                    }
                    else match = MakeFinal(aka, ao, true);
                }
                else
                {
                    if (i != 0) match = MakeMatch(aka, ao, true, true, 0);
                    else match = MakeMatch(aka, ao, true, false, 0);

                    if (i + 1 == _Repechage.Matches.Count) match = MakeFinal(aka, ao, true);
                }
                Grid.SetRow(match, row);
                Grid.SetColumn(match, i);
                BracketsGrid.Children.Add(match);
                row += 1;
            }
            if (_Repechage.Winner != null)
            {
                BracketsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                row--;
                Label Winner = new Label()
                {
                    Content = _Repechage.Winner.ToString(),
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(1, 1, 1, 1);
                rectangle.BorderBrush = Brushes.White;
                Grid.SetColumn(rectangle, _Repechage.Matches.Count);
                Grid.SetRow(rectangle, row);
                Grid.SetColumn(Winner, _Repechage.Matches.Count);
                Grid.SetRow(Winner, row);

                BracketsGrid.Children.Add(Winner);
                BracketsGrid.Children.Add(rectangle);
            }
        }
        Grid MakeMatch(Competitor nameAka, Competitor nameAo, bool setColor, bool back_b, int b_row = 1)
        {
            Grid res = new Grid();
            RowDefinition myRow = new RowDefinition();
            res.RowDefinitions.Add(new RowDefinition());
            res.RowDefinitions.Add(new RowDefinition());
            if (back_b)
            {
                res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
                res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(4, GridUnitType.Star) });
                res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
            }
            else
            {
                res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
            }

            Border akaB = new Border(); akaB.Background = Brushes.Red;
            Grid.SetRow(akaB, 0);
            if (back_b) Grid.SetColumn(akaB, 1);

            Border aoB = new Border(); aoB.Background = Brushes.Blue;
            Grid.SetRow(aoB, 1);
            if (back_b) Grid.SetColumn(aoB, 1);

            Label aka = new Label();
            if (!nameAka.IsBye && !nameAo.IsBye) aka.Content = nameAka;
            else aka.Content = " ";
            Grid.SetRow(aka, 0);
            if (back_b) Grid.SetColumn(aka, 1);
            Label ao = new Label();
            if (!nameAka.IsBye && !nameAo.IsBye) ao.Content = nameAo;
            else ao.Content = " ";
            Grid.SetRow(ao, 1);
            if (back_b) Grid.SetColumn(ao, 1);

            if (setColor)
            {
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(0, b_row, 1, 1 - b_row);
                rectangle.BorderBrush = Brushes.White;
                if (back_b) Grid.SetColumn(rectangle, 2);
                else Grid.SetColumn(rectangle, 1);
                Grid.SetRow(rectangle, b_row);
                res.Children.Add(akaB);
                res.Children.Add(aoB);
                res.Children.Add(rectangle);
            }

            if (back_b)
            {
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(0, b_row, 0, 1 - b_row);
                rectangle.BorderBrush = Brushes.White;
                Grid.SetRow(rectangle, b_row);
                res.Children.Add(rectangle);
            }

            res.Children.Add(aka);
            res.Children.Add(ao);

            return res;
        }
        Grid MakeFinal(Competitor nameAka, Competitor nameAo,bool back_b=false)
        {
            Grid res = new Grid();
            res.RowDefinitions.Add(new RowDefinition());
            res.RowDefinitions.Add(new RowDefinition());
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(4, GridUnitType.Star) });
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });

            Border akaB = new Border(); akaB.Background = Brushes.Red;
            Grid.SetRow(akaB, 0);
            Grid.SetColumn(akaB, 1);

            Border aoB = new Border(); aoB.Background = Brushes.Blue;
            Grid.SetRow(aoB, 1);
            Grid.SetColumn(aoB, 1);

            Label aka = new Label(); aka.Content = nameAka;
            Grid.SetRow(aka, 0);
            Grid.SetColumn(aka, 1);
            Label ao = new Label(); ao.Content = nameAo;
            Grid.SetRow(ao, 1);
            Grid.SetColumn(ao, 1);

            Border rectangle = new Border();
            rectangle.BorderThickness = new Thickness(0, 0, 0, 1);
            rectangle.BorderBrush = Brushes.White;
            Grid.SetColumn(rectangle, 2);
            res.Children.Add(akaB);
            res.Children.Add(aoB);
            res.Children.Add(rectangle);

            rectangle = new Border();
            if(!back_b)rectangle.BorderThickness = new Thickness(0, 0, 0, 1);
            else rectangle.BorderThickness = new Thickness(1, 0, 0, 1);
            rectangle.BorderBrush = Brushes.White;
            Grid.SetColumn(rectangle, 0);
            res.Children.Add(rectangle);

            res.Children.Add(aka);
            res.Children.Add(ao);

            return res;
        }

        #endregion

        void ExportCategoryResultsToExcel(List<TournamentsBracketsBase.ICompetitor> winners)
        {

            Excel.Workbook wb = workbook;
            Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(wb.Worksheets[wb.Worksheets.Count]);
            ws.Name = "Results";
            ws.Cells[1, 2] = $"{CategoryName}";
            ws.Cells[2, 1] = "1.";
            ws.Cells[2, 2] = winners[0].ToString();
            ws.Cells[3, 1] = "2.";
            ws.Cells[3, 2] = winners[1].ToString();
            if (winners.Count() > 2 && winners[2] != null) { ws.Cells[4, 1] = "3."; ws.Cells[4, 2] = winners[2].ToString(); }
            if (winners.Count() > 3 && winners[3] != null) { ws.Cells[5, 1] = "3."; ws.Cells[5, 2] = winners[3].ToString(); }
        }

        int CategoryID;
        public void UpdateTree()
        {
            int curRound;
            int curMatch = GlobalCategory.GetCurMatchID();
            curRound = groups_List.SelectedIndex;
            Competitor AKA, Winner, AO, Looser;
            int repech = -1;
            int r_count = GlobalCategory.Rounds.Count();
            if (curRound < r_count)
            {
                AKA = GlobalCategory.Rounds[curRound].Matches[curMatch].AKA as Competitor;
                Winner = GlobalCategory.Rounds[curRound].Matches[curMatch].Winner as Competitor;
                AO = GlobalCategory.Rounds[curRound].Matches[curMatch].AO as Competitor;
                Looser = GlobalCategory.Rounds[curRound].Matches[curMatch].Looser as Competitor;
            }
            else if (curRound == r_count)
            {
                if (!GlobalCategory.is1third)
                {
                    AKA = GlobalCategory.RepechageAKA.Matches[curMatch].AKA as Competitor;
                    Winner = GlobalCategory.RepechageAKA.Matches[curMatch].Winner as Competitor;
                    AO = GlobalCategory.RepechageAKA.Matches[curMatch].AO as Competitor;
                    Looser = GlobalCategory.RepechageAKA.Matches[curMatch].Looser as Competitor;
                    repech = 0;
                }
                else
                {
                    AKA = GlobalCategory.BronzeMatch.AKA as Competitor;
                    Winner = GlobalCategory.BronzeMatch.Winner as Competitor;
                    AO = GlobalCategory.BronzeMatch.AO as Competitor;
                    Looser= GlobalCategory.BronzeMatch.Looser as Competitor;
                    repech = 2;
                    
                }
            }
            else if (curRound == r_count + 1)
            {
                AKA = GlobalCategory.RepechageAO.Matches[curMatch].AKA as Competitor;
                Winner = GlobalCategory.RepechageAO.Matches[curMatch].Winner as Competitor;
                AO = GlobalCategory.RepechageAO.Matches[curMatch].AO as Competitor;
                Looser = GlobalCategory.RepechageAO.Matches[curMatch].Looser as Competitor;
                repech = 1;
            }
            else
            {
                AKA = null; Winner = null; AO = null; Looser = null;
            }
            if (repech != -1)
            {
                m_sqlCmd.CommandText = $"SELECT * FROM Round WHERE Category={CategoryID} and Repechage={repech}";
                using (SQLiteDataReader reader = m_sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var id = reader["ID"];
                            curRound = Convert.ToInt32(id);
                        }
                    }
                }
            }
            if (AKA != null && AO != null)
            {  
                string scoreAka = "0";
                for (int i = 0; i < AKA.AllScores.Count; i++) { scoreAka = scoreAka + AKA.AllScores[i].ToString() + ' ';}

                string scoreAO = "0";
                for (int i = 0; i < AO.AllScores.Count; i++) { scoreAO = scoreAO + AO.AllScores[i].ToString() + ' '; }
 
                m_sqlCmd.CommandText = $"UPDATE Match SET " +
                   $"AKA_C1 = {AKA.Fouls_C1}, AKA_C2 = {AKA.Fouls_C2}, " +
                   $"AO_C1 = {AO.Fouls_C1}, AO_C2 = {AO.Fouls_C2}," +
                   $"isFinished = 1," +
                   $"AKA_score = '{scoreAka}', AO_score='{scoreAO}' " +
                   $"WHERE ID={curMatch + 1} and Round={curRound} and Category={CategoryID}";
                m_sqlCmd.ExecuteNonQuery();

                if (AKA.Senshu)
                {
                    m_sqlCmd.CommandText = $"UPDATE Match SET " +
                   $"Senshu=1 " +
                   $"WHERE ID={curMatch + 1} and Round={curRound} and Category={CategoryID}";
                    m_sqlCmd.ExecuteNonQuery();
                }
                else if(AO.Senshu)
                {
                    m_sqlCmd.CommandText = $"UPDATE Match SET " +
                  $"Senshu=2 " +
                  $"WHERE ID={curMatch + 1} and Round={curRound} and Category={CategoryID}";
                    m_sqlCmd.ExecuteNonQuery();
                }

                if(Winner!=null)
                {
                    m_sqlCmd.CommandText = $"UPDATE Match SET " +
                  $"Winner={Winner.ID} " +
                  $"WHERE ID={curMatch + 1} and Round={curRound} and Category={CategoryID}";
                    m_sqlCmd.ExecuteNonQuery();
                    if (curRound + 1 < GlobalCategory.Rounds.Count && repech==-1) //Update next default match
                    {
                        if (curMatch % 2 == 0)
                        {
                            m_sqlCmd.CommandText = $"UPDATE Match SET " +
                            $"AKA={Winner.ID} " +
                            $"WHERE ID={curMatch / 2 + 1} and Round={curRound + 1} and Category={CategoryID}";
                            m_sqlCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            m_sqlCmd.CommandText = $"UPDATE Match SET " +
                           $"AO={Winner.ID} " +
                           $"WHERE ID={(curMatch - 1) / 2 + 1} and Round={curRound + 1} and Category={CategoryID}";
                            m_sqlCmd.ExecuteNonQuery();
                        }
                    }
                    if(repech == 0 && curMatch+1<GlobalCategory.RepechageAKA.Matches.Count) // Update next repechageAKA match
                    {
                        m_sqlCmd.CommandText = $"UPDATE Match SET " +
                            $"AKA={Winner.ID} " +
                            $"WHERE ID={curMatch + 2} and Round={curRound} and Category={CategoryID}";
                        m_sqlCmd.ExecuteNonQuery();
                    }
                    if(repech == 1 && curMatch+1<GlobalCategory.RepechageAO.Matches.Count) // Update next repechageAO match
                    {
                        m_sqlCmd.CommandText = $"UPDATE Match SET " +
                           $"AKA={Winner.ID} " +
                           $"WHERE ID={curMatch + 2} and Round={curRound} and Category={CategoryID}";
                        m_sqlCmd.ExecuteNonQuery();
                    }
                }
                if(Looser != null)
                {
                    m_sqlCmd.CommandText = $"UPDATE Match SET " +
                  $"Looser={Looser.ID} " +
                  $"WHERE ID={curMatch + 1} and Round={curRound} and Category={CategoryID}";
                    m_sqlCmd.ExecuteNonQuery();
                }
            }
            
            BracketsGrid.Children.Clear();
            BracketsGrid.RowDefinitions.Clear();
            BracketsGrid.ColumnDefinitions.Clear();
            DrawBrackets(BracketsGrid);
        }
        #region EXPORT CATEGORY
        void ExportCategory()
        {
            Excel.Application ex = new Excel.Application();
            ex.Workbooks.Add();
            Excel.Workbook wb = ex.ActiveWorkbook;
            workbook = wb;
            Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(Type.Missing, wb.Worksheets[wb.Worksheets.Count]);
            ws.Name = "Visualizing";
            if (GlobalCategory.Rounds != null)
            {
                int col = 1;
                int count = 0;
                int start_row = 3;
                ExportFirstVisual(ws);
                for (int i = 1; i < GlobalCategory.Rounds.Count(); i++)
                {
                    col += 3;
                    start_row += Convert.ToInt32(Math.Pow(2, i));
                    int row = start_row;
                    int add = Convert.ToInt32(Math.Pow(2, i + 2));
                    foreach (var m in GlobalCategory.Rounds[i].Matches)
                    {
                        Excel.Range range = ws.Cells[row, col].EntireColumn;

                        if (m.AKA != null) ws.Cells[row, col].Value = m.AKA.GetName();
                        ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        ws.Cells[row, col].Borders.Weight = 2d;
                        row += 1;

                        if (m.AO != null) ws.Cells[row, col].Value = m.AO.GetName();
                        ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        ws.Cells[row, col].Borders.Weight = 2d;

                        ws.Cells[row, col + 1].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                        ws.Cells[row, col + 1].Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;

                        if (count % 2 == 0 && i + 1 != GlobalCategory.Rounds.Count())
                        {
                            for (int k = 0; k < add; k++)
                            {
                                ws.Cells[row + k, col + 1].Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                                ws.Cells[row + k, col + 1].Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;
                            }
                            ws.Cells[row + (add / 2), col + 2].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                            ws.Cells[row + (add / 2), col + 2].Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                            ws.Columns[col + 2].ColumnWidth = 3;
                        }

                        row += (add - 1);
                        count++;
                        range.EntireColumn.AutoFit();
                    }
                    ws.Columns[col + 1].ColumnWidth = 3;
                    ws.Columns[col].ColumnWidth = 32;
                }
                col += 2;
                int _row = Convert.ToInt32(Math.Pow(2, GlobalCategory.Rounds.Count())) + 1;
                Excel.Range _range = ws.Range[ws.Cells[_row, col], ws.Cells[_row + 1, col]];
                _range.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                _range.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;

                _range.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                _range.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2d;

                _range.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                _range.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = 2d;

                _range.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                _range.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;

                _range.Merge();

                if(GlobalCategory.Rounds[GlobalCategory.Rounds.Count()-1].Matches[0].Winner!=null)
                {
                    ws.Cells[_row, col].Value = $"{GlobalCategory.Rounds[GlobalCategory.Rounds.Count() - 1].Matches[0].Winner}";
                    ws.Cells[_row, col].Style.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    ws.Cells[_row, col].Style.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                }

                ws.Columns[col].ColumnWidth = 32;
                ws.Cells[1, 1].Value = $"Категория: {CategoryName}";
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.Font.Bold = true;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.Font.Size = 14;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, ws.UsedRange.Columns.Count]].Merge();

                ws.Range[ws.Cells[2, 1], ws.Cells[ws.UsedRange.Rows.Count, ws.UsedRange.Columns.Count]].Cells.Font.Size = 12;

                if (wb.Worksheets.Count > 1) wb.Worksheets[1].Delete();
                
                ExportRounds(wb);
                exApp = ex;
                exApp.Visible = true;
                exApp.DisplayAlerts = false;
            }

            if (GlobalCategory.RepechageAKA != null && GlobalCategory.RepechageAKA.Matches.Count > 0) 
                ExportRepechage(wb, 0);
            if(GlobalCategory.RepechageAO != null && GlobalCategory.RepechageAO.Matches.Count > 0) 
                ExportRepechage(wb, 1);

            if (GlobalCategory.Winners != null && GlobalCategory.Winners.Count > 0)
                ExportCategoryResultsToExcel(GlobalCategory.Winners);

        }
        void ExportFirstVisual(Excel.Worksheet ws)
        {
            int row = 3;
            int col = 1;
            int count = 0;
            foreach (var m in GlobalCategory.Rounds[0].Matches)
            {
                Excel.Range range = ws.Cells[row, col].EntireColumn;

                if (!m.AKA.IsBye && !m.AO.IsBye) ws.Cells[row, col].Value = m.AKA.GetName();
                ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Cells[row, col].Borders.Weight = 2d;
                row += 1;


                if (!m.AKA.IsBye && !m.AO.IsBye) ws.Cells[row, col].Value = m.AO.GetName();
                ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Cells[row, col].Borders.Weight = 2d;

                ws.Cells[row, col + 1].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Cells[row, col + 1].Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;

                if (count % 2 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ws.Cells[row + i, col + 1].Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                        ws.Cells[row + i, col + 1].Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;
                    }
                    ws.Cells[row + 2, col + 2].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                    ws.Cells[row + 2, col + 2].Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                    ws.Columns[col + 2].ColumnWidth = 3;
                }
                row += 3;

                count++;
                range.EntireColumn.AutoFit();
            }
            ws.Columns[col + 1].ColumnWidth = 3;
            ws.Columns[col].ColumnWidth = 32;
        }
        void ExportRounds(Excel.Workbook wb)
        {
            foreach (var r in GlobalCategory.Rounds)
            {
                Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(wb.Worksheets[wb.Worksheets.Count]);
                ws.Name = $"1%{r.ToString()}";
                ExportMatchesToExcelSheet(ws, r.Matches);
            }
        }
        void ExportRepechage(Excel.Workbook workbook,int num)
        {
            if (num == 0)
            {
                Excel.Worksheet ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws.Name = "Repechage 1";
                ExportMatchesToExcelSheet(ws, GlobalCategory.RepechageAKA.Matches);

                Excel.Worksheet ws_ = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws_.Name = "Repechage 1(Visual)";
                ExportRepechageVisual(ws_, GlobalCategory.RepechageAKA);
                
            } //Export Repechage AKA
            else if(num==1) 
            {
                Excel.Worksheet ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws.Name = "Repechage 2";
                ExportMatchesToExcelSheet(ws, GlobalCategory.RepechageAO.Matches);

                Excel.Worksheet _ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                _ws.Name = "Repechage 2(Visual)";
                ExportRepechageVisual(_ws, GlobalCategory.RepechageAO);
            } //Export Repechage AO
            else if(num==2)
            {
                Excel.Worksheet ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws.Name = "Bronze Match";
                Repechage temp = new Repechage();
                temp.Matches = new List<TournamentsBracketsBase.IMatch>() { GlobalCategory.BronzeMatch };
                ExportMatchesToExcelSheet(ws, temp.Matches);

                Excel.Worksheet _ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                _ws.Name = "Bronze Match(Visual)";
                ExportRepechageVisual(_ws, temp);
            } //Export Bronze match

        }

        void ExportMatchesToExcelSheet(Excel.Worksheet ws, List<TournamentsBracketsBase.IMatch> matches)
        {
            int row = 2;
            SetupPageHeader(ws);

            foreach (var m in matches)
            {
                if (m.AKA != null)
                {
                    ws.Cells[row, 1].Value = m.AKA.ID;
                    ws.Cells[row, 2].Value = m.AKA.FirstName;
                    ws.Cells[row, 3].Value = m.AKA.LastName;
                    ws.Cells[row, 4].Value = m.AKA.Club;
                    ws.Cells[row, 5].Value = m.AKA.GetFoulsC1();
                    ws.Cells[row, 6].Value = (m.AKA as Competitor).Score;
                }
                if (m.AO != null)
                {
                    ws.Cells[row, 14].Value = m.AO.ID;
                    ws.Cells[row, 13].Value = m.AO.FirstName;
                    ws.Cells[row, 12].Value = m.AO.LastName;
                    ws.Cells[row, 11].Value = m.AO.Club;
                    ws.Cells[row, 10].Value = m.AO.GetFoulsC1();
                    ws.Cells[row, 9].Value = (m.AO as Competitor).Score;
                }
                if (m.Winner != null && m.Winner.ID == m.AKA.ID && m.Winner.FirstName == m.AKA.FirstName)
                    ws.Cells[row, 7].Value = "X";
                else if (m.Winner != null && m.Winner.ID == m.AO.ID && m.Winner.FirstName == m.AO.FirstName) 
                    ws.Cells[row, 8].Value = "X";
                row++;
            }
            Excel.Range range = ws.UsedRange;
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Borders.Weight = 2d;
        }

        void SetupPageHeader(Excel.Worksheet ws)
        {
            ws.Cells[1, 1].Value = "ID_AKA";
            ws.Cells[1, 2].Value = "AKA First_Name";
            ws.Cells[1, 3].Value = "AKA Last_Name";
            ws.Cells[1, 4].Value = "AKA Club";
            ws.Cells[1, 5].Value = "AKA Fouls";
            ws.Cells[1, 6].Value = "AKA Score";
            ws.Cells[1, 7].Value = "Winner AKA";
            for (int i = 1; i <= 7; i++)
                ws.Cells[1, i].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
            ws.Cells[1, 8].Value = "Winner AO";
            ws.Cells[1, 14].Value = "ID_AO";
            ws.Cells[1, 13].Value = "AO First_Name";
            ws.Cells[1, 12].Value = "AO Last_Name";
            ws.Cells[1, 11].Value = "AO Club";
            ws.Cells[1, 10].Value = "AO Fouls";
            ws.Cells[1, 9].Value = "AO Score";
            for (int i = 8; i <= 14; i++)
                ws.Cells[1, i].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Blue);
        }

        void ExportRepechageVisual(Excel.Worksheet ws, Repechage repechage)
        {
            
            int col = 1,i=0;
            int row = 3;
            int add = 2;
            foreach (var m in repechage.Matches)
            {
                
                Excel.Range range = ws.Cells[row, col].EntireColumn;

                 if (m.AKA != null) ws.Cells[row, col].Value = m.AKA.GetName();
                 ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                 ws.Cells[row, col].Borders.Weight = 2d;
                 row += 1;

                 if (m.AO != null) ws.Cells[row, col].Value = m.AO.GetName();
                 ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                 ws.Cells[row, col].Borders.Weight = 2d;

                 ws.Cells[row, col + 1].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                 ws.Cells[row, col + 1].Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                for (int k = 0; k < add ; k++)
                {
                    ws.Cells[row + k, col + 1].Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                    ws.Cells[row + k, col + 1].Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;
                }
                ws.Cells[row + add, col + 2].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Cells[row + add, col + 2].Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                ws.Columns[col + 2].ColumnWidth = 3;
                row++;
                range.EntireColumn.AutoFit();
                i++;
                ws.Columns[col + 1].ColumnWidth = 3;
                ws.Columns[col].ColumnWidth = 32;
                col += 3;
            }
            ws.Columns[col + 1].ColumnWidth = 3;
            ws.Columns[col].ColumnWidth = 32;
            Excel.Range _range = ws.Range[ws.Cells[row, col], ws.Cells[row+1, col]];
            _range.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;

            _range.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2d;

            _range.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = 2d;

            _range.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;

            _range.Merge();

            if (repechage.Winner!=null)
            {
                ws.Cells[row, col].Value = $"{repechage.Winner}";
                ws.Cells[row,col].Style.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                ws.Cells[row,col].Style.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            }
        }
        
        void SetCellStyle(int row,int col,Excel.Worksheet ws)
        {
            ws.Columns[col + 1].ColumnWidth = 3;
            ws.Columns[col].ColumnWidth = 32;
            ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            ws.Cells[row, col].Borders.Weight = 2d;
        }

        
        #endregion

        private void MatchesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalCategory.Rounds.Count() > 0 && MatchesGrid.SelectedIndex>=0)
            {
                Match match = new Match(null, null, -1);
                List<Competitor> comps = new List<Competitor>();

                if (groups_List.SelectedIndex < GlobalCategory.Rounds.Count())
                    match = GlobalCategory.Rounds[groups_List.SelectedIndex].Matches[MatchesGrid.SelectedIndex] as Match;
                else if(groups_List.SelectedIndex == GlobalCategory.Rounds.Count())
                {
                    if (!GlobalCategory.is1third)
                        match = GlobalCategory.RepechageAKA.Matches[MatchesGrid.SelectedIndex] as Match;
                    else
                        match = GlobalCategory.BronzeMatch;
                }
                else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count() + 1)
                    match = GlobalCategory.RepechageAO.Matches[MatchesGrid.SelectedIndex] as Match;

                if(match.AKA != null) 
                    comps.Add(match.AKA as Competitor);
                if(match.AO != null) 
                    comps.Add(match.AO as Competitor);
                
                MatchWinnerLabel.Content = $"Winner: ";
                if (match.Winner != null)
                    MatchWinnerLabel.Content += $"{match.Winner}";

                CompetitorsGrid.ItemsSource = comps;
                CompetitorsGrid.Items.Refresh();

            }
        }

        private void groups_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (groups_List.SelectedIndex >= 0)
            {
                if (GlobalCategory.Rounds.Count() > 0 && groups_List.SelectedIndex < GlobalCategory.Rounds.Count())
                {
                    MatchesGrid.ItemsSource = GlobalCategory.Rounds[groups_List.SelectedIndex].Matches;
                }
                else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count())
                {
                    if (!GlobalCategory.is1third) MatchesGrid.ItemsSource = GlobalCategory.RepechageAKA.Matches;
                    else MatchesGrid.ItemsSource = new List<Match>() { GlobalCategory.BronzeMatch };
                }
                else if(groups_List.SelectedItem.ToString() == "Results")
                {
                    MatchesGrid.ItemsSource = null;
                    CompetitorsGrid.ItemsSource = GlobalCategory.Winners;
                }
                else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count() + 1)
                {
                    MatchesGrid.ItemsSource = GlobalCategory.RepechageAO.Matches;
                }
                MatchesGrid.Items.Refresh();
                DrawBrackets(BracketsGrid);
            }
        }
        
        void LoadRoundMatch(int round, int match) {
            GetMatchEv?.Invoke(match, round);
        }

        private void LoadMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalCategory.isCurMFinished())
            {
                MatchWinnerLabel.Content = "Winner: ";
                LoadRoundMatch(groups_List.SelectedIndex,MatchesGrid.SelectedIndex);
            }
            else
            {
                DisplayFinishMatchDialog();
            }
        }

        private void CompetitorsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            GetMatchEv?.Invoke(MatchesGrid.SelectedIndex, groups_List.SelectedIndex);
        }

        private void FinishCurMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalCategory.FinishCurrentMatch();
            if (GlobalCategory.isCurMFinished()) 
            {
                UpdateTree(); 
                DisplayMessageDialog("Info", "Match finished"); 
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
           /* if (exApp != null) exApp.Quit();*/
        }

        private void exportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (exApp != null) 
                exApp.Quit();
            ExportCategory();
            exApp.ActiveWorkbook.SaveAs(Properties.Settings.Default.DataPath + "\\" + CategoryName);
            if (exApp.ActiveWorkbook.Saved) 
            { 
                try 
                { 
                    DisplayMessageDialog("Info", "Category file saved"); 
                } 
                catch { } 
            }
        }

        private void regenerateBronze_Click(object sender, RoutedEventArgs e)
        {
            if(GlobalCategory != null)
            {
                if (!GlobalCategory.is1third && GlobalCategory.RepechageAKA != null && GlobalCategory.RepechageAO != null)
                {
                    groups_List.Items.RemoveAt(groups_List.Items.Count - 1);
                    groups_List.Items.RemoveAt(groups_List.Items.Count - 1);
                    GlobalCategory.RepechageAKA = null;
                    GlobalCategory.RepechageAO = null;
                    GlobalCategory.GenerateBronze();
                }
                else if (GlobalCategory.is1third && GlobalCategory.BronzeMatch != null)
                {
                    groups_List.Items.RemoveAt(groups_List.Items.Count - 1);
                    GlobalCategory.BronzeMatch = null;
                    GlobalCategory.GenerateBronze();
                }
                DrawBrackets(BracketsGrid);
            }
        }

        private void MatchesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GlobalCategory.isCurMFinished())
            {
                MatchWinnerLabel.Content = "Winner: ";
                LoadRoundMatch(groups_List.SelectedIndex, MatchesGrid.SelectedIndex);
            }
            else
                DisplayFinishMatchDialog();
        }


        #region Dialog Functions
        private async void DisplayMessageDialog(string caption, string message)
        {
            await ContentDialogMaker.CreateContentDialogAsync(new ContentDialog
            {
                Title = caption,
                Content = message,
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }, awaitPreviousDialog: true);

        }
        private async void DisplayFinishMatchDialog()
        {
            ContentDialog FinishMatchDialog = new ContentDialog
            {
                Title = "Finish current match?",
                Content = "This match isn't finished. Do you want to finish it?",
                PrimaryButtonText = "Finish",
                SecondaryButtonText = "Load without finishing",
                DefaultButton = ContentDialogButton.Primary,
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await FinishMatchDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                GlobalCategory.FinishCurrentMatch();
                if (GlobalCategory.isCurMFinished()) { DisplayMessageDialog("Info", "Match finished"); }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                LoadRoundMatch(groups_List.SelectedIndex, MatchesGrid.SelectedIndex);
            }
        }
        #endregion
    }
}
