using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.UseCases.Auth.Register;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.UnitTests.UseCases.Auth.Register
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IValidator<RegisterUserCommand>> _validatorMock;
        private readonly RegisterUserHandler _handler;

        public RegisterUserHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _validatorMock = new Mock<IValidator<RegisterUserCommand>>();

            _handler = new RegisterUserHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _validatorMock.Object);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Structural_Validation_Fails()
        {
            // Arrange
            var command = new RegisterUserCommand("invalid_user", "123", "different_123");
            var validationFailures = new List<ValidationFailure>
            {
                new("ConfirmPassword", "Passwords do not match.")
            };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _handler.HandleAsync(command, Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("Passwords do not match.", result.Error);

            _userRepositoryMock.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
            _passwordHasherMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Username_Is_Already_Taken()
        {
            // Arrange
            var command = new RegisterUserCommand("duplicateUser", "password123", "password123");

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("duplicateUser"))
                .ReturnsAsync(new User { Username = "duplicateUser" });

            // Act
            var result = await _handler.HandleAsync(command, Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Username is already taken.", result.Error);

            _passwordHasherMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Hash_Password_Persist_User_And_Return_Success_With_Empty_Record()
        {
            // Arrange
            var command = new RegisterUserCommand("  cleanUser  ", "securePassword123", "securePassword123");

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("cleanUser"))
                .ReturnsAsync((User?)null);

            _passwordHasherMock.Setup(p => p.HashPassword("securePassword123"))
                .Returns("encrypted_hash_token_string");

            // Act
            var result = await _handler.HandleAsync(command, Guid.Empty);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);

            Assert.NotNull(result.Value);
            Assert.IsType<RegisterUserResponse>(result.Value);

            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(user =>
                user.Username == "cleanUser" &&
                user.PasswordHash == "encrypted_hash_token_string" &&
                user.Id != Guid.Empty
            )), Times.Once);
        }
    }
}
