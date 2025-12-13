// Naviguard.WPF/Views/Browser/BrowserView.xaml.cs
using CefSharp.Wpf;
using Naviguard.Domain.Entities;
using Naviguard.WPF.ViewModels;
using Naviguard.WPF.Services; // Necesario para UserSession
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using System.IO;

namespace Naviguard.WPF.Views.Browser
{
    public partial class BrowserView : UserControl
    {
        private BrowserViewModel? _viewModel;
        private Pagina? _currentPage;
        private (string Username, string Password)? _loginCredentials;
        private bool _isAutoLoginRunning = false;
        private bool _loginExecuted = false;
        
        // Mantener referencia al navegador creado dinámicamente
        private ChromiumWebBrowser? _browser;

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

            // ✅ 1. Crear instancia dinámica del navegador
            _browser = new ChromiumWebBrowser();

            // ✅ 2. Configurar aislamiento de sesión por Usuario
            try
            {
                if (UserSession.IsLoggedIn)
                {
                    long userId = UserSession.ApiUserId;
                    string userCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", $"user_{userId}");
                    
                    if (!Directory.Exists(userCachePath))
                    {
                        Directory.CreateDirectory(userCachePath);
                    }

                    Debug.WriteLine($"[BrowserView] 🛡️ Configurando aislamiento de sesión en: {userCachePath}");

                    var requestContextSettings = new RequestContextSettings
                    {
                        CachePath = userCachePath,
                        PersistSessionCookies = true
                    };

                    _browser.RequestContext = new RequestContext(requestContextSettings);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] ⚠️ Error al configurar contexto de usuario: {ex.Message}");
                // Si falla, usará el contexto global por defecto
            }

            // ✅ 3. Configurar opciones del navegador
            _browser.BrowserSettings = new BrowserSettings
            {
                Javascript = CefState.Enabled,
                JavascriptAccessClipboard = CefState.Enabled,
                JavascriptCloseWindows = CefState.Disabled,
                LocalStorage = CefState.Enabled,
                Databases = CefState.Enabled,
                ImageLoading = CefState.Enabled
            };

            // ✅ 4. Agregar al contenedor visual
            BrowserContainer.Content = _browser;

            // Suscribirse a eventos
            _browser.FrameLoadEnd += Browser_FrameLoadEnd;
            _browser.FrameLoadStart += Browser_FrameLoadStart;
            _browser.LoadError += Browser_LoadError;

            // Cargar credenciales
            _loginCredentials = await viewModel.GetCredentialsForPageAsync(page);

            Debug.WriteLine($"[BrowserView] ¿Credenciales listas para usar? -> {(_loginCredentials.HasValue ? "Sí" : "No")}");

            // Inicializar el navegador en el ViewModel
            await viewModel.InitializeBrowserAsync(_browser, page);
        }

        // ✅ Nuevo método para detectar inicio de navegación
        private void Browser_FrameLoadStart(object? sender, FrameLoadStartEventArgs e)
        {
            try
            {
                if (e?.Frame == null || !e.Frame.IsMain) return;

                Debug.WriteLine($"[BrowserView] FrameLoadStart: {e.Url}");

                // ✅ Si detectamos que se está saliendo del login, marcar como completado
                if (_loginExecuted && e.Url.Contains("login.php") == false)
                {
                    Debug.WriteLine($"[BrowserView] 🎉 Navegación POST-login detectada");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] ⚠️ Error en FrameLoadStart: {ex.Message}");
            }
        }

        private void Browser_LoadError(object? sender, LoadErrorEventArgs e)
        {
            // ✅ Ignorar errores comunes que no son críticos
            if (e.ErrorCode == CefErrorCode.Aborted ||
                e.ErrorCode == CefErrorCode.None)
            {
                Debug.WriteLine($"[BrowserView] ⚠️ Error no crítico ignorado: {e.ErrorCode}");
                return;
            }

            if (!e.Frame.IsMain)
            {
                return;
            }

            Debug.WriteLine($"[BrowserView] ❌ Error de carga: {e.ErrorText} ({e.ErrorCode}) en {e.FailedUrl}");
        }

