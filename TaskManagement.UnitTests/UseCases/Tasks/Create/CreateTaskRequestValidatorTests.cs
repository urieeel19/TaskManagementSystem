using FluentValidation.TestHelper;
using TaskManagement.Application.UseCases.Tasks.Create;
using TaskManagement.Domain.Enums;

namespace TaskManagement.UnitTests.UseCases.Tasks.Create
{
    public class CreateTaskCommandValidatorTests
    {
        private readonly CreateTaskCommandValidator _validator;

        public CreateTaskCommandValidatorTests()
        {
            _validator = new CreateTaskCommandValidator();
        }

        [Fact]
        public void Validator_Should_Not_Have_Error_When_Title_Is_Valid()
        {
            // Arrange
            var command = new CreateTaskCommand("Valid Task Title", "Description", 0, DateTime.UtcNow.AddDays(1));

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validator_Should_Have_Error_When_Title_Is_Null_Or_Whitespace(string invalidTitle)
        {
            // Arrange
            var command = new CreateTaskCommand(invalidTitle, "Description", 0, DateTime.UtcNow.AddDays(1));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage("The task title cannot be empty.");
        }

        [Fact]
        public void Validator_Should_Have_Error_When_Title_Exceeds_100_Characters()
        {
            // Arrange: 
            string longTitle = new string('A', 101);
            var command = new CreateTaskCommand(longTitle, "Description", 0, DateTime.UtcNow.AddDays(1));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage("The task title cannot exceed 100 characters.");
        }

        [Theory]
        [InlineData((int)TodoStatus.Pending)] 
        [InlineData((int)TodoStatus.Completed)] 
        public void Validator_Should_Not_Have_Error_When_Status_Is_Defined_In_Enum(int validStatus)
        {
            // Arrange
            var command = new CreateTaskCommand("Title", "Description", validStatus, DateTime.UtcNow.AddDays(1));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Status);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(99)]
        public void Validator_Should_Have_Error_When_Status_Is_Not_Defined_In_Enum(int invalidStatus)
        {
            // Arrange: Enviamos un entero que no corresponde a ningún miembro del Enum TodoStatus
            var command = new CreateTaskCommand("Title", "Description", invalidStatus, DateTime.UtcNow.AddDays(1));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage("Invalid todo status value.");
        }
    }
}
