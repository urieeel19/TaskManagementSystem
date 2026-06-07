using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.UseCases.Auth.Login;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.UnitTests.UseCases.Auth.Login
{
    public class LoginHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IValidator<LoginCommand>> _validatorMock;
        private readonly LoginHandler _handler;

        public LoginHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenServiceMock = new Mock<ITokenService>();
            _validatorMock = new Mock<IValidator<LoginCommand>>();

            _handler = new LoginHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object,
                _validatorMock.Object);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_FluentValidation_Fails()
        {
            // Arrange
            var command = new LoginCommand("", "password123");
            var failures = new List<ValidationFailure> { new("Username", "Username is required.") };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _handler.HandleAsync(command, Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Username is required.", result.Error);
            _tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_User_Does_Not_Exist()
        {
            // Arrange
            var command = new LoginCommand("nonexistentUser", "password123");

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("nonexistentUser"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _handler.HandleAsync(command, Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Invalid username or password.", result.Error);
            _passwordHasherMock.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Failure_When_Password_Is_Incorrect()
        {
            // Arrange
            var command = new LoginCommand("validUser", "wrongPassword");
            var dbUser = new User { Username = "validUser", PasswordHash = "correct_hash_in_db" };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("validUser")).ReturnsAsync(dbUser);
            _passwordHasherMock.Setup(p => p.VerifyPassword("wrongPassword", "correct_hash_in_db")).Returns(false);

            // Act
            var result = await _handler.HandleAsync(command, Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Invalid username or password.", result.Error);
            _tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Token_Payload_When_Credentials_Are_Valid()
        {
            // Arrange
            var command = new LoginCommand("validUser", "correctPassword");
            var dbUser = new User { Username = "validUser", PasswordHash = "correct_hash_in_db" };

            _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("validUser")).ReturnsAsync(dbUser);
            _passwordHasherMock.Setup(p => p.VerifyPassword("correctPassword", "correct_hash_in_db")).Returns(true);
            _tokenServiceMock.Setup(t => t.GenerateToken(dbUser)).Returns("mocked_jwt_token_string");

            // Act
            var result = await _handler.HandleAsync(command, Guid.Empty);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("mocked_jwt_token_string", result.Value.Token);
            Assert.Equal("validUser", result.Value.Username);
        }
    }
}
