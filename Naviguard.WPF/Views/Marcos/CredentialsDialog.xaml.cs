// Naviguard.WPF/Views/Marcos/CredentialsDialog.xaml.cs
using System.Windows;

namespace Naviguard.WPF.Views.Marcos
{
    /// <summary>
    /// Diálogo para solicitar credenciales durante la reproducción de macros.
    /// </summary>
    public partial class CredentialsDialog : Window
    {
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public bool WasCancelled { get; private set; } = true;

        public CredentialsDialog(string siteName)
        {
            InitializeComponent();
            TxtSiteName.Text = $"Para el sitio: {siteName}";
            TxtUsername.Focus();

            // Permitir mover la ventana
            this.MouseLeftButtonDown += (s, e) => { if (e.ClickCount == 1) DragMove(); };
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                MessageBox.Show("Por favor ingresa un usuario.", 
                    "Campo Requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(TxtPassword.Password))
            {
                MessageBox.Show("Por favor ingresa una contraseña.", 
                    "Campo Requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPassword.Focus();
                return;
            }

            Username = TxtUsername.Text;
            Password = TxtPassword.Password;
            WasCancelled = false;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            WasCancelled = true;
            DialogResult = false;
            Close();
        }
    }
}
