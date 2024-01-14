using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using TournamentsBracketsBase;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using LanguageLibrary;

namespace SharedComponentsLibrary
{
    public abstract partial class InternalBoardViewModel : ObservableObject
    {
        [ObservableProperty]
        bool isNextMatchButtonEnabled;

        [ObservableProperty]
        IMatch currentMatch;
        protected RoundDTO currentMatchRound;

        [ObservableProperty]
        string currentMatchAkaText;
        [ObservableProperty]
        string currentMatchAoText;

        [ObservableProperty]
        IMatch nextMatch;
        protected RoundDTO nextMatchRound;

        [ObservableProperty]
        string nextMatchAkaText;
        [ObservableProperty]
        string nextMatchAoText;

        [ObservableProperty]
        Visibility tieButtonVisibility;

        [ObservableProperty]
        bool isTimerBoardOpened;

        protected TimerBoard timerBoard;

        protected CategoryDTO currentCategory;

        protected DBService dbService;

        [ObservableProperty]
        bool isExternalBoardOpened;

        [ObservableProperty]
        FlowDocument loggerDocument;

        [ObservableProperty]
        bool isNameFieldReadOnly;

        protected ICategoryViewer categoryViewer;

        protected System.Media.SoundPlayer endOfMatchSound;

        public InternalBoardViewModel()
        {

            if (Properties.Settings.Default.EndOfMatchSound != "")
                endOfMatchSound = new System.Media.SoundPlayer(Properties.Settings.Default.EndOfMatchSound);

            IsNextMatchButtonEnabled = Properties.Settings.Default.IsAutoLoadNextMatchEnabled;
            IsNameFieldReadOnly = false;

            LoggerDocument = new FlowDocument();

            CurrentMatch = new TournamentTree.Match(new TournamentTree.Competitor(false, -1, "AKA"), new TournamentTree.Competitor(false, -2, "AO"), -1);
            CurrentMatchAkaText = CurrentMatch.AKA?.ToString();
            CurrentMatchAoText = CurrentMatch.AO?.ToString();

            SetupMatch(CurrentMatch);
        }


        protected async Task SetupDbService()
        {
            string database = Properties.Settings.Default.DatabasePath;
            while (String.IsNullOrEmpty(Properties.Settings.Default.DataPath))
            {
                await Helpers.DisplayMessageDialog(Resources.ChooseDefaultDataPath, Resources.Info);
                OpenSettings();
            }
            if (String.IsNullOrEmpty(database))
            {
                database = Properties.Settings.Default.DataPath + @"\tournaments.sqlite";
                Properties.Settings.Default.DatabasePath = database;
                Properties.Settings.Default.Save();
            }

            this.dbService = new DBService(database);
        }

        protected abstract void LoadSettings();

        [RelayCommand]
        private void OpenSettings()
        {
            Settings settings = new Settings(new UserSettings()
            {
                DataPath = Properties.Settings.Default.DataPath,
                DatabasePath = Properties.Settings.Default.DatabasePath,
                EndOfMatchSound = Properties.Settings.Default.EndOfMatchSound,
                WarningSound = Properties.Settings.Default.WarningSound,
                ExternalMonitorIndex = Properties.Settings.Default.ExternalScreenIndex,
                Tatami = Properties.Settings.Default.Tatami,
                IsAutoLoadNextMatchEnabled = Properties.Settings.Default.IsAutoLoadNextMatchEnabled,
                IsNextMatchShownOnExternalBoard = Properties.Settings.Default.IsNextMatchShownOnExternalBoard,
                Language = new Language() { CultureInfo = Properties.Settings.Default.Language }
            });
            settings.SaveSettings += Settings_SaveSettings;
            settings.ShowDialog();
        }
        protected abstract void Settings_SaveSettings(UserSettings settings);

        protected void Close()
        {
            timerBoard?.Close();
            (categoryViewer as CategoryViewer)?.Close();
        }

        [RelayCommand]
        private void OpenTimerBoard()
        {
            if (IsTimerBoardOpened)
                timerBoard?.Close();
            else
            {
                timerBoard = new TimerBoard();
                timerBoard.Loaded += (sender, e) => IsTimerBoardOpened = true;
                timerBoard.Closed += (sender, e) => IsTimerBoardOpened = false;
                timerBoard.Show();
            }
        }

        protected async void ResetMatch()
        {
            CurrentMatch.Reset();
            ClearLog();
            OnPropertyChanged(nameof(CurrentMatch));
            await Helpers.DisplayMessageDialog(Resources.MatchRested, Resources.Info);
        }

        [RelayCommand]
        private void SetMatchWiner(ICompetitor comp)
        {
            if (comp == null)
                CurrentMatch.SetWinner(0);
            else if (comp.Equals(CurrentMatch.AKA))
                CurrentMatch.SetWinner(1);
            else if (comp.Equals(CurrentMatch.AO))
                CurrentMatch.SetWinner(2);
        }

        [RelayCommand]
        protected virtual void FinishMatch()
        {
            categoryViewer?.WriteMatchResults(currentMatchRound, CurrentMatch);
            if (CurrentMatch.Winner?.IsBye == false)
                AddInfoToLog($"{Resources.MatchWinner}: {CurrentMatch.Winner}");
            else
                AddInfoToLog($"{Resources.MatchTie}");
            Helpers.DisplayMessageDialog(Resources.MatchResultsSaved, Resources.Info);
        }

        [RelayCommand]
        private void LoadNextMatch()
        {
            categoryViewer?.LoadMatch(nextMatchRound, NextMatch);
        }

