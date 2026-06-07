using FluentValidation;

namespace TaskManagement.Application.UseCases.Auth.Login
{
    public class UserLoginDtoValidator : AbstractValidator<LoginCommand>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
