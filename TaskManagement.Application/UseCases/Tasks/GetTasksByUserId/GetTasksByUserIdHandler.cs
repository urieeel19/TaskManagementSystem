using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.UseCases.Tasks.GetTasksByUserId
{
    public class GetTasksByUserIdHandler : IQueryHandler<GetTasksByUserIdQuery, Result<IEnumerable<GetTaskResponse>>>
    {
        private readonly ITaskRepository _taskRepository;

        public GetTasksByUserIdHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result<IEnumerable<GetTaskResponse>>> HandleAsync(GetTasksByUserIdQuery query, Guid userId)
        {
            // 1. Fetch domain entities from MongoDB matching the authenticated User ID
            var taskEntities = await _taskRepository.GetByUserIdAsync(userId);

            // 2. Project domain entities using our unified mapping extension
            var responseDto = taskEntities.ToResponseList();

            // 3. Return encapsulating result
            return Result<IEnumerable<GetTaskResponse>>.Success(responseDto);
        }
    }
}
