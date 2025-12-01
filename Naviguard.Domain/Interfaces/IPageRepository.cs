using Naviguard.Domain.Entities;

namespace Naviguard.Domain.Interfaces
{
    public interface IPageRepository
    {
        // Consultas
        Task<List<Pagina>> GetAllPagesAsync();
        Task<Pagina?> GetPageByIdAsync(long pageId);
        Task<List<Pagina>> GetPagesRequiringCustomLoginAsync();

        // Comandos
        Task<long> AddPageAsync(Pagina page);
        Task UpdatePageAsync(Pagina page);
        Task SoftDeletePageAsync(long pageId);

        // Credenciales
        Task AddCredentialAsync(long pageId, string username, string password);
        Task UpdateOrInsertCredentialAsync(long pageId, string username, string password);
        Task<PageCredential?> GetCredentialForPageAsync(long pageId);

        // Relaciones
        Task AddPagesToGroupAsync(long groupId, List<long> pageIds);
    }
}