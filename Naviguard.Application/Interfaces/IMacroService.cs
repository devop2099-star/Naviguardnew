// Naviguard.Application/Interfaces/IMacroService.cs
using Naviguard.Domain.Entities;

namespace Naviguard.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestionar macros de automatización web.
    /// </summary>
    public interface IMacroService
    {
        /// <summary>
        /// Guarda una lista de eventos grabados en un archivo.
        /// </summary>
        /// <param name="events">Lista de eventos a guardar</param>
        /// <param name="fileName">Nombre del archivo (por defecto: macro.json)</param>
        Task SaveMacroAsync(List<RecordedEvent> events, string fileName = "macro.json");

        /// <summary>
        /// Carga una macro desde un archivo.
        /// </summary>
        /// <param name="fileName">Nombre del archivo a cargar</param>
        /// <returns>Lista de eventos grabados</returns>
        Task<List<RecordedEvent>> LoadMacroAsync(string fileName = "macro.json");

        /// <summary>
        /// Verifica si existe una macro guardada.
        /// </summary>
        /// <param name="fileName">Nombre del archivo a verificar</param>
        /// <returns>True si existe el archivo</returns>
        bool MacroExists(string fileName = "macro.json");

        /// <summary>
        /// Obtiene la ruta completa del archivo de macro.
        /// </summary>
        /// <param name="fileName">Nombre del archivo</param>
        /// <returns>Ruta completa del archivo</returns>
        string GetMacroFilePath(string fileName = "macro.json");
    }
}
