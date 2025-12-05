// Naviguard.WPF/ViewModels/BrowserViewModel.cs
using CefSharp;
using CefSharp.Wpf;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.ExternalServices;
using Naviguard.WPF.Handlers;
using Naviguard.WPF.Services;
using System.Diagnostics;
using System.Windows;
using WpfApp = System.Windows.Application;

namespace Naviguard.WPF.ViewModels
{
    public partial class BrowserViewModel : ObservableObject
    {
        private readonly ICredentialRepository _credentialRepository;
        private readonly IPageCredentialRepository _pageCredentialRepository;
        private readonly ProxyManager _proxyManager;

        [ObservableProperty]
        private string _currentUrl = "about:blank";

        [ObservableProperty]
        private string _pageTitle = "Navegador";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canGoBack;

        [ObservableProperty]
        private bool _canGoForward;

        public ChromiumWebBrowser? Browser { get; set; }
        private Pagina? _currentPage;

        public BrowserViewModel(
            ICredentialRepository credentialRepository,
            IPageCredentialRepository pageCredentialRepository,
            ProxyManager proxyManager)
        {
            _credentialRepository = credentialRepository;
            _pageCredentialRepository = pageCredentialRepository;
            _proxyManager = proxyManager;
        }

        public async Task InitializeBrowserAsync(ChromiumWebBrowser browser, Pagina page)
        {
            Browser = browser;
            _currentPage = page;

            // ✅ Obtener credenciales ANTES de cargar
            var credentials = await GetCredentialsForPageAsync(page);

            // Configurar proxy si es necesario
            if (page.RequiresProxy)
            {
                await ConfigureProxyAsync();
            }

            // Configurar RequestHandler para inyectar credenciales HTTP
            if (page.RequiresLogin || page.RequiresCustomLogin)
            {
                if (credentials.HasValue)
                {
                    var requestHandler = new CustomRequestHandler(
                        credentials.Value.Username,
                        credentials.Value.Password,
                        page.RequiresRedirects);

                    Browser.RequestHandler = requestHandler;
                }
            }

            // ✅ AGREGAR: Suscribirse a eventos
            Browser.AddressChanged += OnAddressChanged;
            Browser.TitleChanged += OnTitleChanged;
            Browser.LoadingStateChanged += OnLoadingStateChanged;

            // ✅ NUEVO: Suscribirse a FrameLoadEnd para auto-login
            Browser.FrameLoadEnd += OnFrameLoadEnd;

            // Navegar a la URL
            Browser.Load(page.Url);
            CurrentUrl = page.Url;
        }

        private async Task ConfigureProxyAsync()
        {
            try
            {
                var proxyInfo = await _proxyManager.GetProxyAsync();
                if (proxyInfo != null && proxyInfo.IsValid())
                {
                    var proxyAddress = proxyInfo.GetProxyAddress();
                    Debug.WriteLine($"Configurando proxy: {proxyAddress}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al configurar proxy: {ex.Message}");
            }
        }

        private async Task<(string Username, string Password)?> GetCredentialsForPageAsync(Pagina page)
        {
            if (!UserSession.IsLoggedIn) return null;

            try
            {
                if (page.RequiresCustomLogin)
                {
                    var userCredential = await _credentialRepository.GetCredentialAsync(
                        UserSession.ApiUserId,
                        page.PageId);

                    if (userCredential != null)
                    {
                        Debug.WriteLine($"✅ Credenciales de usuario encontradas para {page.PageName}");
                        return (userCredential.Username, userCredential.Password);
                    }
                }

                if (page.RequiresLogin)
                {
                    var pageCredential = await _pageCredentialRepository.GetCredentialByPageIdAsync(page.PageId);

                    if (pageCredential != null)
                    {
                        Debug.WriteLine($"✅ Credenciales de página encontradas para {page.PageName}");
                        return (pageCredential.Username, pageCredential.Password);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener credenciales: {ex.Message}");
            }

            return null;
        }

        private void OnAddressChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string newUrl)
            {
                // ✅ Ejecutar en UI thread
                WpfApp.Current.Dispatcher.Invoke(() =>
                {
                    CurrentUrl = newUrl;
                    UpdateNavigationState();
                });
            }
        }

        private void OnTitleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string newTitle)
            {
                // ✅ Ejecutar en UI thread
                WpfApp.Current.Dispatcher.Invoke(() =>
                {
                    PageTitle = newTitle;
                });
            }
        }

        private void OnLoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
        {
            // ✅ Ejecutar en UI thread
            WpfApp.Current.Dispatcher.Invoke(() =>
            {
                IsLoading = e.IsLoading;
                UpdateNavigationState();
            });
        }

