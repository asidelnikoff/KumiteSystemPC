using ModernWpf.Controls;
using SharedComponentsLibrary.DTO;
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

namespace Category_Generator
{
    /// <summary>
    /// Логика взаимодействия для AddCategoryDialog.xaml
    /// </summary>
    public partial class AddCategoryDialog : ContentDialog
    {
        public CategoryDTO ResultCategory;

        public AddCategoryDialog(CategoryDTO category)
        {
            InitializeComponent();

            DataContext = new AddCategoryDialogViewModel(category);
            ResultCategory = (DataContext as AddCategoryDialogViewModel).Category;
        }
    }
}
