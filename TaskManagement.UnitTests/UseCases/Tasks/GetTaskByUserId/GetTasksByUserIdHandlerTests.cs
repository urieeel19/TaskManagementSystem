using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.UseCases.Tasks.GetTasksByUserId;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.UnitTests.UseCases.Tasks.GetTaskByUserId
{
    public class GetTasksByUserIdHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly GetTasksByUserIdHandler _handler;

        public GetTasksByUserIdHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();

            // Instanciamos el SUT (System Under Test) inyectando el mock del repositorio
            _handler = new GetTasksByUserIdHandler(_taskRepositoryMock.Object);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Success_With_Empty_List_When_User_Has_No_Tasks()
        {
            // Arrange: Setup standard parameters and mock an empty database return payload
            var userId = Guid.NewGuid();
            var query = new GetTasksByUserIdQuery();
            var emptyList = new List<TaskEntity>();

            _taskRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(emptyList);

            // Act: Dispatch the read query straight into the isolated application handler
            var result = await _handler.HandleAsync(query, userId);

            // Assert: Verify that a user without tasks safely receives an empty collection rather than a failure state
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);

            // Verify infrastructure database read interaction occurred exactly once
            _taskRepositoryMock.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Success_With_Mapped_Responses_When_User_Has_Tasks()
        {
            // Arrange: Setup compliant mock domain entities attached to the authenticated user footprint
            var userId = Guid.NewGuid();
            var query = new GetTasksByUserIdQuery();

            var repositoryPayload = new List<TaskEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Review Pull Requests",
                    Description = "Check architecture constraints",
                    Status = TodoStatus.Pending,
                    DueDate = DateTime.UtcNow.AddDays(1)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Prepare Demo",
                    Description = "Present Clean Architecture slides",
                    Status = TodoStatus.Completed,
                    DueDate = DateTime.UtcNow.AddDays(2)
                }
            };

            _taskRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(repositoryPayload);

            // Act
            var result = await _handler.HandleAsync(query, userId);

            // Assert: Validate data capsule projections match internal specifications perfectly
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());

            // Deep validation of structural mapping features for the first item
            var firstItem = result.Value.First();
            Assert.Equal(repositoryPayload[0].Id, firstItem.Id);
            Assert.Equal("Review Pull Requests", firstItem.Title);
            Assert.Equal("Check architecture constraints", firstItem.Description);
            Assert.Equal((int)TodoStatus.Pending, firstItem.Status);
            Assert.Equal(repositoryPayload[0].DueDate, firstItem.DueDate);

            // Deep validation of structural mapping features for the second item
            var secondItem = result.Value.Last();
            Assert.Equal(repositoryPayload[1].Id, secondItem.Id);
            Assert.Equal("Prepare Demo", secondItem.Title);
            Assert.Equal("Present Clean Architecture slides", secondItem.Description);
            Assert.Equal((int)TodoStatus.Completed, secondItem.Status);
            Assert.Equal(repositoryPayload[1].DueDate, secondItem.DueDate);

            _taskRepositoryMock.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        }
    }
}
