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
        private bool _loginExecuted = false; // ✅ Evitar múltiples ejecuciones

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

                // ✅ Suscribirse a eventos
                browser.FrameLoadEnd += Browser_FrameLoadEnd;
                browser.LoadError += Browser_LoadError; // ✅ NUEVO: Detectar errores

                // ✅ Cargar credenciales
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

        // ✅ NUEVO: Detectar errores de carga
        private void Browser_LoadError(object? sender, LoadErrorEventArgs e)
        {
            if (e.ErrorCode == CefErrorCode.Aborted)
            {
                Debug.WriteLine($"[BrowserView] ⚠️ Navegación abortada (normal después de login): {e.FailedUrl}");
                return; // Esto es normal después de hacer clic en login
            }

            Debug.WriteLine($"[BrowserView] ❌ Error de carga: {e.ErrorText} ({e.ErrorCode}) en {e.FailedUrl}");
        }

        private void Browser_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain) return;

            Debug.WriteLine($"[BrowserView] FrameLoadEnd para: {e.Url}. ¿Hay credenciales?: {(_loginCredentials.HasValue ? "Sí" : "No")}");

            // ✅ Solo ejecutar si hay credenciales, no se ha ejecutado y estamos en la página de login
            if (_loginCredentials.HasValue &&
                !_isAutoLoginRunning &&
                !_loginExecuted &&
                e.Url.Contains("login.php")) // ✅ Verificar que estamos en la página de login
            {
                _isAutoLoginRunning = true;
                Debug.WriteLine("[BrowserView] Ejecutando AutoLogin...");

                Dispatcher.InvokeAsync(async () =>
                {
                    await Task.Delay(1500); // ✅ Aumentar delay a 1.5 segundos
                    await ExecuteAutoLoginAsync();
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private async Task ExecuteAutoLoginAsync()
        {
            if (!_loginCredentials.HasValue || BrowserControl == null)
            {
                _isAutoLoginRunning = false;
                return;
            }

            int maxAttempts = 3;
            int currentAttempt = 0;

            while (currentAttempt < maxAttempts)
            {
                currentAttempt++;

                try
                {
                    await Task.Delay(500 * currentAttempt);

                    if (!BrowserControl.IsBrowserInitialized)
                    {
                        Debug.WriteLine($"[BrowserView] Intento {currentAttempt}/{maxAttempts}: Navegador no listo");
                        continue;
                    }

                    Debug.WriteLine($"[BrowserView] Intento {currentAttempt}/{maxAttempts}: Ejecutando auto-login...");

                    string username = _loginCredentials.Value.Username.Replace("'", "\\'");
                    string password = _loginCredentials.Value.Password.Replace("'", "\\'");

                    // ✅ Script mejorado con validaciones
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
                                    console.error('❌ [Auto-Login] Elementos no encontrados');
                                    return false;
                                }}

                                // Llenar campos
                                email.value = '{username}';
                                pass.value = '{password}';
                                carac.value = codcarac.value;
                                
                                console.log('✅ [Auto-Login] Campos llenados');
                                
                                // ✅ Disparar eventos de cambio (algunos formularios lo requieren)
                                email.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                pass.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                carac.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                
                                // Hacer clic después de un delay
                                setTimeout(function() {{
                                    btn.click();
                                    console.log('✅ [Auto-Login] Click ejecutado');
                                }}, 200);
                                
                                return true;
                            }} catch(e) {{
                                console.error('💥 [Auto-Login] Error:', e);
                                return false;
                            }}
                        }})();
                    ";

                    var frame = BrowserControl.GetMainFrame();
                    if (frame == null)
                    {
                        Debug.WriteLine($"[BrowserView] Intento {currentAttempt}/{maxAttempts}: Frame no disponible");
                        continue;
                    }

                    var response = await frame.EvaluateScriptAsync(script);

                    if (response.Success && response.Result is bool result && result)
                    {
                        Debug.WriteLine($"[BrowserView] ✅ Auto-login exitoso en intento {currentAttempt}");
                        _loginExecuted = true; // ✅ Marcar como ejecutado
                        break;
                    }
                    else
                    {
                        Debug.WriteLine($"[BrowserView] ⚠️ Intento {currentAttempt} falló, reintentando...");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BrowserView] ❌ Error en intento {currentAttempt}: {ex.Message}");

                    if (currentAttempt >= maxAttempts)
                    {
                        Debug.WriteLine($"[BrowserView] 💥 Todos los intentos fallaron");
                    }
                }
            }

            _isAutoLoginRunning = false;
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