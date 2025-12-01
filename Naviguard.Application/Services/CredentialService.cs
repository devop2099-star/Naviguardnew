// Naviguard.Application/Services/CredentialService.cs
using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly ICredentialRepository _credentialRepository;
        private readonly IPageCredentialRepository _pageCredentialRepository;

        public CredentialService(
            ICredentialRepository credentialRepository,
            IPageCredentialRepository pageCredentialRepository)
        {
            _credentialRepository = credentialRepository;
            _pageCredentialRepository = pageCredentialRepository;
        }

        public async Task<Result<UserCredentialDto?>> GetUserCredentialAsync(long userId, long pageId)
        {
            if (userId <= 0 || pageId <= 0)
                return Result<UserCredentialDto?>.Failure("IDs inválidos"); // ✅ Corregido

            try
            {
                var credential = await _credentialRepository.GetCredentialAsync(userId, pageId);

                if (credential == null)
                    return Result<UserCredentialDto?>.Success(null); // ✅ Corregido

                var dto = new UserCredentialDto
                {
                    UserId = credential.ExternalUserId,
                    PageId = credential.PageId,
                    Username = credential.Username,
                    Password = credential.Password
                };

                return Result<UserCredentialDto?>.Success(dto); // ✅ Corregido
            }
            catch (Exception ex)
            {
                return Result<UserCredentialDto?>.Failure($"Error al obtener credencial: {ex.Message}"); // ✅ Corregido
            }
        }

        public async Task<Result> UpdateOrInsertCredentialAsync(UserCredentialDto dto)
        {
            // Validaciones
            if (dto.UserId <= 0)
                return Result.Failure("ID de usuario inválido"); // ✅ Corregido (sin genérico)

            if (dto.PageId <= 0)
                return Result.Failure("ID de página inválido"); // ✅ Corregido (sin genérico)

            if (string.IsNullOrWhiteSpace(dto.Username))
                return Result.Failure("El usuario es obligatorio"); // ✅ Corregido (sin genérico)

            if (dto.Username.Length > 100)
                return Result.Failure("El usuario no puede exceder 100 caracteres"); // ✅ Corregido (sin genérico)

            try
            {
                await _credentialRepository.UpdateOrInsertCredentialAsync(
                    dto.UserId,
                    dto.PageId,
                    dto.Username,
                    dto.Password);

                return Result.Success(); // ✅ Corregido (sin genérico)
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al guardar credencial: {ex.Message}"); // ✅ Corregido (sin genérico)
            }
        }
    }
}