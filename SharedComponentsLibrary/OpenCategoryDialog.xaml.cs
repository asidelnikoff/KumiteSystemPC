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

namespace SharedComponentsLibrary
{
    /// <summary>
    /// Логика взаимодействия для OpenCategoryDialog.xaml
    /// </summary>
    public partial class OpenCategoryDialog : ContentDialog
    {
        public CategoryDTO ResultCategory { get; private set; }
        public OpenCategoryDialog(DBService dbService)
        {
            InitializeComponent();

            DataContext = new OpenCategoryDialogViewModel(dbService);
            (DataContext as OpenCategoryDialogViewModel).GotResultCategory += SetResultCategory;
        }

        private void SetResultCategory(CategoryDTO category)
        {
            ResultCategory = category;
        }
    }
}
