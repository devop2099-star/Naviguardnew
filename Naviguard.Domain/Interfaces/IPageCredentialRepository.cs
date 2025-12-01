using Naviguard.Domain.Entities;

namespace Naviguard.Domain.Interfaces
{
    public interface IPageCredentialRepository
    {
        Task<PageCredential?> GetCredentialByPageIdAsync(long pageId);
    }
}