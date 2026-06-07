using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Moq;
using TaskManagement.Application.UseCases.Tasks.Update;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.UnitTests.UseCases.Tasks.Update
{
    public class UpdateTaskHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IValidator<UpdateTaskCommand>> _validatorMock;
        private readonly UpdateTaskHandler _handler;

        public UpdateTaskHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _validatorMock = new Mock<IValidator<UpdateTaskCommand>>();

            // Instanciamos el SUT (System Under Test) inyectando ambos mocks
            _handler = new UpdateTaskHandler(_taskRepositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_FluentValidation_Fails()
        {
            // Arrange: Step 1 - Setup a command that will trigger a structural validation error
            var userId = Guid.NewGuid();
            var command = new UpdateTaskCommand(Guid.NewGuid(), "", "Desc", 0, DateTime.UtcNow.AddDays(1));

            var validationFailures = new List<ValidationFailure>
            {
                new("Title", "The task title cannot be empty.")
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert: Execution must short-circuit immediately
            Assert.True(result.IsFailure);
            Assert.Equal("The task title cannot be empty.", result.Error);

            // Verify infrastructure methods are completely bypassed
            _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _taskRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<TaskEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Task_Does_Not_Exist()
        {
            // Arrange: Step 2 - Validation passes but MongoDB returns null simulation
            var userId = Guid.NewGuid();
            var command = new UpdateTaskCommand(Guid.NewGuid(), "Valid Title", "Desc", 0, DateTime.UtcNow.AddDays(1));

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id))
                .ReturnsAsync((TaskEntity?)null);

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("The requested task does not exist.", result.Error);

            // Verify update mutation is never triggered
            _taskRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<TaskEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_User_Is_Not_The_Owner()
        {
            // Arrange: Step 3 - Multi-tenancy security boundary check (IDOR Protection)
            var attackerUserId = Guid.NewGuid();
            var victimUserId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var command = new UpdateTaskCommand(taskId, "Hacked Title", "Desc", 0, DateTime.UtcNow.AddDays(1));
            var existingTask = new TaskEntity { Id = taskId, UserId = victimUserId, Title = "Original Title" };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _handler.HandleAsync(command, attackerUserId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("You do not have permission to modify this task.", result.Error);

            // Verify data integrity is safeguarded and update is bypassed
            _taskRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<TaskEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_DueDate_Is_In_The_Past()
        {
            // Arrange: Step 4 - Business rule validation (Forbidden past dates)
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var command = new UpdateTaskCommand(taskId, "Valid Title", "Desc", 0, DateTime.UtcNow.AddDays(-2)); // Past date
            var existingTask = new TaskEntity { Id = taskId, UserId = userId, Title = "Original Title" };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("The due date cannot be set in the past.", result.Error);

            // Verify changes were never persisted
            _taskRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<TaskEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Mutate_Fields_Save_And_Return_Success_When_All_Criteria_Are_Met()
        {
            // Arrange: Steps 5, 6 & 7 - Happy Path execution flow
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var futureDate = DateTime.UtcNow.AddDays(5);

            var command = new UpdateTaskCommand(taskId, "  Clean Architecture Update   ", "New Description", (int)TodoStatus.Completed, futureDate);
            var existingTask = new TaskEntity
            {
                Id = taskId,
                UserId = userId,
                Title = "Old Title",
                Description = "Old Description",
                Status = TodoStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            // Verify safe property mutation (Step 5) occurred correctly (including Trim)
            Assert.Equal("Clean Architecture Update", existingTask.Title);
            Assert.Equal("New Description", existingTask.Description);
            Assert.Equal(TodoStatus.Completed, existingTask.Status);
            Assert.Equal(futureDate, existingTask.DueDate);

            // Verify mapping matches response DTO contract specifications (Step 7)
            Assert.Equal(taskId, result.Value.Id);
            Assert.Equal("Clean Architecture Update", result.Value.Title);
            Assert.Equal("New Description", result.Value.Description);
            Assert.Equal((int)TodoStatus.Completed, result.Value.Status);
            Assert.Equal(futureDate, result.Value.DueDate);

            // Verify persistency interaction (Step 6) was executed exactly once with the modified entity
            _taskRepositoryMock.Verify(repo => repo.UpdateAsync(existingTask), Times.Once);
        }
    }
}
