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
    /// Логика взаимодействия для CategoryGenerator.xaml
    /// </summary>
    public partial class CategoryGenerator : Window
    {
        List<Competitor> CompetitorsList;
        Category GlobalCategory;
        string CategoryName;
        Excel.Application exApp;
        public delegate void GetMatchHandler(int mID,int rID);
        public event GetMatchHandler GetMatchEv;
        public CategoryGenerator()
        {
            InitializeComponent();
            CompetitorsList = new List<Competitor>();
            CompetitorsGrid.ItemsSource = CompetitorsList;
        }
        CategoryGenerator(List<Competitor> competitors, string categoryName, bool generate = false)
        {
            InitializeComponent();
            CompetitorsList = new List<Competitor>(competitors);
            CategoryName = categoryName;
            if (generate)
            {
                GenerateCategory();
                openCompL.Visibility = Visibility.Collapsed;
                generateCat.Visibility = Visibility.Collapsed;
                saveCat.Visibility = Visibility.Visible;
                
                CompetitorsGrid.IsReadOnly = true;
            }
        }
        public CategoryGenerator(Category category,string categoryName)
        {
            InitializeComponent();
            GlobalCategory = category;
            CategoryName = categoryName;
            openCompL.Visibility = Visibility.Collapsed;
            generateCat.Visibility = Visibility.Collapsed;
            saveCat.Visibility = Visibility.Collapsed;
            shuffleComp.Visibility = Visibility.Collapsed;

            MatchesGrid.Visibility = Visibility.Visible;
            this.Title = categoryName;
            foreach(var g in GlobalCategory.Rounds)
            {
                groups_List.Items.Add($"1/{g.ToString()}");
            }
            groups_List.SelectedIndex = 0;
            CompetitorsGrid.IsReadOnly = true;
        }

        private void addComp_Click(object sender, RoutedEventArgs e)
        {
            if(GlobalCategory == null) { }
            AddCompetitorDialog addCompetitorDialog = new AddCompetitorDialog();
            addCompetitorDialog.Owner = this;
            if (addCompetitorDialog.ShowDialog() == true)
            {
                //Excel.Worksheet ws = (Excel.Worksheet)ex.Worksheets[groups_List.SelectedIndex + 1];
                Competitor comp = new Competitor(false, addCompetitorDialog.ID,addCompetitorDialog.FirstName, addCompetitorDialog.LastName);
                //int row = ws.UsedRange.Rows.Count + 1;
                //ws.Cells[row, 1].Value = comp.ID;
                //ws.Cells[row, 2].Value = comp.FirstName;
                //ws.Cells[row, 3].Value = comp.LastName;
                //Competitors.Add(comp.ID, comp);
                CompetitorsList.Add(comp);
                CompetitorsGrid.Items.Refresh();
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
                int col=1;
                ExportFirstVisual(ws);
                for(int i=1;i<GlobalCategory.Rounds.Count();i++)
                {
                    col += 2;
                    int row = Convert.ToInt32(Math.Pow(2, i)) + 2;
                    int add = Convert.ToInt32(Math.Pow(2, (i + 1)));
                    foreach (var m in GlobalCategory.Rounds[i].Matches)
                    {
                        Excel.Range range = ws.Cells[row, col].EntireColumn;

                        if(m.AKA!=null)ws.Cells[row, col].Value = m.AKA.GetName();
                        ws.Cells[row,col].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        ws.Cells[row, col].Borders.Weight = 2d;
                        row += add;

                        

                        if (m.AO!=null)ws.Cells[row, col].Value = m.AO.GetName();
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

        void ExportRounds(Excel.Workbook wb)
        {
            foreach (var r in GlobalCategory.Rounds)
            {
                Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(wb.Worksheets[wb.Worksheets.Count]);
                int row = 2;
                ws.Name = $"1%{r.ToString()}";
                ws.Cells[1, 1].Value = "ID_AKA";
                ws.Cells[1, 2].Value = "AKA First_Name";
                ws.Cells[1, 3].Value = "AKA Last_Name";
                ws.Cells[1, 4].Value = "AKA Fouls C1";
                ws.Cells[1, 5].Value = "AKA Fouls C2";
                ws.Cells[1, 6].Value = "AKA Score";
                ws.Cells[1, 7].Value = "Winner AKA";
                for (int i = 1; i <= 7; i++) { ws.Cells[1,i].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red); }
                ws.Cells[1, 8].Value = "Winner AO";
                ws.Cells[1, 14].Value = "ID_AO";
                ws.Cells[1, 13].Value = "AO First_Name";
                ws.Cells[1, 12].Value = "AO Last_Name";
                ws.Cells[1, 11].Value = "AO Fouls C1";
                ws.Cells[1, 10].Value = "AO Fouls C2";
                ws.Cells[1, 9].Value = "AO Score";
                for (int i = 8; i <= 14; i++) { ws.Cells[1, i].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Blue); }
                foreach (var m in r.Matches)
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
        private void createCategory_Click(object sender, RoutedEventArgs e)
        {
            CategoryViewer createCategory = new CategoryViewer(CompetitorsList, "", true);
            createCategory.Show();
        }


        void GenerateCategory()
        {
            GlobalCategory = new Category(CompetitorsList);
            GlobalCategory.GenerateTree();
            foreach (var g in GlobalCategory.Rounds)
            {
                groups_List.Items.Add($"1/{g.ToString()}");
            }

            //TODO: Export Category
            ExportCategory();

            groups_List.SelectedIndex = 0;
            CompetitorsGrid.Items.Refresh();
            DisplayMessageDialog("Category", "Category created");
        }

        private async void DisplayMessageDialog(string caption, string message)
        {
            ContentDialog ServerDialog = new ContentDialog
            {
                Title = caption,
                CloseButtonText = "Ok",
                Content = message,
            };

            ContentDialogResult result = await ServerDialog.ShowAsync();
        }


        private void groups_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalCategory.Rounds.Count() > 0)
            {
                MatchesGrid.ItemsSource = GlobalCategory.Rounds[groups_List.SelectedIndex].Matches;
                MatchesGrid.SelectedIndex = 0;
            }
            MatchesGrid.Items.Refresh();
            //CompetitorsGrid.Items.Refresh();
        }

        private void openCompList_Click(object sender, RoutedEventArgs e)
        {
            //Competitors = new Dictionary<int, Competitor>();
            groups_List.Items.Clear();
            CompetitorsList = new List<Competitor>();
            /*OpenFileDialog openFile = new OpenFileDialog();
            try { openFile.InitialDirectory = Properties.Settings.Default.DataPath; }
            catch { }
            openFile.Title = "Open Competitors List";
            openFile.Filter = "Excel Files(*.xls;*.xlsx)|*.xls;*.xlsx";
            if (openFile.ShowDialog() == true)
            {
                ex = new Excel.Application();
                string fileName = openFile.FileName;
                ex.Workbooks.Open(fileName,
                  Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                  Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                  Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                  Type.Missing, Type.Missing);

                ex.DisplayAlerts = false;
                ex.Visible = true;

                Excel.Worksheet worksheet;
                worksheet = (Excel.Worksheet)ex.Worksheets[1];
                MessageBox.Show("Text");
                foreach (Excel.Worksheet ws in ex.Worksheets)
                {
                    groups_List.Items.Add(ws.Name);
                }
                groups_List.SelectedIndex = 0;
            }*/
        }

        private void saveCat_Click(object sender, RoutedEventArgs e)
        {
           /* CategoryApp.ActiveWorkbook.Save();
            if (CategoryApp.ActiveWorkbook.Saved) { try { DisplayMessageDialog("Info", "Category file saved"); } catch { } }*/
        }

        private void CompetitorsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*if(GlobalCategory!=null)
            {
                Match match = (Match)CompetitorsGrid.SelectedItem;
                if (!match.AKA.IsBye && !match.AO.IsBye)
                {
                    GetMatchEv?.Invoke(CompetitorsGrid.SelectedIndex,groups_List.SelectedIndex);
                }
            }*/
        }

        private void MatchesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalCategory.Rounds.Count() > 0)
            {
                List<Competitor> comps = new List<Competitor>() { GlobalCategory.Rounds[groups_List.SelectedIndex].Matches[MatchesGrid.SelectedIndex].AKA,
                GlobalCategory.Rounds[groups_List.SelectedIndex].Matches[MatchesGrid.SelectedIndex].AO};
                CompetitorsGrid.ItemsSource = comps;
                
            }
            //MatchesGrid.Items.Refresh();
            GetMatchEv?.Invoke(MatchesGrid.SelectedIndex, groups_List.SelectedIndex);
            CompetitorsGrid.Items.Refresh();
        }
    }
}
