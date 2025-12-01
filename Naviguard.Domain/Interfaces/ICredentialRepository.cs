using Naviguard.Domain.Entities;

namespace Naviguard.Domain.Interfaces
{
    public interface ICredentialRepository
    {
        Task<UserPageCredential?> GetCredentialAsync(long userId, long pageId);
        Task UpdateOrInsertCredentialAsync(long userId, long pageId, string username, string password);
    }
}