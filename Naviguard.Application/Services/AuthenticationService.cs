// Naviguard.Application/Services/AuthenticationService.cs
using Naviguard.Application.Common;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.Services
{
    public class AuthenticationService
    {
        private readonly IAuthRepository _authRepository;

        public AuthenticationService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<Result<bool>> ValidateUserPermissionsAsync(long userId)
        {
            if (userId <= 0)
                return Result<bool>.Failure("ID de usuario inválido"); // ✅ Corregido

            try
            {
                var hasPermissions = await _authRepository.UserHasRolesAsync(userId);
                return Result<bool>.Success(hasPermissions); // ✅ Corregido
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error al validar permisos: {ex.Message}"); // ✅ Corregido
            }
        }
    }
}