using Naviguard.Application.Common;
using Naviguard.Domain.Interfaces;

namespace Naviguard.Application.UseCases.Groups
{
    public class DeleteGroupUseCase
    {
        private readonly IGroupRepository _groupRepository;

        public DeleteGroupUseCase(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<Result> ExecuteAsync(long groupId)
        {
            if (groupId <= 0)
                return Result.Failure("ID de grupo inválido");

            try
            {
                await _groupRepository.SoftDeleteGroupAsync(groupId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al eliminar el grupo: {ex.Message}");
            }
        }
    }
}