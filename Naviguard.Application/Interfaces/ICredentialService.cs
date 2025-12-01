using Naviguard.Application.Common;
using Naviguard.Application.DTOs;

namespace Naviguard.Application.Interfaces
{
    public interface ICredentialService
    {
        Task<Result<UserCredentialDto?>> GetUserCredentialAsync(long userId, long pageId);
        Task<Result> UpdateOrInsertCredentialAsync(UserCredentialDto dto);
    }
}