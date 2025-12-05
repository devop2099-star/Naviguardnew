// Naviguard.WPF/Handlers/CustomLifeSpanHandler.cs
using CefSharp;
using System.Diagnostics;

namespace Naviguard.WPF.Handlers
{
    public class CustomLifeSpanHandler : CefSharp.Handler.LifeSpanHandler
    {
        protected override bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("[CustomLifeSpanHandler] DoClose llamado - Previniendo cierre");
            // ✅ Retornar true previene que el navegador se cierre
            return false; // false = permitir cierre normal, true = prevenir cierre
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
            Debug.WriteLine($"[CustomLifeSpanHandler] Bloqueando popup: {targetUrl}");

            // ✅ Bloquear popups y cargar en el mismo navegador
            chromiumWebBrowser.Load(targetUrl);
            newBrowser = null;
            return true; // true = bloquear popup
        }
    }
}