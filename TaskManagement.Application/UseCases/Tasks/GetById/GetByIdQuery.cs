using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.UseCases.Tasks.GetById
{
    public record GetByIdQuery(Guid Id);

    public record GetByIdResponse(Guid Id,string Title,string Description,int Status,DateTime DueDate
    );
}
