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
using System.Globalization;
using System.Threading;

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

        protected ITimerBoard timerBoard;

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

        protected UserSettings userSettings;

        public Func<Settings> OnOpenSettings;
        public Func<DBService, CategoryDTO, bool, bool, bool, ICategoryViewer> OnOpenCategoryViewer;
        public Func<ITimerBoard> OnOpenTimerBoard;

        public InternalBoardViewModel(DBService dbService, UserSettings settings)
        {
            this.dbService = dbService;
            this.userSettings = settings;

            if (userSettings.EndOfMatchSound != "")
                endOfMatchSound = new System.Media.SoundPlayer(userSettings.EndOfMatchSound);

            IsNextMatchButtonEnabled = userSettings.IsAutoLoadNextMatchEnabled;
            IsNameFieldReadOnly = false;

            LoggerDocument = new FlowDocument();

            CurrentMatch = new TournamentTree.Match(new TournamentTree.Competitor(false, -1, "AKA"),
                new TournamentTree.Competitor(false, -2, "AO"), -1);
            CurrentMatchAkaText = CurrentMatch.AKA?.ToString();
            CurrentMatchAoText = CurrentMatch.AO?.ToString();

            SetupMatch(CurrentMatch);
        }

        public InternalBoardViewModel()
        {
            userSettings = UserSettings.GetUserSettings();

            var info = new CultureInfo(GetLanguage());
            Thread.CurrentThread.CurrentUICulture = info;
            Thread.CurrentThread.CurrentCulture = info;

            if (userSettings.EndOfMatchSound != "")
                endOfMatchSound = new System.Media.SoundPlayer(userSettings.EndOfMatchSound);

            IsNextMatchButtonEnabled = userSettings.IsAutoLoadNextMatchEnabled;
            IsNameFieldReadOnly = false;

            LoggerDocument = new FlowDocument();

            CurrentMatch = new TournamentTree.Match(new TournamentTree.Competitor(false, -1, "AKA"), 
                new TournamentTree.Competitor(false, -2, "AO"), -1);
            CurrentMatchAkaText = CurrentMatch.AKA?.ToString();
            CurrentMatchAoText = CurrentMatch.AO?.ToString();

            SetupMatch(CurrentMatch);
        }



        protected async Task SetupDbService()
        {
            string database = userSettings.DatabasePath;
            while (String.IsNullOrEmpty(userSettings.DataPath))
            {
                await Helpers.DisplayMessageDialog(Resources.ChooseDefaultDataPath, Resources.Info);
                OpenSettings();
            }
            if (String.IsNullOrEmpty(database))
            {
                database = userSettings.DataPath + @"\tournaments.sqlite";
                userSettings.DatabasePath = database;
                userSettings.Save();
            }

            this.dbService = new DBService(database);
        }

        [RelayCommand]
        private void OpenSettings()
        {
            /*Settings settings = new Settings(new UserSettings()
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
            });*/
            var settings = OnOpenSettings?.Invoke();
            if (settings != null)
            {
                settings.SaveSettings += Settings_SaveSettings;
                settings.ShowDialog();
            }
        }
        protected void Settings_SaveSettings(UserSettings settings)
        {
            userSettings = settings;
        }

        public string GetLanguage() => userSettings.Language.CultureInfo;

        protected void Close()
        {
            timerBoard?.Close();
            categoryViewer?.Close();
        }

        [RelayCommand]
        private void OpenTimerBoard()
        {
            if (IsTimerBoardOpened)
                timerBoard?.Close();
            else
            {
                var board = OnOpenTimerBoard?.Invoke();
                if (board != null)
                {
                    timerBoard = board;
                    IsTimerBoardOpened = true;
                    timerBoard.Closed += () => IsTimerBoardOpened = false;
                }
            }
        }

        protected async void ResetMatch()
        {
            CurrentMatch.Reset();
            ClearLog();
            OnPropertyChanged(nameof(CurrentMatch));
            ResetExternalBoardState();
            await Helpers.DisplayMessageDialog(Resources.MatchRested, Resources.Info);
        }

        protected abstract void ResetExternalBoardState();

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

        protected abstract void ShowCategoryNameOnExternalBoard(string name);

        [RelayCommand]
        private async Task OpenCategory()
        {
            if (currentCategory != null)
            {
                var reopenCategory = await Helpers.DisplayQuestionDialog(Resources.OpenNewCategory, Resources.Open);
                if (reopenCategory != ModernWpf.Controls.ContentDialogResult.Primary)
                    return;

                categoryViewer?.Close();
            }
            var openCategoryDialog = new OpenCategoryDialog(dbService);
            var result = await openCategoryDialog.ShowAsync();
            if (result == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var viewer = OnOpenCategoryViewer?.Invoke(dbService, openCategoryDialog.ResultCategory, false, false, false);
                if (viewer != null)
                {
                    categoryViewer = viewer;
                    categoryViewer.GotMatch += CategoryViewer_GotMatch;
                    categoryViewer.GotNextMatch += CategoryViewer_GotNextMatch;
                    categoryViewer.GotCategoryResults += CategoryViewer_GotCategoryResults;

                    categoryViewer.Closed += () => { currentCategory = null; ShowCategoryNameOnExternalBoard(""); };

                    TieButtonVisibility = openCategoryDialog.ResultCategory.Type == 2 ? Visibility.Visible : Visibility.Collapsed;
                    currentCategory = openCategoryDialog.ResultCategory;
                    ShowCategoryNameOnExternalBoard(currentCategory.Name);
                }
            }
        }

        protected virtual void CategoryViewer_GotMatch(RoundDTO round, IMatch match)
        {
            IsNameFieldReadOnly = true;
            currentMatchRound = round;
            CurrentMatch.HaveWinner -= Match_HaveWinner;
            CurrentMatch = match;
            SetupMatch(CurrentMatch);
            CurrentMatchAkaText = CurrentMatch.AKA?.ToString();
            CurrentMatchAoText = CurrentMatch.AO?.ToString();

            ClearLog();
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

        protected void SetupMatch(IMatch match)
        {
            match.HaveWinner += Match_HaveWinner;
        }

        protected abstract void ShowWinnerOnExternalBoard(ICompetitor winner);

        protected async void Match_HaveWinner(ICompetitor winner)
        {
            endOfMatchSound?.Play();
            ShowWinnerOnExternalBoard(winner);
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
                if (userSettings.DataPath == "")
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
                    fStream = new System.IO.FileStream($"{userSettings.DataPath}\\{currentMatch}.txt", System.IO.FileMode.Create);
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
