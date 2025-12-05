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
        private bool _loginExecuted = false;

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

                // Suscribirse a eventos
                browser.FrameLoadEnd += Browser_FrameLoadEnd;
                browser.LoadError += Browser_LoadError;

                // Cargar credenciales
                _loginCredentials = await viewModel.GetCredentialsForPageAsync(page);

                Debug.WriteLine($"[BrowserView] ¿Credenciales listas para usar? -> {(_loginCredentials.HasValue ? "Sí" : "No")}");

                // Inicializar el navegador
                await viewModel.InitializeBrowserAsync(browser, page);
            }
            else
            {
                Debug.WriteLine($"[BrowserView] ERROR: ChromiumWebBrowser NO encontrado");
            }
        }

        private void Browser_LoadError(object? sender, LoadErrorEventArgs e)
        {
            // ✅ Ignorar errores de navegación abortada (normal después de login)
            if (e.ErrorCode == CefErrorCode.Aborted)
            {
                Debug.WriteLine($"[BrowserView] ⚠️ Navegación abortada (normal): {e.FailedUrl}");
                return;
            }

            // ✅ Ignorar errores en frames que no son principales
            if (!e.Frame.IsMain)
            {
                return;
            }

            Debug.WriteLine($"[BrowserView] ❌ Error de carga: {e.ErrorText} ({e.ErrorCode}) en {e.FailedUrl}");
        }

        private void Browser_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain) return;

            Debug.WriteLine($"[BrowserView] FrameLoadEnd para: {e.Url}. ¿Hay credenciales?: {(_loginCredentials.HasValue ? "Sí" : "No")}");

            // Solo ejecutar en la página de login y si no se ha ejecutado antes
            if (_loginCredentials.HasValue &&
                !_isAutoLoginRunning &&
                !_loginExecuted &&
                e.Url.Contains("login.php"))
            {
                _isAutoLoginRunning = true;
                Debug.WriteLine("[BrowserView] Ejecutando AutoLogin...");

                // ✅ NO usar Dispatcher.InvokeAsync, usar Task.Run
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1500);
                    await ExecuteAutoLoginAsync();
                });
            }
        }

        private async Task ExecuteAutoLoginAsync()
        {
            if (!_loginCredentials.HasValue || BrowserControl == null)
            {
                _isAutoLoginRunning = false;
                return;
            }

            try
            {
                await Task.Delay(500);

                if (!BrowserControl.IsBrowserInitialized)
                {
                    Debug.WriteLine($"[BrowserView] Navegador no inicializado");
                    _isAutoLoginRunning = false;
                    return;
                }

                Debug.WriteLine($"[BrowserView] 💉 Inyectando credenciales...");

                string username = _loginCredentials.Value.Username.Replace("'", "\\'");
                string password = _loginCredentials.Value.Password.Replace("'", "\\'");

                string script = $@"
                    (function() {{
                        try {{
                            console.log('🔄 [Auto-Login] Iniciando...');
                            
                            var email = document.getElementById('txtemail');
                            var pass = document.getElementById('txtpas');
                            var carac = document.getElementById('txtcarac');
                            var codcarac = document.getElementById('txtcodcarac');
                            var btn = document.querySelector('.btn_access');
                            
                            if (!email || !pass || !carac || !codcarac || !btn) {{
                                console.error('❌ Elementos no encontrados');
                                return false;
                            }}

                            email.value = '{username}';
                            pass.value = '{password}';
                            carac.value = codcarac.value;
                            
                            console.log('✅ Campos llenados');
                            
                            // Disparar eventos
                            email.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            pass.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            carac.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            
                            // Click con delay
                            setTimeout(function() {{
                                btn.click();
                                console.log('✅ Click ejecutado');
                            }}, 300);
                            
                            return true;
                        }} catch(e) {{
                            console.error('💥 Error:', e);
                            return false;
                        }}
                    }})();
                ";

                var frame = BrowserControl.GetMainFrame();
                if (frame != null)
                {
                    var response = await frame.EvaluateScriptAsync(script);

                    if (response.Success && response.Result is bool result && result)
                    {
                        Debug.WriteLine($"[BrowserView] ✅ Auto-login ejecutado correctamente");
                        _loginExecuted = true;
                    }
                    else
                    {
                        Debug.WriteLine($"[BrowserView] ⚠️ Auto-login falló");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] ❌ Error: {ex.Message}");
            }
            finally
            {
                _isAutoLoginRunning = false;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[BrowserView] Unloaded event");

            if (BrowserControl != null)
            {
                BrowserControl.FrameLoadEnd -= Browser_FrameLoadEnd;
                BrowserControl.LoadError -= Browser_LoadError;
            }

            _viewModel?.Cleanup();
        }
    }
}