// Naviguard.WPF/Views/Marcos/MarcosView.xaml.cs
using CefSharp;
using CefSharp.Wpf;
using Naviguard.Domain.Entities;
using Naviguard.WPF.Handlers;
using Naviguard.WPF.ViewModels;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Naviguard.WPF.Views.Marcos
{
    /// <summary>
    /// Vista del módulo Marcos para automatización web.
    /// </summary>
    public partial class MarcosView : UserControl
    {
        private MarcosViewModel? _viewModel;
        private string _lastUrl = string.Empty;
        private bool _isRealNavigation = false;
        private DateTime _lastNavigationTime = DateTime.MinValue;
        private string _usernameSelector = string.Empty;
        private string _passwordSelector = string.Empty;

        public MarcosView()
        {
            InitializeComponent();
            Debug.WriteLine("[MarcosView] Constructor llamado");

            // ✅ Configurar handlers ANTES de que el navegador se inicialice
            if (BrowserControl is ChromiumWebBrowser browser)
            {
                // Handler de popups SIN restricción de dominio
                browser.LifeSpanHandler = new MarcosLifeSpanHandler();

                // Handler para diálogos JS
                browser.JsDialogHandler = new CustomJsDialogHandler();

                // Suscribirse a eventos
                browser.FrameLoadEnd += Browser_FrameLoadEnd;
                browser.AddressChanged += Browser_AddressChanged;
                browser.LoadingStateChanged += Browser_LoadingStateChanged;
                browser.JavascriptMessageReceived += Browser_JavascriptMessageReceived;

                // ✅ Configurar URL inicial - esto crea el navegador
                browser.Address = "https://www.google.com";
                _lastUrl = browser.Address;

                Debug.WriteLine("[MarcosView] Navegador configurado en constructor");
            }

            Loaded += MarcosView_Loaded;
        }

        private void MarcosView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MarcosViewModel;
            Debug.WriteLine("[MarcosView] Loaded - ViewModel asignado");
        }

        private void Browser_LoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
            {
                _isRealNavigation = true;
                _lastNavigationTime = DateTime.Now;
            }
        }

        private void Browser_AddressChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (BrowserControl != null)
                {
                    TxtUrl.Text = BrowserControl.Address;
                    _viewModel?.UpdateCurrentUrl(BrowserControl.Address);
                }
            });

            string newUrl = BrowserControl?.Address ?? "";

            if (_viewModel == null || !_viewModel.IsRecording || 
                string.IsNullOrEmpty(_lastUrl) || _lastUrl == newUrl)
            {
                _lastUrl = newUrl;
                return;
            }

            // Registrar navegación si es real
            if (_isRealNavigation && (DateTime.Now - _lastNavigationTime).TotalMilliseconds < 1000)
            {
                Dispatcher.Invoke(() =>
                {
                    _viewModel.AddEvent(new RecordedEvent
                    {
                        Type = "navigate",
                        Url = newUrl
                    });
                });

                _isRealNavigation = false;
            }

            _lastUrl = newUrl;
        }

        private async void Browser_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain) return;

            Debug.WriteLine($"[MarcosView] Página cargada: {e.Url}");

            // Inyectar script de window.close para popups
            if (MarcosLifeSpanHandler.IsPopupRedirect)
            {
                InjectWindowCloseOverride(e.Frame);
            }

            // Inyectar tracker de foco siempre
            await InjectFocusTrackerAsync();

            // Si está grabando, inyectar script de grabación
            if (_viewModel?.IsRecording == true)
            {
                await Task.Delay(500);
                await InjectRecordingScriptAsync();
            }
        }

        private void Browser_JavascriptMessageReceived(object? sender, JavascriptMessageReceivedEventArgs e)
        {
            if (_viewModel == null || !_viewModel.IsRecording) return;

            try
            {
                string json = e.Message.ToString() ?? "";
                var eventData = JsonSerializer.Deserialize<RecordedEvent>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (eventData != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _viewModel.AddEvent(eventData);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MarcosView] Error procesando mensaje JS: {ex.Message}");
            }
        }

        private void InjectWindowCloseOverride(IFrame frame)
        {
            try
            {
                string script = @"
                    (function() {
                        var originalClose = window.close;
                        window.close = function() {
                            console.log('[Marcos] window.close interceptado');
                            if (history.length > 1) {
                                history.back();
                            } else {
                                try { originalClose.call(window); } catch(e) {}
                            }
                        };
                    })();
                ";

                frame.ExecuteJavaScriptAsync(script);
                Debug.WriteLine("[MarcosView] ✅ Script window.close inyectado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MarcosView] Error inyectando script: {ex.Message}");
            }
        }

        private async Task InjectFocusTrackerAsync()
        {
            string script = @"
                (function() {
                    if (window.__focusTrackerInstalled) return;
                    window.__focusTrackerInstalled = true;
                    window.__lastInteractedElement = null;

                    function track(e) {
                        if (e.target && (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA')) {
                            window.__lastInteractedElement = e.target;
                        }
                    }

                    document.addEventListener('focus', track, true);
                    document.addEventListener('click', track, true);
                    document.addEventListener('input', track, true);
                    console.log('[Marcos] Focus tracker instalado');
                })();
            ";

            try
            {
                await BrowserControl.EvaluateScriptAsync(script);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MarcosView] Error inyectando focus tracker: {ex.Message}");
            }
        }

        private async Task InjectRecordingScriptAsync()
        {
            string script = @"
                (function() {
                    if (window.__recorderActive) return;
                    window.__recorderActive = true;
                    console.log('[Marcos] 🔴 RECORDER ACTIVO');

                    function getSelector(el) {
                        try {
                            if (el.id && document.querySelectorAll('#' + el.id).length === 1) {
                                return '#' + el.id;
                            }
                            if (el.name) {
                                let byName = document.querySelectorAll('[name=""' + el.name + '""]');
                                if (byName.length === 1) return '[name=""' + el.name + '""]';
                            }
                            let path = [];
                            let current = el;
                            let depth = 0;
                            while (current && current !== document.body && depth < 8) {
                                let tag = current.tagName.toLowerCase();
                                let part = tag;
                                if (current.className && typeof current.className === 'string') {
                                    let classes = current.className.trim().split(/\s+/).filter(c => c && !c.match(/^[a-f0-9]{6,}$/i));
                                    if (classes.length > 0 && classes.length < 4) {
                                        part += '.' + classes.slice(0, 2).join('.');
                                    }
                                }
                                path.unshift(part);
                                current = current.parentElement;
                                depth++;
                            }
                            return path.join(' > ');
                        } catch (err) {
                            return 'body';
                        }
                    }

                    window.getSelector = getSelector;

                    let lastClickTime = 0;
                    document.addEventListener('click', function(e) {
                        try {
                            let now = Date.now();
                            if (now - lastClickTime < 100) return;
                            lastClickTime = now;
                            
                            let data = {
                                type: 'click',
                                selector: getSelector(e.target),
                                text: (e.target.textContent || '').trim().substring(0, 100),
                                tag: e.target.tagName,
                                ariaLabel: e.target.getAttribute('aria-label') || ''
                            };
                            CefSharp.PostMessage(JSON.stringify(data));
                        } catch (err) {}
                    }, true);

                    let inputTimers = new Map();
                    document.addEventListener('input', function(e) {
                        try {
                            let selector = getSelector(e.target);
                            if (inputTimers.has(selector)) {
                                clearTimeout(inputTimers.get(selector));
                            }
                            let timer = setTimeout(function() {
                                let data = {
                                    type: 'input',
                                    selector: selector,
                                    value: e.target.value,
                                    tag: e.target.tagName,
                                    elementType: e.target.type || ''
                                };
                                CefSharp.PostMessage(JSON.stringify(data));
                                inputTimers.delete(selector);
                            }, 800);
                            inputTimers.set(selector, timer);
                        } catch (err) {}
                    }, true);

                    document.addEventListener('keydown', function(e) {
                        if (e.key === 'Enter' && (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA')) {
                            try {
                                let data = {
                                    type: 'keypress',
                                    selector: getSelector(e.target),
                                    key: 'Enter',
                                    value: e.target.value
                                };
                                CefSharp.PostMessage(JSON.stringify(data));
                            } catch (err) {}
                        }
                    }, true);

                    console.log('[Marcos] ✅ Listeners instalados');
                })();
            ";

            try
            {
                var response = await BrowserControl.EvaluateScriptAsync(script);
                if (!response.Success)
                {
                    Debug.WriteLine($"[MarcosView] Error inyectando script: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MarcosView] Excepción: {ex.Message}");
            }
        }

        #region Event Handlers

        private void TxtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NavigateToUrl();
            }
        }



        private void NavigateToUrl()
        {
            string url = TxtUrl.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(url)) return;

            // Si no parece URL, buscar en Google
            if (!url.Contains(".") && !url.StartsWith("http"))
            {
                url = $"https://www.google.com/search?q={System.Net.WebUtility.UrlEncode(url)}";
            }
            else if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            BrowserControl.Address = url;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserControl.CanGoBack)
            {
                BrowserControl.Back();
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.StartRecording();

            // Re-inyectar script de grabación
            Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(300);
                await InjectRecordingScriptAsync();
            });

            MessageBox.Show(
                "🔴 GRABACIÓN INICIADA\n\n" +
                "Ahora graba:\n" +
                "✓ Clicks en botones, enlaces\n" +
                "✓ Texto en campos\n" +
                "✓ Tecla Enter\n" +
                "✓ Navegaciones\n\n" +
                "💡 Para marcar campos de login:\n" +
                "1. Click en el campo de usuario\n" +
                "2. Presiona '🔑 Marcar Login'\n" +
                "3. Repite para contraseña\n\n" +
                "Presiona DETENER cuando termines.",
                "Grabación Iniciada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            await _viewModel.StopRecordingAsync();

            int count = _viewModel.EventCount;
            if (count > 0)
            {
                MessageBox.Show(
                    $"✅ GRABACIÓN GUARDADA\n\n" +
                    $"📦 Total: {count} eventos\n\n" +
                    $"Puedes reproducir esta macro presionando 'Reproducir'.",
                    "Grabación Guardada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async void BtnReplay_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            await _viewModel.LoadMacroAsync();
            var events = _viewModel.GetEventsForReplay();

            if (events.Count == 0)
            {
                MessageBox.Show("❌ No hay macro guardada.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"▶️ ¿Reproducir {events.Count} eventos?\n\nSe ejecutarán todas las acciones grabadas.",
                "Confirmar reproducción",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            await ReplayMacroAsync(events);
        }

        private async Task ReplayMacroAsync(List<RecordedEvent> events)
        {
            int eventNum = 0;

            foreach (var evt in events)
            {
                eventNum++;
                await Task.Delay(700);

                try
                {
                    switch (evt.Type)
                    {
                        case "navigate":
                            _viewModel?.UpdateStatus($"[{eventNum}/{events.Count}] 🌐 Navegando...");
                            BrowserControl.Address = evt.Url;
                            await Task.Delay(3000);
                            break;

                        case "setCredentials":
                            _viewModel?.UpdateStatus($"[{eventNum}/{events.Count}] 🔑 Solicitando credenciales...");
                            var dialog = new CredentialsDialog(GetDomain(evt.Url));
                            bool? dialogResult = dialog.ShowDialog();

                            if (dialogResult != true || dialog.WasCancelled)
                            {
                                MessageBox.Show("❌ Reproducción cancelada.", "Cancelado", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                _viewModel?.UpdateStatus("❌ Reproducción cancelada");
                                return;
                            }

                            // Inyectar credenciales
                            await InjectCredentialsAsync(evt.UsernameSelector, evt.PasswordSelector, 
                                dialog.Username, dialog.Password);
                            await Task.Delay(1000);
                            break;

                        case "click":
                            string displayText = !string.IsNullOrEmpty(evt.Text)
                                ? evt.Text.Substring(0, Math.Min(30, evt.Text.Length))
                                : evt.AriaLabel ?? "elemento";
                            _viewModel?.UpdateStatus($"[{eventNum}/{events.Count}] 🖱️ Click en: {displayText}");
                            await ExecuteClickAsync(evt.Selector);
                            break;

                        case "input":
                            _viewModel?.UpdateStatus($"[{eventNum}/{events.Count}] ⌨️ Escribiendo...");
                            await ExecuteInputAsync(evt.Selector, evt.Value);
                            await Task.Delay(300);
                            break;

                        case "keypress":
                            if (evt.Key == "Enter")
                            {
                                _viewModel?.UpdateStatus($"[{eventNum}/{events.Count}] ⏎ Presionando Enter...");
                                await ExecuteEnterAsync(evt.Selector);
                                await Task.Delay(1500);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MarcosView] Error en evento {eventNum}: {ex.Message}");
                    _viewModel?.UpdateStatus($"❌ Error en evento {eventNum}");
                }
            }

            MessageBox.Show("✅ Macro completada exitosamente!", "Éxito", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            _viewModel?.UpdateStatus("✅ Reproducción finalizada");
        }

        private async Task InjectCredentialsAsync(string userSelector, string passSelector, 
            string username, string password)
        {
            string script = $@"
                (function() {{
                    let userEl = document.querySelector('{EscapeSelector(userSelector)}');
                    if (userEl) {{
                        userEl.focus();
                        userEl.value = '{EscapeValue(username)}';
                        userEl.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        userEl.dispatchEvent(new Event('change', {{ bubbles: true }}));
                    }}
                    
                    let passEl = document.querySelector('{EscapeSelector(passSelector)}');
                    if (passEl) {{
                        passEl.focus();
                        passEl.value = '{EscapeValue(password)}';
                        passEl.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        passEl.dispatchEvent(new Event('change', {{ bubbles: true }}));
                    }}
                }})();
            ";

            await BrowserControl.EvaluateScriptAsync(script);
        }

        private async Task ExecuteClickAsync(string selector)
        {
            string script = $@"
                (function() {{
                    let el = document.querySelector('{EscapeSelector(selector)}');
                    if (el) {{
                        el.scrollIntoView({{ behavior: 'smooth', block: 'center' }});
                        setTimeout(() => el.click(), 300);
                    }}
                }})();
            ";
            await BrowserControl.EvaluateScriptAsync(script);
        }

        private async Task ExecuteInputAsync(string selector, string value)
        {
            string script = $@"
                (function() {{
                    let el = document.querySelector('{EscapeSelector(selector)}');
                    if (el) {{
                        el.focus();
                        el.value = '{EscapeValue(value)}';
                        el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                    }}
                }})();
            ";
            await BrowserControl.EvaluateScriptAsync(script);
        }

        private async Task ExecuteEnterAsync(string selector)
        {
            string script = $@"
                (function() {{
                    let el = document.querySelector('{EscapeSelector(selector)}');
                    if (el) {{
                        el.dispatchEvent(new KeyboardEvent('keydown', {{ key: 'Enter', code: 'Enter', keyCode: 13, bubbles: true }}));
                        el.dispatchEvent(new KeyboardEvent('keypress', {{ key: 'Enter', code: 'Enter', keyCode: 13, bubbles: true }}));
                    }}
                }})();
            ";
            await BrowserControl.EvaluateScriptAsync(script);
        }

        private async void BtnMarkCredentials_Click(object sender, RoutedEventArgs e)
        {
            await InjectFocusTrackerAsync();

            string script = @"
                (function() {
                    let el = document.activeElement;
                    if (!el || el === document.body) {
                        if (window.__lastInteractedElement) {
                            el = window.__lastInteractedElement;
                        }
                    }
                    if (el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA')) {
                        if (typeof getSelector === 'function') {
                            return getSelector(el);
                        }
                        if (el.id) return '#' + el.id;
                        if (el.name) return '[name=""' + el.name + '""]';
                        return el.tagName.toLowerCase();
                    }
                    return null;
                })();
            ";

            var response = await BrowserControl.EvaluateScriptAsync(script);
            string? selector = response.Result?.ToString();

            if (string.IsNullOrEmpty(selector) || selector == "null")
            {
                MessageBox.Show(
                    "❌ No hay un campo de texto enfocado.\n\n" +
                    "Para marcar un campo:\n" +
                    "1. HAZ CLICK dentro del campo de usuario o contraseña\n" +
                    "2. Luego presiona el botón '🔑 Marcar Login'",
                    "Campo no detectado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Resaltar campo
            await BrowserControl.EvaluateScriptAsync($@"
                (function() {{
                    let el = document.querySelector('{EscapeSelector(selector)}');
                    if (el) {{
                        el.style.outline = '3px solid #10B981';
                        el.style.outlineOffset = '2px';
                        setTimeout(() => {{
                            el.style.outline = '';
                            el.style.outlineOffset = '';
                        }}, 2000);
                    }}
                }})();
            ");

            var result = MessageBox.Show(
                $"✅ Campo detectado:\n\n{selector}\n\n" +
                "¿Es este el campo de USUARIO?\n\n" +
                "• Sí = Campo de USUARIO\n" +
                "• No = Campo de CONTRASEÑA\n" +
                "• Cancelar = Salir",
                "Confirmar tipo de campo",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _usernameSelector = selector;
                _viewModel?.SetUsernameSelector(selector);
                MessageBox.Show(
                    $"✅ Campo de USUARIO marcado\n\nAhora haz click en el campo de CONTRASEÑA y vuelve a presionar '🔑 Marcar Login'",
                    "Usuario Guardado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (result == MessageBoxResult.No)
            {
                _passwordSelector = selector;
                _viewModel?.SetPasswordSelector(selector, BrowserControl.Address);

                if (!string.IsNullOrEmpty(_usernameSelector))
                {
                    MessageBox.Show(
                        "✅ Ambos campos de login han sido marcados.\n\n" +
                        "Cuando reproduzcas la macro, se te pedirán las credenciales.",
                        "Login Marcado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    _usernameSelector = string.Empty;
                    _passwordSelector = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        $"✅ Campo de CONTRASEÑA marcado\n\nAhora marca el campo de USUARIO",
                        "Contraseña Guardada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        #endregion

        #region Helpers

        private string GetDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return url;
            }
        }

        private string EscapeSelector(string selector)
        {
            return selector?.Replace("'", "\\'").Replace("\"", "\\\"") ?? "";
        }

        private string EscapeValue(string value)
        {
            return value?.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", "\\n") ?? "";
        }

        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MarcosView] Unloaded");

            if (BrowserControl != null)
            {
                BrowserControl.FrameLoadEnd -= Browser_FrameLoadEnd;
                BrowserControl.AddressChanged -= Browser_AddressChanged;
                BrowserControl.LoadingStateChanged -= Browser_LoadingStateChanged;
                BrowserControl.JavascriptMessageReceived -= Browser_JavascriptMessageReceived;
            }

            MarcosLifeSpanHandler.ResetPopupState();
        }
    }
}
