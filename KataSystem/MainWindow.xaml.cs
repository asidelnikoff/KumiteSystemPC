using SharedComponentsLibrary;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KataSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            (DataContext as MainWindowViewModel).OnOpenTimerBoard += OpenTimerBoard;
            (DataContext as MainWindowViewModel).OnOpenCategoryViewer += OpenCategoryViewer;
            (DataContext as MainWindowViewModel).OnOpenSettings += OpenSettings;

            InitializeComponent();
            
        }

        private ICategoryViewer OpenCategoryViewer(DBService dbService, CategoryDTO category, bool isGenerationNeeded,
            bool shuffleCompetitors = false, bool isSwapCompetitorsEnabled = false)
        {
            var categoryViewer = new CategoryViewer(dbService, category, isGenerationNeeded, shuffleCompetitors, isSwapCompetitorsEnabled);
            categoryViewer.Owner = this;
            categoryViewer.Show();
            
            return categoryViewer;
        }

        private ITimerBoard OpenTimerBoard()
        {
            var board = new TimerBoard();
            board.Owner = this;
            board.Show();

            return board;
        }

        private Settings OpenSettings()
        {
            var settings = new Settings();
            settings.Owner = this;

            return settings;
        }
    }
}
