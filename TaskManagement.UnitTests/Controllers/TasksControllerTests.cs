using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagement.API.Controllers;
using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Application.UseCases.Tasks.Create;
using TaskManagement.Application.UseCases.Tasks.Delete;
using TaskManagement.Application.UseCases.Tasks.GetById;
using TaskManagement.Application.UseCases.Tasks.GetTasksByUserId;
using TaskManagement.Application.UseCases.Tasks.Update;
using TaskManagement.Domain.Common;

namespace TaskManagement.UnitTests.Controllers
{
    public class TasksControllerTests
    {
        private readonly TasksController _controller;
        private readonly Guid _authenticatedUserId;

        public TasksControllerTests()
        {
            _controller = new TasksController();
            _authenticatedUserId = Guid.NewGuid();

            // Setup a mock HttpContext with JWT User claims to bypass GetAuthenticatedUserId() requirements
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, _authenticatedUserId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        // ==========================================
        // TESTS FOR: GET ALL TASKS
        // ==========================================

        [Fact]
        public async Task GetAll_Should_Return_Ok_With_Tasks_When_Successful()
        {
            // Arrange
            var mockHandler = new Mock<IQueryHandler<GetTasksByUserIdQuery, Result<IEnumerable<GetTaskResponse>>>>();
            var responses = new List<GetTaskResponse> { new(Guid.NewGuid(), "Task 1", "Desc 1", 0, DateTime.UtcNow) };

            mockHandler.Setup(h => h.HandleAsync(It.IsAny<GetTasksByUserIdQuery>(), _authenticatedUserId))
                .ReturnsAsync(Result<IEnumerable<GetTaskResponse>>.Success(responses));

            // Act
            var actionResult = await _controller.GetAll(mockHandler.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedValue = Assert.IsAssignableFrom<IEnumerable<GetTaskResponse>>(okResult.Value);
            Assert.Single(returnedValue);
        }

        [Fact]
        public async Task GetAll_Should_Return_BadRequest_When_Handler_Fails()
        {
            // Arrange
            var mockHandler = new Mock<IQueryHandler<GetTasksByUserIdQuery, Result<IEnumerable<GetTaskResponse>>>>();
            mockHandler.Setup(h => h.HandleAsync(It.IsAny<GetTasksByUserIdQuery>(), _authenticatedUserId))
                .ReturnsAsync(Result<IEnumerable<GetTaskResponse>>.Failure("Database connectivity issue."));

            // Act
            var actionResult = await _controller.GetAll(mockHandler.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequestResult.Value);
        }

        // ==========================================
        // TESTS FOR: GET TASK BY ID
        // ==========================================

        [Fact]
        public async Task GetById_Should_Return_Ok_With_Task_When_Found()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var mockHandler = new Mock<IQueryHandler<GetByIdQuery, Result<GetByIdResponse>>>();
            var response = new GetByIdResponse(taskId, "Task 1", "Desc 1", 0, DateTime.UtcNow);

            mockHandler.Setup(h => h.HandleAsync(It.Is<GetByIdQuery>(q => q.Id == taskId), _authenticatedUserId))
                .ReturnsAsync(Result<GetByIdResponse>.Success(response));

            // Act
            var actionResult = await _controller.GetById(taskId, mockHandler.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedValue = Assert.IsType<GetByIdResponse>(okResult.Value);
            Assert.Equal(taskId, returnedValue.Id);
        }

        [Fact]
        public async Task GetById_Should_Return_BadRequest_When_Task_Does_Not_Exist()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var mockHandler = new Mock<IQueryHandler<GetByIdQuery, Result<GetByIdResponse>>>();

            mockHandler.Setup(h => h.HandleAsync(It.IsAny<GetByIdQuery>(), _authenticatedUserId))
                .ReturnsAsync(Result<GetByIdResponse>.Failure("The requested task does not exist."));

            // Act
            var actionResult = await _controller.GetById(taskId, mockHandler.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequestResult.Value);
        }

        // ==========================================
        // TESTS FOR: CREATE TASK
        // ==========================================

        [Fact]
        public async Task Create_Should_Return_CreatedAtAction_When_Command_Is_Valid()
        {
            // Arrange
            var command = new CreateTaskCommand("New Task", "Desc", 0, DateTime.UtcNow.AddDays(1));
            var mockHandler = new Mock<ICommandHandler<CreateTaskCommand, Result<CreateTaskResponse>>>();
            var response = new CreateTaskResponse(Guid.NewGuid(), "New Task", "Desc", 0, DateTime.UtcNow.AddDays(1));

            mockHandler.Setup(h => h.HandleAsync(command, _authenticatedUserId))
                .ReturnsAsync(Result<CreateTaskResponse>.Success(response));

            // Act
            var actionResult = await _controller.Create(command, mockHandler.Object);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(response.Id, createdResult.RouteValues?["id"]);
            Assert.Equal(response, createdResult.Value);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_When_Validation_Or_Business_Rules_Fail()
        {
            // Arrange
            var command = new CreateTaskCommand("", "Desc", 0, DateTime.UtcNow.AddDays(-1)); // Invalid
            var mockHandler = new Mock<ICommandHandler<CreateTaskCommand, Result<CreateTaskResponse>>>();

            mockHandler.Setup(h => h.HandleAsync(command, _authenticatedUserId))
                .ReturnsAsync(Result<CreateTaskResponse>.Failure("The task title cannot be empty."));

            // Act
            var actionResult = await _controller.Create(command, mockHandler.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequestResult.Value);
        }

        // ==========================================
        // TESTS FOR: UPDATE TASK
        // ==========================================

        [Fact]
        public async Task Update_Should_Return_Ok_With_Updated_Payload_When_Successful()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var incomingCommand = new UpdateTaskCommand(Guid.Empty, "Updated Title", "Updated Desc", 1, DateTime.UtcNow.AddDays(2));
            var mockHandler = new Mock<ICommandHandler<UpdateTaskCommand, Result<UpdateTaskResponse>>>();
            var response = new UpdateTaskResponse(taskId, "Updated Title", "Updated Desc", 1, DateTime.UtcNow.AddDays(2));

            // The controller overrides the internal command Id with the route parameter 'id' using C# record 'with' expression
            mockHandler.Setup(h => h.HandleAsync(It.Is<UpdateTaskCommand>(c => c.Id == taskId), _authenticatedUserId))
                .ReturnsAsync(Result<UpdateTaskResponse>.Success(response));

            // Act
            var actionResult = await _controller.Update(taskId, incomingCommand, mockHandler.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedValue = Assert.IsType<UpdateTaskResponse>(okResult.Value);
            Assert.Equal(taskId, returnedValue.Id);
        }

        [Fact]
        public async Task Update_Should_Return_BadRequest_When_MultiTenancy_Or_Rules_Fail()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var incomingCommand = new UpdateTaskCommand(taskId, "Hacked Title", "Desc", 0, DateTime.UtcNow);
            var mockHandler = new Mock<ICommandHandler<UpdateTaskCommand, Result<UpdateTaskResponse>>>();

            mockHandler.Setup(h => h.HandleAsync(It.IsAny<UpdateTaskCommand>(), _authenticatedUserId))
                .ReturnsAsync(Result<UpdateTaskResponse>.Failure("You do not have permission to modify this task."));

            // Act
            var actionResult = await _controller.Update(taskId, incomingCommand, mockHandler.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequestResult.Value);
        }

        // ==========================================
        // TESTS FOR: DELETE TASK
        // ==========================================

        [Fact]
        public async Task Delete_Should_Return_NoContent_When_Successful()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var mockHandler = new Mock<ICommandHandler<DeleteTaskCommand, Result>>();

            mockHandler.Setup(h => h.HandleAsync(It.Is<DeleteTaskCommand>(c => c.Id == taskId), _authenticatedUserId))
                .ReturnsAsync(Result.Success());

            // Act
            var actionResult = await _controller.Delete(taskId, mockHandler.Object);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task Delete_Should_Return_BadRequest_When_Target_Missing()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var mockHandler = new Mock<ICommandHandler<DeleteTaskCommand, Result>>();

            mockHandler.Setup(h => h.HandleAsync(It.IsAny<DeleteTaskCommand>(), _authenticatedUserId))
                .ReturnsAsync(Result.Failure("The requested task does not exist."));

            // Act
            var actionResult = await _controller.Delete(taskId, mockHandler.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.NotNull(badRequestResult.Value);
        }
    }
}
