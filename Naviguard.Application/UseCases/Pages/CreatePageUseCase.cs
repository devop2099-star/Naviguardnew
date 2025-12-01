using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.UseCases.Pages
{
    public class CreatePageUseCase
    {
        private readonly IPageRepository _pageRepository;

        public CreatePageUseCase(IPageRepository pageRepository)
        {
            _pageRepository = pageRepository;
        }

        public async Task<Result<long>> ExecuteAsync(CreatePageDto dto)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(dto.PageName))
                return Result<long>.Failure("El nombre de la página es obligatorio"); // ✅ CORREGIDO

            if (string.IsNullOrWhiteSpace(dto.Url))
                return Result<long>.Failure("La URL es obligatoria"); // ✅ CORREGIDO

            if (!Uri.TryCreate(dto.Url, UriKind.Absolute, out _))
                return Result<long>.Failure("La URL no es válida"); // ✅ CORREGIDO

            if (dto.RequiresLogin && string.IsNullOrWhiteSpace(dto.CredentialUsername))
                return Result<long>.Failure("Si requiere login, debe proporcionar un usuario"); // ✅ CORREGIDO

            try
            {
                var page = new Pagina
                {
                    PageName = dto.PageName,
                    Description = dto.Description,
                    Url = dto.Url,
                    RequiresProxy = dto.RequiresProxy,
                    RequiresLogin = dto.RequiresLogin,
                    RequiresCustomLogin = dto.RequiresCustomLogin,
                    RequiresRedirects = dto.RequiresRedirects,
                    State = 1,
                    CreatedAt = DateTime.UtcNow
                };

                var pageId = await _pageRepository.AddPageAsync(page);

                // Si requiere login, guardar credenciales
                if (dto.RequiresLogin && !string.IsNullOrWhiteSpace(dto.CredentialUsername))
                {
                    await _pageRepository.AddCredentialAsync(
                        pageId,
                        dto.CredentialUsername,
                        dto.CredentialPassword ?? string.Empty);
                }

                return Result<long>.Success(pageId); // ✅ CORREGIDO
            }
            catch (Exception ex)
            {
                return Result<long>.Failure($"Error al crear la página: {ex.Message}"); // ✅ CORREGIDO
            }
        }
    }
}