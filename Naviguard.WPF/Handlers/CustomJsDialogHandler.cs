// Naviguard.WPF/Handlers/CustomJsDialogHandler.cs
using CefSharp;
using System.Diagnostics;

namespace Naviguard.WPF.Handlers
{
    /// <summary>
    /// Manejador personalizado para diálogos JavaScript (alert, confirm, prompt)
    /// Automáticamente acepta todos los diálogos
    /// </summary>
    public class CustomJsDialogHandler : IJsDialogHandler
    {
        /// <summary>
        /// Se llama cuando aparece un diálogo JavaScript
        /// </summary>
        public bool OnJSDialog(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string originUrl,
            CefJsDialogType dialogType,
            string messageText,
            string defaultPromptText,
            IJsDialogCallback callback,
            ref bool suppressMessage)
        {
            Debug.WriteLine($"[CustomJsDialogHandler] Diálogo detectado: {dialogType} - Mensaje: {messageText}");

            // ✅ Automáticamente aceptar el diálogo
            // Para 'confirm': true = Aceptar, false = Cancelar
            // Para 'alert': solo se cierra
            // Para 'prompt': se usa defaultPromptText como valor
            callback.Continue(true, defaultPromptText ?? string.Empty);

            Debug.WriteLine($"[CustomJsDialogHandler] ✅ Diálogo aceptado automáticamente");

            // Retornar true indica que hemos manejado el diálogo
            return true;
        }

        /// <summary>
        /// Se llama cuando aparece un diálogo de antes de descargar (beforeunload)
        /// </summary>
        public bool OnBeforeUnloadDialog(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string messageText,
            bool isReload,
            IJsDialogCallback callback)
        {
            Debug.WriteLine($"[CustomJsDialogHandler] BeforeUnload detectado: {messageText}");

            // ✅ Permitir la navegación/recarga automáticamente
            callback.Continue(true);

            Debug.WriteLine($"[CustomJsDialogHandler] ✅ BeforeUnload aceptado automáticamente");

            return true;
        }

        /// <summary>
        /// Se llama para reiniciar el estado del diálogo
        /// </summary>
        public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("[CustomJsDialogHandler] Estado de diálogo reiniciado");
        }

        /// <summary>
        /// Se llama cuando un diálogo es cerrado
        /// </summary>
        public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("[CustomJsDialogHandler] Diálogo cerrado");
        }
    }
}
