using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Category_Generator;
using SharedComponentsLibrary;
using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CompetitionSystem
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
            (DataContext as MainWindowViewModel).OnOpenSettings += OpenSettings;
            (DataContext as MainWindowViewModel).OnOpenCategoryViewer += OpenCategoryViewer;
            (DataContext as MainWindowViewModel).OnLoadLayout += LoadLayout;

            InitializeComponent();
        }

        private void LoadLayout(object content, string title)
        {
            var anchorable = new LayoutAnchorable();
            anchorable.CanHide = false;
            anchorable.CanAutoHide = false;

            anchorable.Content = content;
            anchorable.Title = title;

            anchorable.AddToLayout(dockManager, AnchorableShowStrategy.Most);
        }

        private void SaveLayout()
        {
            var serializer = new XmlLayoutSerializer(dockManager);
            using (var stream = new StreamWriter(string.Format(@".\{0}_layout.config", "kumiteSystem")))
                serializer.Serialize(stream);
        }

        private ICategoryViewer OpenCategoryViewer(DBService dbService, CategoryDTO category, bool isGenerationNeeded,
           bool shuffleCompetitors = false, bool isSwapCompetitorsEnabled = false)
        {
            var view = new CategoryViewerControl(new CategoryViewerViewModel(category, dbService, isGenerationNeeded, shuffleCompetitors, isSwapCompetitorsEnabled));
            var anchorable = new LayoutAnchorable()
            {
                Title = category.Name,
                Content = view
            };

            anchorable.CanAutoHide = false;

            view.Closed += () => { anchorable.Close(); };
            anchorable.Hiding += (sender, e) => { view.Close(); anchorable.Close(); };

            anchorable.AddToLayout(dockManager, AnchorableShowStrategy.Right);

            return view;
        }

        private ITimerBoard OpenTimerBoard()
        {

            var view = new TimerBoardControl(new TimerBoardViewModel());
            var anchorable = new LayoutAnchorable()
            {
                Title = "Timer board",
                Content = view
            };

            view.Closed += () => { anchorable.Close(); };
            anchorable.Hiding += (sender, e) => { view.Close(); anchorable.Close(); };

            anchorable.AddToLayout(dockManager, AnchorableShowStrategy.Most);
            anchorable.Float();

            return view;
        }

        private Settings OpenSettings()
        {
            var settings = new Settings();
            settings.Owner = this;

            return settings;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveLayout();
        }
    }
}
