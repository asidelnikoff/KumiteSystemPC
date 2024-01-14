using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ModernWpf.Controls;
using SharedComponentsLibrary.DTO;
using SharedComponentsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TournamentsBracketsBase;
using Excel = Microsoft.Office.Interop.Excel;
using LanguageLibrary;

namespace SharedComponentsLibrary
{
    public partial class CategoryViewerViewModel : ObservableObject, ICategoryViewer
    {
        [ObservableProperty]
        bool isBusy;

        [ObservableProperty]
        CategoryDTO category;

        [ObservableProperty]
        Visibility categoryComplitnessVisibility;

        [ObservableProperty]
        Visibility categoryNotComplitnessVisibility;

        [ObservableProperty]
        bool isRegenerateBronzeButtonEnabled;

        [ObservableProperty]
        ObservableCollection<RoundDTO> rounds;

        [ObservableProperty]
        RoundDTO selectedRound;

        [ObservableProperty]
        ObservableCollection<IMatch> matches;

        [ObservableProperty]
        IMatch selectedMatch;

        [ObservableProperty]
        ObservableCollection<ICompetitor> competitors;

        [ObservableProperty]
        Grid bracketsGrid;

        [ObservableProperty]
        Visibility matchesContextMenuVisibility;

        [ObservableProperty]
        Visibility categoryResultsButtonVisibility;

        [ObservableProperty]
        Visibility swapCompetitorsButtonVisibility;

        [ObservableProperty]
        bool isExternalResultsOpened;

        [ObservableProperty]
        string matchWinner;

        ICategory currentCategory;

        private DBService dbService;

        public Action<RoundDTO, IMatch> GotMatch { get; set; }
        public Action<RoundDTO, IMatch> GotNextMatch { get; set; }

        public Action<IList<ICompetitor>> GotCategoryResults { get; set; }

        ExternalResults externalResultsBoard;

        public CategoryViewerViewModel(CategoryDTO category, DBService _dbService, bool isGenereationNeeded
            , bool shuffleCompetitors, bool isSwapCompetitorsEnabled)
        {
            Category = category;
            dbService = _dbService;
            if (isGenereationNeeded)
                GenerateCategory(category, shuffleCompetitors);

            MatchesContextMenuVisibility = Visibility.Collapsed;
            CategoryResultsButtonVisibility = Visibility.Collapsed;
            IsExternalResultsOpened = false;
            SwapCompetitorsButtonVisibility = isSwapCompetitorsEnabled && Category.Type != 2 ? Visibility.Visible : Visibility.Collapsed;

            SetupCategory();

            if (Category.Type != 2)
            {
                (currentCategory as TournamentTree.Category).BronzeGen += CategoryViewerViewModel_BronzeGen;
                (currentCategory as TournamentTree.Category).RepechageGen += CategoryViewerViewModel_BronzeGen;
            }

            if (currentCategory.Winners != null && currentCategory.Winners.Count > 0)
                CategoryResultsButtonVisibility = Visibility.Visible;

            SetCategoryStatus();
            Rounds = new ObservableCollection<RoundDTO>(dbService.GetRoundsInCategory(Category));
            IsRegenerateBronzeButtonEnabled = Rounds.Where(a => a.Repechage != -1).Any();
            BracketsGrid = DrawDeafultRoundsBrackets();

            PropertyChanged += CategoryViewerViewModel_PropertyChanged;
           
        }

        private void SetupCategory()
        {
            currentCategory = dbService.GetCategory(Category);
            currentCategory.HaveNxtMatch += CurrentCategory_HaveNxtMatch;
            currentCategory.HaveCategoryResults += CurrentCategory_HaveCategoryResults;
            currentCategory.RoundUpdated += CurrentCategory_RoundUpdated;
            SelectedRound = null;
            SelectedMatch = null;
        }

        private void CurrentCategory_RoundUpdated(int roundId, IList<IMatch> matches)
        {
            var round = dbService.GetRoundsInCategory(Category).Where(a => (int)a.Id == roundId).First();
            dbService.UpdateRound(round, matches);
        }

