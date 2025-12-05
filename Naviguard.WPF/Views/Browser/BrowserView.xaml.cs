// Naviguard.WPF/Views/Browser/BrowserView.xaml.cs
using CefSharp.Wpf;
using Naviguard.Domain.Entities;
using Naviguard.WPF.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CefSharp;

namespace Naviguard.WPF.Views.Browser
{
    public partial class BrowserView : UserControl
    {
        private BrowserViewModel? _viewModel;
        private Pagina? _currentPage;
        private (string Username, string Password)? _loginCredentials;
        private bool _isAutoLoginRunning = false;

        public BrowserView()
        {
            InitializeComponent();
            Debug.WriteLine("[BrowserView] Constructor llamado");
        }

        public async Task InitializeAsync(BrowserViewModel viewModel, Pagina page)
        {
            Debug.WriteLine($"[BrowserView] InitializeAsync llamado para: {page.PageName}");

            _viewModel = viewModel;
            _currentPage = page;
            DataContext = viewModel;

            if (BrowserControl is ChromiumWebBrowser browser)
            {
                Debug.WriteLine($"[BrowserView] ChromiumWebBrowser encontrado, inicializando...");

                // ✅ Suscribirse al evento FrameLoadEnd para auto-login
                browser.FrameLoadEnd += Browser_FrameLoadEnd;

                // ✅ Cargar credenciales ANTES de inicializar el navegador
                _loginCredentials = await viewModel.GetCredentialsForPageAsync(page);

                Debug.WriteLine($"[BrowserView] ¿Credenciales listas para usar? -> {(_loginCredentials.HasValue ? "Sí" : "No")}");

                // Ahora inicializar el navegador
                await viewModel.InitializeBrowserAsync(browser, page);
            }
            else
            {
                Debug.WriteLine($"[BrowserView] ERROR: ChromiumWebBrowser NO encontrado");
            }
        }

        // ✅ NUEVO: Evento para auto-login
        private void Browser_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                Debug.WriteLine($"[BrowserView] FrameLoadEnd para: {e.Url}. ¿Hay credenciales?: {(_loginCredentials.HasValue ? "Sí" : "No")}");

                if (_loginCredentials != null && !_isAutoLoginRunning)
                {
                    _isAutoLoginRunning = true;
                    Debug.WriteLine("[BrowserView] Ejecutando AutoLogin...");
                    ExecuteAutoLogin();
                }
            }
        }

        // ✅ NUEVO: Ejecutar auto-login
        private async void ExecuteAutoLogin()
        {
            if (!_loginCredentials.HasValue || BrowserControl == null) return;

            try
            {
                Debug.WriteLine($"[BrowserView] 💉 Inyectando login con Usuario: '{_loginCredentials.Value.Username}'");

                string script = $@"
                    (function() {{
                        try {{
                            var emailField = document.getElementById('txtemail');
                            var passwordField = document.getElementById('txtpas');
                            var characField = document.getElementById('txtcarac');
                            var codcaracField = document.getElementById('txtcodcarac');
                            var loginButton = document.querySelector('.btn_access');

                            if (emailField && passwordField && characField && codcaracField && loginButton) {{
                                emailField.value = '{_loginCredentials.Value.Username}';
                                passwordField.value = '{_loginCredentials.Value.Password}';
                                characField.value = codcaracField.value;
                                loginButton.click();
                                return 'Login ejecutado correctamente';
                            }} else {{
                                return 'Elementos del formulario no encontrados';
                            }}
                        }} catch(e) {{
                            return 'Error: ' + e.message;
                        }}
                    }})();
                ";

                Debug.WriteLine($"[BrowserView] 📋 Ejecutando script JS...");

                var response = await BrowserControl.GetMainFrame().EvaluateScriptAsync(script);

                if (response.Success)
                {
                    Debug.WriteLine($"[BrowserView] ✅ Script ejecutado: {response.Result}");
                }
                else
                {
                    Debug.WriteLine($"[BrowserView] ❌ Error en script: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] 💥 Error en ExecuteAutoLogin: {ex.Message}");
            }
            finally
            {
                _isAutoLoginRunning = false;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[BrowserView] Unloaded event");

            // ✅ Desuscribirse del evento
            if (BrowserControl != null)
            {
                BrowserControl.FrameLoadEnd -= Browser_FrameLoadEnd;
            }

            _viewModel?.Cleanup();
        }
    }
}