// Naviguard.Domain/Entities/RecordedEvent.cs
namespace Naviguard.Domain.Entities
{
    /// <summary>
    /// Representa un evento grabado durante una sesión de macro.
    /// </summary>
    public class RecordedEvent
    {
        /// <summary>
        /// Tipo de evento: click, input, keypress, navigate, focus, setCredentials
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Selector CSS del elemento objetivo
        /// </summary>
        public string Selector { get; set; } = string.Empty;

        /// <summary>
        /// Valor para eventos de tipo input
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// URL para eventos de navegación
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Tecla presionada para eventos keypress
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Texto visible del elemento
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Tag HTML del elemento
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de elemento (ej: text, password, submit)
        /// </summary>
        public string ElementType { get; set; } = string.Empty;

        /// <summary>
        /// Atributo aria-label del elemento
        /// </summary>
        public string AriaLabel { get; set; } = string.Empty;

        /// <summary>
        /// Atributo title del elemento
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Placeholder del campo de texto
        /// </summary>
        public string Placeholder { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp Unix del evento
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Selector del campo de usuario (para setCredentials)
        /// </summary>
        public string UsernameSelector { get; set; } = string.Empty;

        /// <summary>
        /// Selector del campo de contraseña (para setCredentials)
        /// </summary>
        public string PasswordSelector { get; set; } = string.Empty;

        /// <summary>
        /// Selector del botón de submit (para setCredentials)
        /// </summary>
        public string SubmitSelector { get; set; } = string.Empty;
    }
}