        [RelayCommand]
        private async Task OpenCategory()
        {
            if (categoryViewer != null)
            {
                var reopenCategory = await Helpers.DisplayQuestionDialog(Resources.OpenNewCategory, Resources.Open);
                if (reopenCategory != ModernWpf.Controls.ContentDialogResult.Primary)
                    return;

                (categoryViewer as CategoryViewer)?.Close();
            }
            var openCategoryDialog = new OpenCategoryDialog(dbService);
            var result = await openCategoryDialog.ShowAsync();
            if (result == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                categoryViewer = new CategoryViewer(dbService, openCategoryDialog.ResultCategory, false);
                categoryViewer.GotMatch += CategoryViewer_GotMatch;
                categoryViewer.GotNextMatch += CategoryViewer_GotNextMatch;
                categoryViewer.GotCategoryResults += CategoryViewer_GotCategoryResults;
                TieButtonVisibility = openCategoryDialog.ResultCategory.Type == 2 ? Visibility.Visible : Visibility.Collapsed;

                (categoryViewer as CategoryViewer).Show();
            }
        }

        protected virtual void CategoryViewer_GotMatch(RoundDTO round, IMatch match)
        {
            IsNameFieldReadOnly = true;
            currentMatchRound = round;
            CurrentMatch = match;
            SetupMatch(CurrentMatch);
            CurrentMatchAkaText = CurrentMatch.AKA?.ToString();
            CurrentMatchAoText = CurrentMatch.AO?.ToString();
            Helpers.DisplayMessageDialog(Resources.MatchLodaed, Resources.Info);
        }

        private void CategoryViewer_GotNextMatch(RoundDTO round, IMatch match)
        {
            nextMatchRound = round;
            NextMatch = match;
            NextMatchAkaText = NextMatch.AKA?.ToString();
            NextMatchAoText = NextMatch.AO?.ToString();
        }

        private void CategoryViewer_GotCategoryResults(IList<ICompetitor> winners)
        {

        }

        protected abstract void SetupMatch(IMatch match);

        protected async void Match_HaveWinner(ICompetitor winner)
        {
            endOfMatchSound?.Play();

            if (winner == null)
                await Helpers.DisplayMessageDialog(Resources.MatchEnded, Resources.Info);
            else
                await Helpers.DisplayMessageDialog($"{Resources.MatchWinner}: {winner}", Resources.Info);
        }

        [RelayCommand]
        private void SetupCompetitorName(object[] parameters)
        {
            if (currentCategory != null)
                return;

            ICompetitor competitor = parameters[0] as ICompetitor;
            string name = (string)parameters[1];

            var splitted = name.Split(' ', 2);

            if (splitted.Length > 0)
                competitor.FirstName = splitted[0];
            if (splitted.Length > 1)
                competitor.LastName = splitted[1];

            OnPropertyChanged(nameof(CurrentMatch));
        }

        [RelayCommand]
        private void SetCompetitorKIKEN(ICompetitor competitor)
        {
            competitor?.SetStatus(1);

            if (competitor != null)
                dbService.UpdateCompetitor(dbService.GetCompetitor(competitor.ID));

            if (competitor?.Equals(CurrentMatch?.AKA) == true)
                AddInfoToLog($"AKA {Resources.kiken}");
            else if (competitor?.Equals(CurrentMatch?.AO) == true)
                AddInfoToLog($"AO {Resources.kiken}");
        }

        [RelayCommand]
        private void SetCompetitorSHIKAKU(ICompetitor competitor)
        {
            competitor?.SetStatus(2);

            if (competitor != null)
                dbService.UpdateCompetitor(dbService.GetCompetitor(competitor.ID));

            if (competitor?.Equals(CurrentMatch?.AKA) == true)
                AddInfoToLog($"AKA {Resources.shikaku}");
            else if (competitor?.Equals(CurrentMatch?.AO) == true)
                AddInfoToLog($"AO {Resources.shikaku}");
        }

        [RelayCommand]
        protected async Task SaveLogFile()
        {
            try
            {
                TextRange range;
                System.IO.FileStream fStream;
                range = new TextRange(LoggerDocument.ContentStart, LoggerDocument.ContentEnd);
                if (Properties.Settings.Default.DataPath == "")
                {
                    SaveFileDialog saveFile = new SaveFileDialog();
                    saveFile.Filter = "txt file(*.txt) | *.txt";
                    if (saveFile.ShowDialog() == true)
                    {
                        fStream = new System.IO.FileStream(saveFile.FileName, System.IO.FileMode.Create);
                        range.Save(fStream, DataFormats.Text);
                        fStream.Close();
                    }
                }
                else
                {
                    fStream = new System.IO.FileStream($"{Properties.Settings.Default.DataPath}\\{currentMatch}.txt", System.IO.FileMode.Create);
                    range.Save(fStream, DataFormats.Text);
                    fStream.Close();
                }
                await Helpers.DisplayMessageDialog($"{Resources.LogFileSaved}", Resources.Info);

            }
            catch (Exception ex)
            {
                await Helpers.DisplayMessageDialog($"{Resources.SmthWentWrong}\n{ex.Message}", Resources.Error);
            }
        }

        [RelayCommand]
        private void ClearLog() => LoggerDocument.Blocks.Clear();

        protected void AddInfoToLog(string info)
        {
            LoggerDocument.Blocks.Add(new Paragraph(new Run($"{DateTime.Now}\n[{Resources._INFO}] {info}")));
        }
    }
}
