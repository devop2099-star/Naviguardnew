using Naviguard.WPF.ViewModels;
using System.Windows;

namespace Naviguard.WPF.Views.Users
{
    public partial class CredentialsUserPage : Window
    {
        public CredentialsUserPage()
        {
            InitializeComponent();
        }

        public CredentialsUserPage(CredentialsUserPageViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        // ✅ Método que faltaba
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}