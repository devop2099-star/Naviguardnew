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
            if (isRedirect)
            {
                Debug.WriteLine($"[CustomRequestHandler] Redireccionando a: {request.Url}");
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