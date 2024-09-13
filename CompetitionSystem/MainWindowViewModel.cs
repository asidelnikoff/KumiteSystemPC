using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedComponentsLibrary;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageLibrary;
using System.Threading;
using System.Globalization;
using KataSystem;

namespace CompetitionSystem
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        KataSystem.MainWindowViewModel kataSystemViewModel;

        [ObservableProperty]
        KumiteSystem.MainWindowViewModel kumiteSystemViewModel;

        [ObservableProperty]
        Category_Generator.MainViewModel categoryGeneratorViewModel;

        UserSettings userSettings;
        DBService dBService;

        public Func<Settings> OnOpenSettings;
        public Func<DBService, CategoryDTO, bool, bool, bool, ICategoryViewer> OnOpenCategoryViewer;
        public Func<ITimerBoard> OnOpenTimerBoard;

        public Action<object, string> OnLoadLayout;

        public MainWindowViewModel()
        {
            userSettings = UserSettings.GetUserSettings();

            var info = new CultureInfo(GetLanguage());
            Thread.CurrentThread.CurrentUICulture = info;
            Thread.CurrentThread.CurrentCulture = info;

            var task = SetupDbService(); 
        }

        private void SetupViewModels()
        {
            KataSystemViewModel = new KataSystem.MainWindowViewModel(dBService, userSettings);
            KataSystemViewModel.OnOpenCategoryViewer += (DBService dbService, CategoryDTO category, bool isGenerationNeeded,
            bool shuffleCompetitors, bool isSwapCompetitorsEnabled) => OnOpenCategoryViewer?.Invoke(dbService, category, isGenerationNeeded, shuffleCompetitors, isSwapCompetitorsEnabled);
            KataSystemViewModel.OnOpenTimerBoard += () => OnOpenTimerBoard?.Invoke();

            KumiteSystemViewModel = new KumiteSystem.MainWindowViewModel(dBService, userSettings);
            KumiteSystemViewModel.OnOpenCategoryViewer += (DBService dbService, CategoryDTO category, bool isGenerationNeeded,
            bool shuffleCompetitors, bool isSwapCompetitorsEnabled) => OnOpenCategoryViewer?.Invoke(dbService, category, isGenerationNeeded, shuffleCompetitors, isSwapCompetitorsEnabled);
            KumiteSystemViewModel.OnOpenTimerBoard += () => OnOpenTimerBoard?.Invoke();

            //CategoryGeneratorViewModel = new Category_Generator.MainViewModel(dBService, userSettings);
            //CategoryGeneratorViewModel.OnOpenCategoryViewer += (DBService dbService, CategoryDTO category, bool isGenerationNeeded,
            //bool shuffleCompetitors, bool isSwapCompetitorsEnabled) => OnOpenCategoryViewer?.Invoke(dbService, category, isGenerationNeeded, shuffleCompetitors, isSwapCompetitorsEnabled);

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

            this.dBService = new DBService(database);
            SetupViewModels();
        }

        [RelayCommand]
        private void LoadLayout(string layoutName)
        {
            switch (layoutName)
            {
                case "categoryGenerator":
                    OnLoadLayout?.Invoke(new Category_Generator.CategoryGeneratorControl(new Category_Generator.MainViewModel(dBService, userSettings)), "Category Generator");
                    break;
                case "kataSystem":
                    OnLoadLayout?.Invoke(new KataSystem.KataSystemControl(KataSystemViewModel), "Kata System");
                    break;
                case "kumiteSystem":
                    OnLoadLayout?.Invoke(new KumiteSystem.KumiteSystemControl(KumiteSystemViewModel), "Kumite System");
                    break;

            }
        }

        [RelayCommand]
        private void Close()
        {
            KataSystemViewModel.CloseCommand.Execute(this);
            KumiteSystemViewModel.CloseCommand.Execute(this);
        }

        [RelayCommand]
        private void OpenSettings()
        {
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
    }
}
