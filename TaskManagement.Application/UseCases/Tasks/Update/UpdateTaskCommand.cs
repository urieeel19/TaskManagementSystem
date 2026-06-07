using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.UseCases.Tasks.Update
{
    public record UpdateTaskCommand(Guid Id, string Title, string Description, int Status, DateTime DueDate);
    public record UpdateTaskResponse(Guid Id, string Title, string Description, int Status, DateTime DueDate);
}
