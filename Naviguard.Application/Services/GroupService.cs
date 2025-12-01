// Naviguard.Application/Services/GroupService.cs
using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Application.UseCases.Groups;

namespace Naviguard.Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly GetGroupsUseCase _getGroupsUseCase;
        private readonly CreateGroupUseCase _createGroupUseCase;
        private readonly UpdateGroupUseCase _updateGroupUseCase;
        private readonly DeleteGroupUseCase _deleteGroupUseCase;

        public GroupService(
            GetGroupsUseCase getGroupsUseCase,
            CreateGroupUseCase createGroupUseCase,
            UpdateGroupUseCase updateGroupUseCase,
            DeleteGroupUseCase deleteGroupUseCase)
        {
            _getGroupsUseCase = getGroupsUseCase;
            _createGroupUseCase = createGroupUseCase;
            _updateGroupUseCase = updateGroupUseCase;
            _deleteGroupUseCase = deleteGroupUseCase;
        }

        public async Task<Result<List<GroupDto>>> GetAllGroupsAsync()
        {
            return await _getGroupsUseCase.ExecuteAsync(includePages: false);
        }

        public async Task<Result<List<GroupDto>>> GetGroupsWithPagesAsync()
        {
            return await _getGroupsUseCase.ExecuteAsync(includePages: true);
        }

        public async Task<Result<GroupDto>> GetGroupByIdAsync(long groupId)
        {
            var result = await _getGroupsUseCase.ExecuteAsync(includePages: true);

            if (!result.IsSuccess)
                return Result<GroupDto>.Failure(result.Error); // ✅ Corregido

            var group = result.Value?.FirstOrDefault(g => g.GroupId == groupId);

            if (group == null)
                return Result<GroupDto>.Failure($"No se encontró el grupo con ID {groupId}"); // ✅ Corregido

            return Result<GroupDto>.Success(group); // ✅ Corregido
        }

        public async Task<Result<long>> CreateGroupAsync(CreateGroupDto dto)
        {
            return await _createGroupUseCase.ExecuteAsync(dto);
        }

        public async Task<Result> UpdateGroupAsync(UpdateGroupDto dto)
        {
            return await _updateGroupUseCase.ExecuteAsync(dto);
        }

        public async Task<Result> DeleteGroupAsync(long groupId)
        {
            return await _deleteGroupUseCase.ExecuteAsync(groupId);
        }
    }
}