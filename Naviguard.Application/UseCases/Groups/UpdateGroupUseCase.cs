using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.Domain.ValueObjects; // ✅ AGREGADO

namespace Naviguard.Application.UseCases.Groups
{
    public class UpdateGroupUseCase
    {
        private readonly IGroupRepository _groupRepository;

        public UpdateGroupUseCase(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<Result> ExecuteAsync(UpdateGroupDto dto)
        {
            // Validaciones
            if (dto.GroupId <= 0)
                return Result.Failure("ID de grupo inválido"); // ✅ Correcto (sin genérico)

            if (string.IsNullOrWhiteSpace(dto.GroupName))
                return Result.Failure("El nombre del grupo es obligatorio"); // ✅ Correcto

            if (dto.GroupName.Length > 100)
                return Result.Failure("El nombre no puede exceder 100 caracteres"); // ✅ Correcto

            try
            {
                var group = new Group
                {
                    GroupId = dto.GroupId,
                    GroupName = dto.GroupName,
                    Description = dto.Description,
                    Pin = dto.Pin
                };

                var pages = dto.Pages.Select(p => new PageAssignmentInfo
                {
                    PageId = p.PageId,
                    IsPinned = p.IsPinned
                }).ToList();

                await _groupRepository.UpdateGroupAsync(group, pages);

                return Result.Success(); // ✅ Correcto (sin genérico)
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al actualizar el grupo: {ex.Message}"); // ✅ Correcto
            }
        }
    }
}