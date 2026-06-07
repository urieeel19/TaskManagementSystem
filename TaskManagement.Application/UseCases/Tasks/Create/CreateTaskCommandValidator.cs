using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.UseCases.Tasks.Create
{
    public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
    {
        public CreateTaskCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("The task title cannot be empty.")
                .MaximumLength(100).WithMessage("The task title cannot exceed 100 characters.");

            RuleFor(x => x.Status)
                .Must(status => Enum.IsDefined(typeof(TodoStatus), status))
                .WithMessage("Invalid todo status value.");
        }
    }
}
