using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf.Controls;
using RoundRobin;
using SharedComponentsLibrary;
using SharedComponentsLibrary.DTO;
using SharedComponentsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using LanguageLibrary;
using System.Globalization;
using System.Threading;

namespace Category_Generator
{
    public partial class MainViewModel : ObservableObject
    {

        [ObservableProperty]
        ICollectionView competitors;

        [ObservableProperty]
        CompetitorDTO selectedCompetitor;

        [ObservableProperty]
        Visibility addCompetitorToCategoryButtonVisibility;

        [ObservableProperty]
        Visibility competitorContextMenuVisibility;

        [ObservableProperty]
        ObservableCollection<Tournament> tournaments;

        [ObservableProperty]
        Tournament selectedTournament;

        [ObservableProperty]
        string filter;

        [ObservableProperty]
        bool isAddCategoryButtonEnabled;

        [ObservableProperty]
        ObservableCollection<CategoryDTO> categories;

        [ObservableProperty]
        CategoryDTO selectedCategory;

        [ObservableProperty]
        ObservableCollection<CompetitorDTO> competitorsInCategory;

        [ObservableProperty]
        ObservableCollection<string> categoryTypes;

        [ObservableProperty]
        Visibility categoriesContextMenuVisibility;

        [ObservableProperty]
        Visibility viewCategoryButtonVisibility;

        [ObservableProperty]
        bool isGenerateCategoryButtonEnabled;

        DBService dbService;

        ObservableCollection<CompetitorDTO> competitorsOC;

        UserSettings userSettings;

        public Func<Settings> OnOpenSettings;
        public Func<DBService, CategoryDTO, bool, bool, bool, ICategoryViewer> OnOpenCategoryViewer;


        public MainViewModel()
        {
            LoadSettings();
            SetupDbService();
            MainSetup();
        }

        public MainViewModel(DBService dbService, UserSettings settings)
        {
            this.dbService = dbService;
            this.userSettings = settings;

            MainSetup();
        }

        private void MainSetup()
        {
            try
            {
                SetCompetitors();
                Competitors.Filter = new Predicate<object>(CheckFilter);

                SetTournaments();

                SelectedCompetitor = null;
                CompetitorContextMenuVisibility = Visibility.Collapsed;
                CategoriesContextMenuVisibility = Visibility.Collapsed;
                AddCompetitorToCategoryButtonVisibility = Visibility.Collapsed;
                ViewCategoryButtonVisibility = Visibility.Collapsed;
                IsGenerateCategoryButtonEnabled = false;

                CategoryTypes = new ObservableCollection<string>(CategoryDTO.CategoryTypes.Values);

                PropertyChanged += MainViewModel_PropertyChanged;
            }
            catch { }
        }

