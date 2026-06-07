using FluentValidation;
using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.UseCases.Tasks.Create
{
    public class CreateTaskHandler : ICommandHandler<CreateTaskCommand, Result<CreateTaskResponse>>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IValidator<CreateTaskCommand> _validator;

        public CreateTaskHandler(ITaskRepository taskRepository, IValidator<CreateTaskCommand> validator)
        {
            _taskRepository = taskRepository;
            _validator = validator;
        }

        public async Task<Result<CreateTaskResponse>> HandleAsync(CreateTaskCommand command, Guid userId)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Result<CreateTaskResponse>.Failure(validationResult.Errors[0].ErrorMessage);
            }

            if (command.DueDate.Date < DateTime.UtcNow.Date)
            {
                return Result<CreateTaskResponse>.Failure("The due date cannot be in the past.");
            }

            var taskEntity = new TaskEntity
            {
                Id = Guid.NewGuid(),
                Title = command.Title.Trim(),
                Description = command.Description,
                Status = (TodoStatus)command.Status,
                DueDate = command.DueDate,
                UserId = userId
            };

            await _taskRepository.AddAsync(taskEntity);

            var response = new CreateTaskResponse(taskEntity.Id, taskEntity.Title, taskEntity.Description, (int)taskEntity.Status, taskEntity.DueDate);
            return Result<CreateTaskResponse>.Success(response);
        }
    }
}
