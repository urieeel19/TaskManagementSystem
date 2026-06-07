using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskManagement.Application.UseCases.Tasks.Create;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.UnitTests.UseCases.Tasks.Create
{
    public class CreateTaskHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IValidator<CreateTaskCommand>> _validatorMock;
        private readonly CreateTaskHandler _handler;

        public CreateTaskHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _validatorMock = new Mock<IValidator<CreateTaskCommand>>();

            // Instanciamos el SUT (System Under Test) inyectando los mocks creados
            _handler = new CreateTaskHandler(_taskRepositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Success_And_Call_Repository_When_Command_Is_Valid()
        {
            // Arrange: Setup standard parameters and future date to avoid past validation failures
            var userId = Guid.NewGuid();
            var command = new CreateTaskCommand(
                "Complete Tech Challenge",
                "Implement Clean Architecture and CQS",
                (int)TodoStatus.Pending,
                DateTime.UtcNow.AddDays(3)
            );

            // Mock an empty validation result (signaling no structural validation errors)
            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act: Dispatch command into isolated Application handler workflow
            var result = await _handler.HandleAsync(command, userId);

            // Assert: Assure state validation logic branches execute expected path
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal("Complete Tech Challenge", result.Value.Title);

            // Verify infrastructure persistency interaction was hit exactly once
            _taskRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TaskEntity>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_FluentValidation_Fails()
        {
            // Arrange: Setup command which will simulate structural validation error
            var userId = Guid.NewGuid();
            var command = new CreateTaskCommand("", "Description", 0, DateTime.UtcNow.AddDays(1));

            var validationFailures = new List<ValidationFailure>
            {
                new("Title", "The task title cannot be empty.")
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert: Fail-fast protocol stops the pipe execution flow
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("The task title cannot be empty.", result.Error);

            // Verify database repository interaction was bypassed completely
            _taskRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TaskEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_DueDate_Is_In_The_Past()
        {
            // Arrange: Force payload to target a forbidden past date constraints 
            var userId = Guid.NewGuid();
            var command = new CreateTaskCommand(
                "Past Task",
                "Description",
                0,
                DateTime.UtcNow.AddDays(-5) // Past date
            );

            // Structural checking is simulated as compliant to reach business domain check
            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert: Ensure business rules safeguard database constraints
            Assert.True(result.IsFailure);
            Assert.Equal("The due date cannot be in the past.", result.Error);

            // Database shouldn't be touched on invalid domain states
            _taskRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TaskEntity>()), Times.Never);
        }
    }
}
