using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TournamentTree;
using Excel = Microsoft.Office.Interop.Excel;
using WpfScreenHelper;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

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
        public MainWindow()
        {
            InitializeComponent();
            _Aka = new Competitor(false, 1, "AKA");
            _Ao = new Competitor(false, 2, "AO");
            GlobalMatchNow = new Match(_Aka, _Ao, 0);
            GlobalMatchNow.HaveWinner += Match_HaveWinner;
            NxtMatch = new List<int>() { -1, -1 };
            if (Properties.Settings.Default.EndOfMatch != "") { end_of_m_sound = new System.Media.SoundPlayer(Properties.Settings.Default.EndOfMatch); }
            if (Properties.Settings.Default.WarningSound != "") { warn_sound = new System.Media.SoundPlayer(Properties.Settings.Default.WarningSound); }
        }
        DateTime dateTime;
        void AddInfo(string information)
        {
            dateTime = DateTime.Now;
            TextLog.Blocks.Add(new Paragraph(new Run($"{dateTime}\n[INFO] {information}")));
            LogTB.ScrollToEnd();
            try { if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded) { GlobalCategoryViewer.CompetitorsGrid.Items.Refresh(); } }
            catch { }
        }
        #region OPEN CATEGORY
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
            if (openFile.ShowDialog() == true)
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
                GlobalCategory.HaveNxtMatch += GlobalCategory_HaveNxtMatch;
                GlobalCategory.HaveCategoryResults += GlobalCategory_HaveCategoryResults;

                CategoryName = MainExApp.ActiveWorkbook.Name;

                CategoryViewer CategoryViewer = new CategoryViewer(GlobalCategory, MainExApp.ActiveWorkbook.Name, MainExApp.ActiveWorkbook);
                CategoryViewer.GetMatchEv += GetMatch;
                GlobalCategoryViewer = CategoryViewer;
                GlobalCategoryViewer.Show();
                MainExApp.DisplayAlerts = false;
                MainExApp.Visible = true;

                AKA_curTXT.IsReadOnly = true;
                AO_curTXT.IsReadOnly = true;

                AKA_nxtTXT.IsReadOnly = true;
                AO_nxtTXT.IsReadOnly = true;

                if (externalBoard != null)
                {
                    string[] worrd = GlobalCategoryViewer.CategoryName.Split(new char[] { ' ' }, 2);
                    externalBoard.CategoryEXT.Text += $"{worrd[0]} \n{worrd[1]}";
                }

            }
        }


        CategoryResults CategoryResultsEXT;
        private async void GlobalCategory_HaveCategoryResults(List<Competitor> winners)
        {
            try
            {
                string s_winners="";
                s_winners+=$"1: {winners[0]}\n";
                s_winners += $"2: {winners[1]}\n";
                if (winners.Count() > 2) s_winners += $"3: {winners[2]}\n";
                if (winners.Count() > 3) s_winners += $"3: {winners[3]}\n";
                ContentDialog CategoryResults = new ContentDialog
                {
                    Title = "Info",
                    CloseButtonText = "Close",
                    PrimaryButtonText = "Show Results",
                    Content = $"Have category results:\n{s_winners}----------------------------\nShow external board with results?",
                };

                await ContentDialogMaker.CreateContentDialogAsync(CategoryResults, awaitPreviousDialog: true);

                if(ContentDialogMaker.Result == ContentDialogResult.Primary)
                {
                    //Show External Results
                    ShowResultsEXT(winners);
                    if (externalBoard != null) externalBoard.Close();
                    Console.WriteLine("Got you");
                }
            }
            catch { }
        }


        void ShowResultsEXT(List<Competitor> Winners)
        {
            CategoryResultsEXT = new CategoryResults();
            CategoryResultsEXT.SetCategory(CategoryName);
            switch (Winners.Count())
            {
                case 1:
                    CategoryResultsEXT.SetFirst(Winners[Winners.Count - 1]);
                    break;
                case 2:
                    CategoryResultsEXT.SetFirst(Winners[Winners.Count - 1]);
                    CategoryResultsEXT.SetSecond(Winners[Winners.Count - 2]);
                    break;
                case 3:
                    CategoryResultsEXT.SetFirst(Winners[Winners.Count - 1]);
                    CategoryResultsEXT.SetSecond(Winners[Winners.Count - 2]);
                    CategoryResultsEXT.SetThird(Winners[Winners.Count - 3]);
                    break;
                case 4:
                    CategoryResultsEXT.SetFirst(Winners[Winners.Count - 1]);
                    CategoryResultsEXT.SetSecond(Winners[Winners.Count - 2]);
                    CategoryResultsEXT.SetThird(Winners[Winners.Count - 3]);
                    CategoryResultsEXT.SetThird1(Winners[Winners.Count - 4]);
                    break;

            }

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

            /*if (externalBoard != null)
            {
                externalBoard.ResetScreen();
            }*/

            /*extResCL.Header = "Close ext. category results";
            extResCL.Visibility = Visibility.Visible;*/

        }


        private void GlobalCategory_HaveNxtMatch(int round, int match)
        {
            if (round == -1 && match == -1)
            {
                GlobalMatchNxt = new Match(new Competitor(), new Competitor(), 1);
            }
            else if (round < GlobalCategory.Rounds.Count())
            {
                GlobalMatchNxt = GlobalCategory.Rounds[round].Matches[match];
            }
            else if (round == GlobalCategory.Rounds.Count())
            {
                GlobalMatchNxt = GlobalCategory.RepechageAKA.Matches[match];
            }
            else if (round == GlobalCategory.Rounds.Count() + 1)
            {
                GlobalMatchNxt = GlobalCategory.RepechageAO.Matches[match];
            }
            AKA_nxtTXT.Text = $"{GlobalMatchNxt.AKA.FirstName} {GlobalMatchNxt.AKA.LastName}";
            AO_nxtTXT.Text = $"{GlobalMatchNxt.AO.FirstName} {GlobalMatchNxt.AO.LastName}";
            NxtMatch[0] = round; NxtMatch[1] = match;
        }

        Category ReadCategory(Excel.Workbook wb)
        {
            int count = wb.Worksheets.Count - 1;
            Category category = new Category();
            for (int i = 1; i <= count; i++)
            {
                Round round = new Round();
                Excel.Worksheet ws = wb.Worksheets[i];
                for (int j = 2; j <= ws.UsedRange.Rows.Count; j++)
                {
                    int AkaId = Convert.ToInt32(ws.Cells[j, 1].Value);
                    string AkaFName = Convert.ToString(ws.Cells[j, 2].Value);
                    string AkaLName = Convert.ToString(ws.Cells[j, 3].Value);
                    int AkaF1 = Convert.ToInt32(ws.Cells[j, 4].Value);
                    int AkaF2 = Convert.ToInt32(ws.Cells[j, 5].Value);
                    int Akascore = Convert.ToInt32(ws.Cells[j, 6].Value);

                    int AoId = Convert.ToInt32(ws.Cells[j, 14].Value);
                    string AoFName = Convert.ToString(ws.Cells[j, 13].Value);
                    string AoLName = Convert.ToString(ws.Cells[j, 12].Value);
                    int AoF1 = Convert.ToInt32(ws.Cells[j, 11].Value);
                    int AoF2 = Convert.ToInt32(ws.Cells[j, 10].Value);
                    int Aoscore = Convert.ToInt32(ws.Cells[j, 9].Value);

                    Competitor _aka;
                    if (AkaFName != "BYE") { _aka = new Competitor(false, AkaId, AkaFName, AkaLName, Akascore, AkaF1, AkaF2); }
                    else { _aka = new Competitor(true); }

                    Competitor _ao;
                    if (AoFName != "BYE") { _ao = new Competitor(false, AoId, AoFName, AoLName, Aoscore, AoF1, AoF2); }
                    else { _ao = new Competitor(true); }
                    Match match = new Match(_aka, _ao, j - 1);
                    match.HaveWinner += Match_HaveWinner;
                    if (Convert.ToString(ws.Cells[j, 7].Value) == "X") { match.SetWinner(1); }
                    else if (Convert.ToString(ws.Cells[j, 8].Value) == "X") { match.SetWinner(2); }
                    round.Matches.Add(match);
                }

                category.Rounds.Add(round);

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


        private void Match_HaveWinner()
        {
            if (GlobalCategoryViewer != null) { GlobalCategoryViewer.CompetitorsGrid.Items.Refresh(); }
            if (externalBoard != null)
            {
                if (GlobalMatchNow.Winner.Equals(GlobalMatchNow.AKA))
                {
                    externalBoard.ShowWinner(externalBoard.AkaScoreL, externalBoard.AO_Grid);
                }
                else if (GlobalMatchNow.Winner.Equals(GlobalMatchNow.AO))
                {
                    externalBoard.ShowWinner(externalBoard.AoScoreL,externalBoard.AKA_Grid);
                }
            }
            try { end_of_m_sound.Play(); } catch { }
            try { DisplayMessageDialog("Info", $"Match winner: {GlobalMatchNow.Winner.FirstName} {GlobalMatchNow.Winner.LastName}"); }
            catch { }
        }
        #endregion

        private async void DisplayMessageDialog(string caption, string message)
        {
            /*try
            {
                ContentDialog ServerDialog = new ContentDialog
                {
                    Title = caption,
                    CloseButtonText = "Ok",
                    Content = message,
                };
                ContentDialogResult result = await ServerDialog.ShowAsync();
            }
            catch { }*/
            await ContentDialogMaker.CreateContentDialogAsync(new ContentDialog
            {
                Title = caption,
                Content = message,
                PrimaryButtonText = "OK"
            }, awaitPreviousDialog: true);

        }

        void GetMatch(int mID, int rID)
        {

            GlobalMatchNow = GlobalCategory.GetCurMatch(mID, rID);
            AKA_curTXT.Text = $"{GlobalMatchNow.AKA.FirstName} {GlobalMatchNow.AKA.LastName}";
            AO_curTXT.Text = $"{GlobalMatchNow.AO.FirstName} {GlobalMatchNow.AO.LastName}";
            AKA_ScoreL.Content = $"{GlobalMatchNow.AKA.ScoreProperty}";
            AO_ScoreL.Content = $"{GlobalMatchNow.AO.ScoreProperty}";
            if (GlobalMatchNow.AKA.Senshu) { AKAsenshuCB.IsChecked = true; }
            else if (GlobalMatchNow.AO.Senshu) { AOsenshuCB.IsChecked = true; }
            GlobalMatchNow.HaveWinner += Match_HaveWinner;

            if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded)
            {
                GlobalCategoryViewer.groups_List.SelectedIndex = rID;
                GlobalCategoryViewer.MatchesGrid.SelectedIndex = mID;
            }

            GlobalCategory.GetNext();
            Console.WriteLine(GlobalMatchNow.ToString());
            DisplayMessageDialog("Info", "Match loaded");
        }

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
        #endregion
        #region Timer

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        string CurTime;
        TimeSpan timerTime;
        TimeSpan remainTime = new TimeSpan();
        int min = 0, sec = 0;

        bool IsTimerEnabled;
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
                //TODO: Show milliseconds
                TimeSpan ts = stopWatch.Elapsed;
                string remainTimes;
                //if (!atoshibaraku) 
                remainTimes = String.Format("{0:00}:{1:00}",
                                                     remainTime.Minutes, remainTime.Seconds);
                //else remainTimes = String.Format("{0:00}:{1:00}.{2:000}", remainTime.Minutes, remainTime.Seconds, remainTime.Milliseconds);
                showTime(remainTimes);
                remainTime = timerTime - ts;
                //CurTime = String.Format("{0:00}:{1:00}:{2:00}",
                //ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                if (remainTime <= TimeSpan.Zero) { stopWatch.Stop(); TimerFinished(); }
                if (remainTime <= TimeSpan.FromSeconds(15) && !atoshibaraku) { AtoshiBaraku(); }
                await Task.Delay(1000);
            } while (stopWatch.IsRunning);

        }
        void TimerFinished()
        {
            startTimeBTN.Content = "Start";
            showTime(String.Format("{0:00}:{1:00}", remainTime.Minutes, remainTime.Seconds));
            AddInfo($"Stop timer. Time left: {String.Format("{0:00}:{1:00}", remainTime.Minutes, remainTime.Seconds)}");
            GlobalMatchNow.CheckWinner(true);
            //FinishMatch(CheckWin(0), true);
        }
        void AtoshiBaraku()
        {
            atoshibaraku = true;
            try { warn_sound.Play(); } catch { }
            TimerL.Foreground = Brushes.DarkRed;
        }


        private void _10secBtn_Click(object sender, RoutedEventArgs e)
        {
            min = 0;
            sec = 10;
            // time = min * 60 + sec;
            timerTime = new TimeSpan(0, min, sec);
            remainTime = timerTime;
            TimerL.Foreground = Brushes.DarkRed;
            if (externalBoard != null) { externalBoard.TimerEXT.Foreground = Brushes.DarkRed; }
            IsTimerEnabled = true;

            if (!stopWatch.IsRunning) { startTimeBTN.Content = "Stop"; stopWatch.Start(); }
            //  timer.Start();
            controlTime();
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
                AddInfo($"Stop timer. Time left: {TimerL.Content}");
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
            SetTimeer();
        }


        private void ResetTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            SetTimeer();
            stopWatch.Reset();
        }

        void SetTimeer()
        {
            atoshibaraku = false;
            min = Convert.ToInt32(TimeM.Text);
            sec = Convert.ToInt32(TimeS.Text);
            if (min < 10 && sec < 60)
            {
                TimerL.Foreground = Brushes.White;
                TimerL.Content = String.Format("{0:d2}:{1:d2}", min, sec);
                //if (externalBoard != null) { externalBoard.TimerText(sec, min); }
                TimeM.Text = String.Format("{0:d2}", min);
                TimeS.Text = String.Format("{0:d2}", sec);
                //timer.SetTime(min, sec);
                timerTime = new TimeSpan(0, min, sec);
                remainTime = timerTime;
                time = min * 60 + sec;
            }
            else if (sec > 60)
            {
                min = sec / 60;
                sec -= min * 60;
                TimerL.Foreground = Brushes.White;
                TimerL.Content = String.Format("{0:d2}:{1:d2}", min, sec);
                //if (externalBoard != null) { externalBoard.TimerText(sec, min); }
                TimeM.Text = String.Format("{0:d2}", min);
                TimeS.Text = String.Format("{0:d2}", sec);
                //  timer.SetTime(min, sec);
                timerTime = new TimeSpan(0, min, sec);
                remainTime = timerTime;
                time = min * 60 + sec;
            }
            stopWatch.Reset();
        }

        private void TimeS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
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
            }
        }



        private void TimeM_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
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
            }
        }
        #endregion

        #region SET WINNER
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

            GlobalMatchNow.CheckWinner();
        }

        private void AOsenshuCB_Click(object sender, RoutedEventArgs e)
        {
            if (AOsenshuCB.IsChecked == true)
            {
                GlobalMatchNow.AO.Senshu = true;
                GlobalMatchNow.AKA.Senshu = false;
                AKAsenshuCB.IsChecked = false;
                AddInfo("AO senshu");
                AO_SenshuL.Visibility = Visibility.Visible;
                AKA_SenshuL.Visibility = Visibility.Collapsed;
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.aoSenshu, 1); externalBoard.ShowSanction(externalBoard.akaSenshu, 0); }
            }
            else
            {
                GlobalMatchNow.AO.Senshu = false;
                AddInfo("AO senshu remove");
                AO_SenshuL.Visibility = Visibility.Collapsed;
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.aoSenshu, 0); }
            }
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
                GlobalMatchNow.AO.Senshu = false;
                GlobalMatchNow.AKA.Senshu = true;
                AOsenshuCB.IsChecked = false;
                AddInfo("AKA senshu");
                AKA_SenshuL.Visibility = Visibility.Visible;
                AO_SenshuL.Visibility = Visibility.Collapsed;
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.akaSenshu, 1); externalBoard.ShowSanction(externalBoard.aoSenshu, 0); }
            }
            else
            {
                GlobalMatchNow.AKA.Senshu = false;
                AddInfo("AKA senshu remove");
                AKA_SenshuL.Visibility = Visibility.Collapsed;
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.akaSenshu, 0); }
            }
        }
        #endregion


        #region FOULS C1 AKA
        private void AKA_C1_CB_Uncheked(object srnder, RoutedEventArgs e)
        {
            if (AKA_K1_CB.IsChecked == true || AKA_HC1_CB.IsChecked == true || AKA_H1_CB.IsChecked == true) { AKA_C1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(0);
                AddInfo("AKA remove sanction C1 C");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c1AKA, 0); }
            }
        }
        private void AKA_C1_CB_Checked(object sender, RoutedEventArgs e)
        {

            GlobalMatchNow.AKA.SetFoulsC1(1); AddInfo("AKA sanction C1 C");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c1AKA, 1); }
        }
        private void AKA_K1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_HC1_CB.IsChecked == true || AKA_H1_CB.IsChecked == true) { AKA_K1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(GlobalMatchNow.AKA.Fouls_C1 - 1);
                AddInfo("AKA remove sanction C1 K");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.k1AKA, 0); }
            }
        }
        private void AKA_K1_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C1_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC1(2);
            AddInfo("AKA sanction C1 K");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.k1AKA, 1); }
        }
        private void AKA_HC1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_H1_CB.IsChecked == true) { AKA_HC1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(GlobalMatchNow.AKA.Fouls_C1 - 1);
                AddInfo("AKA remove sanction C1 HC");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc1AKA, 0); }

            }
        }
        private void AKA_HC1_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C1_CB.IsChecked = true; AKA_K1_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC1(3);
            AddInfo("AKA sanction C1 HC");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc1AKA, 1); }
        }


        private void AKA_H1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AKA.SetFoulsC1(GlobalMatchNow.AKA.Fouls_C1 - 1); AddInfo("AKA remove sanction C1 H");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h1AKA, 0); }
        }
        private void AKA_H1_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C1_CB.IsChecked = true; AKA_K1_CB.IsChecked = true; AKA_HC1_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC1(4);
            AddInfo("AKA sanction C1 H");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h1AKA, 1); }
        }

        #endregion
        #region FOULS C2 AKA
        private void AKA_C2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_K2_CB.IsChecked == true || AKA_HC2_CB.IsChecked == true || AKA_H2_CB.IsChecked == true) { AKA_C2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC2(0);
                AddInfo("AKA remove sanction C2 C");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.c2AKA, 0); }
            }
        }
        private void AKA_C2_CB_Checked(object sender, RoutedEventArgs e)
        {

            if (AKA_C2_CB.IsChecked == true)
            {
                GlobalMatchNow.AKA.SetFoulsC2(1); AddInfo("AKA sanction C2 C");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.c2AKA, 1);

                }
            }

        }

        private void AKA_K2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_HC2_CB.IsChecked == true || AKA_H2_CB.IsChecked == true) { AKA_K2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC2(GlobalMatchNow.AKA.Fouls_C2 - 1);
                AddInfo("AKA remove sanction C2 K");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.k2AKA, 0); }
            }
        }
        private void AKA_K2_CB_Checked(object sender, RoutedEventArgs e)
        {

            AKA_C2_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC2(2);
            AddInfo("AKA sanction C2 K");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.k2AKA, 1); }
        }

        private void AKA_HC2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AKA_H2_CB.IsChecked == true) { AKA_HC2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC2(GlobalMatchNow.AKA.Fouls_C2 - 1);
                AddInfo("AKA remove sanction C2 HC");
                if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc2AKA, 0); }
            }
        }
        private void AKA_HC2_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C2_CB.IsChecked = true; AKA_K2_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC2(3);
            AddInfo("AKA sanction C2 HC");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.hc2AKA, 1); }
        }

        private void AKA_H2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AKA.SetFoulsC2(GlobalMatchNow.AKA.Fouls_C2 - 1); AddInfo("AKA remove sanction C2 H");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h2AKA, 0); }
        }
        private void AKA_H2_CB_Checked(object sender, RoutedEventArgs e)
        {
            AKA_C2_CB.IsChecked = true; AKA_K2_CB.IsChecked = true; AKA_HC2_CB.IsChecked = true;
            GlobalMatchNow.AKA.SetFoulsC2(4);
            AddInfo("AKA sanction C2 H");
            if (externalBoard != null) { externalBoard.ShowSanction(externalBoard.h2AKA, 1); }


        }
        #endregion


        #region FOULS C1 AO
        private void AO_C1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_K1_CB.IsChecked == true || AO_HC1_CB.IsChecked == true || AO_H1_CB.IsChecked == true) { AO_C1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(0);
                AddInfo("AO remove sanction C1 C");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.c1AO, 0);

                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOC1, 0); }
            }
        }
        private void AO_C1_CB_Checked(object sender, RoutedEventArgs e)
        {
                GlobalMatchNow.AO.SetFoulsC1(1); AddInfo("AO sanction C1 C");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.c1AO, 1);
                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOC1, 1); }           
        }
        private void AO_K1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_HC1_CB.IsChecked == true || AO_H1_CB.IsChecked == true) { AO_K1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(GlobalMatchNow.AO.Fouls_C1 - 1);
                AddInfo("AO remove sanction C1 K");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.k1AO, 0);


                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOK1, 0); }
            }
        }
        private void AO_K1_CB_Checked(object sender, RoutedEventArgs e)
        {
                AO_C1_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC1(2);
                AddInfo("AO sanction C1 K");
                if (externalBoard != null)
                {

                    externalBoard.ShowSanction(externalBoard.k1AO, 1);

                }
        }


        private void AO_HC1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_H1_CB.IsChecked == true) { AO_HC1_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(GlobalMatchNow.AO.Fouls_C1 - 1);
                AddInfo("AO remove sanction C1 HC");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.hc1AO, 0);

                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOHC1, 0); }
            }
        }
        private void AO_HC1_CB_Checked(object sender, RoutedEventArgs e)
        {
                AO_C1_CB.IsChecked = true; AO_K1_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC1(3);
                AddInfo("AO sanction C1 HC");
                if (externalBoard != null)
                {

                    externalBoard.ShowSanction(externalBoard.hc1AO, 1);

                }

        }

        private void AO_H1_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.SetFoulsC1(GlobalMatchNow.AO.Fouls_C1 - 1); AddInfo("AO remove sanction C1 H");
            if (externalBoard != null)
            {
                externalBoard.ShowSanction(externalBoard.h1AO, 0);

            }
            //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOH1, 0); }

        }
        private void AO_H1_CB_Checked(object sender, RoutedEventArgs e)
        {
                AO_C1_CB.IsChecked = true; AO_K1_CB.IsChecked = true; AO_HC1_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC1(4);
                AddInfo("AO sanction C1 H");
                if (externalBoard != null)
                {

                    externalBoard.ShowSanction(externalBoard.h1AO, 1);

                }

        }

        #endregion
        #region FOULS C2 AO


        private void AO_C2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_K2_CB.IsChecked == true || AO_HC2_CB.IsChecked == true || AO_H2_CB.IsChecked == true) { AO_C2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC2(0);
                AddInfo("AO remove sanction C2 C");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.c2AO, 0);


                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOC2, 0); }
            }
        }
        private void AO_C2_CB_Checked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.SetFoulsC2(1); AddInfo("AO sanction C2 C");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.c2AO, 1);

                }

           
        }

        private void AO_K2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_HC2_CB.IsChecked == true || AO_H2_CB.IsChecked == true) { AO_K2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC2(GlobalMatchNow.AO.Fouls_C2 - 1);
                AddInfo("AO remove sanction C2 K");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.k2AO, 0);

                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOK2, 0); }
            }
        }
        private void AO_K2_CB_Checked(object sender, RoutedEventArgs e)
        {
            
            if (AO_K2_CB.IsChecked == true)
            {
                AO_C2_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC2(2);
                AddInfo("AO sanction C2 K");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.k2AO, 1);

                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOK2, 1); }
            }
            


        }

        private void AO_HC2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AO_H2_CB.IsChecked == true) { AO_HC2_CB.IsChecked = true; }
            else
            {
                GlobalMatchNow.AO.SetFoulsC2(GlobalMatchNow.AO.Fouls_C2 - 1);
                AddInfo("AO remove sanction C2 HC");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.hc2AO, 0);

                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOHC2, 0); }
            }
        }
        private void AO_HC2_CB_Checked(object sender, RoutedEventArgs e)
        {
            
            if (AO_HC2_CB.IsChecked == true)
            {
                AO_C2_CB.IsChecked = true; AO_K2_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC2(3);
                AddInfo("AO sanction C2 HC");
                if (externalBoard != null)
                {
                    externalBoard.ShowSanction(externalBoard.hc2AO, 1);

                }
                //if (externalBoard != null) { externalBoard.SanctionAnimation(externalBoard.AOHC2, 1); }
            }
            


        }


        private void AO_H2_CB_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.AO.SetFoulsC2(GlobalMatchNow.AO.Fouls_C2 - 1); AddInfo("AO remove sanction C2 H");
            if (externalBoard != null)
            {
                externalBoard.ShowSanction(externalBoard.h2AO, 0);

            }
        }
        private void AO_H2_CB_Checked(object sender, RoutedEventArgs e)
        {

                AO_C2_CB.IsChecked = true; AO_K2_CB.IsChecked = true; AO_HC2_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC2(4);
                AddInfo("AO sanction C2 H");
                if (externalBoard != null)
                {

                    externalBoard.ShowSanction(externalBoard.h2AO, 1);

                }

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
            SetTimeer();
            stopWatch.Reset();

            AO_curTXT.Text = GlobalMatchNow.AO.ToString();
            AKA_curTXT.Text = GlobalMatchNow.AKA.ToString();

            AOsenshuCB.IsChecked = GlobalMatchNow.AO.Senshu;
            AKAsenshuCB.IsChecked = GlobalMatchNow.AKA.Senshu;

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
            AKA_C1_CB.IsChecked = false;
            AKA_K1_CB.IsChecked = false;
            AKA_HC1_CB.IsChecked = false;
            AKA_H1_CB.IsChecked = false;

            AKA_C2_CB.IsChecked = false;
            AKA_K2_CB.IsChecked = false;
            AKA_HC2_CB.IsChecked = false;
            AKA_H2_CB.IsChecked = false;

            AO_C1_CB.IsChecked = false;
            AO_K1_CB.IsChecked = false;
            AO_HC1_CB.IsChecked = false;
            AO_H1_CB.IsChecked = false;

            AO_C2_CB.IsChecked = false;
            AO_K2_CB.IsChecked = false;
            AO_HC2_CB.IsChecked = false;
            AO_H2_CB.IsChecked = false;

            if (externalBoard != null) { }
        }

        private void FinishMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalCategory.FinishCurMatch();
            //TODO: Save Log data (ON CHECK)
            if (GlobalCategory.isCurMFinished())
            {
                if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded) { GlobalCategoryViewer.UpdateExcelTree(MainExApp.ActiveWorkbook); }

                string fileName = $"{_Aka}_{_Ao}";
                if (saveLog($"{Properties.Settings.Default.DataPath}\\LOG-{fileName}.txt", LogTB)) { DisplayMessageDialog("Info", "Log saved"); }
                else { DisplayMessageDialog("Info", "Can't save log"); }

                //ResetMatch();
                DisplayMessageDialog("Info", "Match finished");
            }
        }

        private void NextMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            if (NxtMatch[1] != -1 && NxtMatch[0] != -1)
            {
                GetMatch(NxtMatch[1], NxtMatch[0]);
                TextLog.Blocks.Clear();
                //if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded)
                //{
                //    GlobalCategoryViewer.groups_List.SelectedIndex = NxtMatch[0];
                //    GlobalCategoryViewer.MatchesGrid.SelectedIndex = NxtMatch[1];
                //}
            }
        }

        private void NextMatchBTN_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void MainWindow1_Unloaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Closing...");
            if (GlobalCategoryViewer.IsLoaded) { GlobalCategoryViewer.Close(); }
            if (MainExApp != null) { MainExApp.Quit(); }
        }
        ExternalBoard externalBoard;
        ExternalBoard ext;
        private void openExt_btn_Click(object sender, RoutedEventArgs e)
        {
            if (externalBoard == null)
            {
                List<Screen> sc = new List<Screen>();
                sc.AddRange(Screen.AllScreens);
                externalBoard = new ExternalBoard();

                /*if (Category != null)
                {
                    externalBoard.SetCategory($"{CategoryName}: {groups_List.SelectedItem}");



                    if (Category.GetCurComp() != null)
                    {
                        externalBoard.SetCompetitorName(Category.GetCurComp().ToString());
                        externalBoard.SetColor(Category.GetColor());
                    }
                }*/

                if (GlobalCategoryViewer != null && GlobalCategoryViewer.CategoryName != null)
                {
                    string[] worrd = GlobalCategoryViewer.CategoryName.Split(new char[] { ' ' }, 2);
                    externalBoard.CategoryEXT.Text += $"{worrd[0]} \n{worrd[1]}";
                }

                Binding akaScoreBind = new Binding("ScoreProperty");
                akaScoreBind.Source = GlobalMatchNow.AKA;
                externalBoard.AkaScoreL.SetBinding(Label.ContentProperty, akaScoreBind);

                Binding aoScoreBind = new Binding("ScoreProperty");
                aoScoreBind.Source = GlobalMatchNow.AO;
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
                externalBoard = null;
            }

        }
        ExtTimerSet extTimerSet;
        private void openExtTimerSet_btn_Click(object sender, RoutedEventArgs e)
        {
            if (extTimerSet == null)
            {
                extTimerSet = new ExtTimerSet();
                extTimerSet.Owner = this;
                extTimerSet.Show();
            }
            else
            {
                extTimerSet.Close();
                extTimerSet = null;
            }
        }
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

        Settings settings;
        private void SettingsBTN_Click(object sender, RoutedEventArgs e)
        {
            if(settings==null)
            {
                settings = new Settings();
                settings.Show();
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
