using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Interfaces.Common;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.UseCases.Auth.Register
{
    public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<RegisterUserCommand> _validator;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IValidator<RegisterUserCommand> validator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _validator = validator;
        }

        public async Task<Result<RegisterUserResponse>> HandleAsync(RegisterUserCommand command, Guid userId)
        {
            // 1. Structural Validation (Fail-Fast)
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Result<RegisterUserResponse>.Failure(validationResult.Errors[0].ErrorMessage);
            }

            var existingUser = await _userRepository.GetByUsernameAsync(command.Username.Trim());
            if (existingUser != null)
            {
                return Result<RegisterUserResponse>.Failure("Username is already taken.");
            }

            // 3. Domain Entity Creation & Cryptography
            var userEntity = new User
            {
                Id = Guid.NewGuid(),
                Username = command.Username.Trim(),
                PasswordHash = _passwordHasher.HashPassword(command.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(userEntity);

            // 4. Retornamos éxito instanciando el record vacío
            return Result<RegisterUserResponse>.Success(new RegisterUserResponse());
        }
    }
}