        private void Browser_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            try
            {
                if (e?.Frame == null || !e.Frame.IsMain) return;

                Debug.WriteLine($"[BrowserView] FrameLoadEnd para: {e.Url}");

                // ✅ Si estamos en una página que se abrió como popup, inyectar script para manejar window.close()
                if (Handlers.CustomLifeSpanHandler.IsPopupRedirect)
                {
                    InjectWindowCloseOverride(e.Frame);
                }

                // ✅ Detectar si estamos en la página de login
                bool isLoginPage = e.Url?.Contains("login.php") ?? false;
                
                // ✅ Detectar si llegamos a la página destino (después del login)
                bool isTargetPage = e.Url?.Contains("rep_new.php") ?? false;

                // ✅ Si ya ejecutamos login antes y volvemos a login.php, significa que el usuario cerró sesión
                // Por lo tanto, debemos resetear el flag para permitir un nuevo autologin
                if (_loginExecuted && isLoginPage)
                {
                    Debug.WriteLine("[BrowserView] 🔄 Detectado regreso a login - Reseteando para nuevo autologin");
                    _loginExecuted = false;
                    _isAutoLoginRunning = false;
                    
                    // ✅ También resetear el estado de popup si volvemos al login
                    Handlers.CustomLifeSpanHandler.ResetPopupState();
                }

                // ✅ Mostrar splash cuando estamos en login y vamos a hacer autologin
                if (_loginCredentials.HasValue && isLoginPage && !_loginExecuted)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SplashOverlay.Visibility = Visibility.Visible;
                        Debug.WriteLine("[BrowserView] 🎭 Splash MOSTRADO - Autologin en progreso");
                    }));
                }

                // ✅ Ocultar splash cuando llegamos a la página destino
                if (isTargetPage)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SplashOverlay.Visibility = Visibility.Collapsed;
                        Debug.WriteLine("[BrowserView] 🎭 Splash OCULTADO - Llegamos a página destino");
                    }));
                }

                // ✅ Ejecutar auto-login en la página de login
                if (_loginCredentials.HasValue &&
                    !_loginExecuted &&
                    isLoginPage)
                {
                    _loginExecuted = true; // ✅ Marcar ANTES de ejecutar

                    Debug.WriteLine("[BrowserView] Iniciando auto-login...");

                    // ✅ Ejecutar en UI thread con delay razonable
                    Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        try
                        {
                            await Task.Delay(1000); // ✅ Esperar a que el DOM esté listo
                            await ExecuteAutoLoginAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[BrowserView] ⚠️ Error en auto-login async: {ex.Message}");
                            // Si hay error, ocultar el splash
                            SplashOverlay.Visibility = Visibility.Collapsed;
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] ⚠️ Error en FrameLoadEnd: {ex.Message}");
            }
        }

        /// <summary>
        /// Inyecta un script que reemplaza window.close() para navegar hacia atrás
        /// </summary>
        private void InjectWindowCloseOverride(IFrame frame)
        {
            try
            {
                string script = @"
                    (function() {
                        // Guardar referencia original si existe
                        var originalClose = window.close;
                        
                        // Reemplazar window.close con navegación hacia atrás
                        window.close = function() {
                            console.log('[Naviguard] window.close interceptado - navegando hacia atrás');
                            if (history.length > 1) {
                                history.back();
                            } else {
                                // Si no hay historial, intentar cerrar (comportamiento original)
                                try { originalClose.call(window); } catch(e) {}
                            }
                        };
                        
                        console.log('[Naviguard] window.close override instalado');
                    })();
                ";

                frame.ExecuteJavaScriptAsync(script);
                Debug.WriteLine("[BrowserView] ✅ Script de window.close inyectado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] ⚠️ Error al inyectar script: {ex.Message}");
            }
        }

        private async Task ExecuteAutoLoginAsync()
        {
            if (!_loginCredentials.HasValue || _browser == null)
            {
                _isAutoLoginRunning = false;
                return;
            }

            if (_isAutoLoginRunning)
            {
                Debug.WriteLine("[BrowserView] ⚠️ Auto-login ya está ejecutándose, saltando...");
                return;
            }

            _isAutoLoginRunning = true;

            try
            {
                // ✅ Asegurar UI thread
                if (!Dispatcher.CheckAccess())
                {
                    await Dispatcher.InvokeAsync(async () => await ExecuteAutoLoginAsync());
                    return;
                }

                if (!_browser.IsBrowserInitialized)
                {
                    Debug.WriteLine($"[BrowserView] Navegador no inicializado");
                    return;
                }

                Debug.WriteLine($"[BrowserView] 💉 Inyectando credenciales...");

                string username = _loginCredentials.Value.Username
                    .Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\"", "\\\"");

                string password = _loginCredentials.Value.Password
                    .Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\"", "\\\"");

                // ✅ Script simplificado y directo
                string script = $@"
                    (function() {{
                        try {{
                            var email = document.getElementById('txtemail');
                            var pass = document.getElementById('txtpas');
                            var carac = document.getElementById('txtcarac');
                            var codcarac = document.getElementById('txtcodcarac');
                            var btn = document.querySelector('.btn_access');
                            
                            if (!email || !pass || !carac || !codcarac || !btn) {{
                                return {{ success: false, error: 'Elementos no encontrados' }};
                            }}

                            // Asignar valores
                            email.value = '{username}';
                            pass.value = '{password}';
                            carac.value = codcarac.value;
                            
                            // Disparar eventos para validación
                            ['input', 'change'].forEach(function(eventType) {{
                                email.dispatchEvent(new Event(eventType, {{ bubbles: true }}));
                                pass.dispatchEvent(new Event(eventType, {{ bubbles: true }}));
                                carac.dispatchEvent(new Event(eventType, {{ bubbles: true }}));
                            }});
                            
                            // ✅ Enviar formulario en lugar de click
                            var form = btn.closest('form');
                            if (form) {{
                                form.submit();
                                return {{ success: true }};
                            }} else {{
                                btn.click();
                                return {{ success: true }};
                            }}
                            
                        }} catch(e) {{
                            return {{ success: false, error: e.toString() }};
                        }}
                    }})();
                ";

                var frame = _browser.GetMainFrame();
                if (frame != null)
                {
                    var response = await frame.EvaluateScriptAsync(script);

                    if (response.Success)
                    {
                        Debug.WriteLine($"[BrowserView] ✅ Auto-login completado");
                    }
                    else
                    {
                        Debug.WriteLine($"[BrowserView] ⚠️ Auto-login falló: {response.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] ❌ Error en auto-login: {ex.Message}");
            }
            finally
            {
                _isAutoLoginRunning = false;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[BrowserView] Unloaded event");

            if (_browser != null)
            {
                // ✅ Desuscribir TODOS los eventos
                _browser.FrameLoadEnd -= Browser_FrameLoadEnd;
                _browser.FrameLoadStart -= Browser_FrameLoadStart;
                _browser.LoadError -= Browser_LoadError;

                // ✅ IMPORTANTE: Dispose del browser creado dinámicamente
                // Como nosotros lo creamos, nosotros deberíamos ser responsables de él,
                // aunque WPF suele limpiar el árbol visual, explícitamente liberar recursos es mejor.
                try
                {
                    _browser.Dispose();
                }
                catch { }
                _browser = null;
            }

            _viewModel?.Cleanup();
        }
    }
}