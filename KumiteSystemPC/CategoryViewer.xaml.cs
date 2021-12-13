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
        public CategoryViewer()
        {
            InitializeComponent();
        }
        public CategoryViewer(List<Competitor> competitors, string categoryName, bool generate = false)
        {
            InitializeComponent();
            CompetitorsList = new List<Competitor>(competitors);
            CategoryName = categoryName;
            if (generate)
            {
                GenerateCategory();
                //CompetitorsGrid.IsReadOnly = true;
            }
        }
        public CategoryViewer(Category category,string categoryName,Excel.Workbook wb)
        {
            InitializeComponent();
            GlobalCategory = category;
            GlobalCategory.RepechageGen += GlobalCategory_RepechageGen;
            GlobalCategory.BronzeGen += GlobalCategory_BronzeGen;
            //GlobalCategory.HaveNxtMatch += GlobalCategory_HaveNxtMatch;
            CategoryName = categoryName;
            workbook = wb;
            this.Title = categoryName;
            foreach (var g in GlobalCategory.Rounds)
            {
                groups_List.Items.Add($"1/{g.ToString()}");
            }
            groups_List.SelectedIndex = 0;
            //CompetitorsGrid.IsReadOnly = true;
            NxtMatch = new List<int>() { -1,-1};
        }

        private void GlobalCategory_BronzeGen()
        {
            Excel.Worksheet ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
            ws.Name = "Bronze Match";
            AddRows(ws, new List<Match>() { GlobalCategory.BronzeMatch });

            Excel.Worksheet ws_ = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
            ws_.Name = "Bronze Match(Visual)";
            int row = 3;
            int col = 1;
            ws.Cells[row, col].Value = $"{GlobalCategory.BronzeMatch.AKA}";
            SetCellStyle(row, col, ws);
            row += 2;
            ws.Cells[row, col].Value = $"{GlobalCategory.BronzeMatch.AO}";
            SetCellStyle(row, col, ws);
            if (GlobalCategory.BronzeMatch.Winner != null) { UpdateExcelTree(workbook); }
        }

        public List<int> NxtMatch;
        private void GlobalCategory_HaveNxtMatch(int round, int match)
        {
            /*if (round < GlobalCategory.Rounds.Count())
            { DisplayMessageDialog("Info",$"Next match: {GlobalCategory.Rounds[round].Matches[match]}"); }
            else if(round == GlobalCategory.Rounds.Count())
            { DisplayMessageDialog("Info", $"Next match: {GlobalCategory.RepechageAKA.Matches[match]}"); }
            else if(round + 1 == GlobalCategory.Rounds.Count())
            { DisplayMessageDialog("Info", $"Next match: {GlobalCategory.RepechageAO.Matches[match]}"); }*/
            NxtMatch[0] = round;NxtMatch[1] = match;
            /*groups_List.SelectedIndex = round;
            MatchesGrid.SelectedIndex = match;*/
        }

        private void GlobalCategory_RepechageGen()
        {
            groups_List.Items.Add("Repechage 1");
            groups_List.Items.Add("Repechage 2");
            Excel.Worksheet ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
            ws.Name = "Repechage 1";
            ExportRepechage(ws,0);
            
            Excel.Worksheet ws1 = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
            ws1.Name = "Repechage 2";
            ExportRepechage(ws1, 1);

            Excel.Worksheet ws_ = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
            ws_.Name = "Repechage 1(Visual)";
            ExportRepechageVisual(ws_, 0);
            Excel.Worksheet _ws = workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
            _ws.Name = "Repechage 2(Visual)";
            ExportRepechageVisual(_ws, 1);
        }

        void GenerateCategory()
        {
            GlobalCategory = new Category(CompetitorsList);
            GlobalCategory.GenerateTree();
            foreach (var g in GlobalCategory.Rounds)
            {
                groups_List.Items.Add($"1/{g.ToString()}");
            }

            ExportCategory();

            groups_List.SelectedIndex = 0;
            CompetitorsGrid.Items.Refresh();
            DisplayMessageDialog("Category", "Category created");
        }

        public void UpdateExcelTree(Excel.Workbook wb)
        {
            //Update Current Round
            Excel.Worksheet wsRound = (Excel.Worksheet)wb.Worksheets[groups_List.SelectedIndex + 1];
            int curRound;
            int curMatch = GlobalCategory.GetCurMatchID();
            curRound = groups_List.SelectedIndex;
            Competitor AKA, Winner, AO;
            int r_count = GlobalCategory.Rounds.Count();
            if (curRound < r_count)
            {
                AKA = GlobalCategory.Rounds[curRound].Matches[curMatch].AKA;
                Winner = GlobalCategory.Rounds[curRound].Matches[curMatch].Winner;
                AO = GlobalCategory.Rounds[curRound].Matches[curMatch].AO;
            }
            else if(curRound == r_count)
            {
                AKA = GlobalCategory.RepechageAKA.Matches[curMatch].AKA;
                Winner = GlobalCategory.RepechageAKA.Matches[curMatch].Winner;
                AO = GlobalCategory.RepechageAKA.Matches[curMatch].AO;
            }
            else if(curRound == r_count + 1)
            {
                AKA = GlobalCategory.RepechageAO.Matches[curMatch].AKA;
                Winner = GlobalCategory.RepechageAO.Matches[curMatch].Winner;
                AO = GlobalCategory.RepechageAO.Matches[curMatch].AO;
            }
            else
            {
                AKA = null;Winner = null;AO = null;
            }
            wsRound.Cells[curMatch + 2, 1].Value = AKA.ID;
            wsRound.Cells[curMatch + 2, 2].Value = AKA.FirstName;
            wsRound.Cells[curMatch + 2, 3].Value = AKA.LastName;
            wsRound.Cells[curMatch + 2, 4].Value = AKA.Fouls_C1;
            wsRound.Cells[curMatch + 2, 5].Value = AKA.Fouls_C2;
            wsRound.Cells[curMatch + 2, 6].Value = AKA.Score;
            if (Winner.ID == AKA.ID &&
                Winner.FirstName == AKA.FirstName &&
                Winner.LastName == AKA.LastName) { wsRound.Cells[curMatch + 2, 7].Value = "X"; }
            else if (Winner.ID == AO.ID &&
                Winner.FirstName == AO.FirstName &&
                Winner.LastName == AO.LastName) { wsRound.Cells[curMatch + 2, 8].Value = "X"; }
            wsRound.Cells[curMatch + 2, 14].Value = AO.ID;
            wsRound.Cells[curMatch + 2, 13].Value = AO.FirstName;
            wsRound.Cells[curMatch + 2, 12].Value = AO.LastName;
            wsRound.Cells[curMatch + 2, 11].Value = AO.Fouls_C1;
            wsRound.Cells[curMatch + 2, 10].Value = AO.Fouls_C2;
            wsRound.Cells[curMatch + 2, 9].Value = AO.Score;

            //Update Visual Data for default rounds
            
            if (curRound < r_count)
            {
                Excel.Worksheet wsVisual = (Excel.Worksheet)wb.Worksheets[wb.Worksheets.Count];
                int col = 2 * (curRound + 2) - 1;
                int add = Convert.ToInt32(Math.Pow(2, (curRound + 2)));
                int row = Convert.ToInt32(Math.Pow(2, curRound + 1)) + 2 + add * (curMatch);
                wsVisual.Cells[row, col].Value = $"{Winner}";
            }
            else if(curRound==r_count)
            {
                Excel.Worksheet wsVisual = (Excel.Worksheet)wb.Worksheets[wb.Worksheets.Count - 2];
                int col = 2 * (curMatch + 2) - 1;
                int row = 3 + (curMatch + 1);
                wsVisual.Cells[row, col].Value = $"{Winner}";
                SetCellStyle(row, col, wsVisual);
            }
            else if(curRound==r_count+1)
            {
                Excel.Worksheet wsVisual = (Excel.Worksheet)wb.Worksheets[wb.Worksheets.Count - 1];
                int col = 2 * (curMatch + 2)-1;
                int row = 3 + (curMatch + 1);
                wsVisual.Cells[row, col].Value = $"{Winner}";
                SetCellStyle(row, col, wsVisual);
            }
            //Add Rows TO next round
            Excel.Worksheet wsNxtRound;
            if (groups_List.SelectedIndex  + 1< GlobalCategory.Rounds.Count())
            {
                wsNxtRound = (Excel.Worksheet)wb.Worksheets[groups_List.SelectedIndex + 2];
                int _row=2;

                if (curMatch%2==0)
                {
                    _row += (curMatch / 2);
                    wsNxtRound.Cells[_row, 1].Value = Winner.ID;
                    wsNxtRound.Cells[_row, 2].Value = Winner.FirstName;
                    wsNxtRound.Cells[_row, 3].Value = Winner.LastName;

                }
                else
                {
                    _row += +((curMatch - 1) / 2);
                    wsNxtRound.Cells[_row, 14].Value = Winner.ID;
                    wsNxtRound.Cells[_row, 13].Value = Winner.FirstName;
                    wsNxtRound.Cells[_row, 12].Value = Winner.LastName;
                }
                wsNxtRound.UsedRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                wsNxtRound.UsedRange.Borders.Weight = 2d;
            }

        }
        #region EXPORT CATEGORY
        void ExportCategory()
        {
            Excel.Application ex = new Excel.Application();
            ex.Workbooks.Add();
            Excel.Workbook wb = ex.ActiveWorkbook;
            Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(Type.Missing, wb.Worksheets[wb.Worksheets.Count]);
            ws.Name = "Visualizing";
            if (GlobalCategory.Rounds != null)
            {
                int col = 1;
                ExportFirstVisual(ws);
                for (int i = 1; i < GlobalCategory.Rounds.Count(); i++)
                {
                    col += 2;
                    int row = Convert.ToInt32(Math.Pow(2, i)) + 2;
                    int add = Convert.ToInt32(Math.Pow(2, (i + 1)));
                    foreach (var m in GlobalCategory.Rounds[i].Matches)
                    {
                        Excel.Range range = ws.Cells[row, col].EntireColumn;

                        if (m.AKA != null) ws.Cells[row, col].Value = m.AKA.GetName();
                        ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        ws.Cells[row, col].Borders.Weight = 2d;
                        row += add;



                        if (m.AO != null) ws.Cells[row, col].Value = m.AO.GetName();
                        ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        ws.Cells[row, col].Borders.Weight = 2d;
                        row += add;

                        range.EntireColumn.AutoFit();
                    }
                    ws.Columns[col + 1].ColumnWidth = 3;
                    ws.Columns[col].ColumnWidth = 32;
                }
                col += 2;
                int _row = Convert.ToInt32(Math.Pow(2, GlobalCategory.Rounds.Count())) + 2;
                ws.Cells[_row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Cells[_row, col].Borders.Weight = 2d;
                ws.Columns[col].ColumnWidth = 32;

                if (wb.Worksheets.Count > 1) wb.Worksheets[1].Delete();
                ExportRounds(wb);
                exApp = ex;
                exApp.Visible = true;
                exApp.DisplayAlerts = false;
            }
        }
        void ExportRepechage(Excel.Worksheet ws,int num)
        {
            if (num == 0) { AddRows(ws, GlobalCategory.RepechageAKA.Matches); }//Export Repechage AKA
            else if(num==1) { AddRows(ws, GlobalCategory.RepechageAO.Matches); } //Export Repechage AO

        }

        void ExportRepechageVisual(Excel.Worksheet ws,int num)
        {
            if(num==0)
            {
                int row = 3;
                int col = 1;
                foreach(var m in GlobalCategory.RepechageAKA.Matches)
                {
                    ws.Cells[row, col].Value = $"{m.AKA}";
                    SetCellStyle(row, col, ws);
                    row += 2;
                    ws.Cells[row, col].Value = $"{m.AO}";
                    SetCellStyle(row, col, ws);
                    row -= 1;
                    col += 2;
                }
            }
            else if(num==1)
            {
                int row = 3;
                int col = 1;
                foreach (var m in GlobalCategory.RepechageAO.Matches)
                {
                    ws.Cells[row, col].Value = $"{m.AKA}";
                    SetCellStyle(row, col, ws);
                    row += 2;
                    ws.Cells[row, col].Value = $"{m.AO}";
                    SetCellStyle(row, col, ws);
                    row -= 1;
                    col += 2;
                }
            }

        }
        void SetCellStyle(int row,int col,Excel.Worksheet ws)
        {
            ws.Columns[col + 1].ColumnWidth = 3;
            ws.Columns[col].ColumnWidth = 32;
            ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            ws.Cells[row, col].Borders.Weight = 2d;
        }

        void AddRows(Excel.Worksheet ws,List<Match> matches)
        {
            int row = 2;
            ws.Cells[1, 1].Value = "ID_AKA";
            ws.Cells[1, 2].Value = "AKA First_Name";
            ws.Cells[1, 3].Value = "AKA Last_Name";
            ws.Cells[1, 4].Value = "AKA Fouls C1";
            ws.Cells[1, 5].Value = "AKA Fouls C2";
            ws.Cells[1, 6].Value = "AKA Score";
            ws.Cells[1, 7].Value = "Winner AKA";
            for (int i = 1; i <= 7; i++) { ws.Cells[1, i].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red); }
            ws.Cells[1, 8].Value = "Winner AO";
            ws.Cells[1, 14].Value = "ID_AO";
            ws.Cells[1, 13].Value = "AO First_Name";
            ws.Cells[1, 12].Value = "AO Last_Name";
            ws.Cells[1, 11].Value = "AO Fouls C1";
            ws.Cells[1, 10].Value = "AO Fouls C2";
            ws.Cells[1, 9].Value = "AO Score";
            for (int i = 8; i <= 14; i++) { ws.Cells[1, i].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Blue); }
            foreach (var m in matches)
            {
                if (m.AKA != null)
                {
                    ws.Cells[row, 1].Value = m.AKA.ID;
                    ws.Cells[row, 2].Value = m.AKA.FirstName;
                    ws.Cells[row, 3].Value = m.AKA.LastName;
                    ws.Cells[row, 4].Value = m.AKA.GetFoulsC1();
                    ws.Cells[row, 5].Value = m.AKA.GetFoulsC2();
                    ws.Cells[row, 6].Value = m.AKA.Score;
                }
                if (m.AO != null)
                {
                    ws.Cells[row, 14].Value = m.AO.ID;
                    ws.Cells[row, 13].Value = m.AO.FirstName;
                    ws.Cells[row, 12].Value = m.AO.LastName;
                    ws.Cells[row, 11].Value = m.AO.GetFoulsC1();
                    ws.Cells[row, 10].Value = m.AO.GetFoulsC2();
                    ws.Cells[row, 9].Value = m.AO.Score;
                }
                if (m.Winner != null && m.Winner.ID == m.AKA.ID && m.Winner.FirstName == m.AKA.FirstName) { ws.Cells[row, 7].Value = "X"; }
                else if (m.Winner != null && m.Winner.ID == m.AO.ID && m.Winner.FirstName == m.AO.FirstName) { ws.Cells[row, 8].Value = "X"; }
                row++;
            }
            Excel.Range range = ws.UsedRange;
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Borders.Weight = 2d;
        }
        void ExportRounds(Excel.Workbook wb)
        {
            foreach (var r in GlobalCategory.Rounds)
            {
                Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(wb.Worksheets[wb.Worksheets.Count]);
                ws.Name = $"1_|_{r.ToString()}";
                AddRows(ws, r.Matches);
            }
        }
        void ExportFirstVisual(Excel.Worksheet ws)
        {
            int row = 3;
            int col = 1;
            foreach (var m in GlobalCategory.Rounds[0].Matches)
            {
                Excel.Range range = ws.Cells[row, col].EntireColumn;

                ws.Cells[row, col].Value = m.AKA.GetName();
                ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Cells[row, col].Borders.Weight = 2d;
                row += 2;


                ws.Cells[row, col].Value = m.AO.GetName();
                ws.Cells[row, col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Cells[row, col].Borders.Weight = 2d;
                row += 2;

                range.EntireColumn.AutoFit();
            }
            ws.Columns[col + 1].ColumnWidth = 3;
            ws.Columns[col].ColumnWidth = 32;
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

        private void MatchesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalCategory.Rounds.Count() > 0 && MatchesGrid.SelectedIndex>=0)
            {
                List<Competitor> comps = new List<Competitor>();
                if (groups_List.SelectedIndex < GlobalCategory.Rounds.Count())
                {
                    comps.Add(GlobalCategory.Rounds[groups_List.SelectedIndex].Matches[MatchesGrid.SelectedIndex].AKA);
                    comps.Add(GlobalCategory.Rounds[groups_List.SelectedIndex].Matches[MatchesGrid.SelectedIndex].AO);
                }
                else if(groups_List.SelectedIndex == GlobalCategory.Rounds.Count())
                {
                    comps.Add(GlobalCategory.RepechageAKA.Matches[MatchesGrid.SelectedIndex].AKA);
                    comps.Add(GlobalCategory.RepechageAKA.Matches[MatchesGrid.SelectedIndex].AO);
                }
                else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count() + 1)
                {
                    comps.Add(GlobalCategory.RepechageAO.Matches[MatchesGrid.SelectedIndex].AKA);
                    comps.Add(GlobalCategory.RepechageAO.Matches[MatchesGrid.SelectedIndex].AO);
                }
                CompetitorsGrid.ItemsSource = comps;
                //GetMatchEv?.Invoke(MatchesGrid.SelectedIndex, groups_List.SelectedIndex);
                CompetitorsGrid.Items.Refresh();

            }
            //MatchesGrid.Items.Refresh();
            
            
        }
        public void UpdateCompGrid()
        {
            CompetitorsGrid.Items.Refresh();
        }
        private void groups_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (groups_List.SelectedIndex >= 0)
            {
                if (GlobalCategory.Rounds.Count() > 0 && groups_List.SelectedIndex < GlobalCategory.Rounds.Count())
                {
                    MatchesGrid.ItemsSource = GlobalCategory.Rounds[groups_List.SelectedIndex].Matches;
                    // MatchesGrid.SelectedIndex = 0;
                }
                else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count())
                {
                    MatchesGrid.ItemsSource = GlobalCategory.RepechageAKA.Matches;
                }
                else if (groups_List.SelectedIndex == GlobalCategory.Rounds.Count() + 1)
                {
                    MatchesGrid.ItemsSource = GlobalCategory.RepechageAO.Matches;
                }
                MatchesGrid.Items.Refresh();
            }
            //CompetitorsGrid.Items.Refresh();
        }
        private async void DisplayFinishMatchDialog()
        {
            ContentDialog FinishMatchDialog = new ContentDialog
            {
                Title = "Finish current match?",
                Content = "This match isn't finished. Do you want to finish it?",
                PrimaryButtonText = "Finish",
                DefaultButton = ContentDialogButton.Primary,
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await FinishMatchDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                GlobalCategory.FinishCurMatch();
                if (GlobalCategory.isCurMFinished()) { DisplayMessageDialog("Info", "Match finished"); }
            }
        }

        void LoadRoundMatch(int round, int match) {
            GetMatchEv?.Invoke(match, round);
            //DisplayMessageDialog("Info", "Match loaded");
        }

        private void LoadMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalCategory.isCurMFinished())
            {
                LoadRoundMatch(groups_List.SelectedIndex,MatchesGrid.SelectedIndex);
            }
            else
            {
                DisplayFinishMatchDialog();
               // DisplayMessageDialog("Info", "Match isn't finished");
            }
        }

        private void CompetitorsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            GetMatchEv?.Invoke(MatchesGrid.SelectedIndex, groups_List.SelectedIndex);
        }

        private void FinishCurMatchBTN_Click(object sender, RoutedEventArgs e)
        {
            GlobalCategory.FinishCurMatch();
            if (GlobalCategory.isCurMFinished()) { UpdateExcelTree(workbook); DisplayMessageDialog("Info", "Match finished"); }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
           /* if (exApp != null) exApp.Quit();*/
        }
    }
}
