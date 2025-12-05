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

            // ✅ CRÍTICO: Configurar LifeSpanHandler ANTES de cualquier otra cosa
            Browser.LifeSpanHandler = new CustomLifeSpanHandler();
            Debug.WriteLine("[BrowserViewModel] LifeSpanHandler configurado");

            // Configurar proxy si es necesario
            if (page.RequiresProxy)
            {
                await ConfigureProxyAsync();
            }

            // NO usar RequestHandler para auto-login (se hace solo con JavaScript)
            if (page.RequiresProxy)
            {
                var credentials = await GetCredentialsForPageAsync(page);
                if (credentials.HasValue)
                {
                    var requestHandler = new CustomRequestHandler(
                        credentials.Value.Username,
                        credentials.Value.Password,
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

        public async Task<(string Username, string Password)?> GetCredentialsForPageAsync(Pagina page)
        {
            if (!UserSession.IsLoggedIn) return null;

            try
            {
                if (page.RequiresCustomLogin)
                {
                    Debug.WriteLine($"[BrowserViewModel] 🔎 Buscando credencial PERSONALIZADA para User: {UserSession.ApiUserId}, Page: {page.PageId}");

                    var userCredential = await _credentialRepository.GetCredentialAsync(
                        UserSession.ApiUserId,
                        page.PageId);

                    if (userCredential != null)
                    {
                        Debug.WriteLine($"[BrowserViewModel] ✅ Credencial PERSONALIZADA encontrada. Usuario: '{userCredential.Username}'");
                        return (userCredential.Username, userCredential.Password);
                    }
                    else
                    {
                        Debug.WriteLine($"[BrowserViewModel] ❌ NO se encontró credencial PERSONALIZADA.");
                    }
                }

                if (page.RequiresLogin)
                {
                    Debug.WriteLine($"[BrowserViewModel] 🔎 Buscando credencial GENERAL para Page: {page.PageId}");

                    var pageCredential = await _pageCredentialRepository.GetCredentialByPageIdAsync(page.PageId);

                    if (pageCredential != null)
                    {
                        Debug.WriteLine($"[BrowserViewModel] ✅ Credencial GENERAL encontrada. Usuario: '{pageCredential.Username}'");
                        return (pageCredential.Username, pageCredential.Password);
                    }
                    else
                    {
                        Debug.WriteLine($"[BrowserViewModel] ❌ NO se encontró credencial GENERAL.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserViewModel] 💥 ERROR AL BUSCAR CREDENCIALES: {ex.Message}");
            }

            return null;
        }

        private void OnAddressChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string newUrl)
            {
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
                WpfApp.Current.Dispatcher.Invoke(() =>
                {
                    PageTitle = newTitle;
                });
            }
        }

        private void OnLoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
        {
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
            WpfApp.Current.Dispatcher.Invoke(() =>
            {
                Browser?.Reload();
            });
        }

        [RelayCommand]
        private void NavigateToUrl()
        {
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
            }
        }
    }
}