using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.UseCases.Groups
{
    public class GetGroupsUseCase
    {
        private readonly IGroupRepository _groupRepository;

        public GetGroupsUseCase(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<Result<List<GroupDto>>> ExecuteAsync(bool includePages = false)
        {
            try
            {
                var groups = includePages
                    ? await _groupRepository.GetGroupsWithPagesAsync()
                    : await _groupRepository.GetAllGroupsAsync();

                var groupDtos = groups.Select(g => new GroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    Pin = g.Pin,
                    Pages = g.Pages.Select(p => new PageDto
                    {
                        PageId = p.PageId,
                        PageName = p.PageName,
                        Description = p.Description,
                        Url = p.Url,
                        RequiresProxy = p.RequiresProxy,
                        RequiresLogin = p.RequiresLogin,
                        RequiresCustomLogin = p.RequiresCustomLogin,
                        RequiresRedirects = p.RequiresRedirects,
                        PinInGroup = p.PinInGroup
                    }).ToList()
                }).ToList();

                return Result<List<GroupDto>>.Success(groupDtos); // ✅ CORREGIDO
            }
            catch (Exception ex)
            {
                return Result<List<GroupDto>>.Failure($"Error al obtener grupos: {ex.Message}"); // ✅ CORREGIDO
            }
        }
    }
}