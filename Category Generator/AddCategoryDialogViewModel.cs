using CommunityToolkit.Mvvm.ComponentModel;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Category_Generator
{
    public partial class AddCategoryDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        CategoryDTO category;

        [ObservableProperty]
        string categoryName;

        [ObservableProperty]
        long selectedCategoryType;

        [ObservableProperty]
        ObservableCollection<string> categoryTypes;

        [ObservableProperty]
        bool isPrimaryButtonEnabled;

        public AddCategoryDialogViewModel(CategoryDTO category)
        {
            Category = new CategoryDTO() { Type = 0, Name = "" };
            if(category != null)
            {
                Category.Name = category.Name;
                Category.Type = category.Type;
            }

            CategoryTypes = new ObservableCollection<string>(CategoryDTO.CategoryTypes.Values);
            SelectedCategoryType = Category.Type;
            CategoryName = Category.Name;

            PropertyChanged += AddCategoryDialogViewModel_PropertyChanged;
        }

        private void AddCategoryDialogViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Category.Name = CategoryName == null ? "" : CategoryName.Trim();
            Category.Type = SelectedCategoryType;
            IsPrimaryButtonEnabled = Category.Name.Length > 0 && CategoryDTO.CategoryTypes.ContainsKey(Category.Type);
        }
    }
}
