using Naviguard.Application.Common;
using Naviguard.Application.DTOs;

namespace Naviguard.Application.Interfaces
{
    public interface IPageService
    {
        Task<Result<List<PageDto>>> GetAllPagesAsync();
        Task<Result<PageDto>> GetPageByIdAsync(long pageId);
        Task<Result<long>> CreatePageAsync(CreatePageDto dto);
        Task<Result> UpdatePageAsync(UpdatePageDto dto);
        Task<Result> DeletePageAsync(long pageId);
        Task<Result<List<PageDto>>> GetPagesRequiringCustomLoginAsync();
    }
}