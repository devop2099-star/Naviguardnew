// Naviguard.WPF/Handlers/CustomLifeSpanHandler.cs
using CefSharp;
using CefSharp.Wpf;
using System.Diagnostics;

namespace Naviguard.WPF.Handlers
{
    public class CustomLifeSpanHandler : CefSharp.Handler.LifeSpanHandler
    {
        // ✅ Guardar información del popup para manejar el cierre
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
        /// Reinicia el estado del popup (llamar después de regresar)
        /// </summary>
        public static void ResetPopupState()
        {
            _isPopupRedirect = false;
            _previousUrl = null;
            Debug.WriteLine("[CustomLifeSpanHandler] Estado de popup reiniciado");
        }

        protected override bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("[CustomLifeSpanHandler] DoClose llamado");
            
            // ✅ Si estamos en un popup redirigido, regresar a la página anterior
            if (_isPopupRedirect)
            {
                Debug.WriteLine("[CustomLifeSpanHandler] Intentando regresar a página anterior desde popup");
                
                // ✅ Capturar la URL anterior ANTES del Dispatcher (es solo un string, thread-safe)
                var previousUrl = _previousUrl;
                
                // ✅ TODA la lógica de WPF debe estar dentro del Dispatcher
                System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new System.Action(() =>
                {
                    try
                    {
                        // Verificar el tipo DENTRO del Dispatcher
                        if (chromiumWebBrowser is ChromiumWebBrowser wpfBrowser)
                        {
                            if (wpfBrowser.CanGoBack)
                            {
                                wpfBrowser.Back();
                                ResetPopupState();
                                Debug.WriteLine("[CustomLifeSpanHandler] ✅ Regresando a página anterior");
                            }
                            else if (!string.IsNullOrEmpty(previousUrl))
                            {
                                wpfBrowser.Load(previousUrl);
                                ResetPopupState();
                                Debug.WriteLine($"[CustomLifeSpanHandler] ✅ Cargando URL anterior: {previousUrl}");
                            }
                            else
                            {
                                Debug.WriteLine("[CustomLifeSpanHandler] ⚠️ No hay página anterior para regresar");
                                ResetPopupState();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine($"[CustomLifeSpanHandler] ❌ Error al regresar: {ex.Message}");
                        ResetPopupState();
                    }
                }));
                
                return true; // Prevenir cierre del navegador
            }
            
            return false; // Permitir cierre normal
        }

        protected override void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("[CustomLifeSpanHandler] OnBeforeClose llamado");
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

            // ✅ Verificar que hay una URL válida
            if (string.IsNullOrEmpty(targetUrl) || targetUrl == "about:blank")
            {
                Debug.WriteLine($"[CustomLifeSpanHandler] Ignorando popup sin URL válida");
                return true; // Bloquear
            }

            Debug.WriteLine($"[CustomLifeSpanHandler] Redirigiendo popup a mismo navegador: {targetUrl}");

            // ✅ Obtener la URL actual usando CEF (thread-safe) antes de cambiar
            try
            {
                _previousUrl = browser.MainFrame?.Url;
                _isPopupRedirect = true;
                Debug.WriteLine($"[CustomLifeSpanHandler] URL anterior guardada (CEF): {_previousUrl}");
            }
            catch
            {
                _previousUrl = null;
                _isPopupRedirect = true;
            }

            // ✅ Capturar targetUrl en variable local para el closure
            var urlToLoad = targetUrl;

            // ✅ TODA la lógica WPF debe estar en el Dispatcher
            System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new System.Action(() =>
            {
                try
                {
                    if (chromiumWebBrowser is ChromiumWebBrowser wpfBrowser)
                    {
                        wpfBrowser.Load(urlToLoad);
                        Debug.WriteLine($"[CustomLifeSpanHandler] ✅ URL de popup cargada: {urlToLoad}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"[CustomLifeSpanHandler] ❌ Error al cargar URL: {ex.Message}");
                    ResetPopupState();
                }
            }));

            return true; // true = bloquear la creación del popup
        }
    }
}
