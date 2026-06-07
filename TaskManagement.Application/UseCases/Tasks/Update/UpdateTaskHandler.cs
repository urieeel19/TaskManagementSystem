using FluentValidation;
using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.UseCases.Tasks.Update
{
    public class UpdateTaskHandler : ICommandHandler<UpdateTaskCommand, Result<UpdateTaskResponse>>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IValidator<UpdateTaskCommand> _validator;

        public UpdateTaskHandler(ITaskRepository taskRepository, IValidator<UpdateTaskCommand> validator)
        {
            _taskRepository = taskRepository;
            _validator = validator;
        }

        public async Task<Result<UpdateTaskResponse>> HandleAsync(UpdateTaskCommand command, Guid userId)
        {
            // 1. Structural Validation (Fail-Fast)
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Result<UpdateTaskResponse>.Failure(validationResult.Errors[0].ErrorMessage);
            }

            // 2. Retrieve entity from MongoDB
            var existingTask = await _taskRepository.GetByIdAsync(command.Id);
            if (existingTask == null)
            {
                return Result<UpdateTaskResponse>.Failure("The requested task does not exist.");
            }

            // 3. Multi-tenancy Security Check: Ensure the user owns the task
            if (existingTask.UserId != userId)
            {
                return Result<UpdateTaskResponse>.Failure("You do not have permission to modify this task.");
            }

            // 4. Business Rule: Do not allow active tasks to be postponed to a past date
            if (command.DueDate.Date < DateTime.UtcNow.Date)
            {
                return Result<UpdateTaskResponse>.Failure("The due date cannot be set in the past.");
            }

            // 5. Safe property mutation in the Application Layer
            existingTask.Title = command.Title.Trim();
            existingTask.Description = command.Description; 
            existingTask.Status = (TodoStatus)command.Status;
            existingTask.DueDate = command.DueDate;

            // 6. Persist changes in MongoDB
            await _taskRepository.UpdateAsync(existingTask);

            // 7. Map and return the Output DTO
            var response = new UpdateTaskResponse(
                existingTask.Id,
                existingTask.Title,
                existingTask.Description,
                (int)existingTask.Status,
                existingTask.DueDate
            );

            return Result<UpdateTaskResponse>.Success(response);
        }
    }
}
