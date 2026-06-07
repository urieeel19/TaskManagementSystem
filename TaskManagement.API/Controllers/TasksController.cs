using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Application.UseCases.Tasks.Create;
using TaskManagement.Application.UseCases.Tasks.Delete;
using TaskManagement.Application.UseCases.Tasks.GetById;
using TaskManagement.Application.UseCases.Tasks.GetTasksByUserId;
using TaskManagement.Application.UseCases.Tasks.Update;
using TaskManagement.Domain.Common;

namespace TaskManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        public TasksController(){}
        private IActionResult HandleResult(Result result, Func<IActionResult> onSuccess)
        {
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }
            return onSuccess();
        }

        private Guid GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User identification token claim is missing or corrupt.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] IQueryHandler<GetTasksByUserIdQuery, Result<IEnumerable<GetTaskResponse>>> handler)
        {
            // 1. Extract the authenticated user ID from the security context (JWT Claims)
            Guid userId = GetAuthenticatedUserId();

            // 2. Instantiate our empty query token
            var query = new GetTasksByUserIdQuery();

            // 3. Dispatch the query to the dedicated handler
            var result = await handler.HandleAsync(query, userId);

            // 4. Return HTTP 200 OK with the collection list
            return HandleResult(result, () => Ok(result.Value));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id,[FromServices] IQueryHandler<GetByIdQuery, Result<GetByIdResponse>> handler)
        {
            Guid userId = GetAuthenticatedUserId();

            var query = new GetByIdQuery(id);

            // 3. Dispatch the query straight to the isolated application slice
            var result = await handler.HandleAsync(query, userId);

            // 4. Process the domain outcome
            return HandleResult(result, () => Ok(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskCommand command, [FromServices] ICommandHandler<CreateTaskCommand, Result<CreateTaskResponse>> handler)
        {
            // 1. Extract the authenticated user ID safely from the JWT token
            Guid userId = GetAuthenticatedUserId();

            // 2. Dispatch the command directly to the application layer handler
            var result = await handler.HandleAsync(command, userId);

            // 3. Process the domain outcome
            // If successful, it automatically returns a 201 Created with the location header pointing to GetById
            return HandleResult(result, () =>
                CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskCommand command, [FromServices] ICommandHandler<UpdateTaskCommand, Result<UpdateTaskResponse>> handler)
        {
            var commandWithCorrectId = command with { Id = id };

            var userId = GetAuthenticatedUserId();

            var result = await handler.HandleAsync(commandWithCorrectId, userId);

            return HandleResult(result, () => Ok(result.Value));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromServices] ICommandHandler<DeleteTaskCommand, Result> handler)
        {
            // 1. Extract the authenticated user ID safely from the token
            Guid userId = GetAuthenticatedUserId();

            // 2. Wrap the route parameter into our semantic CQS Command
            var command = new DeleteTaskCommand(id);

            // 3. Dispatch the command directly to the application layer handler
            var result = await handler.HandleAsync(command, userId);

            // 4. Process the domain outcome. If successful, returns HTTP 204 No Content
            return HandleResult(result, () => NoContent());
        }
    }
}
