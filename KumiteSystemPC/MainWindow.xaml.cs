using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TournamentTree;
using Excel = Microsoft.Office.Interop.Excel;
using WpfScreenHelper;
using System.Data.SQLite;
using System.Windows.Controls;
using TournamentsBracketsBase;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TournamentsBracketsBase.ICompetitor _Aka;
        TournamentsBracketsBase.ICompetitor _Ao;

        TournamentsBracketsBase.IMatch GlobalMatchNow;
        TournamentsBracketsBase.IMatch GlobalMatchNxt;

        List<int> NxtMatch;

        TournamentsBracketsBase.ICategory GlobalCategory;
        string CategoryName = "";

        //OLD//
        Excel.Application MainExApp;
        Excel.Worksheet VisualBracket;
        ////

        CategoryViewer GlobalCategoryViewer;
        CategoryViewer_RoundRobin GlobalCategoryViewerRR;
        ExternalBoard externalBoard;

        System.Media.SoundPlayer end_of_m_sound;
        System.Media.SoundPlayer warn_sound;



        public MainWindow()
        {
            InitializeComponent();
            _Aka = new Competitor(false, 1, "");
            _Ao = new Competitor(false, 2, "");
            GlobalMatchNow = new Match(_Aka as Competitor, _Ao as Competitor, -1);
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

        void AddInfo(string information)
        {
            TextLog.Blocks.Add(new Paragraph(new Run($"{DateTime.Now}\n[INFO] {information}")));
            LogTB.ScrollToEnd();
            try { if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded) { GlobalCategoryViewer.CompetitorsGrid.Items.Refresh(); } }
            catch { }
        }

        #region OPEN CATEGORY
        bool CanOpen = true;

        string dbFileName = "tournaments.sqlite";
        SQLiteConnection m_dbConn;
        SQLiteCommand m_sqlCmd;

        private void DBOpenCategory_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.DefaultDBPath == "")
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

                if (openCategoryDialog.categoryType == 0 || openCategoryDialog.categoryType == 1)
                {
                    CategoryViewer CategoryViewer = new CategoryViewer(GlobalCategory as Category, CategoryName, m_dbConn,
                        openCategoryDialog.CategoryID);
                    CategoryViewer.GetMatchEv += GetMatch;
                    GlobalCategoryViewer = CategoryViewer;
                    GlobalCategoryViewer.Show();
                }
                else if (openCategoryDialog.categoryType == 3)
                {
                    CategoryViewer_RoundRobin categoryViewer = new CategoryViewer_RoundRobin(GlobalCategory as RoundRobin.Category,
                        CategoryName, m_dbConn, openCategoryDialog.CategoryID);
                    categoryViewer.GetMatchEv += GetMatch;
                    GlobalCategoryViewerRR = categoryViewer;
                    GlobalCategoryViewerRR.Show();
                    TieBTN.Visibility = Visibility.Visible;
                }

                if(CategoryResultsEXT != null)
                {
                    CategoryResultsEXT.Close();
                    CategoryResultsEXT = null;
                    closeExtRes.Visibility = Visibility.Collapsed;
                }

                AKA_curTXT.IsReadOnly = true;
                AO_curTXT.IsReadOnly = true;

                AKA_nxtTXT.IsReadOnly = true;
                AO_nxtTXT.IsReadOnly = true;
                try
                {
                    string[] worrd = CategoryName.Split(new char[] { ' ' }, 2);
                    externalBoard.CategoryEXT.Text += $"{worrd[0]} \n{worrd[1]}";
                }
                catch
                {
                    if (externalBoard != null && externalBoard.IsLoaded)
                        externalBoard.CategoryEXT.Text = CategoryName;
                }

            }
        }


        #region OLD VERSION
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

        private void openCategoryBTN_Click(object sender, RoutedEventArgs e)
        {
            OpenCategory();
        }

        void OpenCategory()
        {
            /*Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
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
                    if(externalBoard!= null && externalBoard.IsLoaded)
                        externalBoard.CategoryEXT.Text = GlobalCategoryViewer.CategoryName; 
                }
                CanOpen = false;

            
            }*/
        }

        /*Category ReadCategory(Excel.Workbook wb)
        {
            
             * //TODO: Read competitor's club
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
                    if (AkaFName != "BYE") { _aka = new Competitor(false, AkaId, AkaFName, AkaLName, AkaClub,Akascore, AkaF1, AkaF2); }
                    else { _aka = new Competitor(true); }

                    Competitor _ao;
                    if (AoFName != "BYE") { _ao = new Competitor(false, AoId, AoFName, AoLName, AoClub,Aoscore, AoF1, AoF2); }
                    else { _ao = new Competitor(true); }
                    Match match = new Match(_aka, _ao, j - 1);
                    //match.HaveWinner += Match_HaveWinner;
                    if (Convert.ToString(ws.Cells[j, 8].Value) == "X") { match.SetWinner(1); }
                    else if (Convert.ToString(ws.Cells[j, 9].Value) == "X") { match.SetWinner(2); }


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
        }*/
        #endregion
        #endregion

        #region Category Results

        CategoryResults CategoryResultsEXT;
        private async void GlobalCategory_HaveCategoryResults(List<TournamentsBracketsBase.ICompetitor> winners)
        {
            try
            {
                string s_winners = "";
                s_winners += $"1: {winners[0]}\n";
                s_winners += $"2: {winners[1]}\n";
                if (winners.Count > 2 && winners[2] != null) s_winners += $"3: {winners[2]}\n";
                if (winners.Count > 3 && winners[3] != null) s_winners += $"3: {winners[3]}\n";


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
            catch (Exception ex)
            {
                DisplayMessageDialog("Error", ex.Message);
            }
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
        void ShowResultsEXT(List<TournamentsBracketsBase.ICompetitor> Winners)
        {
            if (CategoryResultsEXT == null) CategoryResultsEXT = new CategoryResults();
            CategoryResultsEXT.SetCategory(CategoryName);
            if (Winners[0] != null) CategoryResultsEXT.SetFirst(Winners[0]);
            if (Winners[1] != null) CategoryResultsEXT.SetSecond(Winners[1]);
            if (Winners.Count > 2 && Winners[2] != null) CategoryResultsEXT.SetThird(Winners[2]);
            if (Winners.Count > 3 && Winners[3] != null) CategoryResultsEXT.SetThird1(Winners[3]);

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

        string GetCompetitorString(TournamentsBracketsBase.ICompetitor competitor)
        {
            string res = competitor.ToString();
            if (Properties.Settings.Default.ShowCompetitorClub) { res += $" ({competitor.Club})"; }
            return res;
        }

        void GetMatch(int mID, int rID)
        {
            ResetMatch();

            GlobalMatchNow = GlobalCategory.GetMatch(mID, rID);

            AKA_curTXT.Text = GetCompetitorString(GlobalMatchNow.AKA);
            AO_curTXT.Text = GetCompetitorString(GlobalMatchNow.AO);



            AKA_ScoreL.Content = $"{GlobalMatchNow.AKA.ScoreProperty}";
            AO_ScoreL.Content = $"{GlobalMatchNow.AO.ScoreProperty}";

            if (GlobalMatchNow.AKA.Senshu) { AKAsenshuCB.IsChecked = true; }
            else if (GlobalMatchNow.AO.Senshu) { AOsenshuCB.IsChecked = true; }

            SetAkaFouls((Competitor.Fouls)GlobalMatchNow.AKA.Fouls_C1);
            SetAoFouls((Competitor.Fouls)GlobalMatchNow.AO.Fouls_C1);

            GlobalMatchNow.HaveWinner += Match_HaveWinner;

            if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded)
            {
                GlobalCategoryViewer.groups_List.SelectedIndex = rID;
                GlobalCategoryViewer.MatchesGrid.SelectedIndex = mID;
            }
            else if (GlobalCategoryViewerRR != null && GlobalCategoryViewerRR.IsLoaded)
            {
                GlobalCategoryViewerRR.groups_List.SelectedIndex = rID;
                GlobalCategoryViewerRR.MatchesGrid.SelectedIndex = mID;
            }

            //if (Properties.Settings.Default.AutoNextLoad) GlobalCategory.GetNext();
            //Console.WriteLine(GlobalMatchNow.ToString());
            DisplayMessageDialog("Info", "Match loaded");
        }

        void SetAkaFouls(Competitor.Fouls fouls)
        {
            if (fouls == Competitor.Fouls.Chui1)
                AKA_C1_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.Chui2)
                AKA_C2_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.Chui3)
                AKA_C3_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.HansokuChui)
                AKA_HC1_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.Hansoku)
                AKA_H1_CB.IsChecked = true;
        }
        void SetAoFouls(Competitor.Fouls fouls)
        {
            if (fouls == Competitor.Fouls.Chui1)
                AO_C1_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.Chui2)
                AO_C2_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.Chui3)
                AO_C3_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.HansokuChui)
                AO_HC1_CB.IsChecked = true;
            else if (fouls == Competitor.Fouls.Hansoku)
                AO_H1_CB.IsChecked = true;
        }

        private void GlobalCategory_HaveNxtMatch(int round, int match, TournamentsBracketsBase.IMatch nxtMatch)
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
            AKA_nxtTXT.Text = GetCompetitorString(GlobalMatchNxt.AKA); ;
            AO_nxtTXT.Text = GetCompetitorString(GlobalMatchNxt.AO); ;


            NxtMatch[0] = round; NxtMatch[1] = match;
        }

        private void Match_HaveWinner()
        {
            if (GlobalMatchNow.Winner != null)
            {
                if (GlobalCategoryViewer != null)
                {
                    GlobalCategoryViewer.CompetitorsGrid.Items.Refresh();
                    if (!GlobalMatchNow.Winner.IsBye) GlobalCategoryViewer.MatchWinnerLabel.Content = $"Winner: {GetCompetitorString(GlobalMatchNow.Winner)}";
                }
                else if (GlobalCategoryViewerRR != null)
                {
                    GlobalCategoryViewerRR.CompetitorsGrid.Items.Refresh();
                    if (!GlobalMatchNow.Winner.IsBye) GlobalCategoryViewerRR.MatchWinnerLabel.Content = $"Winner: {GetCompetitorString(GlobalMatchNow.Winner)}";
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

                try { DisplayMessageDialog("Info", $"Match winner: {GetCompetitorString(GlobalMatchNow.Winner)}"); }
                catch { }
            }

            try { end_of_m_sound.Play(); } catch { }
        }

        #region DIALOGS
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

        #endregion

        #region LOG
        private void SaveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileName;
            fileName = DateTime.Now.ToShortDateString();
            if (saveLog($"{Properties.Settings.Default.DataPath}\\LOG-{fileName}.txt", LogTB)) { DisplayMessageDialog("Info", "Log saved"); }
            else { DisplayMessageDialog("Info", "Can't save log"); }
        }
        bool saveLog(string _fileName, RichTextBox log)
        {
            try
            {
                TextRange range;
                System.IO.FileStream fStream;
                range = new TextRange(log.Document.ContentStart, log.Document.ContentEnd);
                fStream = new System.IO.FileStream(_fileName, System.IO.FileMode.Create);
                range.Save(fStream, DataFormats.Text);
                fStream.Close();
                return true;
            }
            catch { return false; }
        }

        private void clearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            TextLog.Blocks.Clear();
        }
        #endregion
        #region Timer

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        //  string CurTime;
        TimeSpan timerTime;
        TimeSpan remainTime = new TimeSpan();
        int min = 0, sec = 0;

        // bool IsTimerEnabled;
        int time;

        bool atoshibaraku;
        void showTime(string time)
        {
            TimerL.Content = time;
            //if (externalBoard != null) { externalBoard.TimerEXT.Content = time; }
        }
        public async void controlTime()
        {
            do
            {
                TimeSpan ts = stopWatch.Elapsed;

                showTime(String.Format("{0:mm}:{0:ss}", remainTime));
                TimerLms.Content = String.Format(".{0:ff}", remainTime);
                
                remainTime = timerTime - ts;

                if (remainTime <= TimeSpan.Zero)
                {
                    stopWatch.Stop();
                    TimerFinished();
                }

                if (remainTime <= TimeSpan.FromSeconds(15) && !atoshibaraku) { AtoshiBaraku(); }

                if (!atoshibaraku) await Task.Delay(1000);
                else await Task.Delay(10);

            } while (stopWatch.IsRunning);

            if (remainTime <= TimeSpan.Zero)
            {
                showTime(String.Format("{0:mm}:{0:ss}", TimeSpan.Zero));
                TimerLms.Content = String.Format(".{0:ff}", TimeSpan.Zero);
            }
        }
        void TimerFinished()
        {
            startTimeBTN.Content = "Start";
            showTime(String.Format("{0:00}:{1:00}", remainTime.Minutes, remainTime.Seconds));
            AddInfo($"Stop timer. Time left: {String.Format("{0:00}:{1:00}", remainTime.Minutes, remainTime.Seconds)}");
            GlobalMatchNow.CheckWinner(true);
            //msViewBox.Visibility = Visibility.Collapsed;
            //FinishMatch(CheckWin(0), true);
        }
        void AtoshiBaraku()
        {
            showTime(String.Format("{0:mm}:{0:ss}", remainTime));
            atoshibaraku = true;
            try { warn_sound.Play(); } catch { }
            TimerL.Foreground = Brushes.DarkRed;
            TimerLms.Foreground = Brushes.DarkRed;
            msViewBox.Visibility = Visibility.Visible;
        }


        private void _10secBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!stopWatch.IsRunning)
            {
                min = 0;
                sec = 10;
                // time = min * 60 + sec;
                timerTime = new TimeSpan(0, min, sec);
                remainTime = timerTime;
                TimerL.Foreground = Brushes.DarkRed;
                TimerLms.Foreground = Brushes.DarkRed;
                msViewBox.Visibility = Visibility.Visible;
                if (externalBoard != null) { externalBoard.TimerEXT.Foreground = Brushes.DarkRed; }
                //IsTimerEnabled = true;

                if (!stopWatch.IsRunning) { startTimeBTN.Content = "Stop"; stopWatch.Start(); }
                //  timer.Start();
                controlTime();
            }
        }

        private void startTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                stopWatch.Stop();
                AddInfo($"Stop timer. Time left: {TimerL.Content}");
                startTimeBTN.Content = "Start";
                TimerL.IsEnabled = false;
                //if (externalBoard != null) externalBoard.TimerEXT.IsEnabled = false;
            }
            else if (remainTime > TimeSpan.Zero)
            {
                stopWatch.Start();
                controlTime();
                AddInfo($"Start timer. Time left: {TimerL.Content}");
                startTimeBTN.Content = "Stop";
                TimerL.IsEnabled = true;
                //if (externalBoard != null) externalBoard.TimerEXT.IsEnabled = true;
            }
        }

        //extTimerSet extTimerSet;


        /*private void extTimer_Click(object sender, RoutedEventArgs e)
        {
            if (extTimerSet == null)
            {
                extTimerSet = new extTimerSet();
                extTimerSet.Owner = this;
                extTimerSet.Show();
            }
            else { extTimerSet.Close(); extTimerSet = null; }
        }*/

        private void SetTime_Click(object sender, RoutedEventArgs e)
        {
            if (!stopWatch.IsRunning)
                SetTimeer();
        }

        private void ResetTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!stopWatch.IsRunning)
            {
                SetTimeer();
                stopWatch.Reset();
            }
        }

        void SetTimeer()
        {
            atoshibaraku = false;
            min = Convert.ToInt32(TimeM.Text);
            sec = Convert.ToInt32(TimeS.Text);

            if (sec > 60)
            {
                min = sec / 60;
                sec -= min * 60;
            }

            TimerL.Foreground = Brushes.White;
            TimerLms.Foreground = Brushes.White;
            TimerL.Content = String.Format("{0:d2}:{1:d2}", min, sec);
            msViewBox.Visibility = Visibility.Collapsed;
            //if (externalBoard != null) { externalBoard.TimerText(sec, min); }
            TimeM.Text = String.Format("{0:d2}", min);
            TimeS.Text = String.Format("{0:d2}", sec);
            //timer.SetTime(min, sec);
            timerTime = new TimeSpan(0, min, sec);
            remainTime = timerTime;
            time = min * 60 + sec;
            if (time <= 15) { msViewBox.Visibility = Visibility.Visible; }

            stopWatch.Reset();
        }

        private void TimeS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !stopWatch.IsRunning)
            {
                try
                {
                    int chislo = Convert.ToInt32(TimeS.Text);
                    SetTimeer();
                }
                catch
                {
                    //   SetTime.IsEnabled = false;
                    if (MessageBox.Show("Invalid Values are entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    {
                        min = 0;
                        sec = 0;
                        TimeM.Text = String.Format("{0:d2}", min);
                        TimeS.Text = String.Format("{0:d2}", sec);
                        SetTimeer();
                    }
                }
                Keyboard.ClearFocus();
            }
        }



        private void TimeM_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !stopWatch.IsRunning)
            {
                try
                {
                    int chislo = Convert.ToInt32(TimeM.Text);
                    SetTimeer();
                }
                catch
                {
                    //  SetTime.IsEnabled = false;
                    if (MessageBox.Show("Invalid Values are entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    {
                        min = 0;
                        sec = 0;
                        time = 0;
                        TimeM.Text = String.Format("{0:d2}", min);
                        TimeS.Text = String.Format("{0:d2}", sec);
                        SetTimeer();
                    }
                }
                Keyboard.ClearFocus();
            }
        }
        #endregion

        #region SET WINNER BUTTONS
        private void AKA_WinnerBTN_Click(object sender, RoutedEventArgs e)
        {
            AddInfo($"Winner AKA( {GlobalMatchNow.AKA.FirstName} {GlobalMatchNow.AKA.LastName} )");
            GlobalMatchNow.SetWinner(1);
        }

        private void AO_WinnerBTN_Click(object sender, RoutedEventArgs e)
        {
            AddInfo($"Winner AO( {GlobalMatchNow.AO.FirstName} {GlobalMatchNow.AO.LastName} )");
            GlobalMatchNow.SetWinner(2);
        }

        private void TieBTN_Click(object sender, RoutedEventArgs e)
        {
            AddInfo($"Match ended with tie");
            GlobalMatchNow.SetWinner(0);
        }

        #endregion

        #region SENSHU + POINTS

        void AddPoints_wCh(int competitor, int points)
        {
            if (competitor == 0)
            {
                GlobalMatchNow.AKA.AddPoints(points);
                if (GlobalMatchNow.AKA.ScoreProperty >= 0)
                {
                    if (points > 0) { AddInfo($"AKA add point {points}. Points: {GlobalMatchNow.AKA.ScoreProperty}"); }
                    else if (points < 0) { AddInfo($"AKA remove point. Points: {GlobalMatchNow.AKA.ScoreProperty}"); }
                }
                else { GlobalMatchNow.AKA.AddPoints(-points); }
            }
            else if (competitor == 1)
            {
                GlobalMatchNow.AO.AddPoints(points);
                if (GlobalMatchNow.AO.ScoreProperty >= 0)
                {
                    if (points > 0) { AddInfo($"AO add point {points}. Points: {GlobalMatchNow.AO.ScoreProperty}"); }
                    else if (points < 0) { AddInfo($"AO remove point. Points: {GlobalMatchNow.AO.ScoreProperty}"); }
                }
                else { GlobalMatchNow.AO.AddPoints(-points); }
            }
            AO_ScoreL.Content = GlobalMatchNow.AO.ScoreProperty;
            AKA_ScoreL.Content = GlobalMatchNow.AKA.ScoreProperty;

            GlobalMatchNow.CheckWinner(remainTime <= TimeSpan.Zero);
        }

        private void AOsenshuCB_Click(object sender, RoutedEventArgs e)
        {
            if (AOsenshuCB.IsChecked == true)
            {
                
            }
            else
            {
                
            }
        }

        private void AOsenshuCB_Checked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.Senshu = true;
            GlobalMatchNow.AKA.Senshu = false;
            AKAsenshuCB.IsChecked = false;
            AddInfo("AO senshu");
            //AO_SenshuL.Visibility = Visibility.Visible;
            AoSenshuBorder.Visibility = Visibility.Visible;
            AkaSenshuBorder.Visibility = Visibility.Collapsed;
            //AKA_SenshuL.Visibility = Visibility.Collapsed;
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.aoSenshu, 1); externalBoard.ShowSanction(externalBoard.akaSenshu, 0); }
        }

        private void AOsenshuCB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.Senshu = false;
            AddInfo("AO senshu remove");
            //AO_SenshuL.Visibility = Visibility.Collapsed;
            AoSenshuBorder.Visibility = Visibility.Collapsed;
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.aoSenshu, 0); }
        }

       


        private void AKAipponBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(0, 3);
        }

        private void AKAwazariBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(0, 2);
        }

        private void AKAyukoBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(0, 1);
        }

        private void AOyukoBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(1, 1);
        }

        private void AOwazariBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(1, 2);
        }

        private void AOipponBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(1, 3);
        }

        private void AKAplusBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(0, 1);
        }

        private void AOplusBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(1, 1);
        }

        private void AKAminusBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(0, -1);
        }

        private void AOminusBTN_Click(object sender, RoutedEventArgs e)
        {
            AddPoints_wCh(1, -1);
        }

        private void AKAsenshuCB_Click(object sender, RoutedEventArgs e)
        {
            if (AKAsenshuCB.IsChecked == true)
            {
            }
            else
            {

            }
        }

        private void AKAsenshuCB_Checked(object sender, RoutedEventArgs e)
        {
                GlobalMatchNow.AO.Senshu = false;
                GlobalMatchNow.AKA.Senshu = true;
                AOsenshuCB.IsChecked = false;
                AddInfo("AKA senshu");
                //AKA_SenshuL.Visibility = Visibility.Visible;
                // AO_SenshuL.Visibility = Visibility.Collapsed;
                AoSenshuBorder.Visibility = Visibility.Collapsed;
                AkaSenshuBorder.Visibility = Visibility.Visible;
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.akaSenshu, 1); externalBoard.ShowSanction(externalBoard.aoSenshu, 0);
            }
        }

        private void AKAsenshuCB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AKA.Senshu = false;
            AddInfo("AKA senshu remove");
            //AKA_SenshuL.Visibility = Visibility.Collapsed;
            AkaSenshuBorder.Visibility = Visibility.Collapsed;
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.akaSenshu, 0); }
        }
        #endregion


        #region FOULS C1 AKA
        private void AKA_C1_CB_Uncheked(object srnder, RoutedEventArgs e)
        {
            if (AKA_C3_CB.IsChecked == true || AKA_C2_CB.IsChecked == true || AKA_HC1_CB.IsChecked == true
                || AKA_H1_CB.IsChecked == true)
            { AKA_C1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(0);
                AddInfo("AKA remove sanction C1");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c1AKA, 0); }
            }
        }
        private void AKA_C1_CB_Checked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AKA.SetFoulsC1(1); AddInfo("AKA sanction C1");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c1AKA, 1); }
        }
        private void AKA_C2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_C3_CB.IsChecked == true || AKA_HC1_CB.IsChecked == true || AKA_H1_CB.IsChecked == true)
            { AKA_C2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(GlobalMatchNow.AKA.Fouls_C1 - 1);
                AddInfo("AKA remove sanction C2");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c2AKA, 0); }
            }
        }
        private void AKA_C2_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C1_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC1(2);
            AddInfo("AKA sanction C2");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c2AKA, 1); }
        }
        private void AKA_C3_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_HC1_CB.IsChecked == true || AKA_H1_CB.IsChecked == true)
            { AKA_C3_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(GlobalMatchNow.AKA.Fouls_C1 - 1);
                AddInfo("AKA remove sanction C3");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c3AKA, 0); }
            }
        }
        private void AKA_C3_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C1_CB.IsChecked = true;
            AKA_C2_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC1(3);
            AddInfo("AKA sanction C3");
            //TODO: Animation
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c3AKA, 1); }
        }
        private void AKA_HC1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_H1_CB.IsChecked == true) { AKA_HC1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(GlobalMatchNow.AKA.Fouls_C1 - 1);
                AddInfo("AKA remove sanction HC");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc1AKA, 0); }

            }
        }
        private void AKA_HC1_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C1_CB.IsChecked = true; AKA_C2_CB.IsChecked = true; AKA_C3_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC1(4);
            AddInfo("AKA sanction HC");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc1AKA, 1); }
        }


        private void AKA_H1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AKA.SetFoulsC1(GlobalMatchNow.AKA.Fouls_C1 - 1);
            AddInfo("AKA remove sanction H");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h1AKA, 0); }
        }
        private void AKA_H1_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C1_CB.IsChecked = true; AKA_C2_CB.IsChecked = true; AKA_C3_CB.IsChecked = true; AKA_HC1_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC1(5);
            AddInfo("AKA sanction H");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h1AKA, 1); }
        }

        #endregion

        //Not used
        #region FOULS C2 AKA
        //private void AKA_C2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (AKA_K2_CB.IsChecked == true || AKA_HC2_CB.IsChecked == true || AKA_H2_CB.IsChecked == true) { AKA_C2_CB.IsChecked = true; }
        //    else
        //    {
        //        GlobalMatchNow.AKA.SetFoulsC2(0);
        //        AddInfo("AKA remove sanction C2 C");
        //        if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c2AKA, 0); }
        //    }
        //}
        //private void AKA_C2_CB_Checked(object sender, RoutedEventArgs e)
        //{

        //    if (AKA_C2_CB.IsChecked == true)
        //    {
        //        GlobalMatchNow.AKA.SetFoulsC2(1); AddInfo("AKA sanction C2 C");
        //        if (externalBoard != null)
        //        {
        //            externalBoard.ShowSanction(externalBoard.c2AKA, 1);

        //        }
        //    }

        //}

        //private void AKA_K2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (AKA_HC2_CB.IsChecked == true || AKA_H2_CB.IsChecked == true) { AKA_K2_CB.IsChecked = true; }
        //    else
        //    {
        //        GlobalMatchNow.AKA.SetFoulsC2(GlobalMatchNow.AKA.Fouls_C2 - 1);
        //        AddInfo("AKA remove sanction C2 K");
        //        if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.k2AKA, 0); }
        //    }
        //}
        //private void AKA_K2_CB_Checked(object sender, RoutedEventArgs e)
        //{

        //    AKA_C2_CB.IsChecked = true;
        //    GlobalMatchNow.AKA.SetFoulsC2(2);
        //    AddInfo("AKA sanction C2 K");
        //    if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.k2AKA, 1); }
        //}

        //private void AKA_HC2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (AKA_H2_CB.IsChecked == true) { AKA_HC2_CB.IsChecked = true; }
        //    else
        //    {
        //        GlobalMatchNow.AKA.SetFoulsC2(GlobalMatchNow.AKA.Fouls_C2 - 1);
        //        AddInfo("AKA remove sanction C2 HC");
        //        if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc2AKA, 0); }
        //    }
        //}
        //private void AKA_HC2_CB_Checked(object sender, RoutedEventArgs e)
        //{
        //    AKA_C2_CB.IsChecked = true; AKA_K2_CB.IsChecked = true;
        //    GlobalMatchNow.AKA.SetFoulsC2(3);
        //    AddInfo("AKA sanction C2 HC");
        //    if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc2AKA, 1); }
        //}

        //private void AKA_H2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    GlobalMatchNow.AKA.SetFoulsC2(GlobalMatchNow.AKA.Fouls_C2 - 1); AddInfo("AKA remove sanction C2 H");
        //    if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h2AKA, 0); }
        //}
        //private void AKA_H2_CB_Checked(object sender, RoutedEventArgs e)
        //{
        //    AKA_C2_CB.IsChecked = true; AKA_K2_CB.IsChecked = true; AKA_HC2_CB.IsChecked = true;
        //    GlobalMatchNow.AKA.SetFoulsC2(4);
        //    AddInfo("AKA sanction C2 H");
        //    if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h2AKA, 1); }


        //}
        #endregion


        #region FOULS C1 AO
        private void AO_C1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_C3_CB.IsChecked == true || AO_C2_CB.IsChecked == true || AO_HC1_CB.IsChecked == true
                || AO_H1_CB.IsChecked == true)
            { AO_C1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(0);
                AddInfo("AO remove sanction C1");
                if (externalBoard != null)
                    externalBoard.ShowSanction(externalBoard.c1AO, 0);
            }
        }
        private void AO_C1_CB_Checked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.SetFoulsC1(1);
            AddInfo("AO sanction C1");
            if (externalBoard != null)
                externalBoard.ShowSanction(externalBoard.c1AO, 1);
        }
        private void AO_C2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_C3_CB.IsChecked == true || AO_HC1_CB.IsChecked == true || AO_H1_CB.IsChecked == true)
            { AO_C2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(GlobalMatchNow.AO.Fouls_C1 - 1);
                AddInfo("AO remove sanction C2");
                if (externalBoard != null)
                    externalBoard.ShowSanction(externalBoard.c2AO, 0);
            }
        }
        private void AO_C2_CB_Checked(object sender, RoutedEventArgs e)
        {
            AO_C1_CB.IsChecked = true;
            GlobalMatchNow.AO.SetFoulsC1(2);
            AddInfo("AO sanction C2");
            if (externalBoard != null)
                externalBoard.ShowSanction(externalBoard.c2AO, 1);
        }

        private void AO_C3_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_HC1_CB.IsChecked == true || AO_H1_CB.IsChecked == true)
            { AO_C3_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(GlobalMatchNow.AO.Fouls_C1 - 1);
                AddInfo("AO remove sanction C3");
                if (externalBoard != null)
                    externalBoard.ShowSanction(externalBoard.c3AO, 0);
            }
        }
        private void AO_C3_CB_Checked(object sender, RoutedEventArgs e)
        {
            AO_C1_CB.IsChecked = true;
            AO_C2_CB.IsChecked = true;
            GlobalMatchNow.AO.SetFoulsC1(3);
            AddInfo("AO sanction C3");
            if (externalBoard != null)
                externalBoard.ShowSanction(externalBoard.c3AO, 1);
        }


        private void AO_HC1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_H1_CB.IsChecked == true) { AO_HC1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(GlobalMatchNow.AO.Fouls_C1 - 1);
                AddInfo("AO remove sanction HC");
                if (externalBoard != null)
                    externalBoard.ShowSanction(externalBoard.hc1AO, 0);
            }
        }
        private void AO_HC1_CB_Checked(object sender, RoutedEventArgs e)
        {
            AO_C1_CB.IsChecked = true; AO_C2_CB.IsChecked = true; AO_C3_CB.IsChecked = true;
            GlobalMatchNow.AO.SetFoulsC1(4);
            AddInfo("AO sanction HC");
            if (externalBoard != null)
                externalBoard.ShowSanction(externalBoard.hc1AO, 1);
        }

        private void AO_H1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.SetFoulsC1(GlobalMatchNow.AO.Fouls_C1 - 1); AddInfo("AO remove sanction H");
            if (externalBoard != null)
                externalBoard.ShowSanction(externalBoard.h1AO, 0);
        }
        private void AO_H1_CB_Checked(object sender, RoutedEventArgs e)
        {
            AO_C1_CB.IsChecked = true; AO_C2_CB.IsChecked = true; AO_C3_CB.IsChecked = true; AO_HC1_CB.IsChecked = true;
            GlobalMatchNow.AO.SetFoulsC1(5);
            AddInfo("AO sanction H");
            if (externalBoard != null)
                externalBoard.ShowSanction(externalBoard.h1AO, 1);

        }

        #endregion

        //Not Used
        #region FOULS C2 AO
        //private void AO_C2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (AO_K2_CB.IsChecked == true || AO_HC2_CB.IsChecked == true || AO_H2_CB.IsChecked == true) { AO_C2_CB.IsChecked = true; }
        //    else
        //    {
        //        GlobalMatchNow.AO.SetFoulsC2(0);
        //        AddInfo("AO remove sanction C2 C");
        //        if (externalBoard != null)
        //        {
        //            externalBoard.ShowSanction(externalBoard.c2AO, 0);


        //        }
        //        //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOC2, 0); }
        //    }
        //}
        //private void AO_C2_CB_Checked(object sender, RoutedEventArgs e)
        //{
        //    GlobalMatchNow.AO.SetFoulsC2(1); AddInfo("AO sanction C2 C");
        //    if (externalBoard != null)
        //    {
        //        externalBoard.ShowSanction(externalBoard.c2AO, 1);

        //    }


        //}

        //private void AO_K2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (AO_HC2_CB.IsChecked == true || AO_H2_CB.IsChecked == true) { AO_K2_CB.IsChecked = true; }
        //    else
        //    {
        //        GlobalMatchNow.AO.SetFoulsC2(GlobalMatchNow.AO.Fouls_C2 - 1);
        //        AddInfo("AO remove sanction C2 K");
        //        if (externalBoard != null)
        //        {
        //            externalBoard.ShowSanction(externalBoard.k2AO, 0);

        //        }
        //        //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOK2, 0); }
        //    }
        //}
        //private void AO_K2_CB_Checked(object sender, RoutedEventArgs e)
        //{

        //    if (AO_K2_CB.IsChecked == true)
        //    {
        //        AO_C2_CB.IsChecked = true;
        //        GlobalMatchNow.AO.SetFoulsC2(2);
        //        AddInfo("AO sanction C2 K");
        //        if (externalBoard != null)
        //        {
        //            externalBoard.ShowSanction(externalBoard.k2AO, 1);

        //        }
        //        //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOK2, 1); }
        //    }



        //}

        //private void AO_HC2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (AO_H2_CB.IsChecked == true) { AO_HC2_CB.IsChecked = true; }
        //    else
        //    {
        //        GlobalMatchNow.AO.SetFoulsC2(GlobalMatchNow.AO.Fouls_C2 - 1);
        //        AddInfo("AO remove sanction C2 HC");
        //        if (externalBoard != null)
        //        {
        //            externalBoard.ShowSanction(externalBoard.hc2AO, 0);

        //        }
        //        //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOHC2, 0); }
        //    }
        //}
        //private void AO_HC2_CB_Checked(object sender, RoutedEventArgs e)
        //{

        //    if (AO_HC2_CB.IsChecked == true)
        //    {
        //        AO_C2_CB.IsChecked = true; AO_K2_CB.IsChecked = true;
        //        GlobalMatchNow.AO.SetFoulsC2(3);
        //        AddInfo("AO sanction C2 HC");
        //        if (externalBoard != null)
        //        {
        //            externalBoard.ShowSanction(externalBoard.hc2AO, 1);

        //        }
        //        //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOHC2, 1); }
        //    }



        //}


        //private void AO_H2_CB_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    GlobalMatchNow.AO.SetFoulsC2(GlobalMatchNow.AO.Fouls_C2 - 1); AddInfo("AO remove sanction C2 H");
        //    if (externalBoard != null)
        //    {
        //        externalBoard.ShowSanction(externalBoard.h2AO, 0);

        //    }
        //}
        //private void AO_H2_CB_Checked(object sender, RoutedEventArgs e)
        //{

        //    AO_C2_CB.IsChecked = true; AO_K2_CB.IsChecked = true; AO_HC2_CB.IsChecked = true;
        //    GlobalMatchNow.AO.SetFoulsC2(4);
        //    AddInfo("AO sanction C2 H");
        //    if (externalBoard != null)
        //    {

        //        externalBoard.ShowSanction(externalBoard.h2AO, 1);

        //    }

        //}
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (GlobalCategoryViewer != null) { GlobalCategoryViewer.Close(); }
            else if (GlobalCategoryViewerRR != null) { GlobalCategoryViewerRR.Close(); }
            if (MainExApp != null) { MainExApp.Quit(); }
        }

        void ResetMatch()
        {
            GlobalMatchNow.Reset();
            SetTimeer();
            stopWatch.Reset();

            AO_curTXT.Text = GlobalMatchNow.AO.ToString();
            AKA_curTXT.Text = GlobalMatchNow.AKA.ToString();

            AOsenshuCB.IsChecked = GlobalMatchNow.AO.Senshu;
            AKAsenshuCB.IsChecked = GlobalMatchNow.AKA.Senshu;

            /*AO_SenshuL.Visibility = Visibility.Collapsed;
            AKA_SenshuL.Visibility = Visibility.Collapsed;*/

            AoSenshuBorder.Visibility = Visibility.Collapsed;
            AkaSenshuBorder.Visibility = Visibility.Collapsed;

            AKA_ScoreL.Content = GlobalMatchNow.AKA.ScoreProperty;
            AO_ScoreL.Content = GlobalMatchNow.AO.ScoreProperty;

            ResetFouls();

            if (externalBoard != null)
            {
                externalBoard.GridOpacityAnim(externalBoard.AKA_Grid, 1);
                externalBoard.GridOpacityAnim(externalBoard.AO_Grid, 1);
                externalBoard.ShowSanction(externalBoard.akaSenshu, 0);
                externalBoard.ShowSanction(externalBoard.aoSenshu, 0);
            }

            TextLog.Blocks.Clear();

            TimerLms.Visibility = Visibility.Collapsed;

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

        void ResetFouls()
        {
            AKA_H1_CB.IsChecked = false;
            AKA_HC1_CB.IsChecked = false;
            AKA_C3_CB.IsChecked = false;
            AKA_C2_CB.IsChecked = false;
            AKA_C1_CB.IsChecked = false;

            //AKA_H2_CB.IsChecked = false;
            //AKA_HC2_CB.IsChecked = false;
            //AKA_K2_CB.IsChecked = false;
            //AKA_C2_CB.IsChecked = false;

            AO_H1_CB.IsChecked = false;
            AO_HC1_CB.IsChecked = false;
            AO_C3_CB.IsChecked = false;
            AO_C2_CB.IsChecked = false;
            AO_C1_CB.IsChecked = false;

            //AO_H2_CB.IsChecked = false;
            //AO_HC2_CB.IsChecked = false;
            //AO_K2_CB.IsChecked = false;
            //AO_C2_CB.IsChecked = false;

            // if (externalBoard != null) { }
        }

        private void FinishMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalMatchNow.ID == -1)
                return;

            if (GlobalMatchNow.Winner == null)
            {
                DisplayMessageDialog("Info", "Mark the match result");
                return;
            }

            GlobalCategory.FinishCurrentMatch();

            if (GlobalCategory.isCurMFinished())
            {
                if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded)
                { /*GlobalCategoryViewer.UpdateExcelTree(MainExApp.ActiveWorkbook);*/
                    GlobalCategoryViewer.UpdateTree();
                }
                else if (GlobalCategoryViewerRR != null && GlobalCategoryViewerRR.IsLoaded)
                { /*GlobalCategoryViewer.UpdateExcelTree(MainExApp.ActiveWorkbook);*/
                    GlobalCategoryViewerRR.UpdateTree();
                }

                string fileName = $"{DateTime.Now.ToShortDateString()}-{GlobalMatchNow.AKA}_{GlobalMatchNow.AO}";
                if (saveLog($"{Properties.Settings.Default.DataPath}\\LOG-{fileName}.txt", LogTB))
                    DisplayMessageDialog("Info", "Log saved");
                else
                    DisplayMessageDialog("Info", "Can't save log");

                //ResetMatch();

                if (GlobalCategoryViewer != null) { GlobalCategoryViewer.MatchesGrid.Items.Refresh(); }
                if (GlobalCategoryViewerRR != null) { GlobalCategoryViewerRR.MatchesGrid.Items.Refresh(); }
                GlobalMatchNow.HaveWinner -= Match_HaveWinner;

                DisplayMessageDialog("Info", "Match finished");
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
            if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded) { GlobalCategoryViewer.Close(); }
            if (GlobalCategoryViewerRR != null && GlobalCategoryViewerRR.IsLoaded) { GlobalCategoryViewerRR.Close(); }
            //if (MainExApp != null) { MainExApp.Quit(); }
        }


        void MakeBindingExternalBoard()
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

            Binding timerTxt = new Binding("Content");
            timerTxt.Source = TimerL;
            externalBoard.TimerEXT.SetBinding(Label.ContentProperty, timerTxt);

            Binding timerColor = new Binding("Foreground");
            timerColor.Source = TimerL;
            externalBoard.TimerEXT.SetBinding(Label.ForegroundProperty, timerColor);

            Binding timerMsColor = new Binding("Foreground");
            timerMsColor.Source = TimerLms;
            externalBoard.TimerEXTms.SetBinding(Label.ForegroundProperty, timerColor);

            Binding timerTxtms = new Binding("Content");
            timerTxtms.Source = TimerLms;
            externalBoard.TimerEXTms.SetBinding(Label.ContentProperty, timerTxtms);

            Binding timerVisMs = new Binding("Visibility");
            timerVisMs.Source = msViewBox;
            externalBoard.TimerEXTms_ViewBox.SetBinding(Viewbox.VisibilityProperty, timerVisMs);
        }


        private void openExt_btn_Click(object sender, RoutedEventArgs e)
        {
            if (externalBoard == null || !externalBoard.IsLoaded)
            {
                List<Screen> sc = new List<Screen>();
                sc.AddRange(Screen.AllScreens);
                externalBoard = new ExternalBoard();
                externalBoard.Send_Status += ExternalBoard_Send_Status;
                try
                {
                    string[] worrd = CategoryName.Split(new char[] { ' ' }, 2);
                    externalBoard.CategoryEXT.Text += $"{worrd[0]} \n{worrd[1]}";
                }
                catch
                {
                    if (externalBoard != null && externalBoard.IsLoaded && !String.IsNullOrEmpty(CategoryName))
                        externalBoard.CategoryEXT.Text = CategoryName;
                }


                MakeBindingExternalBoard();

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
            if (extTimerSet == null || !extTimerSet.IsLoaded)
            {
                extTimerSet = new ExtTimerSet();
                extTimerSet.Owner = this;
                extTimerSet.Show();
            }
        }


        Settings settings;
        private void SettingsBTN_Click(object sender, RoutedEventArgs e)
        {
            if (settings == null || !settings.IsLoaded)
            {
                settings = new Settings();
                settings.Show();
            }
        }

        private void MainWindow1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        #region TeamKumite

        int roundCounter = 1;
        private void roundPlus_Click(object sender, RoutedEventArgs e)
        {
            /*if (roundCounter <= rounds)
            {
                switch (CheckWin(0))
                {
                    case 1:
                        {
                            _roundWinAka();
                            ResetMatch();
                            break;
                        }
                    case 2:
                        {
                            _roundWinAo();
                            ResetMatch();
                            break;
                        }
                    case 0:
                        {
                            _roundDraw();
                            ResetMatch();
                            break;
                        }

                }
            }
            else { finishRounds(); }*/
        }

        void _roundWinAka()
        {
            /* AddInfo($"Round {roundCounter}/{rounds}: winner AKA ({scoreAKA}:{scoreAO})");
             if (externalBoard != null)
             {
                 externalBoard.roundsExt.Children.RemoveAt(roundCounter - 1);
                 externalBoard.addRound(roundCounter, scoreAKA, scoreAO, 1);
                 if (roundCounter < rounds) externalBoard.addText(roundCounter + 1, rounds);
             }
             if (roundCounter <= rounds)
             {
                 takascore += scoreAKA;

                 if (!roundWinner.ContainsKey(roundCounter)) { roundWinner.Add(roundCounter, 1); MessageBox.Show(roundWinner[roundCounter].ToString()); CheckWin(1); roundCounter++; }
                 else
                 {
                     if (MessageBox.Show("Rewrite winner of this match?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                     { roundWinner.Remove(roundCounter); roundWinner.Add(roundCounter, 1); CheckWin(1); roundCounter++; }
                 }

                 if (roundCounter == rounds + 1) { finishRounds(); }
             }
             //  else { finishRounds(); }*/
        }

        void _roundWinAo()
        {
            /* AddInfo($"Round {roundCounter}/{rounds}: winner AO ({scoreAKA}:{scoreAO})");
             if (externalBoard != null)
             {
                 externalBoard.roundsExt.Children.RemoveAt(roundCounter - 1);
                 externalBoard.addRound(roundCounter, scoreAKA, scoreAO, 2);
                 if (roundCounter < rounds) externalBoard.addText(roundCounter + 1, rounds);
             }
             if (roundCounter <= rounds)
             {
                 taoscore += scoreAO;
                 if (!roundWinner.ContainsKey(roundCounter)) { roundWinner.Add(roundCounter, 2); CheckWin(2); MessageBox.Show(roundWinner[roundCounter].ToString()); roundCounter++; }
                 else
                 {
                     if (MessageBox.Show("Rewrite winner of this match?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                     { roundWinner.Remove(roundCounter); roundWinner.Add(roundCounter, 2); CheckWin(2); roundCounter++; }
                 }

                 if (roundCounter == rounds + 1) { finishRounds(); }
             }
             //   else { finishRounds(); }*/
        }

        void _roundDraw()
        {
            /* AddInfo($"Round {roundCounter}/{rounds}:Draw ({scoreAKA}:{scoreAO})");
             if (externalBoard != null)
             {
                 externalBoard.roundsExt.Children.RemoveAt(roundCounter - 1);
                 externalBoard.addRound(roundCounter, scoreAKA, scoreAO, 0);
                 if (roundCounter < rounds) externalBoard.addText(roundCounter + 1, rounds);
             }
             if (roundCounter <= rounds)
             {
                 if (!roundWinner.ContainsKey(roundCounter)) { roundWinner.Add(roundCounter, 0); CheckWin(0); MessageBox.Show(roundWinner[roundCounter].ToString()); roundCounter++; }
                 else
                 {
                     if (MessageBox.Show("Rewrite winner of this match?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                     { roundWinner.Remove(roundCounter); roundWinner.Add(roundCounter, 0); CheckWin(0); roundCounter++; }
                 }

                 if (roundCounter == rounds + 1) { finishRounds(); }
             }
             //    else { finishRounds(); }*/
        }

        private void roundWinAka_Click(object sender, RoutedEventArgs e)
        {
            /* FinishMatch(1, true);
             _roundWinAka();
             ResetMatch();*/
        }
        private void roundWinAo_Click(object sender, RoutedEventArgs e)
        {
            /* FinishMatch(2, true);
             _roundWinAo();

             ResetMatch();*/
        }


        private void roundDraw_Click(object sender, RoutedEventArgs e)
        {
            /*FinishMatch(0, true);
            _roundDraw();

            ResetMatch();*/
        }


        void finishRounds()
        {
            /* int aka = 0, ao = 0;
             if (roundWinner != null)
             {
                 foreach (int i in roundWinner.Keys)
                 {
                     if (roundWinner[i] == 1) { aka++; }
                     else if (roundWinner[i] == 2) { ao++; }
                     //    else if (roundWinner[i] == 0) { draw++; }
                 }
             }
             if (aka > ao) { FinishMatch(CheckWin(1), true); MessageBox.Show("Team AKA wins"); }
             else if (ao > aka) { FinishMatch(CheckWin(2), true); MessageBox.Show("Team AO wins"); }
             else if (aka == ao)
             {
                 if (takascore > taoscore) { FinishMatch(CheckWin(1), true); MessageBox.Show("Team AKA wins"); }
                 else if (taoscore > takascore) { FinishMatch(CheckWin(2), true); MessageBox.Show("Team AO wins"); }
             }
             ResetMatch();
             if (MessageBox.Show("Reset rounds?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) { ResetRounds(); }*/
        }

        void ResetRounds()
        {
            /* roundCounter = 1;
             takascore = 0;
             taoscore = 0;
             roundWinner.Clear();
             if (externalBoard != null) { externalBoard.roundsExt.Children.Clear(); }
             MessageBox.Show("Rounds reseted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);*/
        }

        private void roundMinus_Click(object sender, RoutedEventArgs e)
        {
            /*if (roundWinner.ContainsKey(roundCounter - 2))
            {
                roundWinner.Remove(roundCounter - 2);
                if (externalBoard != null) { externalBoard.roundsExt.Children.RemoveAt(roundCounter - 2); }
                roundCounter--;
            }*/
        }

        

        private void roundTB_KeyDown(object sender, KeyEventArgs e)
        {
            /*if (e.Key == Key.Enter)
            {
                try
                {
                    rounds = Convert.ToInt32(roundTB.Text);
                    if (roundWinner == null) { roundWinner = new Dictionary<int, int>(); }
                    if (externalBoard != null) { externalBoard.addText(1, rounds); }
                    MessageBox.Show($"Rounds are setted to {rounds}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { MessageBox.Show("Invalid Values are entered", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); roundTB.Text = null; }*/
        }
    }
    #endregion


}
