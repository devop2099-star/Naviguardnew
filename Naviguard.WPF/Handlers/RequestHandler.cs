// Naviguard.WPF/Handlers/RequestHandler.cs
using CefSharp;
using System.Diagnostics;

namespace Naviguard.WPF.Handlers
{
    public class CustomRequestHandler : CefSharp.Handler.RequestHandler
    {
        private readonly string _username;
        private readonly string _password;
        private readonly bool _handleRedirects;
        private bool _credentialsInjected = false;

        // ✅ Dominio base permitido para navegación
        private string? _allowedBaseDomain;
        private bool _domainRestrictionEnabled = true;
        private string? _lastAllowedUrl; // ✅ Última URL permitida para regresar

        /// <summary>
        /// Habilita o deshabilita la restricción de dominio
        /// </summary>
        public bool DomainRestrictionEnabled
        {
            get => _domainRestrictionEnabled;
            set => _domainRestrictionEnabled = value;
        }

        /// <summary>
        /// Establece el dominio base permitido
        /// </summary>
        public string? AllowedBaseDomain
        {
            get => _allowedBaseDomain;
            set => _allowedBaseDomain = value;
        }

        public CustomRequestHandler(string username, string password, bool handleRedirects = false)
        {
            _username = username;
            _password = password;
            _handleRedirects = handleRedirects;
        }

        /// <summary>
        /// Extrae el dominio base de una URL (sin subdominio www)
        /// </summary>
        private static string? ExtractBaseDomain(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return null;

                var uri = new Uri(url);
                var host = uri.Host.ToLowerInvariant();

                // Eliminar "www." si existe
                if (host.StartsWith("www."))
                    host = host.Substring(4);

                // Para dominios como "maps.google.com", extraer "google.com"
                var parts = host.Split('.');
                if (parts.Length >= 2)
                {
                    // Tomar las últimas 2 partes para el dominio base
                    return $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
                }

                return host;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Verifica si una URL pertenece al dominio permitido
        /// </summary>
        private bool IsUrlAllowed(string url)
        {
            if (!_domainRestrictionEnabled || string.IsNullOrEmpty(_allowedBaseDomain))
                return true;

            var urlDomain = ExtractBaseDomain(url);
            if (urlDomain == null) return true; // Permitir URLs que no se pueden parsear

            // Verificar si el dominio coincide
            bool isAllowed = urlDomain.Equals(_allowedBaseDomain, StringComparison.OrdinalIgnoreCase);
            
            Debug.WriteLine($"[CustomRequestHandler] URL: {url} | Dominio: {urlDomain} | Base: {_allowedBaseDomain} | Permitido: {isAllowed}");
            
            return isAllowed;
        }

        protected override IResourceRequestHandler? GetResourceRequestHandler(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            bool isNavigation,
            bool isDownload,
            string requestInitiator,
            ref bool disableDefaultHandling)
        {
            // Solo inyectar credenciales en la primera navegación
            if (!_credentialsInjected && isNavigation && frame.IsMain)
            {
                _credentialsInjected = true;
                return new CustomResourceRequestHandler(_username, _password);
            }

            return null;
        }

        protected override bool OnBeforeBrowse(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            bool userGesture,
            bool isRedirect)
        {
            var url = request.Url;

            // ✅ Establecer dominio base en la primera navegación del frame principal
            if (frame.IsMain && string.IsNullOrEmpty(_allowedBaseDomain))
            {
                _allowedBaseDomain = ExtractBaseDomain(url);
                _lastAllowedUrl = url; // Guardar la primera URL como referencia
                Debug.WriteLine($"[CustomRequestHandler] 🔒 Dominio base establecido: {_allowedBaseDomain}");
            }

            // ✅ Verificar si la navegación está permitida (solo para frame principal)
            if (frame.IsMain && _domainRestrictionEnabled)
            {
                if (!IsUrlAllowed(url))
                {
                    Debug.WriteLine($"[CustomRequestHandler] ❌ Navegación bloqueada a dominio externo: {url}");
                    
                    // ✅ Regresar a la página anterior
                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new System.Action(() =>
                    {
                        try
                        {
                            if (chromiumWebBrowser is CefSharp.Wpf.ChromiumWebBrowser wpfBrowser)
                            {
                                if (wpfBrowser.CanGoBack)
                                {
                                    wpfBrowser.Back();
                                    Debug.WriteLine("[CustomRequestHandler] ✅ Regresando a página anterior");
                                }
                                else if (!string.IsNullOrEmpty(_lastAllowedUrl))
                                {
                                    wpfBrowser.Load(_lastAllowedUrl);
                                    Debug.WriteLine($"[CustomRequestHandler] ✅ Cargando última URL permitida: {_lastAllowedUrl}");
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.WriteLine($"[CustomRequestHandler] ⚠️ Error al regresar: {ex.Message}");
                        }
                    }));
                    
                    return true; // BLOQUEAR navegación
                }
                else
                {
                    // Guardar esta URL como última permitida
                    _lastAllowedUrl = url;
                }
            }

            if (isRedirect)
            {
                Debug.WriteLine($"[CustomRequestHandler] Redireccionando a: {url}");
            }

            return false; // Permitir navegación
        }

        // ✅ CORREGIDO: Firma correcta con todos los parámetros
        protected override void OnRenderProcessTerminated(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            CefTerminationStatus status,
            int errorCode,
            string errorString)
        {
            Debug.WriteLine($"[CustomRequestHandler] ⚠️ Proceso de renderizado terminado: {status}, ErrorCode: {errorCode}, Error: {errorString}");
            base.OnRenderProcessTerminated(chromiumWebBrowser, browser, status, errorCode, errorString);
        }
    }

    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        private readonly string _username;
        private readonly string _password;

        public CustomResourceRequestHandler(string username, string password)
        {
            _username = username;
            _password = password;
        }

        protected override CefReturnValue OnBeforeResourceLoad(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            IRequestCallback callback)
        {
            // Solo inyectar en el request principal (HTML), no en recursos (CSS, JS, imágenes)
            if (!string.IsNullOrWhiteSpace(_username) &&
                frame.IsMain &&
                request.ResourceType == ResourceType.MainFrame)
            {
                var headers = request.Headers;
                var authValue = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{_username}:{_password}"));
                headers["Authorization"] = $"Basic {authValue}";
                request.Headers = headers;

                Debug.WriteLine($"Credenciales inyectadas para: {request.Url}");
            }

            return CefReturnValue.Continue;
        }

        protected override IResponseFilter? GetResourceResponseFilter(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            IResponse response)
        {
            return null;
        }
    }
}