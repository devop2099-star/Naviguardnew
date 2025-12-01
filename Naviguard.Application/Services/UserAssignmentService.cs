// Naviguard.Application/Services/UserAssignmentService.cs
using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.Services
{
    public class UserAssignmentService : IUserAssignmentService
    {
        private readonly IUserAssignmentRepository _assignmentRepository;
        private readonly IBusinessStructureRepository _businessRepository;
        private readonly IGroupRepository _groupRepository;

        public UserAssignmentService(
            IUserAssignmentRepository assignmentRepository,
            IBusinessStructureRepository businessRepository,
            IGroupRepository groupRepository)
        {
            _assignmentRepository = assignmentRepository;
            _businessRepository = businessRepository;
            _groupRepository = groupRepository;
        }

        public async Task<Result<List<GroupDto>>> GetGroupsByUserIdAsync(int userId)
        {
            if (userId <= 0)
                return Result<List<GroupDto>>.Failure("ID de usuario inválido"); // ✅ Corregido

            try
            {
                var groups = await _assignmentRepository.GetGroupsByUserIdAsync(userId);

                var groupDtos = groups.Select(g => new GroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    Pin = g.Pin
                }).ToList();

                return Result<List<GroupDto>>.Success(groupDtos); // ✅ Corregido
            }
            catch (Exception ex)
            {
                return Result<List<GroupDto>>.Failure($"Error al obtener grupos del usuario: {ex.Message}"); // ✅ Corregido
            }
        }

        public async Task<Result> AssignGroupsToUserAsync(int userId, List<long> groupIds)
        {
            // Validaciones
            if (userId <= 0)
                return Result.Failure("ID de usuario inválido"); // ✅ Corregido (sin genérico)

            if (groupIds == null || !groupIds.Any())
                return Result.Failure("Debe seleccionar al menos un grupo"); // ✅ Corregido (sin genérico)

            if (groupIds.Any(id => id <= 0))
                return Result.Failure("IDs de grupo inválidos"); // ✅ Corregido (sin genérico)

            try
            {
                // Verificar que los grupos existan
                var allGroups = await _groupRepository.GetAllGroupsAsync();
                var existingGroupIds = allGroups.Select(g => g.GroupId).ToHashSet();

                var invalidIds = groupIds.Where(id => !existingGroupIds.Contains(id)).ToList();
                if (invalidIds.Any())
                    return Result.Failure($"Los siguientes grupos no existen: {string.Join(", ", invalidIds)}"); // ✅ Corregido (sin genérico)

                await _assignmentRepository.AssignGroupsToUserAsync(userId, groupIds);

                return Result.Success(); // ✅ Corregido (sin genérico)
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al asignar grupos: {ex.Message}"); // ✅ Corregido (sin genérico)
            }
        }

        public async Task<Result> RemoveGroupFromUserAsync(int userId, long groupId)
        {
            if (userId <= 0)
                return Result.Failure("ID de usuario inválido"); // ✅ Corregido (sin genérico)

            if (groupId <= 0)
                return Result.Failure("ID de grupo inválido"); // ✅ Corregido (sin genérico)

            try
            {
                await _assignmentRepository.RemoveGroupFromUserAsync(userId, groupId);
                return Result.Success(); // ✅ Corregido (sin genérico)
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al quitar grupo: {ex.Message}"); // ✅ Corregido (sin genérico)
            }
        }

        public async Task<Result<List<UserDto>>> FilterUsersAsync(FilterUsersDto filter)
        {
            try
            {
                var users = await _businessRepository.FilterUsersAsync(
                    filter.Name,
                    filter.DepartmentId,
                    filter.AreaId,
                    filter.SubareaId);

                var userDtos = users.Select(u => new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName
                }).ToList();

                return Result<List<UserDto>>.Success(userDtos); // ✅ Corregido
            }
            catch (Exception ex)
            {
                return Result<List<UserDto>>.Failure($"Error al filtrar usuarios: {ex.Message}"); // ✅ Corregido
            }
        }
    }
}