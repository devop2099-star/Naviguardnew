using Naviguard.Domain.Entities;
using Naviguard.Domain.ValueObjects;

namespace Naviguard.Domain.Interfaces
{
    public interface IGroupRepository
    {
        // Consultas
        Task<List<Group>> GetAllGroupsAsync();
        Task<List<Group>> GetGroupsWithPagesAsync();
        Task<Group?> GetGroupByIdAsync(long groupId);
        Task<List<Pagina>> GetPagesByGroupIdAsync(long groupId);

        // Comandos
        Task<long> AddGroupAsync(string groupName, string description);
        Task UpdateGroupAsync(Group group, List<PageAssignmentInfo> pages);
        Task SoftDeleteGroupAsync(long groupId);
    }
}