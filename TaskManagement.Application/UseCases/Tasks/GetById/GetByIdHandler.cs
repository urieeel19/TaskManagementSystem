using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.UseCases.Tasks.GetById
{
    public class GetByIdHandler : IQueryHandler<GetByIdQuery, Result<GetByIdResponse>>
    {
        private readonly ITaskRepository _taskRepository;

        public GetByIdHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result<GetByIdResponse>> HandleAsync(GetByIdQuery query, Guid userId)
        {
            // 1. Structural Validation (Fail-Fast)
            if (query.Id == Guid.Empty)
            {
                return Result<GetByIdResponse>.Failure("A valid task ID must be provided.");
            }

            // 2. Fetch the entity from MongoDB
            var task = await _taskRepository.GetByIdAsync(query.Id);
            if (task == null)
            {
                return Result<GetByIdResponse>.Failure("The requested task does not exist.");
            }

            // 3. Multi-tenancy Security Check: Ensure ownership
            if (task.UserId != userId)
            {
                return Result<GetByIdResponse>.Failure("You do not have permission to view this task.");
            }

            // 4. Map Domain Entity to Output DTO
            var response = task.ToResponse();

            return Result<GetByIdResponse>.Success(response);
        }
    }
}
