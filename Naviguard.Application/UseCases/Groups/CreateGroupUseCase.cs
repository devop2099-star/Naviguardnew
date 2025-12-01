using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.UseCases.Groups
{
    public class CreateGroupUseCase
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IPageRepository _pageRepository;

        public CreateGroupUseCase(
            IGroupRepository groupRepository,
            IPageRepository pageRepository)
        {
            _groupRepository = groupRepository;
            _pageRepository = pageRepository;
        }

        public async Task<Result<long>> ExecuteAsync(CreateGroupDto dto)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(dto.GroupName))
                return Result<long>.Failure("El nombre del grupo es obligatorio"); // ✅ CORREGIDO

            if (dto.GroupName.Length > 100)
                return Result<long>.Failure("El nombre del grupo no puede exceder 100 caracteres"); // ✅ CORREGIDO

            if (dto.GroupName.Length < 3)
                return Result<long>.Failure("El nombre del grupo debe tener al menos 3 caracteres"); // ✅ CORREGIDO

            if (dto.PageIds == null || !dto.PageIds.Any())
                return Result<long>.Failure("Debe seleccionar al menos una página"); // ✅ CORREGIDO

            try
            {
                // Crear el grupo
                var groupId = await _groupRepository.AddGroupAsync(
                    dto.GroupName,
                    dto.Description ?? string.Empty);

                // Asignar páginas al grupo
                await _pageRepository.AddPagesToGroupAsync(groupId, dto.PageIds);

                return Result<long>.Success(groupId); // ✅ CORREGIDO
            }
            catch (Exception ex)
            {
                return Result<long>.Failure($"Error al crear el grupo: {ex.Message}"); // ✅ CORREGIDO
            }
        }
    }
}