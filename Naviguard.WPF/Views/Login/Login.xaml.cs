// Naviguard.WPF/Views/Login/Login.xaml.cs
using Naviguard.Infrastructure.ExternalServices;
using Naviguard.WPF.Services;
using Naviguard.WPF.Views;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Naviguard.Domain.Interfaces;

namespace Naviguard.WPF.Views.Login
{
    public partial class Login : Window
    {
        private readonly ApiClient _apiClient;
        private readonly IAuthRepository _authRepository;

        public Login()
        {
            InitializeComponent();

            // Obtener servicios del contenedor DI
            _apiClient = App.GetService<ApiClient>();
            _authRepository = App.GetService<IAuthRepository>();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(btnLogin, new RoutedEventArgs());
            }
        }

        private void txtUsername_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            placeholderUsername.Visibility = string.IsNullOrEmpty(txtUsername.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            placeholderPassword.Visibility = string.IsNullOrEmpty(txtPassword.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            borderUsername.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D7"));
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            borderUsername.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            borderPassword.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D7"));
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            borderPassword.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            btnLogin.IsEnabled = false;

            var loginResult = await _apiClient.LoginAsync(username, password);

            if (loginResult.Success)
            {
                try
                {
                    UserSession.StartSession(loginResult.UserId, loginResult.Username);

                    bool userHasAdminAccess = await _authRepository.UserHasRolesAsync(loginResult.UserId);

                    var menu = App.GetService<MenuMain>();
                    menu.SetAdminAccess(userHasAdminAccess);
                    menu.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al verificar sus permisos: {ex.Message}",
                        "Error de Autorización");
                    UserSession.EndSession();
                    btnLogin.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Credenciales inválidas", "Error de Login");
                btnLogin.IsEnabled = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}