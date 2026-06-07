using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces.Common
{
    public interface ICommandHandler<in TCommand, TResponse>
    {
        Task<TResponse> HandleAsync(TCommand command, Guid userId);
    }
}
