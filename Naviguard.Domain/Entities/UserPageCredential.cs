using Naviguard.Domain.Common;

namespace Naviguard.Domain.Entities
{
    /// <summary>
    /// Credenciales personalizadas de un usuario específico para una página
    /// </summary>
    public class UserPageCredential : Entity
    {
        public long PageReqLgId { get; set; }
        public long ExternalUserId { get; set; }
        public long PageId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty; // Nombre de la persona (no mapeado en DB local)

        // Navegación
        public Pagina? Page { get; set; }

        // Constructor
        public UserPageCredential()
        {
        }

        public UserPageCredential(long userId, long pageId, string username, string password)
        {
            ExternalUserId = userId;
            PageId = pageId;
            Username = username;
            Password = password;
        }

        // Métodos de dominio
        public bool BelongsToUser(long userId) => ExternalUserId == userId;

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