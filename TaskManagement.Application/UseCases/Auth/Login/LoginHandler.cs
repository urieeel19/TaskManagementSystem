using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.UseCases.Auth.Login
{
    public class LoginHandler : ICommandHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IValidator<LoginCommand> _validator;

        public LoginHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IValidator<LoginCommand> validator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _validator = validator;
        }

        public async Task<Result<LoginResponse>> HandleAsync(LoginCommand command, Guid userId)
        {
            // Structural Validation (Fail-Fast)
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Result<LoginResponse>.Failure(validationResult.Errors[0].ErrorMessage);
            }

            var user = await _userRepository.GetByUsernameAsync(command.Username.Trim());
            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid username or password.");
            }

            // Cryptographic Verification
            var isValidPassword = _passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                return Result<LoginResponse>.Failure("Invalid username or password.");
            }

            //Token Generation
            var token = _tokenService.GenerateToken(user);

            // Output Projection
            var response = new LoginResponse(token, user.Username);

            return Result<LoginResponse>.Success(response);
        }
    }
}
