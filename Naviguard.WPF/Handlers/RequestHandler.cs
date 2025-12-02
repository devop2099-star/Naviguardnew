// Naviguard.WPF/Handlers/RequestHandler.cs
using CefSharp;
using System.Diagnostics;

namespace Naviguard.WPF.Handlers
{
    // ✅ CAMBIAR EL NOMBRE DE LA CLASE para evitar conflicto
    public class CustomRequestHandler : CefSharp.Handler.RequestHandler
    {
        private readonly string _username;
        private readonly string _password;
        private readonly bool _handleRedirects;

        public CustomRequestHandler(string username, string password, bool handleRedirects = false)
        {
            _username = username;
            _password = password;
            _handleRedirects = handleRedirects;
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
            return new CustomResourceRequestHandler(_username, _password);
        }

        protected override bool OnBeforeBrowse(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            bool userGesture,
            bool isRedirect)
        {
            if (_handleRedirects && isRedirect)
            {
                Debug.WriteLine($"Redireccionando a: {request.Url}");
            }

            return false; // false = permitir navegación
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
            // Inyectar credenciales en las cabeceras (para autenticación básica)
            if (!string.IsNullOrWhiteSpace(_username))
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