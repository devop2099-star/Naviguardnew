using Naviguard.Application.Common;
using Naviguard.Application.DTOs;

namespace Naviguard.Application.Interfaces
{
    public interface IGroupService
    {
        Task<Result<List<GroupDto>>> GetAllGroupsAsync();
        Task<Result<List<GroupDto>>> GetGroupsWithPagesAsync();
        Task<Result<GroupDto>> GetGroupByIdAsync(long groupId);
        Task<Result<long>> CreateGroupAsync(CreateGroupDto dto);
        Task<Result> UpdateGroupAsync(UpdateGroupDto dto);
        Task<Result> DeleteGroupAsync(long groupId);
    }
}