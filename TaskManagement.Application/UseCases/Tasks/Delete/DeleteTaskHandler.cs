using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.UseCases.Tasks.Delete
{
    public class DeleteTaskHandler : ICommandHandler<DeleteTaskCommand, Result>
    {
        private readonly ITaskRepository _taskRepository;

        public DeleteTaskHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result> HandleAsync(DeleteTaskCommand command, Guid userId)
        {
            if (command.Id == Guid.Empty)
            {
                return Result.Failure("A valid task ID must be provided.");
            }

            var existingTask = await _taskRepository.GetByIdAsync(command.Id);
            if (existingTask == null)
            {
                return Result.Failure("The requested task does not exist.");
            }

            if (existingTask.UserId != userId)
            {
                return Result.Failure("You do not have permission to delete this task.");
            }

            await _taskRepository.DeleteAsync(command.Id);

            return Result.Success();
        }
    }
}
