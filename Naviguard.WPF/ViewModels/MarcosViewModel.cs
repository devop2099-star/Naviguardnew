// Naviguard.WPF/ViewModels/MarcosViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Naviguard.WPF.ViewModels
{
    /// <summary>
    /// ViewModel para el módulo Marcos de automatización web.
    /// </summary>
    public partial class MarcosViewModel : ObservableObject
    {
        private readonly IMacroService _macroService;

        [ObservableProperty]
        private string _currentUrl = "https://www.google.com";

        [ObservableProperty]
        private bool _isRecording;

        [ObservableProperty]
        private string _statusMessage = "✅ Listo";

        [ObservableProperty]
        private int _eventCount;

        [ObservableProperty]
        private ObservableCollection<RecordedEvent> _events = new();

        // Para marcar campos de login
        private string _usernameSelector = string.Empty;
        private string _passwordSelector = string.Empty;

        public MarcosViewModel(IMacroService macroService)
        {
            _macroService = macroService;
            Debug.WriteLine("[MarcosViewModel] Inicializado");
        }

        /// <summary>
        /// Inicia la grabación de una macro
        /// </summary>
        [RelayCommand]
        public void StartRecording()
        {
            if (IsRecording) return;

            IsRecording = true;
            Events.Clear();
            EventCount = 0;

            // Agregar evento inicial de navegación
            Events.Add(new RecordedEvent
            {
                Type = "navigate",
                Url = CurrentUrl,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            });
            EventCount = Events.Count;

            StatusMessage = "🔴 GRABANDO...";
            Debug.WriteLine("[MarcosViewModel] Grabación iniciada");
        }

        /// <summary>
        /// Detiene la grabación y guarda la macro
        /// </summary>
        [RelayCommand]
        public async Task StopRecordingAsync()
        {
            if (!IsRecording) return;

            IsRecording = false;

            if (Events.Count == 0)
            {
                StatusMessage = "⚠️ No se grabaron eventos";
                return;
            }

            try
            {
                await _macroService.SaveMacroAsync(Events.ToList());
                StatusMessage = $"✅ Macro guardada ({Events.Count} eventos)";
                Debug.WriteLine($"[MarcosViewModel] Macro guardada: {Events.Count} eventos");
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error al guardar: {ex.Message}";
                Debug.WriteLine($"[MarcosViewModel] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Carga una macro existente para reproducción
        /// </summary>
        [RelayCommand]
        public async Task LoadMacroAsync()
        {
            try
            {
                if (!_macroService.MacroExists())
                {
                    StatusMessage = "❌ No hay macro guardada";
                    return;
                }

                var loadedEvents = await _macroService.LoadMacroAsync();
                Events = new ObservableCollection<RecordedEvent>(loadedEvents);
                EventCount = Events.Count;
                StatusMessage = $"✅ Macro cargada ({Events.Count} eventos)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error al cargar: {ex.Message}";
            }
        }

        /// <summary>
        /// Agrega un evento grabado
        /// </summary>
        public void AddEvent(RecordedEvent evt)
        {
            if (!IsRecording) return;

            evt.Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Events.Add(evt);
            EventCount = Events.Count;

            // Actualizar status según tipo de evento
            string icon = evt.Type switch
            {
                "click" => "🖱️",
                "input" => "⌨️",
                "keypress" => "⏎",
                "focus" => "🎯",
                "navigate" => "🌐",
                _ => "📝"
            };

            string display = evt.Type == "input" && !string.IsNullOrEmpty(evt.Value)
                ? $"{icon} {evt.Type.ToUpper()}: \"{evt.Value}\""
                : $"{icon} {evt.Type.ToUpper()}: {evt.Text ?? evt.AriaLabel ?? evt.Selector}";

            StatusMessage = display;
            Debug.WriteLine($"[MarcosViewModel] Evento agregado: {display}");
        }

        /// <summary>
        /// Marca el selector de usuario para credenciales
        /// </summary>
        public void SetUsernameSelector(string selector)
        {
            _usernameSelector = selector;
            Debug.WriteLine($"[MarcosViewModel] Username selector: {selector}");
        }

        /// <summary>
        /// Marca el selector de password y agrega evento de credenciales si ambos están listos
        /// </summary>
        public void SetPasswordSelector(string selector, string currentUrl)
        {
            _passwordSelector = selector;
            Debug.WriteLine($"[MarcosViewModel] Password selector: {selector}");

            // Si ambos selectores están listos, agregar evento setCredentials
            if (!string.IsNullOrEmpty(_usernameSelector) && !string.IsNullOrEmpty(_passwordSelector))
            {
                if (IsRecording)
                {
                    Events.Add(new RecordedEvent
                    {
                        Type = "setCredentials",
                        Url = currentUrl,
                        UsernameSelector = _usernameSelector,
                        PasswordSelector = _passwordSelector,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    });
                    EventCount = Events.Count;
                    StatusMessage = "🔑 Evento 'setCredentials' agregado";
                }

                // Resetear para próxima marcación
                _usernameSelector = string.Empty;
                _passwordSelector = string.Empty;
            }
        }

        /// <summary>
        /// Obtiene los eventos para reproducción
        /// </summary>
        public List<RecordedEvent> GetEventsForReplay()
        {
            return Events.ToList();
        }

        /// <summary>
        /// Actualiza la URL actual
        /// </summary>
        public void UpdateCurrentUrl(string url)
        {
            CurrentUrl = url;
        }

        /// <summary>
        /// Actualiza el mensaje de estado
        /// </summary>
        public void UpdateStatus(string message)
        {
            StatusMessage = message;
        }
    }
}
