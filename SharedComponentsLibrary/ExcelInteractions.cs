using Microsoft.Office.Interop.Excel;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentsBracketsBase;
using Excel = Microsoft.Office.Interop.Excel;

namespace SharedComponentsLibrary
{
    public static class ExcelInteractions
    {
        public static Excel.Application ExportCategory(ICategory category, string categoryName)
        {
            if (category as TournamentTree.Category != null)
                return ExportCategory(category as TournamentTree.Category, categoryName);
            else if (category as RoundRobin.Category != null)
                return ExportCategory(category as RoundRobin.Category, categoryName);

            return null;
        }

        public static Excel.Application ExportCategory(TournamentTree.Category category, string CategoryName)
        {
            Excel.Application ex = new Excel.Application();
            ex.Workbooks.Add();
            Excel.Workbook wb = ex.ActiveWorkbook;
            Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(Type.Missing, wb.Worksheets[wb.Worksheets.Count]);
            ws.Name = "Print";
            if (category.Rounds != null)
            {
                int col = 1;
                int count = 0;
                int start_row = 3;
                ExportFirstVisual(ws, category);
                for (int i = 1; i < category.Rounds.Count(); i++)
                {
                    col += 3;
                    start_row += Convert.ToInt32(Math.Pow(2, i));
                    int row = start_row;
                    int add = Convert.ToInt32(Math.Pow(2, i + 2));
                    foreach (var m in category.Rounds[i].Matches)
                    {
                        Excel.Range range = ((Excel.Range)ws.Cells[row, col]).EntireColumn;

                        if (m.AKA != null) 
                            ((Excel.Range)ws.Cells[row, col]).Value = m.AKA.GetName();
                        ((Excel.Range)ws.Cells[row, col]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        ((Excel.Range)ws.Cells[row, col]).Borders.Weight = 2d;
                        row += 1;

                        if (m.AO != null) 
                            ((Excel.Range)ws.Cells[row, col]).Value = m.AO.GetName();
                        ((Excel.Range)ws.Cells[row, col]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        ((Excel.Range)ws.Cells[row, col]).Borders.Weight = 2d;

                        ((Excel.Range)ws.Cells[row, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                        ((Excel.Range)ws.Cells[row, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;

                        if (count % 2 == 0 && i + 1 != category.Rounds.Count())
                        {
                            for (int k = 0; k < add; k++)
                            {
                                ((Excel.Range)ws.Cells[row + k, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                                ((Excel.Range)ws.Cells[row + k, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;
                            }
                            ((Excel.Range)ws.Cells[row + (add / 2), col + 2]).Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                            ((Excel.Range)ws.Cells[row + (add / 2), col + 2]).Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                            ((Excel.Range)ws.Columns[col + 2]).ColumnWidth = 3;
                        }

                        row += (add - 1);
                        count++;
                        range.EntireColumn.AutoFit();
                    }
                    ((Excel.Range)ws.Columns[col + 1]).ColumnWidth = 3;
                    ((Excel.Range)ws.Columns[col]).ColumnWidth = 32;
                }
                col += 2;
                int _row = Convert.ToInt32(Math.Pow(2, category.Rounds.Count())) + 1;
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

                if (category.Rounds[category.Rounds.Count() - 1].Matches[0].Winner != null)
                {
                    ((Excel.Range)ws.Cells[_row, col]).Value = $"{category.Rounds[category.Rounds.Count() - 1].Matches[0].Winner}";
                    ((Excel.Style)((Excel.Range)ws.Cells[_row, col]).Style).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    ((Excel.Style)((Excel.Range)ws.Cells[_row, col]).Style).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                }

                ((Excel.Range)ws.Columns[col]).ColumnWidth = 32;
                ((Excel.Range)ws.Cells[1, 1]).Value = $"Категория: {CategoryName}";
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.Font.Bold = true;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.Font.Size = 14;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, ws.UsedRange.Columns.Count]].Merge();

                ws.Range[ws.Cells[2, 1], ws.Cells[ws.UsedRange.Rows.Count, ws.UsedRange.Columns.Count]].Cells.Font.Size = 12;

                if (wb.Worksheets.Count > 1)
                    ((Excel.Worksheet)wb.Worksheets[1]).Delete();

                ExportRounds(wb, category);
            }

            if (category.RepechageAKA != null && category.RepechageAKA.Matches.Count > 0)
                ExportRepechage(wb, 0, category);
            if (category.RepechageAO != null && category.RepechageAO.Matches.Count > 0)
                ExportRepechage(wb, 1, category);

            if (category.BronzeMatch != null)
                ExportRepechage(wb, 2, category);

            if (category.Winners != null && category.Winners.Count > 0)
                ExportCategoryResultsToExcel(category.Winners, CategoryName, wb);

            return ex;
        }
        static void ExportFirstVisual(Excel.Worksheet ws, TournamentTree.Category category)
        {
            int row = 3;
            int col = 1;
            int count = 0;
            foreach (var m in category.Rounds[0].Matches)
            {
                Excel.Range range = ((Excel.Range)ws.Cells[row, col]).EntireColumn;

                if (!m.AKA.IsBye && !m.AO.IsBye) 
                    ((Excel.Range)ws.Cells[row, col]).Value = m.AKA.GetName();
                ((Excel.Range)ws.Cells[row, col]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ((Excel.Range)ws.Cells[row, col]).Borders.Weight = 2d;
                row += 1;


                if (!m.AKA.IsBye && !m.AO.IsBye) 
                    ((Excel.Range)ws.Cells[row, col]).Value = m.AO.GetName();
                ((Excel.Range)ws.Cells[row, col]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ((Excel.Range)ws.Cells[row, col]).Borders.Weight = 2d;

                ((Excel.Range)ws.Cells[row, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                ((Excel.Range)ws.Cells[row, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;

                if (count % 2 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ((Excel.Range)ws.Cells[row + i, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                        ((Excel.Range)ws.Cells[row + i, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;
                    }
                    ((Excel.Range)ws.Cells[row + 2, col + 2]).Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                    ((Excel.Range)ws.Cells[row + 2, col + 2]).Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                    ((Excel.Range)ws.Columns[col + 2]).ColumnWidth = 3;
                }
                row += 3;

                count++;
                range.EntireColumn.AutoFit();
            }
            ((Excel.Range)ws.Columns[col + 1]).ColumnWidth = 3;
            ((Excel.Range)ws.Columns[col]).ColumnWidth = 32;
        }
        static void ExportRounds(Excel.Workbook wb, ICategory Category)
        {
            foreach (var r in Category.Rounds)
            {
                Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(wb.Worksheets[wb.Worksheets.Count]);
                ws.Name = $"{r}";
                ExportMatchesToExcelSheet(ws, r.Matches);
            }
        }
        static void ExportRepechage(Excel.Workbook workbook, int num, TournamentTree.Category category)
        {
            if (num == 0)
            {
                Excel.Worksheet ws = (Excel.Worksheet)workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws.Name = "Repechage 1";
                ExportMatchesToExcelSheet(ws, category.RepechageAKA.Matches);

                Excel.Worksheet ws_ = (Excel.Worksheet)workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws_.Name = "Repechage 1(Visual)";
                ExportRepechageVisual(ws_, category.RepechageAKA);

            } //Export Repechage AKA
            else if (num == 1)
            {
                Excel.Worksheet ws = (Excel.Worksheet)workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws.Name = "Repechage 2";
                ExportMatchesToExcelSheet(ws, category.RepechageAO.Matches);

                Excel.Worksheet _ws = (Excel.Worksheet)workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                _ws.Name = "Repechage 2(Visual)";
                ExportRepechageVisual(_ws, category.RepechageAO);
            } //Export Repechage AO
            else if (num == 2)
            {
                Excel.Worksheet ws = (Excel.Worksheet)workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                ws.Name = "Bronze Match";
                TournamentTree.Repechage temp = new TournamentTree.Repechage();
                temp.Matches = new List<TournamentsBracketsBase.IMatch>() { category.BronzeMatch };
                ExportMatchesToExcelSheet(ws, temp.Matches);

                Excel.Worksheet _ws = (Excel.Worksheet)workbook.Worksheets.Add(workbook.Worksheets[workbook.Worksheets.Count]);
                _ws.Name = "Bronze Match(Visual)";
                ExportRepechageVisual(_ws, temp);
            } //Export Bronze match

        }

        static Excel.Application ExportCategory(RoundRobin.Category category, string CategoryName)
        {
            Excel.Application ex = new Excel.Application();
            ex.Workbooks.Add();
            Excel.Workbook wb = ex.ActiveWorkbook;
            Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets.Add(Type.Missing, wb.Worksheets[wb.Worksheets.Count]);
            ws.Name = "Visualizing";
            if (category.Rounds != null)
            {
                int row = 4;
                for (int i = 0; i < category.Rounds.Count(); i++)
                {
                    ((Excel.Range)ws.Cells[row, 1]).Value = $"Раунд {i + 1}";
                    Excel.Range _round_range = ws.Range[ws.Cells[row, 1], ws.Cells[row, 3]];
                    _round_range.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    _round_range.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    ((Excel.Range)ws.Rows[row]).RowHeight = 24;
                    _round_range.Merge();
                    row++;
                    foreach (var m in category.Rounds[i].Matches)
                    {
                        if (!m.AKA.IsBye && !m.AO.IsBye)
                        {
                            if (m.AKA != null) 
                                ((Excel.Range)ws.Cells[row, 1]).Value = m.AKA.GetName();
                            ((Excel.Range)ws.Cells[row, 1]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                            ((Excel.Range)ws.Cells[row, 1]).Borders.Weight = 2d;
                            Excel.Range _merge = ws.Range[ws.Cells[row, 1], ws.Cells[row + 1, 1]];
                            _merge.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                            _merge.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                            _merge.Borders.Weight = 2d;
                            _merge.Merge();

                            if (m.AO != null) 
                                ((Excel.Range)ws.Cells[row, 3]).Value = m.AO.GetName();
                            ((Excel.Range)ws.Cells[row, 3]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                            ((Excel.Range)ws.Cells[row, 3]).Borders.Weight = 2d;
                            _merge = ws.Range[ws.Cells[row, 3], ws.Cells[row + 1, 3]];
                            _merge.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                            _merge.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                            _merge.Borders.Weight = 2d;
                            _merge.Merge();

                            Excel.Range _border = ws.Range[ws.Cells[row, 2], ws.Cells[row, 2]];
                            _border.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                            _border.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2d;

                            row += 2;
                        }
                    }

                    row += 2;
                }
                ((Excel.Range)ws.Columns[1]).ColumnWidth = 32;
                ((Excel.Range)ws.Columns[3]).ColumnWidth = 32;
                ((Excel.Range)ws.Columns[2]).ColumnWidth = 4;

                ((Excel.Range)ws.Cells[1, 1]).Value = $"Категория: {CategoryName}";
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.Font.Bold = true;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, 1]].Cells.Font.Size = 14;
                ws.Range[ws.Cells[1, 1], ws.Cells[1, ws.UsedRange.Columns.Count]].Merge();

                ws.Range[ws.Cells[2, 1], ws.Cells[ws.UsedRange.Rows.Count, ws.UsedRange.Columns.Count]].Cells.Font.Size = 12;

                if (wb.Worksheets.Count > 1) 
                    ((Excel.Worksheet)wb.Worksheets[1]).Delete();
                ExportRounds(wb, category);
            }

            if (category.Winners != null && category.Winners.Count > 0)
                ExportCategoryResultsToExcel(category.Winners, CategoryName, wb);

            return ex;
        }

        static void ExportCategoryResultsToExcel(List<ICompetitor> winners, string CategoryName, Excel.Workbook wb)
        {
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

        static void ExportMatchesToExcelSheet(Excel.Worksheet ws, List<TournamentsBracketsBase.IMatch> matches)
        {
            int row = 2;
            SetupPageHeader(ws);

            foreach (var m in matches)
            {
                if (m.AKA != null)
                {
                    ((Excel.Range)ws.Cells[row, 1]).Value = m.AKA.ID;
                    ((Excel.Range)ws.Cells[row, 2]).Value = m.AKA.FirstName;
                    ((Excel.Range)ws.Cells[row, 3]).Value = m.AKA.LastName;
                    ((Excel.Range)ws.Cells[row, 4]).Value = m.AKA.Club;
                    ((Excel.Range)ws.Cells[row, 5]).Value = m.AKA.GetFoulsC1();
                    ((Excel.Range)ws.Cells[row, 6]).Value = m.AKA.Score;
                }
                if (m.AO != null)
                {
                    ((Excel.Range)ws.Cells[row, 14]).Value = m.AO.ID;
                    ((Excel.Range)ws.Cells[row, 13]).Value = m.AO.FirstName;
                    ((Excel.Range)ws.Cells[row, 12]).Value = m.AO.LastName;
                    ((Excel.Range)ws.Cells[row, 11]).Value = m.AO.Club;
                    ((Excel.Range)ws.Cells[row, 10]).Value = m.AO.GetFoulsC1();
                    ((Excel.Range)ws.Cells[row, 9]).Value = m.AO.Score;
                }
                if (m.Winner != null && m.Winner.ID == m.AKA.ID && m.Winner.FirstName == m.AKA.FirstName)
                    ((Excel.Range)ws.Cells[row, 7]).Value = "X";
                else if (m.Winner != null && m.Winner.ID == m.AO.ID && m.Winner.FirstName == m.AO.FirstName)
                    ((Excel.Range)ws.Cells[row, 8]).Value = "X";
                row++;
            }
            Excel.Range range = ws.UsedRange;
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Borders.Weight = 2d;
        }

        static void SetupPageHeader(Excel.Worksheet ws)
        {
            ((Excel.Range)ws.Cells[1, 1]).Value = "ID_AKA";
            ((Excel.Range)ws.Cells[1, 2]).Value = "AKA First_Name";
            ((Excel.Range)ws.Cells[1, 3]).Value = "AKA Last_Name";
            ((Excel.Range)ws.Cells[1, 4]).Value = "AKA Club";
            ((Excel.Range)ws.Cells[1, 5]).Value = "AKA Fouls";
            ((Excel.Range)ws.Cells[1, 6]).Value = "AKA Score";
            ((Excel.Range)ws.Cells[1, 7]).Value = "Winner AKA";
            for (int i = 1; i <= 7; i++)
                ((Excel.Range)ws.Cells[1, i]).Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
            ((Excel.Range)ws.Cells[1, 8]).Value = "Winner AO";
            ((Excel.Range)ws.Cells[1, 14]).Value = "ID_AO";
            ((Excel.Range)ws.Cells[1, 13]).Value = "AO First_Name";
            ((Excel.Range)ws.Cells[1, 12]).Value = "AO Last_Name";
            ((Excel.Range)ws.Cells[1, 11]).Value = "AO Club";
            ((Excel.Range)ws.Cells[1, 10]).Value = "AO Fouls";
            ((Excel.Range)ws.Cells[1, 9]).Value = "AO Score";
            for (int i = 8; i <= 14; i++)
                ((Excel.Range)ws.Cells[1, i]).Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Blue);
        }

        static void ExportRepechageVisual(Excel.Worksheet ws, TournamentTree.Repechage repechage)
        {
            int col = 1, i = 0;
            int row = 3;
            int add = 2;
            foreach (var m in repechage.Matches)
            {

                Excel.Range range = ((Excel.Range)ws.Cells[row, col]).EntireColumn;

                if (m.AKA != null) 
                    ((Excel.Range)ws.Cells[row, col]).Value = m.AKA.GetName();
                ((Excel.Range)ws.Cells[row, col]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ((Excel.Range)ws.Cells[row, col]).Borders.Weight = 2d;
                row += 1;

                if (m.AO != null) 
                    ((Excel.Range)ws.Cells[row, col]).Value = m.AO.GetName();
                ((Excel.Range)ws.Cells[row, col]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ((Excel.Range)ws.Cells[row, col]).Borders.Weight = 2d;

                ((Excel.Range)ws.Cells[row, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                ((Excel.Range)ws.Cells[row, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                for (int k = 0; k < add; k++)
                {
                    ((Excel.Range)ws.Cells[row + k, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                    ((Excel.Range)ws.Cells[row + k, col + 1]).Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;
                }
                ((Excel.Range)ws.Cells[row + add, col + 2]).Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                ((Excel.Range)ws.Cells[row + add, col + 2]).Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;
                ((Excel.Range)ws.Columns[col + 2]).ColumnWidth = 3;
                row++;
                range.EntireColumn.AutoFit();
                i++;
                ((Excel.Range)ws.Columns[col + 1]).ColumnWidth = 3;
                ((Excel.Range)ws.Columns[col]).ColumnWidth = 32;
                col += 3;
            }
            ((Excel.Range)ws.Columns[col + 1]).ColumnWidth = 3;
            ((Excel.Range)ws.Columns[col]).ColumnWidth = 32;
            Excel.Range _range = ws.Range[ws.Cells[row, col], ws.Cells[row + 1, col]];
            _range.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2d;

            _range.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2d;

            _range.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = 2d;

            _range.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            _range.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2d;

            _range.Merge();

            if (repechage.Winner != null)
            {
                ((Excel.Range)ws.Cells[row, col]).Value = $"{repechage.Winner}";
                ((Excel.Style)((Excel.Range)ws.Cells[row, col]).Style).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                ((Excel.Style)((Excel.Range)ws.Cells[row, col]).Style).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            }
        }

        static void SetCellStyle(int row, int col, Microsoft.Office.Interop.Excel.Worksheet ws)
        {
            ((Excel.Range)ws.Columns[col + 1]).ColumnWidth = 3;
            ((Excel.Range)ws.Columns[col]).ColumnWidth = 32;
            ((Excel.Range)ws.Cells[row, col]).Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            ((Excel.Range)ws.Cells[row, col]).Borders.Weight = 2d;
        }
    }
}
