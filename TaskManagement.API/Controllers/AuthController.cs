using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Application.UseCases.Auth.Login;
using TaskManagement.Application.UseCases.Auth.Register;
using TaskManagement.Domain.Common;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public AuthController() { }

        /// <summary>
        /// Centralizes domain results management, mapping failures to HTTP 400 automatically.
        /// </summary>
        private IActionResult HandleResult<T>(Result<T> result, Func<IActionResult> onSuccess)
        {
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }
            return onSuccess();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command,[FromServices] ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>> handler)
        {
            // 1. Dispatch the registration command directly to its isolated handler.
            // As registration does not require a tracking user context, we pass Guid.Empty.
            var result = await handler.HandleAsync(command, Guid.Empty);

            // 2. Process the outcome. If successful, returns HTTP 204 No Content for the frontend redirection.
            return HandleResult(result, () => NoContent());
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command,[FromServices] ICommandHandler<LoginCommand, Result<LoginResponse>> handler)
        {
            // 1. Dispatch the authentication command directly to its isolated handler.
            var result = await handler.HandleAsync(command, Guid.Empty);

            // 2. Process the outcome. If successful, returns HTTP 200 OK with the generated security payload.
            return HandleResult(result, () => Ok(result.Value));
        }
    }
}
