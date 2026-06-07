using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.UseCases.Tasks.Update
{
    public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
    {
        public UpdateTaskCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("The task ID is required for updating.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("The task title cannot be empty.")
                .MaximumLength(100).WithMessage("The task title cannot exceed 100 characters.");

            RuleFor(x => x.Status)
                .Must(status => Enum.IsDefined(typeof(TodoStatus), status))
                .WithMessage("Invalid todo status value.");
        }
    }
}
