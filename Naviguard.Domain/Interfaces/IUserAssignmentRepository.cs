using Naviguard.Domain.Entities;

namespace Naviguard.Domain.Interfaces
{
    public interface IUserAssignmentRepository
    {
        Task<List<Group>> GetGroupsByUserIdAsync(int userId);
        Task AssignGroupsToUserAsync(int userId, List<long> groupIds);
        Task RemoveGroupFromUserAsync(int userId, long groupId);
    }
}