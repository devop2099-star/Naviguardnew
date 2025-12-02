using Naviguard.WPF.ViewModels;
using System.Windows;

namespace Naviguard.WPF.Views.Users
{
    /// <summary>
    /// Lógica de interacción para CredentialsUserPage.xaml
    /// </summary>
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
    }
}
