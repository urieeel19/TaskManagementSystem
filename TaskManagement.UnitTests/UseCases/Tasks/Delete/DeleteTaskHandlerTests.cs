using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.UseCases.Tasks.Delete;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.UnitTests.UseCases.Tasks.Delete
{
    public class DeleteTaskHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly DeleteTaskHandler _handler;

        public DeleteTaskHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();

            // Instanciamos el SUT (System Under Test) inyectando el mock del repositorio
            _handler = new DeleteTaskHandler(_taskRepositoryMock.Object);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Id_Is_Empty()
        {
            // Arrange: Setup an invalid payload token with Guid.Empty to trigger structural validation
            var command = new DeleteTaskCommand(Guid.Empty);
            var userId = Guid.NewGuid();

            // Act: Dispatch directly into the handler pipe
            var result = await _handler.HandleAsync(command, userId);

            // Assert: Ensure the Fail-Fast strategy intercepts before database trips
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("A valid task ID must be provided.", result.Error);

            // Critical Check: No database interaction should occur when validation blocks execution
            _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _taskRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Task_Does_Not_Exist()
        {
            // Arrange: Setup valid structured inputs but simulate database missing entity
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new DeleteTaskCommand(taskId);

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync((TaskEntity?)null);

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert: Execution completes and reports missing resource domain safely
            Assert.True(result.IsFailure);
            Assert.Equal("The requested task does not exist.", result.Error);

            // Ensure delete mutator state was never reached
            _taskRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_User_Is_Not_The_Owner()
        {
            // Arrange: Setup multi-tenancy context isolation boundary breach simulation
            var attackerUserId = Guid.NewGuid();
            var victimUserId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var command = new DeleteTaskCommand(taskId);
            var existingTask = new TaskEntity
            {
                Id = taskId,
                UserId = victimUserId, // Belongs to a different identity footprint
                Title = "Sensitive Business Task"
            };

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _handler.HandleAsync(command, attackerUserId);

            // Assert: Interceptor prevents IDOR (Insecure Direct Object Reference) modification attack
            Assert.True(result.IsFailure);
            Assert.Equal("You do not have permission to delete this task.", result.Error);

            // Confirm isolation boundary safeguards database mutation flow completely
            _taskRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Call_Repository_Delete_And_Return_Success_When_Authorized()
        {
            // Arrange: Everything matches perfectly compliant criteria mapping
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var command = new DeleteTaskCommand(taskId);
            var existingTask = new TaskEntity
            {
                Id = taskId,
                UserId = userId, // Authenticated owner
                Title = "Task to be purged"
            };

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _handler.HandleAsync(command, userId);

            // Assert: Processing complete state returns successful void outcomes
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);

            // Verify infrastructure database mutation action happens exactly once
            _taskRepositoryMock.Verify(repo => repo.DeleteAsync(taskId), Times.Once);
        }
    }
}
