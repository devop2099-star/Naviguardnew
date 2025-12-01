using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.UseCases.Pages
{
    public class UpdatePageUseCase
    {
        private readonly IPageRepository _pageRepository;

        public UpdatePageUseCase(IPageRepository pageRepository)
        {
            _pageRepository = pageRepository;
        }

        public async Task<Result> ExecuteAsync(UpdatePageDto dto)
        {
            // Validaciones
            if (dto.PageId <= 0)
                return Result.Failure("ID de página inválido");

            if (string.IsNullOrWhiteSpace(dto.PageName))
                return Result.Failure("El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(dto.Url))
                return Result.Failure("La URL es obligatoria");

            try
            {
                var page = new Pagina
                {
                    PageId = dto.PageId,
                    PageName = dto.PageName,
                    Description = dto.Description,
                    Url = dto.Url,
                    RequiresProxy = dto.RequiresProxy,
                    RequiresLogin = dto.RequiresLogin,
                    RequiresCustomLogin = dto.RequiresCustomLogin,
                    RequiresRedirects = dto.RequiresRedirects
                };

                await _pageRepository.UpdatePageAsync(page);

                // Actualizar credenciales si es necesario
                if (dto.RequiresLogin && !string.IsNullOrWhiteSpace(dto.CredentialUsername))
                {
                    await _pageRepository.UpdateOrInsertCredentialAsync(
                        dto.PageId,
                        dto.CredentialUsername,
                        dto.CredentialPassword ?? string.Empty);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al actualizar la página: {ex.Message}");
            }
        }
    }
}