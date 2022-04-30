using System;
using System.Collections.Generic;
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
using WpfScreenHelper;
using ModernWpf.Controls;
using System.Data.SQLite;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для KataInternal.xaml
    /// </summary>
    public partial class KataInternal : Window
    {
        int JudjesCount;

        Competitor _Aka;
        Competitor _Ao;
        Match GlobalMatchNow;
        Match GlobalMatchNxt;

        List<int> NxtMatch;
        Category GlobalCategory;

        Excel.Application MainExApp;
        Excel.Worksheet VisualBracket;

        CategoryViewer GlobalCategoryViewer;

        System.Media.SoundPlayer end_of_m_sound;
        System.Media.SoundPlayer warn_sound;

        Kata_ExternalBoard externalBoard;

        public KataInternal()
        {
            InitializeComponent();
            for (int i = 1; i < 8; i++) { judjesCB.Items.Add(i); }
            judjesCB.SelectedIndex = Properties.Settings.Default.DefaultJudjesNumber;

            _Aka = new Competitor(false, 1, "");
            _Ao = new Competitor(false, 2, "");
            GlobalMatchNow = new Match(_Aka, _Ao, 0);
            GlobalMatchNow.HaveWinner += Match_HaveWinner;
            NxtMatch = new List<int>() { -1, -1 };
            if (Properties.Settings.Default.EndOfMatch != "") { end_of_m_sound = new System.Media.SoundPlayer(Properties.Settings.Default.EndOfMatch); }
            if (Properties.Settings.Default.WarningSound != "") { warn_sound = new System.Media.SoundPlayer(Properties.Settings.Default.WarningSound); }
            if (!Properties.Settings.Default.AutoNextLoad)
            {
                NextMatchBTN.IsEnabled = false;
                AKA_nxtTXT.IsEnabled = false; AO_nxtTXT.IsEnabled = false;
            }
        }

        #region OPEN CATEGORY
        bool CanOpen = true;
        async void DisplaySaveDialog()
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Info",
                Content = $"Save changes in {CategoryName}",
                PrimaryButtonText = "Save",
                DefaultButton = ContentDialogButton.Primary,
                SecondaryButtonText = "Don't save",
                CloseButtonText = "Cancel"
            };
            await ContentDialogMaker.CreateContentDialogAsync(deleteFileDialog, true);
            /*ContentDialogResult result = await deleteFileDialog.ShowAsync();*/
            if (ContentDialogMaker.Result == ContentDialogResult.Primary)
            {
                try
                {
                    MainExApp.ActiveWorkbook.Save();
                    MainExApp.Quit();
                    MainExApp = null;
                    GlobalCategory = null;
                    CanOpen = true;
                    DisplayMessageDialog("Info", "File saved");
                }
                catch (Exception ex) { DisplayMessageDialog("Info", ex.Message); }
            }
            else if (ContentDialogMaker.Result == ContentDialogResult.Secondary)
            {
                MainExApp.Quit();
                MainExApp = null;
                GlobalCategory = null;
                CanOpen = true;
            }
            else { CanOpen = false; }
        }

        string dbFileName = "tournaments.sqlite";
        SQLiteConnection m_dbConn;
        SQLiteCommand m_sqlCmd;

        private void DBOpenCategory_Click(object sender, RoutedEventArgs e)
        {

            if (Properties.Settings.Default.DefaultDBPath == null)
            {
                Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
                openFile.Title = "Open Categroy";
                openFile.Filter = "SQLite Databases(*.sqlite)|*.sqlite";
                if (openFile.ShowDialog() == true)
                {
                    dbFileName = openFile.FileName;
                    Properties.Settings.Default.DefaultDBPath = dbFileName;
                    Properties.Settings.Default.Save();
                }
            }
            else dbFileName = Properties.Settings.Default.DefaultDBPath;

            m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
            m_dbConn.Open();
            m_sqlCmd = new SQLiteCommand();
            m_sqlCmd.Connection = m_dbConn;

            OpenCategoryDialog openCategoryDialog = new OpenCategoryDialog(m_dbConn);
            openCategoryDialog.Owner = this;
            openCategoryDialog.ShowDialog();

            if (openCategoryDialog.DialogResult == true)
            {
                GlobalCategory = openCategoryDialog.GlobalCategory;
                GlobalCategory.HaveNxtMatch += GlobalCategory_HaveNxtMatch;
                GlobalCategory.HaveCategoryResults += GlobalCategory_HaveCategoryResults;

                CategoryName = (string)openCategoryDialog.cateogryCB.SelectedItem;

                CategoryViewer CategoryViewer = new CategoryViewer(GlobalCategory, CategoryName, m_dbConn,
                    openCategoryDialog.CategoryID);
                CategoryViewer.GetMatchEv += GetMatch;
                GlobalCategoryViewer = CategoryViewer;
                GlobalCategoryViewer.Show();

                AKA_curTXT.IsReadOnly = true;
                AO_curTXT.IsReadOnly = true;

                AKA_nxtTXT.IsReadOnly = true;
                AO_nxtTXT.IsReadOnly = true;
                try
                {
                    string[] worrd = GlobalCategoryViewer.CategoryName.Split(new char[] { ' ' }, 2);
                    externalBoard.CategoryEXT.Text += $"{worrd[0]} \n{worrd[1]}";
                }
                catch
                {
                    if (externalBoard != null && externalBoard.IsLoaded)
                        externalBoard.CategoryEXT.Text = GlobalCategoryViewer.CategoryName;
                }
            }
        }

        private void openCategoryBTN_Click(object sender, RoutedEventArgs e)
        {
            OpenCategory();
        }
        string CategoryName = "";
        void OpenCategory()
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Title = "Open Categroy";
            openFile.Filter = "Excel Files(*.xls;*.xlsx)|*.xls;*.xlsx";
            if (GlobalCategory != null && MainExApp != null)
            {
                DisplaySaveDialog();
            }
            TreeTypeDialog treeTypeDialog = new TreeTypeDialog();
            treeTypeDialog.Owner = this;
            if (CanOpen && treeTypeDialog.ShowDialog() == true && openFile.ShowDialog() == true)
            {
                //Category = new Category();
                MainExApp = new Excel.Application();
                string fileName = openFile.FileName;

                MainExApp.Workbooks.Open(fileName,
                  Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                  Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                  Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                  Type.Missing, Type.Missing);



                GlobalCategory = ReadCategory(MainExApp.ActiveWorkbook);
                if (Properties.Settings.Default.DefaultTreeType == 0) { GlobalCategory.is1third = false; }
                else if (Properties.Settings.Default.DefaultTreeType == 1) { GlobalCategory.is1third = true; }
                GlobalCategory.HaveNxtMatch += GlobalCategory_HaveNxtMatch;
                GlobalCategory.HaveCategoryResults += GlobalCategory_HaveCategoryResults;

                CategoryName = MainExApp.ActiveWorkbook.Name.Substring(0, MainExApp.ActiveWorkbook.Name.IndexOf('.'));

                CategoryViewer CategoryViewer = new CategoryViewer(GlobalCategory, CategoryName, MainExApp.ActiveWorkbook);
                CategoryViewer.GetMatchEv += GetMatch;
                GlobalCategoryViewer = CategoryViewer;
                GlobalCategoryViewer.Show();
                MainExApp.DisplayAlerts = false;
                MainExApp.Visible = true;

                AKA_curTXT.IsReadOnly = true;
                AO_curTXT.IsReadOnly = true;

                AKA_nxtTXT.IsReadOnly = true;
                AO_nxtTXT.IsReadOnly = true;
                try
                {
                    string[] worrd = GlobalCategoryViewer.CategoryName.Split(new char[] { ' ' }, 2);
                    externalBoard.CategoryEXT.Text += $"{worrd[0]} \n{worrd[1]}";
                }
                catch
                {
                    if (externalBoard != null && externalBoard.IsLoaded)
                        externalBoard.CategoryEXT.Text = GlobalCategoryViewer.CategoryName;
                }
                CanOpen = false;
            }
        }

        Category ReadCategory(Excel.Workbook wb)
        {
            //TODO: Read competitor's club
            int count = wb.Worksheets.Count - 1;
            Category category = new Category();
            Match Bronze = new Match();
            Repechage repAo = new Repechage();
            Repechage repAka = new Repechage();
            for (int i = 1; i <= count; i++)
            {
                Excel.Worksheet ws = wb.Worksheets[i];
                Round round = new Round();

                for (int j = 2; j <= ws.UsedRange.Rows.Count; j++)
                {
                    int AkaId = Convert.ToInt32(ws.Cells[j, 1].Value);
                    string AkaFName = Convert.ToString(ws.Cells[j, 2].Value);
                    string AkaLName = Convert.ToString(ws.Cells[j, 3].Value);
                    string AkaClub = Convert.ToString(ws.Cells[j, 4].Value);
                    int AkaF1 = Convert.ToInt32(ws.Cells[j, 5].Value);
                    int AkaF2 = Convert.ToInt32(ws.Cells[j, 6].Value);
                    int Akascore = Convert.ToInt32(ws.Cells[j, 7].Value);

                    int AoId = Convert.ToInt32(ws.Cells[j, 16].Value);
                    string AoFName = Convert.ToString(ws.Cells[j, 15].Value);
                    string AoLName = Convert.ToString(ws.Cells[j, 14].Value);
                    string AoClub = Convert.ToString(ws.Cells[j, 13].Value);
                    int AoF1 = Convert.ToInt32(ws.Cells[j, 12].Value);
                    int AoF2 = Convert.ToInt32(ws.Cells[j, 11].Value);
                    int Aoscore = Convert.ToInt32(ws.Cells[j, 10].Value);

                    Competitor _aka;
                    if (AkaFName != "BYE") { _aka = new Competitor(false, AkaId, AkaFName, AkaLName, AkaClub, Akascore, AkaF1, AkaF2); }
                    else { _aka = new Competitor(true); }

                    Competitor _ao;
                    if (AoFName != "BYE") { _ao = new Competitor(false, AoId, AoFName, AoLName, AoClub, Aoscore, AoF1, AoF2); }
                    else { _ao = new Competitor(true); }
                    Match match = new Match(_aka, _ao, j - 1);
                    /*match.HaveWinner += Match_HaveWinner;*/
                    if (Convert.ToString(ws.Cells[j, 7].Value) == "X") { match.SetWinner(1); }
                    else if (Convert.ToString(ws.Cells[j, 8].Value) == "X") { match.SetWinner(2); }


                    if (!ws.Name.Contains("Repechage") && !ws.Name.Contains("Bronze")) round.Matches.Add(match);
                    else if (ws.Name == "Repechage 1") repAka.Matches.Add(match);
                    else if (ws.Name == "Repechage 2") repAo.Matches.Add(match);
                    else if (ws.Name == "Bronze match") Bronze = new Match(match);
                }

                if (!ws.Name.Contains("Repechage") && !ws.Name.Contains("Bronze")) category.Rounds.Add(round);
                else if (ws.Name == "Repechage 1") category.RepechageAKA = repAka;
                else if (ws.Name == "Repechage 2") category.RepechageAO = repAo;
                else if (ws.Name == "Bronze match") category.BronzeMatch = Bronze;

                if (category.Rounds.Count() > 1)
                {
                    if (category.Rounds[category.Rounds.Count() - 1].Matches.Count() < (category.Rounds[category.Rounds.Count() - 2].Matches.Count() / 2))
                    {
                        int c = (category.Rounds[category.Rounds.Count() - 2].Matches.Count() / 2) - category.Rounds[category.Rounds.Count() - 1].Matches.Count();
                        for (int k = 0; k < c; k++)
                        {

                            Match m = new Match(new Competitor(), new Competitor(), category.Rounds[category.Rounds.Count() - 1].Matches.Count());
                            category.Rounds[category.Rounds.Count() - 1].Matches.Add(m);
                        }
                    }
                }

            }
            VisualBracket = (Excel.Worksheet)wb.Worksheets[wb.Worksheets.Count];

            return category;
        }
        #endregion

        #region Category Results
        CategoryResults CategoryResultsEXT;
        private async void GlobalCategory_HaveCategoryResults(List<Competitor> winners)
        {
            try
            {
                string s_winners = "";
                s_winners += $"1: {winners[0]}\n";
                s_winners += $"2: {winners[1]}\n";
                if (winners.Count() > 2 && winners[2] != null) s_winners += $"3: {winners[2]}\n";
                if (winners.Count() > 3 && winners[3] != null) s_winners += $"3: {winners[3]}\n";

                ContentDialog CategoryResults = new ContentDialog
                {
                    Title = "Info",
                    CloseButtonText = "Close",
                    PrimaryButtonText = "Show Results",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = $"Have category results:\n{s_winners}----------------------------\nShow external board with results?",
                };

                await ContentDialogMaker.CreateContentDialogAsync(CategoryResults, awaitPreviousDialog: true);

                if (ContentDialogMaker.Result == ContentDialogResult.Primary)
                {
                    //Show External Results
                    ShowResultsEXT(winners);
                    if (externalBoard != null) externalBoard.Close();

                    closeExtRes.Visibility = Visibility.Visible;
                }


            }
            catch { }
        }

        private void closeExtRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CategoryResultsEXT.Close();
                CategoryResultsEXT = null;
                closeExtRes.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
        void ShowResultsEXT(List<Competitor> Winners)
        {
            if (CategoryResultsEXT == null) CategoryResultsEXT = new CategoryResults();
            CategoryResultsEXT.SetCategory(CategoryName);
            if (Winners[0] != null) CategoryResultsEXT.SetFirst(Winners[0]);
            if (Winners[1] != null) CategoryResultsEXT.SetSecond(Winners[1]);
            if (Winners[2] != null) CategoryResultsEXT.SetThird(Winners[2]);
            if (Winners[3] != null) CategoryResultsEXT.SetThird1(Winners[3]);

            List<Screen> sc = new List<Screen>();
            sc.AddRange(Screen.AllScreens);
            CategoryResultsEXT.WindowStyle = WindowStyle.None;
            CategoryResultsEXT.Left = sc[Properties.Settings.Default.ScreenNR].Bounds.Left;
            CategoryResultsEXT.Top = sc[Properties.Settings.Default.ScreenNR].Bounds.Top;
            CategoryResultsEXT.Show();
            CategoryResultsEXT.Owner = this;
            CategoryResultsEXT.WindowState = WindowState.Maximized;

            this.Focus();
            this.Activate();

        }

        #endregion

        void GetMatch(int mID, int rID)
        {
            ResetMatch();

            GlobalMatchNow = GlobalCategory.GetCurMatch(mID, rID);
            AKA_curTXT.Text = $"{GlobalMatchNow.AKA.FirstName} {GlobalMatchNow.AKA.LastName}";
            AO_curTXT.Text = $"{GlobalMatchNow.AO.FirstName} {GlobalMatchNow.AO.LastName}";
            AKA_ScoreL.Content = $"{GlobalMatchNow.AKA.ScoreProperty}";
            AO_ScoreL.Content = $"{GlobalMatchNow.AO.ScoreProperty}";
            GlobalMatchNow.HaveWinner += Match_HaveWinner;

            if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded)
            {
                GlobalCategoryViewer.groups_List.SelectedIndex = rID;
                GlobalCategoryViewer.MatchesGrid.SelectedIndex = mID;
            }

            //if (Properties.Settings.Default.AutoNextLoad) GlobalCategory.GetNext();
            Console.WriteLine(GlobalMatchNow.ToString());
            DisplayMessageDialog("Info", "Match loaded");
        }

        private void GlobalCategory_HaveNxtMatch(int round, int match, Match nxtMatch)
        {
            if (round == -1 && match == -1)
            {
                GlobalMatchNxt = new Match(new Competitor(), new Competitor(), 1);
            }
            else if (round < GlobalCategory.Rounds.Count())
            {
                GlobalMatchNxt = nxtMatch;
            }
            else if (round == GlobalCategory.Rounds.Count())
            {
                GlobalMatchNxt = nxtMatch;
            }
            AKA_nxtTXT.Text = $"{GlobalMatchNxt.AKA.FirstName} {GlobalMatchNxt.AKA.LastName}";
            AO_nxtTXT.Text = $"{GlobalMatchNxt.AO.FirstName} {GlobalMatchNxt.AO.LastName}";
            NxtMatch[0] = round; NxtMatch[1] = match;
        }

        private void Match_HaveWinner()
        {
            if (GlobalCategoryViewer != null) 
            { 
                GlobalCategoryViewer.CompetitorsGrid.Items.Refresh();
                GlobalCategoryViewer.MatchWinnerLabel.Content = $"Winner: {GlobalMatchNow.Winner}";
            }
            if (externalBoard != null)
            {
                if (GlobalMatchNow.Winner.Equals(GlobalMatchNow.AKA))
                {
                    externalBoard.ShowWinner(externalBoard.AkaScoreL, externalBoard.AO_Grid);
                }
                else if (GlobalMatchNow.Winner.Equals(GlobalMatchNow.AO))
                {
                    externalBoard.ShowWinner(externalBoard.AoScoreL, externalBoard.AKA_Grid);
                }
            }
            try { end_of_m_sound.Play(); } catch { }
            try { DisplayMessageDialog("Info", $"Match winner: {GlobalMatchNow.Winner.FirstName} {GlobalMatchNow.Winner.LastName}"); }
            catch { }
        }


        private async void DisplayMessageDialog(string caption, string message)
        {
            try
            {
                ContentDialog CategoryResults = new ContentDialog
                {
                    Title = $"{caption}",
                    PrimaryButtonText = "Ok",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = $"{message}",
                };
                await ContentDialogMaker.CreateContentDialogAsync(CategoryResults, awaitPreviousDialog: true);
            }
            catch { }
        }


        #region SET WINNER
        private void AKA_WinnerBTN_Click(object sender, RoutedEventArgs e)
        {
            /*AddInfo($"Winner AKA( {GlobalMatchNow.AKA.FirstName} {GlobalMatchNow.AKA.LastName} )");*/
            GlobalMatchNow.SetWinner(1);
        }

        private void AO_WinnerBTN_Click(object sender, RoutedEventArgs e)
        {
            /*AddInfo($"Winner AO( {GlobalMatchNow.AO.FirstName} {GlobalMatchNow.AO.LastName} )");*/
            GlobalMatchNow.SetWinner(2);
        }
        #endregion

        #region KIKEN and SHIKAKU set
        private void akaKikenBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AKA.SetStatus(1);
        }

        private void akaShikakuBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AKA.SetStatus(2);
        }

        private void aoKikenBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.SetStatus(1);
        }

        private void aoShikakuBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.SetStatus(2);
        }
        #endregion

        private void AO_curTXT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string[] name = AO_curTXT.Text.Split(new char[] { ' ' });
                try { GlobalMatchNow.AO.FirstName = name[0]; } catch { GlobalMatchNow.AO.FirstName = "AO"; Console.WriteLine("No first name found"); }
                try { GlobalMatchNow.AO.LastName = name[1]; } catch { GlobalMatchNow.AO.LastName = ""; Console.WriteLine("No second name found"); }
                this.Focus();
            }
        }
        private void AKA_curTXT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string[] name = AKA_curTXT.Text.Split(new char[] { ' ' });
                try { GlobalMatchNow.AKA.FirstName = name[0]; } catch { GlobalMatchNow.AKA.FirstName = "AKA"; Console.WriteLine("No first name found"); }
                try { GlobalMatchNow.AKA.LastName = name[1]; } catch { GlobalMatchNow.AKA.LastName = ""; Console.WriteLine("No second name found"); }
                this.Focus();
            }
        }


        private void CategoryGenBTN_Click(object sender, RoutedEventArgs e)
        {
            CategoryGenerator CategoryGen = new CategoryGenerator();
            CategoryGen.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (GlobalCategoryViewer != null) { GlobalCategoryViewer.Close(); }
            if (MainExApp != null) { MainExApp.Quit(); }
        }

        void ResetMatch()
        {
            GlobalMatchNow.Reset();

            AO_curTXT.Text = GlobalMatchNow.AO.ToString();
            AKA_curTXT.Text = GlobalMatchNow.AKA.ToString();

            flgsAka.SelectedIndex = -1;
            flgsAo.SelectedIndex = -1;

            AKA_ScoreL.Content = GlobalMatchNow.AKA.ScoreProperty;
            AO_ScoreL.Content = GlobalMatchNow.AO.ScoreProperty;

            if (externalBoard != null)
            {
                externalBoard.GridOpacityAnim(externalBoard.AKA_Grid, 1);
                externalBoard.GridOpacityAnim(externalBoard.AO_Grid, 1);
            }

            try
            {
                DisplayMessageDialog("Info", "Match reseted");
            }
            catch { }
        }

        private void ResetMatchBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetMatch();
        }


        private void FinishMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalMatchNow.Winner != null)
            {
                GlobalCategory.FinishCurMatch();
                if (GlobalCategory.isCurMFinished())
                {
                    if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded) { /*GlobalCategoryViewer.UpdateExcelTree(MainExApp.ActiveWorkbook); */
                        GlobalCategoryViewer.UpdateTree(); }

                    if (GlobalCategoryViewer != null) { GlobalCategoryViewer.MatchesGrid.Items.Refresh(); }
                    GlobalMatchNow.HaveWinner -= Match_HaveWinner;

                    DisplayMessageDialog("Info", "Match finished");
                }
            }
            else
            {
                DisplayMessageDialog("Info", "Mark the winner of match");
            }
        }

        private void NextMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            if (NxtMatch[1] != -1 && NxtMatch[0] != -1)
            {
                ResetMatch();
                GetMatch(NxtMatch[1], NxtMatch[0]);
            }
        }

        private void MainWindow1_Unloaded(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("Closing...");
            if (GlobalCategoryViewer.IsLoaded) { GlobalCategoryViewer.Close(); }
            //if (MainExApp != null) { MainExApp.Quit(); }
        }

        void MakeBindingExternal()
        {
            Binding akaScoreBind = new Binding("Content");
            akaScoreBind.Source = AKA_ScoreL;
            externalBoard.AkaScoreL.SetBinding(Label.ContentProperty, akaScoreBind);

            Binding aoScoreBind = new Binding("Content");
            aoScoreBind.Source = AO_ScoreL;
            externalBoard.AoScoreL.SetBinding(Label.ContentProperty, aoScoreBind);

            Binding akaNowName = new Binding("Text");
            akaNowName.Source = AKA_curTXT;
            externalBoard.AkaNowNameL.SetBinding(Label.ContentProperty, akaNowName);

            Binding aoNowName = new Binding("Text");
            aoNowName.Source = AO_curTXT;
            externalBoard.AoNowNameL.SetBinding(Label.ContentProperty, aoNowName);

            Binding akaNxtName = new Binding("Text");
            akaNxtName.Source = AKA_nxtTXT;
            externalBoard.AkaNextNameL.SetBinding(Label.ContentProperty, akaNxtName);

            Binding aoNxtName = new Binding("Text");
            aoNxtName.Source = AO_nxtTXT;
            externalBoard.AoNextNameL.SetBinding(Label.ContentProperty, aoNxtName);
        }

        private void openExt_btn_Click(object sender, RoutedEventArgs e)
        {
            if (externalBoard == null)
            {
                List<Screen> sc = new List<Screen>();
                sc.AddRange(Screen.AllScreens);
                externalBoard = new Kata_ExternalBoard();
                externalBoard.Send_Status += ExternalBoard_Send_Status;

                if (GlobalCategoryViewer != null && GlobalCategoryViewer.CategoryName != null)
                {
                    try
                    {
                        string[] worrd = GlobalCategoryViewer.CategoryName.Split(new char[] { ' ' }, 2);
                        externalBoard.CategoryEXT.Text += $"{worrd[0]} \n{worrd[1]}";
                    }
                    catch {
                        if (externalBoard != null && externalBoard.IsLoaded)
                            externalBoard.CategoryEXT.Text = GlobalCategoryViewer.CategoryName;
                    }
                }

                MakeBindingExternal();

                externalBoard.WindowStyle = WindowStyle.None;
                externalBoard.Left = sc[Properties.Settings.Default.ScreenNR].Bounds.Left;
                externalBoard.Top = sc[Properties.Settings.Default.ScreenNR].Bounds.Top;
                externalBoard.Owner = this;
                externalBoard.Show();
                externalBoard.WindowState = WindowState.Maximized;

                this.Focus();
                this.Activate();
            }
            else
            {
                externalBoard.Close();
            }

        }

        private void ExternalBoard_Send_Status(bool status)
        {
            if (status) { openExt_btn.Header = "Close ext.board"; }
            else { openExt_btn.Header = "Open ext.board"; }
        }

        ExtTimerSet extTimerSet;
        private void openExtTimerSet_btn_Click(object sender, RoutedEventArgs e)
        {
            if (extTimerSet == null || !extTimerSet.IsLoaded )
            {
                extTimerSet = new ExtTimerSet();
                extTimerSet.Owner = this;
                extTimerSet.Show();
            }
        }


        Settings settings;
        private void SettingsBTN_Click(object sender, RoutedEventArgs e)
        {
            if (settings == null || !settings.IsLoaded )
            {
                settings = new Settings();
                settings.Show();
            }
        }

        private void flgsAka_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flgsAka.SelectedIndex > -1)
            {
                flgsAo.SelectedIndex = JudjesCount - (flgsAka.SelectedIndex);
                GlobalMatchNow.AKA.SetScore(flgsAka.SelectedIndex);
                AKA_ScoreL.Content = $"{GlobalMatchNow.AKA.Score}";
                GlobalMatchNow.CheckWinner(true);
            }
        }

        private void flgsAo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flgsAo.SelectedIndex > -1)
            {
                flgsAka.SelectedIndex = JudjesCount - (flgsAo.SelectedIndex);
                GlobalMatchNow.AO.SetScore(flgsAo.SelectedIndex);
                AO_ScoreL.Content = $"{GlobalMatchNow.AO.Score}";
            }
        }

        private void judjesCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.DefaultJudjesNumber = judjesCB.SelectedIndex;
            Properties.Settings.Default.Save();
            JudjesCount = judjesCB.SelectedIndex + 1;
            for(int i=flgsAka.Items.Count;i<=JudjesCount;i++)
            {
                flgsAka.Items.Add(i);
                flgsAo.Items.Add(i);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}
