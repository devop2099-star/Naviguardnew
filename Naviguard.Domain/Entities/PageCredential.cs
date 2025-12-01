using Naviguard.Domain.Common;

namespace Naviguard.Domain.Entities
{
    /// <summary>
    /// Credenciales generales de una página (compartidas por todos los usuarios)
    /// </summary>
    public class PageCredential : Entity
    {
        public long PageCredentialId { get; set; }
        public long PageId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Navegación
        public Pagina? Page { get; set; }

        // Constructor
        public PageCredential()
        {
        }

        public PageCredential(long pageId, string username, string password)
        {
            PageId = pageId;
            Username = username;
            Password = password;
        }

        // Métodos de dominio
        public bool HasCredentials() => !string.IsNullOrWhiteSpace(Username);

        public void UpdateCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("El usuario no puede estar vacío", nameof(username));

            Username = username;
            Password = password ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}