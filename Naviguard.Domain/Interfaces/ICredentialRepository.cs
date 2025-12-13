using Naviguard.Domain.Entities;

namespace Naviguard.Domain.Interfaces
{
    public interface ICredentialRepository
    {
        Task<UserPageCredential?> GetCredentialAsync(long userId, long pageId);
        Task<List<UserPageCredential>> GetUsersForPageAsync(long pageId);
        Task UpdateOrInsertCredentialAsync(long userId, long pageId, string username, string password);
        Task DeleteCredentialAsync(long userId, long pageId);
    }
}