        private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedCompetitor))
                CompetitorContextMenuVisibility = SelectedCompetitor == null ? Visibility.Collapsed : Visibility.Visible;
            if (e.PropertyName == nameof(SelectedCategory))
            {
                AddCompetitorToCategoryButtonVisibility = SelectedCategory != null ? Visibility.Visible : Visibility.Collapsed;
                if (SelectedCategory != null)
                {
                    ViewCategoryButtonVisibility = dbService.IsCategoryGenerated(SelectedCategory) ? Visibility.Visible : Visibility.Collapsed;
                    IsGenerateCategoryButtonEnabled = dbService.GetCompetitorsInCategory(SelectedCategory).Any();
                }
                else
                    IsGenerateCategoryButtonEnabled = false;
                SetCompetitorsInCategory();
            }
            if (e.PropertyName == nameof(SelectedTournament))
            {
                IsAddCategoryButtonEnabled = SelectedTournament != null;
                SetCategories();
            }
            if (e.PropertyName == nameof(Filter))
                Competitors.Refresh();
        }

        private void LoadSettings()
        {
            userSettings = UserSettings.GetUserSettings();

            var info = new CultureInfo(GetLanguage());
            Thread.CurrentThread.CurrentUICulture = info;
            Thread.CurrentThread.CurrentCulture = info;
            /*var setup = new Settings(null);
            var settings = setup.LoadSettings();
            Settings_SaveSettings(settings);
            setup.Close();*/
        }

        private async void SetupDbService()
        {
            string database = userSettings.DatabasePath;
            while (String.IsNullOrEmpty(userSettings.DataPath))
            {
                await Helpers.DisplayMessageDialog(Resources.ChooseDefaultDataPath, Resources.Error);
                OpenSettings();
            }
            if (String.IsNullOrEmpty(database))
            {
                database = userSettings.DataPath + @"\tournaments.sqlite";
                userSettings.DatabasePath = database;
                userSettings.Save();
            }

            dbService = new DBService(database);
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
            //var settings = new Settings();
            var settings = OnOpenSettings?.Invoke();
            if (settings != null)
            {
                settings.SaveSettings += Settings_SaveSettings;
                settings.ShowDialog();
            }
            //settings.ShowDialog();
        }
        private void Settings_SaveSettings(UserSettings settings)
        {
            userSettings = settings;
        }

        public string GetLanguage() => userSettings.Language.CultureInfo;

        private bool CheckFilter(object obj)
        {
            if (String.IsNullOrEmpty(Filter))
                return true;
            var competitor = (CompetitorDTO)obj;
            var str = $"{competitor.FirstName} {competitor.LastName}";
            return str.Contains(Filter);
        }

        private void SetCompetitors()
        {
            competitorsOC = new ObservableCollection<CompetitorDTO>(dbService.GetCompetitors());
            var collectionVeiwSource = new CollectionViewSource() { Source = competitorsOC };
            Competitors = collectionVeiwSource.View;
            Competitors.Refresh();
        }

        private void SetTournaments()
        {
            Tournaments = new ObservableCollection<Tournament>(dbService.GetTournaments());
        }

        private void SetCategories()
        {
            Categories = new ObservableCollection<CategoryDTO>(dbService.GetCategoriesInTournament(SelectedTournament));
            CategoriesContextMenuVisibility = Categories?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetCompetitorsInCategory()
        {
            CompetitorsInCategory = new ObservableCollection<CompetitorDTO>(dbService.GetCompetitorsInCategory(SelectedCategory));
        }

        [RelayCommand]
        private void Close()
        {
            
        }

        [RelayCommand]
        private void AddCompetitorToCategory()
        {
            if (SelectedCategory == null || SelectedCompetitor == null || SelectedCompetitor.Id == - 1)
                return;

            try
            {
                dbService.AddCompetitorToCategory(SelectedCompetitor, SelectedCategory);
                SetCompetitorsInCategory();
                OnPropertyChanged(nameof(SelectedCategory));
            }
            catch (ArgumentException ex)
            {
                Helpers.DisplayMessageDialog(Resources.ThisCompetitorIsAlreadyInCategory, Resources.Error);
            }

        }

        [RelayCommand]
        private void RemoveCompetitorFromCategory(CompetitorDTO competitor)
        {
            dbService.RemoveCompetitorFromCategory(competitor, SelectedCategory);
            SetCompetitorsInCategory();
            OnPropertyChanged(nameof(SelectedCategory));
        }

        [RelayCommand]
        private async Task AddCategory()
        {
            var categoryDialog = new AddCategoryDialog(null);
            if (await categoryDialog.ShowAsync() == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var category = categoryDialog.ResultCategory;
                category.Tournament = SelectedTournament.Id;
                dbService.AddCategory(category);
                SetCategories();
            }
        }

        [RelayCommand]
        private async Task EditCategory()
        {
            var categoryDialog = new AddCategoryDialog(SelectedCategory);
            if (await categoryDialog.ShowAsync() == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var category = categoryDialog.ResultCategory;
                category.Tournament = SelectedTournament.Id;
                category.Id = SelectedCategory.Id;
                dbService.UpdateCategory(category);
                SetCategories();
                SelectedCategory = Categories.Where(a => a.Id == category.Id).FirstOrDefault();
            }
        }

        [RelayCommand]
        private void DeleteCategory()
        {
            dbService.RemoveCategory(SelectedCategory);
            SetCategories();
        }

        [RelayCommand]
        private async Task GenerateCategory()
        {
            MyContentDialog regenerateCategoryDialog = new MyContentDialog()
            {
                Content = Resources.YouHaveDataInThisCategory,
                PrimaryButtonText = Resources.Regenerate,
                SecondaryButtonText = Resources.Cancel
            };
            if (dbService.IsCategoryGenerated(SelectedCategory)
                && await regenerateCategoryDialog.ShowAsync() != ContentDialogResult.Primary)
                return;

            MyContentDialog shuffleCompetitorsDialog = new MyContentDialog()
            {
                Content = Resources.DoYouWantToShuffleCompetitors,
                PrimaryButtonText = Resources.Shuffle,
                SecondaryButtonText = Resources.WithoutShuffle
            };
            bool shuffleCompetitors = false;
            var dialogResult = await shuffleCompetitorsDialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
                shuffleCompetitors = true;
            else if (dialogResult == ContentDialogResult.None)
                return;

            OnOpenCategoryViewer?.Invoke(dbService, SelectedCategory, true, shuffleCompetitors, true);
        }

        [RelayCommand]
        private void ViewCategory()
        {
            OnOpenCategoryViewer?.Invoke(dbService, SelectedCategory, false, false, true);
        }

        [RelayCommand]
        private async Task AddTournament()
        {
            var tournamentDialog = new AddTournamentDialog(null);
            if(await tournamentDialog.ShowAsync() == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var tournament = tournamentDialog.ResultTournament;
                dbService.AddTournament(tournament);
                SetTournaments();
            }
        }

        [RelayCommand]
        private async Task EditTournament(Tournament tournament)
        {
            var tournamentDialog = new AddTournamentDialog(tournament);
            if(await tournamentDialog.ShowAsync() == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var resultTournament = tournamentDialog.ResultTournament;
                tournament.Name = resultTournament.Name;
                if (SelectedTournament.Id == tournament.Id)
                    SelectedTournament.Name = tournament.Name;
                dbService.UpdateTournament(tournament);
                SetTournaments();
            }
        }

        [RelayCommand]
        private void DeleteTournament(Tournament tournament)
        {
            dbService.RemoveTournament(tournament);
            Tournaments.Remove(tournament);
        }

        [RelayCommand]
        private async Task AddCompetitor()
        {
            var competitorDialog = new AddCompetitorDialog(null);
            if(await competitorDialog.ShowAsync() == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var competitor = competitorDialog.ResultCompetitor;
                dbService.AddCompetitor(competitor);
                SetCompetitors();
            }
        }

        [RelayCommand]
        private async Task EditCompetitor()
        {
            var competitorDialog = new AddCompetitorDialog(SelectedCompetitor);
            if (await competitorDialog.ShowAsync() == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var competitor = competitorDialog.ResultCompetitor;
                SelectedCompetitor.FirstName = competitor.FirstName;
                SelectedCompetitor.LastName = competitor.LastName;
                SelectedCompetitor.Club = competitor.Club;
                dbService.UpdateCompetitor(SelectedCompetitor);
                Competitors.Refresh();
            }
        }

        [RelayCommand]
        private async Task DeleteCompetitor()
        {
            try
            {
                dbService.RemoveCompetitor(SelectedCompetitor);
                competitorsOC.Remove(SelectedCompetitor);
            }
            catch(ArgumentException ex)
            {
                Helpers.DisplayMessageDialog($"{ex.Message}", Resources.Error);
            }
        }
    }
}
