using Naviguard.Application.Common;
using Naviguard.Application.DTOs;

namespace Naviguard.Application.Interfaces
{
    public interface IUserAssignmentService
    {
        Task<Result<List<GroupDto>>> GetGroupsByUserIdAsync(int userId);
        Task<Result> AssignGroupsToUserAsync(int userId, List<long> groupIds);
        Task<Result> RemoveGroupFromUserAsync(int userId, long groupId);
        Task<Result<List<UserDto>>> FilterUsersAsync(FilterUsersDto filter);
    }
}