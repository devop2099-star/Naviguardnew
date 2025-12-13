// Naviguard.WPF/Handlers/MarcosLifeSpanHandler.cs
using CefSharp;
using CefSharp.Wpf;
using System.Diagnostics;

namespace Naviguard.WPF.Handlers
{
    /// <summary>
    /// Handler de popups para el módulo Marcos.
    /// Similar a CustomLifeSpanHandler pero SIN restricción de dominio.
    /// Redirige popups a la misma ventana del navegador.
    /// </summary>
    public class MarcosLifeSpanHandler : CefSharp.Handler.LifeSpanHandler
    {
        private static string? _previousUrl;
        private static bool _isPopupRedirect = false;

        /// <summary>
        /// Indica si la última navegación fue por un popup redirigido
        /// </summary>
        public static bool IsPopupRedirect => _isPopupRedirect;

        /// <summary>
        /// URL anterior antes del popup (para regresar)
        /// </summary>
        public static string? PreviousUrl => _previousUrl;

        /// <summary>
        /// Reinicia el estado del popup
        /// </summary>
        public static void ResetPopupState()
        {
            _isPopupRedirect = false;
            _previousUrl = null;
            Debug.WriteLine("[MarcosLifeSpanHandler] Estado de popup reiniciado");
        }

        protected override bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("[MarcosLifeSpanHandler] DoClose llamado");

            // Si estamos en un popup redirigido, regresar a la página anterior
            if (_isPopupRedirect)
            {
                Debug.WriteLine("[MarcosLifeSpanHandler] Intentando regresar a página anterior desde popup");

                var previousUrl = _previousUrl;

                System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new System.Action(() =>
                {
                    try
                    {
                        if (chromiumWebBrowser is ChromiumWebBrowser wpfBrowser)
                        {
                            if (wpfBrowser.CanGoBack)
                            {
                                wpfBrowser.Back();
                                ResetPopupState();
                                Debug.WriteLine("[MarcosLifeSpanHandler] ✅ Regresando a página anterior");
                            }
                            else if (!string.IsNullOrEmpty(previousUrl))
                            {
                                wpfBrowser.Load(previousUrl);
                                ResetPopupState();
                                Debug.WriteLine($"[MarcosLifeSpanHandler] ✅ Cargando URL anterior: {previousUrl}");
                            }
                            else
                            {
                                Debug.WriteLine("[MarcosLifeSpanHandler] ⚠️ No hay página anterior para regresar");
                                ResetPopupState();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine($"[MarcosLifeSpanHandler] ❌ Error al regresar: {ex.Message}");
                        ResetPopupState();
                    }
                }));

                return true; // Prevenir cierre del navegador
            }

            return false; // Permitir cierre normal
        }

        protected override void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("[MarcosLifeSpanHandler] OnBeforeClose llamado");
            base.OnBeforeClose(chromiumWebBrowser, browser);
        }

        protected override bool OnBeforePopup(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            string targetUrl,
            string targetFrameName,
            WindowOpenDisposition targetDisposition,
            bool userGesture,
            IPopupFeatures popupFeatures,
            IWindowInfo windowInfo,
            IBrowserSettings browserSettings,
            ref bool noJavascriptAccess,
            out IWebBrowser? newBrowser)
        {
            newBrowser = null;

            // Verificar que hay una URL válida
            if (string.IsNullOrEmpty(targetUrl) || targetUrl == "about:blank")
            {
                Debug.WriteLine($"[MarcosLifeSpanHandler] Ignorando popup sin URL válida");
                return true; // Bloquear
            }

            Debug.WriteLine($"[MarcosLifeSpanHandler] Redirigiendo popup a mismo navegador: {targetUrl}");

            // Guardar URL actual antes de navegar al popup
            try
            {
                _previousUrl = browser.MainFrame?.Url;
                _isPopupRedirect = true;
                Debug.WriteLine($"[MarcosLifeSpanHandler] URL anterior guardada: {_previousUrl}");
            }
            catch
            {
                _previousUrl = null;
                _isPopupRedirect = true;
            }

            var urlToLoad = targetUrl;

            // Cargar la URL del popup en el navegador actual
            System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new System.Action(() =>
            {
                try
                {
                    if (chromiumWebBrowser is ChromiumWebBrowser wpfBrowser)
                    {
                        wpfBrowser.Load(urlToLoad);
                        Debug.WriteLine($"[MarcosLifeSpanHandler] ✅ URL de popup cargada: {urlToLoad}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"[MarcosLifeSpanHandler] ❌ Error al cargar URL: {ex.Message}");
                    ResetPopupState();
                }
            }));

            return true; // Bloquear la creación del popup (ya redirigimos a la misma ventana)
        }
    }
}
