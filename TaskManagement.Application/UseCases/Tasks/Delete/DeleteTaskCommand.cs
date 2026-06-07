using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.UseCases.Tasks.Delete
{
    public record DeleteTaskCommand(Guid Id);
}
