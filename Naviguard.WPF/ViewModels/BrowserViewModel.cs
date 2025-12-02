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

            // Configurar proxy si es necesario
            if (page.RequiresProxy)
            {
                await ConfigureProxyAsync();
            }

            // Configurar RequestHandler para inyectar credenciales
            if (page.RequiresLogin || page.RequiresCustomLogin)
            {
                var credentials = await GetCredentialsForPageAsync(page);
                if (credentials.HasValue) // ✅ CORREGIDO
                {
                    var requestHandler = new CustomRequestHandler( // ✅ CAMBIAR NOMBRE
                        credentials.Value.Username, // ✅ CORREGIDO
                        credentials.Value.Password, // ✅ CORREGIDO
                        page.RequiresRedirects);

                    Browser.RequestHandler = requestHandler;
                }
            }

            // Suscribirse a eventos
            Browser.AddressChanged += OnAddressChanged;
            Browser.TitleChanged += OnTitleChanged;
            Browser.LoadingStateChanged += OnLoadingStateChanged;

            // Navegar a la URL
            Browser.Load(page.Url);
            CurrentUrl = page.Url;
        }

        private async Task ConfigureProxyAsync()
        {
            try
            {
                var proxyInfo = await _proxyManager.GetProxyAsync();
                if (proxyInfo != null && proxyInfo.IsValid()) // ✅ Ahora existe el método
                {
                    var proxyAddress = proxyInfo.GetProxyAddress(); // ✅ Ahora existe el método
                    Debug.WriteLine($"Configurando proxy: {proxyAddress}");

                    // CefSharp requiere configurar el proxy antes de inicializar
                    // Esto debería hacerse en la configuración global de CefSettings
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al configurar proxy: {ex.Message}");
            }
        }

        // ✅ CORREGIDO - Cambiar el tipo de retorno a tupla nombrada
        private async Task<(string Username, string Password)?> GetCredentialsForPageAsync(Pagina page)
        {
            if (!UserSession.IsLoggedIn) return null;

            try
            {
                if (page.RequiresCustomLogin)
                {
                    // Intentar obtener credenciales del usuario
                    var userCredential = await _credentialRepository.GetCredentialAsync(
                        UserSession.ApiUserId,
                        page.PageId);

                    if (userCredential != null)
                    {
                        Debug.WriteLine($"✅ Credenciales de usuario encontradas para {page.PageName}");
                        return (userCredential.Username, userCredential.Password);
                    }
                }

                // Si no hay credenciales de usuario o RequiresLogin, usar credenciales de página
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
                CurrentUrl = newUrl;
                UpdateNavigationState();
            }
        }

        private void OnTitleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string newTitle)
            {
                PageTitle = newTitle;
            }
        }

        private void OnLoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
        {
            IsLoading = e.IsLoading;
            UpdateNavigationState();
        }

        private void UpdateNavigationState()
        {
            if (Browser != null)
            {
                CanGoBack = Browser.CanGoBack;
                CanGoForward = Browser.CanGoForward;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            if (Browser?.CanGoBack == true)
            {
                Browser.Back();
            }
        }

        [RelayCommand]
        private void GoForward()
        {
            if (Browser?.CanGoForward == true)
            {
                Browser.Forward();
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            Browser?.Reload();
        }

        [RelayCommand]
        private void NavigateToUrl()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUrl))
            {
                Browser?.Load(CurrentUrl);
            }
        }

        public void Cleanup()
        {
            if (Browser != null)
            {
                Browser.AddressChanged -= OnAddressChanged;
                Browser.TitleChanged -= OnTitleChanged;
                Browser.LoadingStateChanged -= OnLoadingStateChanged;
            }
        }
    }
}