        private void CategoryViewerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedRound))
                if (SelectedRound != null)
                {
                    if (SelectedRound.Repechage == -1)
                        Matches = new ObservableCollection<IMatch>(currentCategory.Rounds[(int)SelectedRound.Id].Matches);
                    else if (SelectedRound.Repechage == 0)
                        Matches = new ObservableCollection<IMatch>((currentCategory as TournamentTree.Category).RepechageAKA.Matches);
                    else if (SelectedRound.Repechage == 1)
                        Matches = new ObservableCollection<IMatch>((currentCategory as TournamentTree.Category).RepechageAO.Matches);
                    else if (SelectedRound.Repechage == 2)
                        Matches = new ObservableCollection<IMatch>(){ (currentCategory as TournamentTree.Category).BronzeMatch };

                    BracketsGrid = DrawBrackets(SelectedRound);
                }
            if (e.PropertyName == nameof(SelectedMatch))
                if (SelectedMatch != null)
                {
                    Competitors = new ObservableCollection<ICompetitor>();
                    if (SelectedMatch.AKA != null)
                        Competitors.Add(SelectedMatch.AKA);
                    if (SelectedMatch.AO != null)
                        Competitors.Add(SelectedMatch.AO);
                    if (SelectedMatch.Winner != null)
                        MatchWinner = $"{Resources.Winner}: {SelectedMatch.Winner}";
                }
            MatchesContextMenuVisibility = Matches?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void GenerateCategory(CategoryDTO category, bool shuffleCompetitors)
        {
            ICategory generatedCategory = null;
            switch (category.Type)
            {
                case 0:
                    generatedCategory = GenerateSingleEliminationCategory(category, true, shuffleCompetitors);
                    break;
                case 1:
                    generatedCategory = GenerateSingleEliminationCategory(category, false, shuffleCompetitors);
                    break;
                case 2:
                    generatedCategory = GenerateRoundRobinCategory(category, shuffleCompetitors);
                    break;
            }

            dbService.AddGeneratedCategory(category, generatedCategory);
        }

        private RoundRobin.Category GenerateRoundRobinCategory(CategoryDTO _category, bool shuffleCompetitors)
        {
            var competitors = dbService.GetCompetitorsInCategory(_category)
               .Select(a => new RoundRobin.Competitor(false, (int)a.Id, a.FirstName, a.LastName, a.Club, 0, 0, 0, (int)a.Status))
               .ToList();
            if (shuffleCompetitors)
                competitors = competitors.OrderBy(a => Guid.NewGuid()).ToList();
            RoundRobin.Category category = new RoundRobin.Category(competitors);
            category.GenerateBrackets();

            return category;
        }

        private TournamentTree.Category GenerateSingleEliminationCategory(CategoryDTO _category, bool is2thirdPlaces, bool shuffleCompetitors)
        {
            var competitors = dbService.GetCompetitorsInCategory(_category)
                .Select(a => new TournamentTree.Competitor(false, (int)a.Id, a.FirstName, a.LastName, a.Club, 0, 0, 0, (int)a.Status))
                .ToList();
            if (shuffleCompetitors)
                competitors = competitors.OrderBy(a => Guid.NewGuid()).ToList();
            TournamentTree.Category category = new TournamentTree.Category(competitors, !is2thirdPlaces);
            category.GenerateBrackets();

            return category;
        }

        [RelayCommand]
        private async Task SwapCompetitors()
        {
            var swapDialog = new SwapCompetitorsDialog(dbService.GetCompetitorsInCategory(Category));
            var result = await swapDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var comp1 = swapDialog.Competitor1;
                var comp2 = swapDialog.Competitor2;
                try
                {
                    dbService.SwapCompetitors(comp1, comp2);
                    SetupCategory();
                    if(Rounds.Count > 0)
                        BracketsGrid = DrawBrackets(Rounds.ElementAt(0));
                    await Helpers.DisplayMessageDialog(Resources.CompetitorsSwaped, Resources.Info);
                }
                catch (InvalidOperationException ex)
                {
                    await Helpers.DisplayMessageDialog(Resources.CantSwapCompetitors, Resources.Error);
                }
            }
        }

        public void WriteMatchResults(RoundDTO round, IMatch match)
        {
            currentCategory.FinishMatch(match.ID, (int)round.Id);
            dbService.UpdateMatch(round, match);
            BracketsGrid = DrawBrackets(SelectedRound);
            OnPropertyChanged(nameof(SelectedMatch));
        }

        private void CurrentCategory_HaveNxtMatch(int round, int match, IMatch nxtMatch)
        {
            if(round >= 0 && Rounds.Count > round)
                GotNextMatch?.Invoke(Rounds[round], nxtMatch);
            else
                GotNextMatch?.Invoke(null, nxtMatch);
        }

        private async void CurrentCategory_HaveCategoryResults(List<ICompetitor> winners)
        {
            var list = new List<WinnerDTO>();
            if (winners.Count > 0)
                list.Add(new WinnerDTO() { Competitor = winners[0].ID, Place = 1 });
            if (winners.Count > 1)
                list.Add(new WinnerDTO() { Competitor = winners[1].ID, Place = 2 });
            if (winners.Count > 2 && winners[2] != null)
                list.Add(new WinnerDTO() { Competitor = winners[2].ID, Place = 3 });
            if (winners.Count > 3 && winners[3] != null)
                list.Add(new WinnerDTO() { Competitor = winners[3].ID, Place = 3 });
            dbService.AddWinners(Category, list);

            CategoryResultsButtonVisibility = Visibility.Visible;
            SetCategoryStatus();
            GotCategoryResults?.Invoke(winners);
            string s_winners = "";
            s_winners += $"1: {winners[0]}\n";
            s_winners += $"2: {winners[1]}\n";
            if (winners.Count > 2 && winners[2] != null) s_winners += $"3: {winners[2]}\n";
            if (winners.Count > 3 && winners[3] != null) s_winners += $"3: {winners[3]}\n";

            var dialogResult = await Helpers.DisplayQuestionDialog($"{Resources.HaveCategoryResults}:\n{s_winners}----------------------------\n" +
                $"{Resources.ShowExternalResultsBoardMessage}",
                Resources.ShowResults);

            if (dialogResult == ContentDialogResult.Primary)
                ShowExternalResults();
        }

        [RelayCommand]
        private void Close()
        {
            externalResultsBoard?.Close();
        }

        [RelayCommand]
        private void ShowExternalResults()
        {
            if (IsExternalResultsOpened)
                externalResultsBoard?.Close();
            else
            {
                externalResultsBoard = new ExternalResults(Category.Name, currentCategory.Winners);
                externalResultsBoard.Loaded += (sender, e) => IsExternalResultsOpened = true;
                externalResultsBoard.Closed += (sender, e) => IsExternalResultsOpened = false;
                externalResultsBoard.Show();
            }
        }

        IMatch loadedMatch;
        RoundDTO loadedMatchRound;
        [RelayCommand]
        private void LoadMatch()
        {
            LoadMatch(SelectedRound, SelectedMatch);
        }

        [RelayCommand]
        private void ShowResults()
        {
            SelectedRound = null;
            SelectedMatch = null;
            Competitors = new ObservableCollection<ICompetitor>(currentCategory.Winners);
        }

        public async void LoadMatch(RoundDTO round, IMatch match)
        {
            if (SelectedMatch == null || SelectedRound == null)
                return;

            if (loadedMatch != null && !loadedMatch.isFinished)
            {
                MyContentDialog finishLoadedMatch = new MyContentDialog()
                {
                    Content = Resources.YouHaventFinishCurrentlyLoadedMatch,
                    PrimaryButtonText = Resources.Finish,
                    SecondaryButtonText = Resources.LoadWithoutFinishing,
                    CloseButtonText = Resources.Cancel
                };
                var result = await finishLoadedMatch.ShowAsync();
                if (result == ContentDialogResult.Primary)
                    WriteMatchResults(loadedMatchRound, loadedMatch);
                else if (result == ContentDialogResult.None)
                    return;
            }
            if (match.AKA == null || match.AO == null)
                return;

            currentCategory.GetMatch(match.ID, (int)round.Id);
            GotMatch?.Invoke(round, match);
        }

        //regenerate bronze button logic
        [RelayCommand]
        private void RegenerateBronzeStage()
        {
            (currentCategory as TournamentTree.Category).GenerateBronze();
        }

        private void CategoryViewerViewModel_BronzeGen()
        {
            dbService.AddGeneratedRepechage(currentCategory as TournamentTree.Category, Category);
            Rounds = new ObservableCollection<RoundDTO>(dbService.GetRoundsInCategory(Category));
            IsRegenerateBronzeButtonEnabled = Rounds.Where(a => a.Repechage != -1).Any();
        }

        private void SetCategoryStatus()
        {
            if (currentCategory.Winners != null && currentCategory.Winners.Count > 0)
            {
                CategoryComplitnessVisibility = Visibility.Visible;
                CategoryNotComplitnessVisibility = Visibility.Collapsed;
            }
            else
            {
                CategoryComplitnessVisibility = Visibility.Collapsed;
                CategoryNotComplitnessVisibility = Visibility.Visible;
            }
        }

        [RelayCommand]
        private async Task ExprotToFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = $"{Category.Name}";
            sfd.DefaultExt = ".xlsx";
            sfd.Filter = "Excel documents (.xlsx)|*.xlsx";

            if (sfd.ShowDialog() == true)
            {
                IsBusy = true;
                Excel.Application app = await Task.Run(() => ExcelInteractions.ExportCategory(currentCategory, Category.Name));
                try
                {
                    app.ActiveWorkbook.SaveAs(sfd.FileName);
                    if (app.ActiveWorkbook.Saved)
                    {
                        MyContentDialog contentDialog = new MyContentDialog()
                        {
                            Content = Resources.CategoryExportedToFile,
                            PrimaryButtonText = Resources.Ok
                        };
                        contentDialog.ShowAsync();
                    }
                }
                finally
                {
                    app.Quit();
                    IsBusy = false;
                }
            }
        }

        public Grid DrawBrackets(RoundDTO round)
        {
            if (round == null)
                return new Grid();

            if (round.Repechage == -1)
                return DrawDeafultRoundsBrackets();
            else if (round.Repechage == 0)
                return DrawRepechageBrackets((currentCategory as TournamentTree.Category).RepechageAKA);
            else if (round.Repechage == 1)
                return DrawRepechageBrackets((currentCategory as TournamentTree.Category).RepechageAO);
            else if (round.Repechage == 2)
            {
                var repechage = new TournamentTree.Repechage();
                repechage.Competitors.Add((currentCategory as TournamentTree.Category).BronzeMatch.AKA as TournamentTree.Competitor);
                repechage.Competitors.Add((currentCategory as TournamentTree.Category).BronzeMatch.AO as TournamentTree.Competitor);
                repechage.Matches.Add((currentCategory as TournamentTree.Category).BronzeMatch);
                if ((currentCategory as TournamentTree.Category).BronzeMatch.Winner != null)
                    repechage.Winner = (currentCategory as TournamentTree.Category).BronzeMatch.Winner as TournamentTree.Competitor;
                return DrawRepechageBrackets(repechage);
            }

            return new Grid();
        }

        Grid DrawDeafultRoundsBrackets()
        {
            Grid result = new Grid();
            if (Category.Type == 0 || Category.Type == 1)
                result = DrawDefaultBrackets(currentCategory as TournamentTree.Category);
            else if (Category.Type == 2)
                result = DrawDefaultBrackets(currentCategory as RoundRobin.Category);

            result.Margin = new Thickness(10);

            return result;
        }
        Grid DrawDefaultBrackets(RoundRobin.Category GlobalCategory)
        {
            Grid BracketsGrid = new Grid();
            BracketsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            BracketsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < GlobalCategory.Rounds.Count; i++)
            {
                BracketsGrid.RowDefinitions.Add(new RowDefinition());
                Label round_Label = new Label();
                round_Label.Content = $"{Resources.Round} {GlobalCategory.Rounds[i].ID + 1}";
                Grid.SetRow(round_Label, BracketsGrid.RowDefinitions.Count - 1);
                BracketsGrid.Children.Add(round_Label);
                for (int j = 0; j < GlobalCategory.Rounds[i].Matches.Count; j++)
                {
                    BracketsGrid.RowDefinitions.Add(new RowDefinition());
                    BracketsGrid.RowDefinitions.Add(new RowDefinition());

                    Grid fool = MakeMatch(new RoundRobin.Match(new RoundRobin.Competitor(), new RoundRobin.Competitor(), 0), false);
                    Grid.SetRow(fool, BracketsGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(fool, 0);
                    BracketsGrid.Children.Add(fool);

                    Grid match;
                    if (j % 2 == 0)
                        match = MakeMatch(GlobalCategory.Rounds[i].Matches[j], true);
                    else
                    {
                        if (i != 0) match = MakeMatch(GlobalCategory.Rounds[i].Matches[j], true, 0);
                        else match = MakeMatch(GlobalCategory.Rounds[i].Matches[j], true, 0);
                    }
                    Grid.SetRow(match, BracketsGrid.RowDefinitions.Count - 2);
                    Grid.SetColumn(match, 0);
                    BracketsGrid.Children.Add(match);
                }
            }
            return BracketsGrid;
        }
        SolidColorBrush RedBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#c84b31");
        SolidColorBrush BlueBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#2d4263");

        Grid MakeMatch(IMatch match, bool setColor, int b_row = 1)
        {
            Grid res = new Grid();
            res.RowDefinitions.Add(new RowDefinition());
            res.RowDefinitions.Add(new RowDefinition());
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });

            Border akaB = new Border();
            akaB.Background = RedBrush;
            akaB.Padding = new Thickness(5);
            Grid.SetRow(akaB, 0);

            Border aoB = new Border();
            aoB.Background = BlueBrush;
            aoB.Padding = new Thickness(5);
            Grid.SetRow(aoB, 1);

            Label aka = new Label();
            if (!match.AKA.IsBye)
                aka.Content = match.AKA;
            else
                aka.Content = " ";
            aka.Foreground = Brushes.White;
            aka.Margin = new Thickness(5, 0, 0, 0);
            aka.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(aka, 0);

            Label ao = new Label();
            if (!match.AO.IsBye)
                ao.Content = match.AO;
            else
                ao.Content = " ";
            ao.Foreground = Brushes.White;
            ao.Margin = new Thickness(5, 0, 0, 0);
            ao.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(ao, 1);

            if (setColor)
            {
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(0, b_row, 0, 1 - b_row);
                rectangle.BorderBrush = Brushes.Black;
                Grid.SetColumn(rectangle, 1);
                Grid.SetRow(rectangle, b_row);
                res.Children.Add(akaB);
                res.Children.Add(aoB);
                res.Children.Add(rectangle);
                Border border = new Border();
                border.BorderThickness = new Thickness(1, 1, 1, 1);
                border.BorderBrush = Brushes.Black;
                Grid.SetRowSpan(border, 2);
                Grid.SetColumn(border, 2);
                res.Children.Add(border);
            }

            if (match.Winner != null)
            {
                Label winner = new Label();
                winner.Content = match.Winner;
                Grid.SetRowSpan(winner, 2);
                winner.HorizontalContentAlignment = HorizontalAlignment.Center;
                winner.VerticalContentAlignment = VerticalAlignment.Center;
                Grid.SetColumn(winner, 2);
                res.Children.Add(winner);
            }

            res.Children.Add(aka);
            res.Children.Add(ao);

            return res;
        }

        Grid DrawDefaultBrackets(TournamentTree.Category GlobalCategory)
        {
            Grid BracketsGrid = new Grid();
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
                    Grid fool = MakeMatch(new TournamentTree.Competitor(), new TournamentTree.Competitor(), false, false);
                    if (row > 0) Grid.SetRow(fool, row - 1);
                    else Grid.SetRow(fool, row);
                    Grid.SetColumn(fool, i);
                    BracketsGrid.Children.Add(fool);
                    TournamentTree.Competitor aka = new TournamentTree.Competitor(),
                        ao = new TournamentTree.Competitor();

                    if (GlobalCategory.Rounds[i].Matches[j].AKA != null)
                        aka = GlobalCategory.Rounds[i].Matches[j].AKA as TournamentTree.Competitor;
                    if (GlobalCategory.Rounds[i].Matches[j].AO != null)
                        ao = GlobalCategory.Rounds[i].Matches[j].AO as TournamentTree.Competitor;
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
                                    BorderBrush = Brushes.Black,
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
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(1, 1, 1, 1);
                rectangle.BorderBrush = Brushes.Black;
                Grid.SetColumn(rectangle, GlobalCategory.Rounds.Count);
                Grid.SetRow(rectangle, row);
                Grid.SetColumn(Winner, GlobalCategory.Rounds.Count);
                Grid.SetRow(Winner, row);

                BracketsGrid.Children.Add(Winner);
                BracketsGrid.Children.Add(rectangle);
            }
            return BracketsGrid;
        }
        Grid DrawRepechageBrackets(TournamentTree.Repechage _Repechage)
        {
            Grid BracketsGrid = new Grid();
            int row = 0;
            for (int i = 0; i < _Repechage.Matches.Count; i++)
            {
                BracketsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                BracketsGrid.RowDefinitions.Add(new RowDefinition());
                BracketsGrid.RowDefinitions.Add(new RowDefinition());
                Grid fool = MakeMatch(new TournamentTree.Competitor(), new TournamentTree.Competitor(), false, false);
                if (row > 0) Grid.SetRow(fool, row - 1);
                else Grid.SetRow(fool, row);
                Grid.SetColumn(fool, i);
                BracketsGrid.Children.Add(fool);
                TournamentTree.Competitor aka = new TournamentTree.Competitor(),
                    ao = new TournamentTree.Competitor();

                if (_Repechage.Matches[i].AKA != null) aka = _Repechage.Matches[i].AKA as TournamentTree.Competitor;
                if (_Repechage.Matches[i].AO != null) ao = _Repechage.Matches[i].AO as TournamentTree.Competitor;
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
                                BorderBrush = Brushes.Black,
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
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(1, 1, 1, 1);
                rectangle.BorderBrush = Brushes.Black;
                Grid.SetColumn(rectangle, _Repechage.Matches.Count);
                Grid.SetRow(rectangle, row);
                Grid.SetColumn(Winner, _Repechage.Matches.Count);
                Grid.SetRow(Winner, row);

                BracketsGrid.Children.Add(Winner);
                BracketsGrid.Children.Add(rectangle);
            }

            BracketsGrid.Margin = new Thickness(10);

            return BracketsGrid;
        }
        Grid MakeMatch(TournamentTree.Competitor nameAka, TournamentTree.Competitor nameAo, bool setColor, bool back_b, int b_row = 1)
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

            Border akaB = new Border(); akaB.Background = RedBrush;
            akaB.Padding = new Thickness(5);
            Grid.SetRow(akaB, 0);
            if (back_b) Grid.SetColumn(akaB, 1);

            Border aoB = new Border(); aoB.Background = BlueBrush;
            aoB.Padding = new Thickness(5);
            Grid.SetRow(aoB, 1);
            if (back_b) Grid.SetColumn(aoB, 1);

            Label aka = new Label();
            if (!nameAka.IsBye && !nameAo.IsBye) aka.Content = nameAka;
            else aka.Content = " ";
            aka.Foreground = Brushes.White;
            aka.Margin = new Thickness(5, 0, 0, 0);
            aka.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(aka, 0);
            if (back_b) Grid.SetColumn(aka, 1);

            Label ao = new Label();
            if (!nameAka.IsBye && !nameAo.IsBye) ao.Content = nameAo;
            else ao.Content = " ";
            ao.Foreground = Brushes.White;
            ao.Margin = new Thickness(5, 0, 0, 0);
            ao.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(ao, 1);
            if (back_b) Grid.SetColumn(ao, 1);

            if (setColor)
            {
                Border rectangle = new Border();
                rectangle.BorderThickness = new Thickness(0, b_row, 1, 1 - b_row);
                rectangle.BorderBrush = Brushes.Black;
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
                rectangle.BorderBrush = Brushes.Black;
                Grid.SetRow(rectangle, b_row);
                res.Children.Add(rectangle);
            }

            res.Children.Add(aka);
            res.Children.Add(ao);

            return res;
        }
        Grid MakeFinal(TournamentTree.Competitor nameAka, TournamentTree.Competitor nameAo, bool back_b = false)
        {
            Grid res = new Grid();
            res.RowDefinitions.Add(new RowDefinition());
            res.RowDefinitions.Add(new RowDefinition());
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(4, GridUnitType.Star) });
            res.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });

            Border akaB = new Border(); akaB.Background = RedBrush;
            akaB.Padding = new Thickness(5);
            Grid.SetRow(akaB, 0);
            Grid.SetColumn(akaB, 1);

            Border aoB = new Border(); aoB.Background = BlueBrush;
            aoB.Padding = new Thickness(5);
            Grid.SetRow(aoB, 1);
            Grid.SetColumn(aoB, 1);

            Label aka = new Label(); aka.Content = nameAka;
            aka.Foreground = Brushes.White;
            aka.Margin = new Thickness(5, 0, 0, 0);
            aka.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(aka, 0);
            Grid.SetColumn(aka, 1);
            Label ao = new Label(); ao.Content = nameAo;
            ao.Foreground = Brushes.White;
            ao.Margin = new Thickness(5, 0, 0, 0);
            ao.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(ao, 1);
            Grid.SetColumn(ao, 1);

            Border rectangle = new Border();
            rectangle.BorderThickness = new Thickness(0, 0, 0, 1);
            rectangle.BorderBrush = Brushes.Black;
            Grid.SetColumn(rectangle, 2);
            res.Children.Add(akaB);
            res.Children.Add(aoB);
            res.Children.Add(rectangle);

            rectangle = new Border();
            if (!back_b) rectangle.BorderThickness = new Thickness(0, 0, 0, 1);
            else rectangle.BorderThickness = new Thickness(1, 0, 0, 1);
            rectangle.BorderBrush = Brushes.Black;
            Grid.SetColumn(rectangle, 0);
            res.Children.Add(rectangle);

            res.Children.Add(aka);
            res.Children.Add(ao);

            return res;
        }
    }
}
