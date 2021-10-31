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
using TournamentTree;

namespace KumiteSystemPC
{
    /// <summary>
    /// Логика взаимодействия для AddCompetitorDialog.xaml
    /// </summary>
    public partial class AddCompetitorDialog : Window
    {
        public AddCompetitorDialog()
        {
            InitializeComponent();
        }
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void addComp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ID = Convert.ToInt32(idTXT.Text);
                FirstName = firstNameTXT.Text;
                LastName = lastNameTXT.Text;
                this.DialogResult = true;
            }
            catch { }

        }


    }
}
