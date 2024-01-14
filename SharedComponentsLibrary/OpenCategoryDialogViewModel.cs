using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary.DTO;
using SharedComponentsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary
{
    public partial class OpenCategoryDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<Tournament> tournaments;

        [ObservableProperty]
        Tournament selectedTournament;

        [ObservableProperty]
        ObservableCollection<CategoryDTO> categoriesInTournament;

        [ObservableProperty]
        CategoryDTO selectedCategory;

        [ObservableProperty]
        bool isOpenButtonEnabled;

        public Action<CategoryDTO> GotResultCategory;

        DBService dbService;

        public OpenCategoryDialogViewModel(DBService _dbService)
        {
            dbService = _dbService;

            Tournaments = new ObservableCollection<Tournament>(dbService.GetTournaments());
            IsOpenButtonEnabled = false;

            PropertyChanged += OpenCategoryDialogViewModel_PropertyChanged;
        }

        private void OpenCategoryDialogViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedCategory))
            {
                IsOpenButtonEnabled = SelectedCategory != null;
                GotResultCategory?.Invoke(SelectedCategory);
            }
            if (e.PropertyName == nameof(SelectedTournament))
                CategoriesInTournament = new ObservableCollection<CategoryDTO>(dbService.GetGeneratedCategoriesInTournament(SelectedTournament.Id));
        }


    }
}
