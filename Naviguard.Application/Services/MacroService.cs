// Naviguard.Application/Services/MacroService.cs
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Diagnostics;
using System.Text.Json;

namespace Naviguard.Application.Services
{
    /// <summary>
    /// Implementación del servicio de macros.
    /// </summary>
    public class MacroService : IMacroService
    {
        private readonly string _macrosDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public MacroService()
        {
            // Directorio de macros en AppData
            _macrosDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Naviguard",
                "Macros");

            // Crear directorio si no existe
            if (!Directory.Exists(_macrosDirectory))
            {
                Directory.CreateDirectory(_macrosDirectory);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task SaveMacroAsync(List<RecordedEvent> events, string fileName = "macro.json")
        {
            try
            {
                string filePath = GetMacroFilePath(fileName);
                string json = JsonSerializer.Serialize(events, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                Debug.WriteLine($"[MacroService] ✅ Macro guardada: {filePath} ({events.Count} eventos)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MacroService] ❌ Error al guardar macro: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RecordedEvent>> LoadMacroAsync(string fileName = "macro.json")
        {
            try
            {
                string filePath = GetMacroFilePath(fileName);

                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"[MacroService] ⚠️ Archivo no encontrado: {filePath}");
                    return new List<RecordedEvent>();
                }

                string json = await File.ReadAllTextAsync(filePath);
                var events = JsonSerializer.Deserialize<List<RecordedEvent>>(json, _jsonOptions);

                Debug.WriteLine($"[MacroService] ✅ Macro cargada: {events?.Count ?? 0} eventos");
                return events ?? new List<RecordedEvent>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MacroService] ❌ Error al cargar macro: {ex.Message}");
                throw;
            }
        }

        public bool MacroExists(string fileName = "macro.json")
        {
            return File.Exists(GetMacroFilePath(fileName));
        }

        public string GetMacroFilePath(string fileName = "macro.json")
        {
            return Path.Combine(_macrosDirectory, fileName);
        }
    }
}
