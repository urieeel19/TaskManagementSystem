using Moq;
using TaskManagement.Application.UseCases.Tasks.GetById;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.UnitTests.UseCases.Tasks.GetById
{
    public class GetByIdHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly GetByIdHandler _handler;

        public GetByIdHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _handler = new GetByIdHandler(_taskRepositoryMock.Object);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Id_Is_Empty()
        {
            // Arrange
            var query = new GetByIdQuery(Guid.Empty);

            // Act
            var result = await _handler.HandleAsync(query, Guid.NewGuid());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("A valid task ID must be provided.", result.Error);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Task_Does_Not_Exist()
        {
            // Arrange
            var query = new GetByIdQuery(Guid.NewGuid());
            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(query.Id)).ReturnsAsync((TaskEntity?)null);

            // Act
            var result = await _handler.HandleAsync(query, Guid.NewGuid());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("The requested task does not exist.", result.Error);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_User_Is_Not_Owner()
        {
            // Arrange
            var authorizedUser = Guid.NewGuid();
            var unauthorizedUser = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var targetTask = new TaskEntity { Id = taskId, UserId = authorizedUser, Title = "Private Task" };

            var query = new GetByIdQuery(taskId);
            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(targetTask);

            // Act
            var result = await _handler.HandleAsync(query, unauthorizedUser);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("You do not have permission to view this task.", result.Error);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Mapped_Response_When_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var targetTask = new TaskEntity { Id = taskId, UserId = userId, Title = "Target Task", Description = "Some Description" };

            var query = new GetByIdQuery(taskId);
            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(targetTask);

            // Act
            var result = await _handler.HandleAsync(query, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Target Task", result.Value.Title);
            Assert.Equal("Some Description", result.Value.Description);
        }
    }
}
