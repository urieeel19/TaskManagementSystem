using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.UseCases.Auth.Login;

namespace TaskManagement.UnitTests.UseCases.Auth.Login
{
    public class UserLoginDtoValidatorTests
    {
        private readonly UserLoginDtoValidator _validator;

        public UserLoginDtoValidatorTests()
        {
            _validator = new UserLoginDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Username_Or_Password_Are_Empty()
        {
            // Arrange
            var model = new LoginCommand("", "");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Username).WithErrorMessage("Username is required.");
            result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required.");
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Login_Payload_Is_Filled()
        {
            // Arrange
            var model = new LoginCommand("auth_user", "Secret123!");

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
