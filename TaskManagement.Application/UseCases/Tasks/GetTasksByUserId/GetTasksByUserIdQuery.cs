using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.UseCases.Tasks.GetTasksByUserId
{
    public record GetTasksByUserIdQuery();

    public record GetTaskResponse(Guid Id,string Title,string Description,int Status,DateTime DueDate);
}
