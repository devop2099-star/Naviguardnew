using Naviguard.Domain.Entities;

namespace Naviguard.Domain.Interfaces
{
    public interface IBusinessStructureRepository
    {
        Task<List<BusinessDepartment>> GetDepartmentsAsync();
        Task<List<BusinessArea>> GetAreasByDepartmentAsync(int departmentId);
        Task<List<BusinessSubarea>> GetSubareasByAreaAsync(int areaId);
        Task<List<FilteredUser>> FilterUsersAsync(
            string? name,
            int? departmentId,
            int? areaId,
            int? subareaId);
    }
}