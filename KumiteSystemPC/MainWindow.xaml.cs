using ModernWpf.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        public MainWindow()
        {
            InitializeComponent();
            _Aka = new Competitor(false,1,"AKA");
            _Ao = new Competitor(false, 2,"AO");
            GlobalMatchNow = new Match(_Aka, _Ao, 0);
            GlobalMatchNow.HaveWinner += Match_HaveWinner;
            NxtMatch = new List<int>() { -1,-1};
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
                CategoryViewer CategoryViewer = new CategoryViewer(GlobalCategory,MainExApp.ActiveWorkbook.Name,MainExApp.ActiveWorkbook);
                CategoryViewer.GetMatchEv += GetMatch;
                GlobalCategoryViewer = CategoryViewer;
                GlobalCategoryViewer.Show();
                MainExApp.DisplayAlerts = false;
                MainExApp.Visible = true;
                
            }
        }
        private void GlobalCategory_HaveNxtMatch(int round, int match)
        {
            if(round == -1 &&  match==-1)
            {
                GlobalMatchNxt = new Match(new Competitor(),new Competitor(),1);
            }
            else if (round < GlobalCategory.Rounds.Count())
            {
                GlobalMatchNxt = GlobalCategory.Rounds[round].Matches[match];
            }
            else if(round == GlobalCategory.Rounds.Count())
            {
                GlobalMatchNxt = GlobalCategory.RepechageAKA.Matches[match];
            }
            else if(round == GlobalCategory.Rounds.Count()+1)
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
            for(int i=1;i<=count;i++)
            {
                Round round = new Round();
                Excel.Worksheet ws = wb.Worksheets[i];
                for(int j=2;j<=ws.UsedRange.Rows.Count;j++)
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
                    int Aoscore =Convert.ToInt32(ws.Cells[j, 9].Value);

                    Competitor _aka;
                    if (AkaFName != "BYE") { _aka = new Competitor(false, AkaId, AkaFName, AkaLName, Akascore, AkaF1, AkaF2); }
                    else { _aka = new Competitor(true); }

                    Competitor _ao;
                    if (AoFName != "BYE") { _ao = new Competitor(false, AoId, AoFName, AoLName, Aoscore, AoF1, AoF2); }
                    else { _ao = new Competitor(true); }
                    Match match = new Match(_aka,_ao,j-1);
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
            if (GlobalCategoryViewer != null) {GlobalCategoryViewer.CompetitorsGrid.Items.Refresh();}
            try { DisplayMessageDialog("Info", $"Match winner: {GlobalMatchNow.Winner.FirstName} {GlobalMatchNow.Winner.LastName}"); }
            catch { }
        }
        #endregion

        private async void DisplayMessageDialog(string caption, string message,bool wait=false)
        {
            try
            {
                ContentDialog ServerDialog = new ContentDialog
                {
                    Title = caption,
                    CloseButtonText = "Ok",
                    Content = message,
                };
                ContentDialogResult result = await ServerDialog.ShowAsync();
            }
            catch { }
            /*await ContentDialogMaker.CreateContentDialogAsync(new ContentDialog
            {
                Title = caption,
                Content = message,
                PrimaryButtonText = "OK"
            }, awaitPreviousDialog: wait);*/
        }

        void GetMatch(int mID,int rID)
        {
            
            GlobalMatchNow = GlobalCategory.GetCurMatch(mID, rID);
            AKA_curTXT.Text = $"{GlobalMatchNow.AKA.FirstName} {GlobalMatchNow.AKA.LastName}";
            AO_curTXT.Text = $"{GlobalMatchNow.AO.FirstName} {GlobalMatchNow.AO.LastName}";
            AKA_ScoreL.Content = $"{GlobalMatchNow.AKA.Score}";
            AO_ScoreL.Content = $"{GlobalMatchNow.AO.Score}";
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
            DisplayMessageDialog("Info", "Match loaded", false);
        }

        #region LOG
        private void SaveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileName;
            fileName = DateTime.Now.ToShortDateString();
            //if (saveLog($"{Properties.Settings.Default.DataDir}\\LOG-{fileName}.txt", LogTB)) { MessageBox.Show("Log save completed", "Infornmation", MessageBoxButton.OK, MessageBoxImage.Information); }
            //else { MessageBox.Show("Something went wrong"); }
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
            //if (kumiteExternal != null) { kumiteExternal.TimerEXT.Content = time; }
        }
        public async void controlTime()
        {
            do
            {
                TimeSpan ts = stopWatch.Elapsed;
                string remainTimes = String.Format("{0:00}:{1:00}",
                                                     remainTime.Minutes, remainTime.Seconds);
                showTime(remainTimes);
                remainTime = timerTime - ts;
                //CurTime = String.Format("{0:00}:{1:00}:{2:00}",
                //ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                if (remainTime <= TimeSpan.Zero) { stopWatch.Stop(); TimerFinished(); }
                if (remainTime <= TimeSpan.FromSeconds(15) && !atoshibaraku) { AtoshiBaraku(); }
                await Task.Delay(1000);
            } while (stopWatch.IsRunning);

        }
        void ShowWinner()
        {
            MessageBox.Show($"{GlobalMatchNow.Winner}");
        }
        void TimerFinished()
        {
            startTimeBTN.Content = "Start";
            AddInfo($"Stop timer. Time left: {String.Format("{0:00}:{1:00}", remainTime.Minutes, remainTime.Seconds)}");
            GlobalMatchNow.CheckWinner(true);
            //FinishMatch(CheckWin(0), true);
        }
        void AtoshiBaraku()
        {
            atoshibaraku = true;
            /*if (Properties.Settings.Default.warningPlayer != null) Properties.Settings.Default.warningPlayer.Play();
            
            if (kumiteExternal != null) { kumiteExternal.TimerEXT.Foreground = Brushes.DarkRed; }*/
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
            //if (kumiteExternal != null) { kumiteExternal.TimerEXT.Foreground = Brushes.DarkRed; }
            IsTimerEnabled = true;
            if (!stopWatch.IsRunning) { startTimeBTN.Content = "Stop"; stopWatch.Start(); }
            controlTime();
            //  timer.Start();
        }

        private void startTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                
                stopWatch.Stop();
                AddInfo($"Stop timer. Time left: {TimerL.Content}");
                startTimeBTN.Content = "Start";
                TimerL.IsEnabled = false;
                //if (kumiteExternal != null) kumiteExternal.TimerEXT.IsEnabled = false;

            }
            else if (remainTime > TimeSpan.Zero)
            {
                stopWatch.Start();
                controlTime();
                AddInfo($"Stop timer. Time left: {TimerL.Content}");
                startTimeBTN.Content = "Stop";
                TimerL.IsEnabled = true;
                //if (kumiteExternal != null) kumiteExternal.TimerEXT.IsEnabled = true;

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
               // if (kumiteExternal != null) { kumiteExternal.TimerText(sec, min); }
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
                sec -= (sec / 60) * 60;
                TimerL.Foreground = Brushes.White;
                TimerL.Content = String.Format("{0:d2}:{1:d2}", min, sec);
                //if (kumiteExternal != null) { kumiteExternal.TimerText(sec, min); }
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
                if (GlobalMatchNow.AKA.Score >= 0)
                {
                    if (points > 0) { AddInfo($"AKA add point {points}. Points: {GlobalMatchNow.AKA.Score}"); }
                    else if (points < 0) { AddInfo($"AKA remove point. Points: {GlobalMatchNow.AKA.Score}"); }
                }
                else { GlobalMatchNow.AKA.AddPoints(-points); }
            }
            else if(competitor==1)
            {
                GlobalMatchNow.AO.AddPoints(points);
                if (GlobalMatchNow.AO.Score >= 0)
                {
                    if (points > 0) { AddInfo($"AO add point {points}. Points: {GlobalMatchNow.AO.Score}"); }
                    else if (points < 0) { AddInfo($"AO remove point. Points: {GlobalMatchNow.AO.Score}"); }
                }
                else { GlobalMatchNow.AO.AddPoints(-points); }
            }
            AO_ScoreL.Content = GlobalMatchNow.AO.Score;
            AKA_ScoreL.Content = GlobalMatchNow.AKA.Score;

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
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.aoSenshu, 1); }
            }
            else
            {
                GlobalMatchNow.AO.Senshu = false;
                AddInfo("AO senshu remove");
                AO_SenshuL.Visibility = Visibility.Collapsed;
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.aoSenshu, 0); }
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
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.aoSenshu, 1); }
            }
            else
            {
                GlobalMatchNow.AKA.Senshu = false;
                AddInfo("AKA senshu remove");
                AKA_SenshuL.Visibility = Visibility.Collapsed;
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.aoSenshu, 0); }
            }
        }
        #endregion


        #region FOULS C1 AKA
        private void AKA_C1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_K1_CB.IsChecked == true || AKA_HC1_CB.IsChecked == true || AKA_H1_CB.IsChecked == true) { AKA_C1_CB.IsChecked = true; }
            if (AKA_C1_CB.IsChecked == true)
            {
                GlobalMatchNow.AKA.SetFoulsC1(1); AddInfo("AKA sanction C1 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaC1, 1); }
            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(0);
                AddInfo("AKA remove sanction C1 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaC1, 0); }
            }
        }

        private void AKA_K1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_HC1_CB.IsChecked == true || AKA_H1_CB.IsChecked == true) { AKA_K1_CB.IsChecked = true; }
            if (AKA_K1_CB.IsChecked == true)
            {
                AKA_C1_CB.IsChecked = true;
                GlobalMatchNow.AKA.SetFoulsC1(2);
                AddInfo("AKA sanction C1 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaK1, 1); }
            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(0);
                AddInfo("AKA remove sanction C1 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaK1, 0); }
            }


        }

        private void AKA_HC1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_H1_CB.IsChecked == true) { AKA_HC1_CB.IsChecked = true; }
            if (AKA_HC1_CB.IsChecked == true)
            {
                AKA_C1_CB.IsChecked = true; AKA_K1_CB.IsChecked = true;
                GlobalMatchNow.AKA.SetFoulsC1(3);
                AddInfo("AKA sanction C1 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaHC1, 1); }
            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(0);
                AddInfo("AKA remove sanction C1 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaHC1, 0); }
            }


        }

        private void AKA_H1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_H1_CB.IsChecked == true)
            {
                AKA_C1_CB.IsChecked = true; AKA_K1_CB.IsChecked = true; AKA_HC1_CB.IsChecked = true;
                GlobalMatchNow.AKA.SetFoulsC1(4);
                AddInfo("AKA sanction C1 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaH1, 1); }

            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC1(0); AddInfo("AKA remove sanction C1 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaH1, 0); }
            }

        }

        #endregion
        #region FOULS C2 AKA

        private void AKA_C2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_K2_CB.IsChecked == true || AKA_HC2_CB.IsChecked == true || AKA_H2_CB.IsChecked == true) { AKA_C2_CB.IsChecked = true; }
            if (AKA_C2_CB.IsChecked == true)
            {
                GlobalMatchNow.AKA.SetFoulsC2(1); AddInfo("AKA sanction C2 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaC2, 1); }
            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC2(0);
                AddInfo("AKA remove sanction C2 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaC2, 0); }
            }
        }

        private void AKA_K2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_HC2_CB.IsChecked == true || AKA_H2_CB.IsChecked == true) { AKA_K2_CB.IsChecked = true; }
            if (AKA_K2_CB.IsChecked == true)
            {
                AKA_C2_CB.IsChecked = true;
                GlobalMatchNow.AKA.SetFoulsC2(2);
                AddInfo("AKA sanction C2 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaK2, 1); }
            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC2(0);
                AddInfo("AKA remove sanction C2 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaK2, 0); }
            }


        }

        private void AKA_HC2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_H2_CB.IsChecked == true) { AKA_HC2_CB.IsChecked = true; }
            if (AKA_HC2_CB.IsChecked == true)
            {
                AKA_C2_CB.IsChecked = true; AKA_K2_CB.IsChecked = true;
                GlobalMatchNow.AKA.SetFoulsC2(3);
                AddInfo("AKA sanction C2 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaHC2, 1); }
            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC2(0);
                AddInfo("AKA remove sanction C2 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaHC2, 0); }
            }


        }

        private void AKA_H2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AKA_H2_CB.IsChecked == true)
            {
                AKA_C2_CB.IsChecked = true; AKA_K2_CB.IsChecked = true; AKA_HC2_CB.IsChecked = true;
                GlobalMatchNow.AKA.SetFoulsC2(4);
                AddInfo("AKA sanction C2 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaH2, 1); }

            }
            else
            {
                GlobalMatchNow.AKA.SetFoulsC2(0); AddInfo("AKA remove sanction C2 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.akaH2, 0); }
            }

        }
        #endregion


        #region FOULS C1 AO
        private void AO_C1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_K1_CB.IsChecked == true || AO_HC1_CB.IsChecked == true || AO_H1_CB.IsChecked == true) { AO_C1_CB.IsChecked = true; }
            if (AO_C1_CB.IsChecked == true)
            {
                GlobalMatchNow.AO.SetFoulsC1(1); AddInfo("AO sanction C1 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOC1, 1); }
            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(0);
                AddInfo("AO remove sanction C1 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOC1, 0); }
            }
        }

        private void AO_K1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_HC1_CB.IsChecked == true || AO_H1_CB.IsChecked == true) { AO_K1_CB.IsChecked = true; }
            if (AO_K1_CB.IsChecked == true)
            {
                AO_C1_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC1(2);
                AddInfo("AO sanction C1 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOK1, 1); }
            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(0);
                AddInfo("AO remove sanction C1 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOK1, 0); }
            }


        }

        private void AO_HC1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_H1_CB.IsChecked == true) { AO_HC1_CB.IsChecked = true; }
            if (AO_HC1_CB.IsChecked == true)
            {
                AO_C1_CB.IsChecked = true; AO_K1_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC1(3);
                AddInfo("AO sanction C1 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOHC1, 1); }
            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(0);
                AddInfo("AO remove sanction C1 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOHC1, 0); }
            }


        }

        private void AO_H1_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_H1_CB.IsChecked == true)
            {
                AO_C1_CB.IsChecked = true; AO_K1_CB.IsChecked = true; AO_HC1_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC1(4);
                AddInfo("AO sanction C1 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOH1, 1); }

            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC1(0); AddInfo("AO remove sanction C1 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOH1, 0); }
            }

        }

        #endregion
        #region FOULS C2 AO

        private void AO_C2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_K2_CB.IsChecked == true || AO_HC2_CB.IsChecked == true || AO_H2_CB.IsChecked == true) { AO_C2_CB.IsChecked = true; }
            if (AO_C2_CB.IsChecked == true)
            {
                GlobalMatchNow.AO.SetFoulsC2(1); AddInfo("AO sanction C2 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOC2, 1); }
            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC2(0);
                AddInfo("AO remove sanction C2 C");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOC2, 0); }
            }
        }

        private void AO_K2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_HC2_CB.IsChecked == true || AO_H2_CB.IsChecked == true) { AO_K2_CB.IsChecked = true; }
            if (AO_K2_CB.IsChecked == true)
            {
                AO_C2_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC2(2);
                AddInfo("AO sanction C2 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOK2, 1); }
            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC2(0);
                AddInfo("AO remove sanction C2 K");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOK2, 0); }
            }


        }

        private void AO_HC2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_H2_CB.IsChecked == true) { AO_HC2_CB.IsChecked = true; }
            if (AO_HC2_CB.IsChecked == true)
            {
                AO_C2_CB.IsChecked = true; AO_K2_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC2(3);
                AddInfo("AO sanction C2 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOHC2, 1); }
            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC2(0);
                AddInfo("AO remove sanction C2 HC");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOHC2, 0); }
            }


        }



        private void AO_H2_CB_Checked(object sender, RoutedEventArgs e)
        {
            if (AO_H2_CB.IsChecked == true)
            {
                AO_C2_CB.IsChecked = true; AO_K2_CB.IsChecked = true; AO_HC2_CB.IsChecked = true;
                GlobalMatchNow.AO.SetFoulsC2(4);
                AddInfo("AO sanction C2 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOH2, 1); }

            }
            else
            {
                GlobalMatchNow.AO.SetFoulsC2(0); AddInfo("AO remove sanction C2 H");
                //if (kumiteExternal != null) { kumiteExternal.SanctionAnimation(kumiteExternal.AOH2, 0); }
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

        private void ResetMatchBtn_Click(object sender, RoutedEventArgs e)
        {
            GlobalMatchNow.Reset();
            SetTimeer();
            stopWatch.Reset();

            AO_curTXT.Text = GlobalMatchNow.AO.ToString();
            AKA_curTXT.Text = GlobalMatchNow.AKA.ToString();

            AOsenshuCB.IsChecked = GlobalMatchNow.AO.Senshu;
            AKAsenshuCB.IsChecked = GlobalMatchNow.AKA.Senshu;

            AKA_ScoreL.Content = GlobalMatchNow.AKA.Score;
            AO_ScoreL.Content = GlobalMatchNow.AO.Score;

            ResetFouls();

            try
            {
                DisplayMessageDialog("Info", "Match reseted");
            }
            catch { }
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
        }

        private void FinishMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalCategory.FinishCurMatch();
            //TODO: Save Log data
            if (GlobalCategory.isCurMFinished())
            {
                if (GlobalCategoryViewer != null && GlobalCategoryViewer.IsLoaded) { GlobalCategoryViewer.UpdateExcelTree(MainExApp.ActiveWorkbook); }
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
                externalBoard.WindowStyle = WindowStyle.None;
                externalBoard.Left = sc[1].Bounds.Left;
                externalBoard.Top = sc[1].Bounds.Top;
                externalBoard.Show();
                externalBoard.Owner = this;
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

        private void AKA_curTXT_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                string[] name = AKA_curTXT.Text.Split(new char[] { ' ' });
                try { GlobalMatchNow.AKA.FirstName = name[0]; } catch { GlobalMatchNow.AKA.FirstName = "AKA"; Console.WriteLine("No first name found"); }
                try { GlobalMatchNow.AKA.LastName = name[1]; } catch { GlobalMatchNow.AKA.LastName = ""; Console.WriteLine("No second name found"); }
                this.Focus();
            }
        }
    }
}
