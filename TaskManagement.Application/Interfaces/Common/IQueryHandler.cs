using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces.Common
{
    public interface IQueryHandler<in TQuery, TResponse>
    {
        Task<TResponse> HandleAsync(TQuery query, Guid userId);
    }
}
