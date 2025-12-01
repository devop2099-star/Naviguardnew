namespace Naviguard.Domain.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> UserHasRolesAsync(long userId);
    }
}