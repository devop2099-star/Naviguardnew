// Naviguard.Application/Services/PageService.cs
using Naviguard.Application.Common;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Application.UseCases.Pages;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.Services
{
    public class PageService : IPageService
    {
        private readonly CreatePageUseCase _createPageUseCase;
        private readonly UpdatePageUseCase _updatePageUseCase;
        private readonly IPageRepository _pageRepository;

        public PageService(
            CreatePageUseCase createPageUseCase,
            UpdatePageUseCase updatePageUseCase,
            IPageRepository pageRepository)
        {
            _createPageUseCase = createPageUseCase;
            _updatePageUseCase = updatePageUseCase;
            _pageRepository = pageRepository;
        }

        public async Task<Result<List<PageDto>>> GetAllPagesAsync()
        {
            try
            {
                var pages = await _pageRepository.GetAllPagesAsync();

                var pageDtos = pages.Select(p => new PageDto
                {
                    PageId = p.PageId,
                    PageName = p.PageName,
                    Description = p.Description,
                    Url = p.Url,
                    RequiresProxy = p.RequiresProxy,
                    RequiresLogin = p.RequiresLogin,
                    RequiresCustomLogin = p.RequiresCustomLogin,
                    RequiresRedirects = p.RequiresRedirects
                }).ToList();

                return Result<List<PageDto>>.Success(pageDtos); // ✅ Corregido
            }
            catch (Exception ex)
            {
                return Result<List<PageDto>>.Failure($"Error al obtener páginas: {ex.Message}"); // ✅ Corregido
            }
        }

        public async Task<Result<PageDto>> GetPageByIdAsync(long pageId)
        {
            try
            {
                var pages = await _pageRepository.GetAllPagesAsync();
                var page = pages.FirstOrDefault(p => p.PageId == pageId);

                if (page == null)
                    return Result<PageDto>.Failure($"No se encontró la página con ID {pageId}"); // ✅ Corregido

                var pageDto = new PageDto
                {
                    PageId = page.PageId,
                    PageName = page.PageName,
                    Description = page.Description,
                    Url = page.Url,
                    RequiresProxy = page.RequiresProxy,
                    RequiresLogin = page.RequiresLogin,
                    RequiresCustomLogin = page.RequiresCustomLogin,
                    RequiresRedirects = page.RequiresRedirects
                };

                return Result<PageDto>.Success(pageDto); // ✅ Corregido
            }
            catch (Exception ex)
            {
                return Result<PageDto>.Failure($"Error al obtener la página: {ex.Message}"); // ✅ Corregido
            }
        }

        public async Task<Result<long>> CreatePageAsync(CreatePageDto dto)
        {
            return await _createPageUseCase.ExecuteAsync(dto);
        }

        public async Task<Result> UpdatePageAsync(UpdatePageDto dto)
        {
            return await _updatePageUseCase.ExecuteAsync(dto);
        }

        public async Task<Result> DeletePageAsync(long pageId)
        {
            if (pageId <= 0)
                return Result.Failure("ID de página inválido"); // ✅ Corregido (sin genérico)

            try
            {
                await _pageRepository.SoftDeletePageAsync(pageId);
                return Result.Success(); // ✅ Corregido (sin genérico)
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al eliminar la página: {ex.Message}"); // ✅ Corregido (sin genérico)
            }
        }

        public async Task<Result<List<PageDto>>> GetPagesRequiringCustomLoginAsync()
        {
            try
            {
                var pages = await _pageRepository.GetPagesRequiringCustomLoginAsync();

                var pageDtos = pages.Select(p => new PageDto
                {
                    PageId = p.PageId,
                    PageName = p.PageName,
                    Description = p.Description,
                    Url = p.Url,
                    RequiresProxy = p.RequiresProxy,
                    RequiresLogin = p.RequiresLogin,
                    RequiresCustomLogin = p.RequiresCustomLogin,
                    RequiresRedirects = p.RequiresRedirects
                }).ToList();

                return Result<List<PageDto>>.Success(pageDtos); // ✅ Corregido
            }
            catch (Exception ex)
            {
                return Result<List<PageDto>>.Failure($"Error: {ex.Message}"); // ✅ Corregido
            }
        }
    }
}