        private void UpdateNavigationState()
        {
            if (Browser != null)
            {
                // ✅ Ya estamos en el UI thread gracias al Dispatcher
                try
                {
                    CanGoBack = Browser.CanGoBack;
                    CanGoForward = Browser.CanGoForward;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al actualizar estado de navegación: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            // ✅ Ejecutar en UI thread
            WpfApp.Current.Dispatcher.Invoke(() =>
            {
                if (Browser?.CanGoBack == true)
                {
                    Browser.Back();
                }
            });
        }

        [RelayCommand]
        private void GoForward()
        {
            // ✅ Ejecutar en UI thread
            WpfApp.Current.Dispatcher.Invoke(() =>
            {
                if (Browser?.CanGoForward == true)
                {
                    Browser.Forward();
                }
            });
        }

        [RelayCommand]
        private void Refresh()
        {
            // ✅ Ejecutar en UI thread
            WpfApp.Current.Dispatcher.Invoke(() =>
            {
                Browser?.Reload();
            });
        }

        [RelayCommand]
        private void NavigateToUrl()
        {
            // ✅ Ejecutar en UI thread
            WpfApp.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(CurrentUrl))
                {
                    Browser?.Load(CurrentUrl);
                }
            });
        }

        public void Cleanup()
        {
            if (Browser != null)
            {
                Browser.AddressChanged -= OnAddressChanged;
                Browser.TitleChanged -= OnTitleChanged;
                Browser.LoadingStateChanged -= OnLoadingStateChanged;
                Browser.FrameLoadEnd -= OnFrameLoadEnd;
            }
        }
        private void OnFrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            // Solo ejecutar en el frame principal
            if (!e.Frame.IsMain) return;

            Debug.WriteLine($"[BrowserViewModel] FrameLoadEnd para: {e.Url}");

            // Verificar si la página requiere auto-login
            if (_currentPage == null) return;
            if (!(_currentPage.RequiresLogin || _currentPage.RequiresCustomLogin)) return;

            // ✅ EJECUTAR EN UI THREAD DE FORMA SEGURA
            WpfApp.Current?.Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    // Obtener credenciales
                    var credentials = await GetCredentialsForPageAsync(_currentPage);
                    if (!credentials.HasValue)
                    {
                        Debug.WriteLine("[BrowserViewModel] ⚠️ No hay credenciales disponibles");
                        return;
                    }

                    Debug.WriteLine($"[BrowserViewModel] 💉 Ejecutando auto-login para: {_currentPage.PageName}");

                    // ✅ Ejecutar script de auto-login
                    await ExecuteAutoLoginAsync(credentials.Value.Username, credentials.Value.Password);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BrowserViewModel] 💥 Error en OnFrameLoadEnd: {ex.Message}");
                }
            });
        }

        private async Task ExecuteAutoLoginAsync(string username, string password)
        {
            if (Browser == null)
            {
                Debug.WriteLine("[BrowserViewModel] ⚠️ Browser es null");
                return;
            }

            try
            {
                // ✅ Verificar que el browser aún esté cargado
                if (Browser.IsBrowserInitialized == false)
                {
                    Debug.WriteLine("[BrowserViewModel] ⚠️ Browser no está inicializado");
                    return;
                }

                // ✅ Obtener el frame principal de forma segura
                var frame = Browser.GetMainFrame();
                if (frame == null || frame.IsValid == false)
                {
                    Debug.WriteLine("[BrowserViewModel] ⚠️ Frame principal no válido");
                    return;
                }

                // ✅ Escapar caracteres especiales en credenciales
                string safeUsername = username
                    .Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\"", "\\\"");

                string safePassword = password
                    .Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\"", "\\\"");

                // ✅ Script JS con manejo de errores
                string script = $@"
            (function() {{
                try {{
                    var emailInput = document.getElementById('txtemail');
                    var passInput = document.getElementById('txtpas');
                    var caracInput = document.getElementById('txtcarac');
                    var caracCode = document.getElementById('txtcodcarac');
                    var loginButton = document.querySelector('.btn_access');

                    if (!emailInput || !passInput || !loginButton) {{
                        console.log('❌ Elementos de login no encontrados');
                        return false;
                    }}

                    emailInput.value = '{safeUsername}';
                    passInput.value = '{safePassword}';
                    
                    if (caracInput && caracCode) {{
                        caracInput.value = caracCode.value;
                    }}

                    console.log('✅ Formulario rellenado, haciendo clic en', loginButton);
                    
                    // ✅ Esperar un momento antes de hacer clic (por si hay validaciones)
                    setTimeout(function() {{
                        loginButton.click();
                    }}, 100);
                    
                    return true;
                }} catch (ex) {{
                    console.error('❌ Error en auto-login:', ex);
                    return false;
                }}
            }})();
        ";

                Debug.WriteLine($"[BrowserViewModel] 📋 Ejecutando script JS");

                // ✅ Ejecutar con timeout
                var response = await frame.EvaluateScriptAsync(script);

                if (response.Success)
                {
                    if (response.Result is bool result && result)
                    {
                        Debug.WriteLine("[BrowserViewModel] ✅ Auto-login ejecutado correctamente");
                    }
                    else
                    {
                        Debug.WriteLine($"[BrowserViewModel] ⚠️ Auto-login retornó: {response.Result}");
                    }
                }
                else
                {
                    Debug.WriteLine($"[BrowserViewModel] ❌ Error en script: {response.Message}");
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine($"[BrowserViewModel] ⚠️ Browser ya fue liberado: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserViewModel] 💥 Error en auto-login: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

    }
}