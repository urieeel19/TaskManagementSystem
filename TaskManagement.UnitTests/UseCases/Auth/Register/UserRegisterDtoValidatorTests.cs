using FluentValidation.TestHelper;
using TaskManagement.Application.UseCases.Auth.Register;

namespace TaskManagement.UnitTests.UseCases.Auth.Register
{
    public class UserRegisterDtoValidatorTests
    {
        private readonly UserRegisterDtoValidator _validator;

        public UserRegisterDtoValidatorTests()
        {
            _validator = new UserRegisterDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Username_Is_Empty()
        {
            // Arrange - 
            var model = new RegisterUserCommand("", "SecurePassword123", "SecurePassword123");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                  .WithErrorMessage("Username is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Username_Is_Too_Short()
        {
            // Arrange
            var model = new RegisterUserCommand("un", "SecurePassword123", "SecurePassword123");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                  .WithErrorMessage("Username must be at least 3 characters long.");
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            // Arrange 
            var model = new RegisterUserCommand("seniorDev", "12345", "12345");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must be at least 6 characters long.");
        }

        [Fact]
        public void Should_Have_Error_When_ConfirmPassword_Is_Empty()
        {
            // Arrange
            var model = new RegisterUserCommand("seniorDev", "SecurePassword123", "");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
                  .WithErrorMessage("Please confirm your password.");
        }

        [Fact]
        public void Should_Have_Error_When_Passwords_Do_Not_Match()
        {
            // Arrange
            var model = new RegisterUserCommand("seniorDev", "SecurePassword123", "DifferentPassword123");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
                  .WithErrorMessage("Passwords do not match.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Registration_Payload_Is_Valid()
        {
            // Arrange 
            var model = new RegisterUserCommand("cleanCoder", "ValidSecurePassword123", "ValidSecurePassword123");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
